using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.Hubs;
using SpinBladeArena.LogicCenter.AI;
using SpinBladeArena.Performance;
using SpinBladeArena.Users;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace SpinBladeArena.LogicCenter;

public record Lobby(int Id, int CreateUserId, DateTime CreateTime, IServiceProvider ServiceProvider)
{
    private readonly IHubContext<GameHub, IGameHubClient> Hub = ServiceProvider.GetRequiredService<IHubContext<GameHub, IGameHubClient>>();
    private readonly UserManager UserManager = ServiceProvider.GetRequiredService<UserManager>();
    private readonly PerformanceManager PerformanceManager = ServiceProvider.GetRequiredService<PerformanceManager>();

    public readonly int ServerFPS = ServiceProvider.GetRequiredKeyedService<int>("ServerFPS");
    public Vector2 MaxSize = new(2000, 2000);
    public List<Player> Players = [];
    public HashSet<Bonus> PickableBonuses = [];
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
        } while (Players.Any(player => Vector2.Distance(player.Position, loc) < player.SafeDistance));
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
        } while (!InBounds(loc) || Players.Where(x => x != player).Any(x => Vector2.Distance(x.Position, loc) < x.Size));
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
        EnsureAIPlayers();

        const int DeadRespawnTimeInSeconds = 3;
        float bonusSpawnCooldown = 0.5f;
        float maxBonusCount = 25;
        float bonusSpawnTimer = 0;

        Stopwatch allTimeStopwatch = Stopwatch.StartNew();
        for (long iterationIndex = 0; !cancellationToken.IsCancellationRequested; ++iterationIndex)
        {
            if (DateTime.Now - LastUpdateTime > TimeSpan.FromSeconds(180))
            {
                return;
            }

            PerformanceCounter stat = PerformanceCounter.Start();
            Thread.Sleep(Math.Max(1, (int)(1000.0 / ServerFPS - PerformanceManager.Latest.AllExceptSleep.TotalMilliseconds)));
            stat.RecordSleep();
            float dt = MathF.Min((float)stat.Sleep.TotalSeconds, 0.25f);

            // handle add player requests
            {
                int destinationSize = Players.Count + _addPlayerRequests.Count;
                Players.EnsureCapacity(destinationSize);
                foreach (AddPlayerRequest req in _addPlayerRequests.Values)
                {
                    Player newPlayer = req is AddAIPlayerRequest ai ? AIPlayer.CreateRespawn(ai, RandomPosition()) : new Player(req.UserId, req.UserName, RandomPosition());
                    Players.Add(newPlayer);
                }
                _addPlayerRequests.Clear();
                stat.RecordAddPlayerRequests();
            }

            // handle AI Think
            {
                foreach (AIPlayer player in Players.OfType<AIPlayer>()) player.Think(dt, this);
                stat.RecordAIThink();
            }

            // handle move
            {
                RectangleF bounds = new(-MaxSize.X / 2, -MaxSize.Y / 2, MaxSize.X, MaxSize.Y);
                foreach (Player player in Players) player.Move(dt, bounds);
                stat.RecordMove();
            }

            // handle bonus
            {
                List<Bonus> toRemove = new(capacity: PickableBonuses.Count / 2);
                foreach (Player player in Players)
                {
                    foreach (Bonus bonus in PickableBonuses)
                    {
                        if (Vector2.Distance(player.Position, bonus.Position) < player.Size)
                        {
                            bonus.Apply(player);
                            player.Score += 1;
                            toRemove.Add(bonus);
                        }

                    }
                }

                foreach (Bonus bonus in toRemove)
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
            bonusSpawnTimer += dt;
            while (bonusSpawnTimer > bonusSpawnCooldown)
            {
                if (PickableBonuses.Count < maxBonusCount)
                {
                    PickableBonuses.Add(Bonus.CreateRandom(RandomPosition()));
                }
                bonusSpawnTimer -= bonusSpawnCooldown;
            }
            // handle bonus spawn from dead players
            foreach (KillingInfo ki in killingInfoConfirmedDeads)
            {
                if (ki.Player1Dead)
                {
                    if (ki.Player1.Score < 3 && Random.Shared.NextDouble() < (1.0 / ki.Player1.Score))
                    {
                        PickableBonuses.Add(Bonus.CreateRandom(RandomPositionWithin(ki.Player1)));
                    }
                    for (int i = 0; i < ki.Player1.Score / 3; ++i)
                    {
                        PickableBonuses.Add(Bonus.CreateRandom(RandomPositionWithin(ki.Player1)));
                    }
                }
                if (ki.Player2Dead)
                {
                    if (ki.Player2.Score < 3 && Random.Shared.NextDouble() < (1.0 / ki.Player2.Score))
                    {
                        PickableBonuses.Add(Bonus.CreateRandom(RandomPositionWithin(ki.Player2)));
                    }
                    for (int i = 0; i < ki.Player2.Score / 3; ++i)
                    {
                        PickableBonuses.Add(Bonus.CreateRandom(RandomPositionWithin(ki.Player2)));
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
                    if (player is AIPlayer || UserManager.IsUserOnline(player.UserId))
                    {
                        // insert into players
                        _addPlayerRequests[player.UserId] = player.CreateRespawnRequest();
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

    private void EnsureAIPlayers()
    {
        if (Players.OfType<AIPlayer>().Any()) return;

        int aiPlayerCount = ServiceProvider.GetRequiredKeyedService<int>("AIPlayerCount");
        HashSet<string> knownNames = [];
        for (int i = 0; i < aiPlayerCount; ++i)
        {
            Player aiPlayer = AIPlayer.CreateRandom(RandomPosition(), knownNames);
            Players.Add(aiPlayer);
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

public record AddAIPlayerRequest(AIPreference AIPreference, int UserId, string UserName) : AddPlayerRequest(UserId, UserName);