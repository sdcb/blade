using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.Hubs;
using SpinBladeArena.Performance;
using SpinBladeArena.Users;
using System.Diagnostics;
using System.Numerics;

namespace SpinBladeArena.LogicCenter;

public record Lobby(int Id, int CreateUserId, DateTime CreateTime, IServiceProvider ServiceProvider)
{
    private readonly IHubContext<GameHub, IGameHubClient> Hub = ServiceProvider.GetRequiredService<IHubContext<GameHub, IGameHubClient>>();
    private readonly UserManager UserManager = ServiceProvider.GetRequiredService<UserManager>();
    private readonly PerformanceManager PerformanceManager = ServiceProvider.GetRequiredService<PerformanceManager>();

    public Vector2 MaxSize = new(2000, 2000);
    public List<Player> Players = [];
    public HashSet<PickableBonus> PickableBonuses = [];
    public List<Player> DeadPlayers = [];
    private CancellationTokenSource? _cancellationTokenSource = null;
    // Key: UserId
    private readonly Dictionary<int, AddPlayerRequest> _addPlayerRequests = [];
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;

    public Player? FindPlayer(int userId) => Players.FirstOrDefault(x => x.UserId == userId) ?? DeadPlayers.FirstOrDefault(x => x.UserId == userId);

    public void AddPlayerToRandomPosition(AddPlayerRequest req)
    {
        LastUpdateTime = DateTime.Now;
        Player? player = FindPlayer(req.UserId);

        if (player != null)
        {
            // existing user, do nothing
        }
        else
        {
            _addPlayerRequests[req.UserId] = req;
        }
    }

    public Vector2 RandomPosition()
    {
        Vector2 loc;
        do
        {
            loc = new(Random.Shared.NextSingle() * MaxSize.X - MaxSize.Y / 2, Random.Shared.NextSingle() * MaxSize.Y - MaxSize.Y / 2);
        } while (Players.Any(x => Vector2.Distance(x.Position, loc) < x.Size));
        return loc;
    }

    private Vector2 RandomPositionWithin(Player player)
    {
        Vector2 loc;
        do
        {
            float distanceToCenter = Random.Shared.NextSingle() * player.Size * 2f;
            float angle = Random.Shared.NextSingle() * MathF.PI * 2;
            loc = player.Position + new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * distanceToCenter;
        } while (InBounds(loc) && Players.Where(x => x != player).Any(x => Vector2.Distance(x.Position, loc) < x.Size));
        return loc;
    }

    private bool InBounds(Vector2 position)
    {
        // -MaxSize.X / 2 <= x <= MaxSize.X / 2
        // -MaxSize.Y / 2 <= y <= MaxSize.Y / 2
        return position.X >= -MaxSize.X / 2 && position.X <= MaxSize.X / 2 && position.Y >= -MaxSize.Y / 2 && position.Y <= MaxSize.Y / 2;
    }

    Thread? _runningThread;

    public void EnsureStart()
    {
        lock (this)
        {
            if (!IsThreadRunning(_runningThread))
            {
                _cancellationTokenSource = new();
                _runningThread = new Thread(() => Run(_cancellationTokenSource.Token));
                _runningThread.Start();
            }
        }

        static bool IsThreadRunning(Thread? thread)
        {
            if (thread == null) return false;
            return thread.ThreadState == System.Threading.ThreadState.Running || thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin;
        }
    }

    public void Terminate() => _cancellationTokenSource?.Cancel();

    public void Run(CancellationToken cancellationToken)
    {
        const int DeadRespawnTimeInSeconds = 3;
        float bonusSpawnCooldown = 1;
        float maxBonusCount = 25;
        float bonusSpawnTimer = 0;

        Stopwatch allTimeStopwatch = Stopwatch.StartNew();
        float oldTime = 0;
        float deltaTime = 0;
        for (long iterationIndex = 0; !cancellationToken.IsCancellationRequested; ++iterationIndex)
        {
            if (DateTime.Now - LastUpdateTime > TimeSpan.FromSeconds(180))
            {
                return;
            }

            PerformanceCounter stat = PerformanceCounter.Start();
            Thread.Sleep(Math.Max(1, (int)(15 - deltaTime)));
            deltaTime = MathF.Min((float)allTimeStopwatch.Elapsed.TotalSeconds - oldTime, 0.25f);
            oldTime = (float)allTimeStopwatch.Elapsed.TotalSeconds;
            stat.RecordSleep();

            // handle add player requests
            {
                int destinationSize = Players.Count + _addPlayerRequests.Count;
                Players.EnsureCapacity(destinationSize);
                foreach (AddPlayerRequest req in _addPlayerRequests.Values)
                {
                    Player newPlayer = new(req.UserId, req.UserName, RandomPosition());
                    Players.Add(newPlayer);
                }
                _addPlayerRequests.Clear();
                stat.RecordAddPlayerRequests();
            }

            // handle move
            {
                foreach (Player player in Players) player.Move(deltaTime);
                stat.RecordMove();
            }

            // handle bonus
            {
                List<PickableBonus> toRemove = new(capacity: PickableBonuses.Count / 2);
                foreach (Player player in Players)
                {
                    foreach (PickableBonus bonus in PickableBonuses)
                    {
                        if (Vector2.Distance(player.Position, bonus.Position) < player.Size)
                        {
                            bonus.Apply(player);
                            player.Score += 1;
                            toRemove.Add(bonus);
                        }

                    }
                }

                foreach (PickableBonus bonus in toRemove)
                {
                    PickableBonuses.Remove(bonus);
                }
                stat.RecordBonus();
            }

            // handle attack
            List<KillingInfo> killingInfoConfirmedDeads = [];
            for (int i = 0; i < Players.Count - 1; ++i)
            {
                Player p1 = Players[i];
                for (int j = i + 1; j < Players.Count; ++j)
                {
                    Player p2 = Players[j];
                    KillingInfo killingInfo = Player.AttackEachOther(p1, p2);
                    if (killingInfo.AnyDead)
                    {
                        killingInfoConfirmedDeads.Add(killingInfo);
                    }
                }
            }
            stat.RecordAttack();

            // handle dead
            for (int i = 0; i < Players.Count; ++i)
            {
                Player player = Players[i];
                if (player.Dead)
                {
                    player.DeadTime = allTimeStopwatch.Elapsed.TotalSeconds;
                    // insert into dead players
                    DeadPlayers.Add(player);
                    // remove from players
                    Players.RemoveAt(i);
                    --i;
                }
            }
            stat.RecordDead();

            // handle bonus spawn cooldown
            bonusSpawnTimer += deltaTime;
            while (bonusSpawnTimer > bonusSpawnCooldown)
            {
                if (PickableBonuses.Count < maxBonusCount)
                {
                    PickableBonuses.Add(PickableBonus.CreateRandom(RandomPosition()));
                }
                bonusSpawnTimer -= bonusSpawnCooldown;
            }
            // handle bonus spawn from dead players
            foreach (KillingInfo ki in killingInfoConfirmedDeads)
            {
                if (ki.Player1Dead)
                {
                    for (int i = 0; i < ki.Player1.Score / 2; ++i)
                    {
                        PickableBonuses.Add(PickableBonus.CreateRandom(RandomPositionWithin(ki.Player1)));
                    }
                }
                if (ki.Player2Dead)
                {
                    for (int i = 0; i < ki.Player2.Score / 2; ++i)
                    {
                        PickableBonuses.Add(PickableBonus.CreateRandom(RandomPositionWithin(ki.Player2)));
                    }
                }
            }
            stat.RecordBonusSpawn();

            // handle player respawn
            for (int i = 0; i < DeadPlayers.Count; ++i)
            {
                Player player = DeadPlayers[i];
                if (allTimeStopwatch.Elapsed.TotalSeconds - player.DeadTime > DeadRespawnTimeInSeconds)
                {
                    if (UserManager.IsUserOnline(player.UserId))
                    {
                        // insert into players
                        _addPlayerRequests[player.UserId] = new(player.UserId, player.UserName);
                    }

                    // remove from dead players
                    DeadPlayers.RemoveAt(i);
                    --i;
                }
            }
            stat.RecordPlayerSpawn();

            DispatchMessage();
            stat.RecordDispatchMessage();

            PerformanceManager.Add(stat.ToPerformanceData(iterationIndex));
        }
    }

    private void DispatchMessage()
    {
        PlayerDto[] playerDtos = Players.Select(x => x.ToDto()).ToArray();
        PickableBonusDto[] pickableBonusDtos = PickableBonuses.Select(x => x.ToDto()).ToArray();
        PlayerDto[] deadPlayerDtos = DeadPlayers.Select(x => x.ToDto()).ToArray();
        Hub.Clients.Group(Id.ToString()).Update(playerDtos, pickableBonusDtos, deadPlayerDtos);
    }

    internal void SetPlayerDestination(int userId, float x, float y)
    {
        LastUpdateTime = DateTime.Now;

        x = Math.Clamp(x, -MaxSize.X / 2, MaxSize.X / 2);
        y = Math.Clamp(y, -MaxSize.Y / 2, MaxSize.Y / 2);

        Player? player = FindPlayer(userId);
        if (player != null)
        {
            player.Destination = new(x, y);
        }
    }
}

public record AddPlayerRequest(int UserId, string UserName);