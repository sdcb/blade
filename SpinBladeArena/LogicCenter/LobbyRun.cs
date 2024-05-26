using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.Hubs;
using SpinBladeArena.LogicCenter.AI;
using SpinBladeArena.Performance;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace SpinBladeArena.LogicCenter;

public partial record Lobby
{
    private readonly PushManager pushManager = new(Id, ServiceProvider.GetRequiredService<IHubContext<GameHub, IGameHubClient>>(), ServiceProvider.GetRequiredKeyedService<int>("ServerFPS"));

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
                    string userName = UserManager.GetUser(req.UserId)?.Name ?? req.UserName;
                    Player newPlayer = req is AddAIPlayerRequest ai ? AIPlayer.CreateRespawn(ai, RandomPosition()) : new Player(req.UserId, userName, RandomPosition())
                    {
                    };
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
                // 如果玩家之间的距离小于玩家圆的直径，则以双方的Size为权重，将双方推开
                for (int i = 0; i < Players.Count - 1; ++i)
                {
                    Player p1 = Players[i];
                    for (int j = i + 1; j < Players.Count; ++j)
                    {
                        Player p2 = Players[j];
                        Vector2 direction = Vector2.Normalize(p2.Position - p1.Position);
                        float distance = Vector2.Distance(p1.Position, p2.Position);
                        float overlap = p1.Size + p2.Size - distance;
                        
                        if (overlap > 0)
                        {
                            p1.Position -= direction * overlap * p2.Size / (p1.Size + p2.Size);
                            p2.Position += direction * overlap * p1.Size / (p1.Size + p2.Size);
                        }
                    }
                }
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

            pushManager.Push(ToPushState());
            stat.RecordDispatchMessage();

            PerformanceManager.Add(stat.ToPerformanceData(iterationIndex));
        }
    }

    private PushState ToPushState()
    {
        PlayerDto[] playerDtos = Players.Select(x => x.ToDto()).ToArray();
        PickableBonusDto[] pickableBonusDtos = PickableBonuses.Select(x => x.ToDto()).ToArray();
        PlayerDto[] deadPlayerDtos = DeadPlayers.Select(x => x.ToDto()).ToArray();
        return new PushState(playerDtos, pickableBonusDtos, deadPlayerDtos);
    }
}
