﻿using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.Hubs;
using System.Diagnostics;
using System.Numerics;

namespace SpinBladeArena.LogicCenter;

public record Lobby(int Id, int CreateUserId, DateTime CreateTime, IHubContext<GameHub, IGameHubClient> Hub)
{
    public Vector2 MaxSize = new(1000, 1000);
    public List<Player> Players = [];
    public HashSet<PickableBonus> PickableBonuses = [];
    public List<Player> DeadPlayers = [];
    private CancellationTokenSource? _cancellationTokenSource = null;
    // Key: UserId
    private readonly Dictionary<int, AddPlayerRequest> _addPlayerRequests = [];

    public Player? FindPlayer(int userId) => Players.FirstOrDefault(x => x.UserId == userId) ?? DeadPlayers.FirstOrDefault(x => x.UserId == userId);

    public void AddPlayerToRandomPosition(AddPlayerRequest req)
    {
        Player? player = FindPlayer(req.UserId);

        if (player != null)
        {
            // existing user, update connection id
            player.ConnectionId = req.ConnectionId;
        }
        else
        {
            _addPlayerRequests[req.UserId] = req;
        }
    }

    public Vector2 RandomPosition()
    {
        return new(Random.Shared.NextSingle() * MaxSize.X - MaxSize.Y / 2, Random.Shared.NextSingle() * MaxSize.Y - MaxSize.Y / 2);
    }

    public void EnsureStart()
    {
        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = new();
            new Thread(() => Run(_cancellationTokenSource.Token)).Start();
        }
    }

    public void Terminate() => _cancellationTokenSource?.Cancel();

    public void Run(CancellationToken cancellationToken)
    {
        const int DeadRespawnTimeInSeconds = 5;
        float bonusSpawnCooldown = 5;
        float maxBonusCount = 7;
        float bonusSpawnTimer = 0;

        Stopwatch sw = Stopwatch.StartNew();
        float oldTime = 0;
        float deltaTime = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            Thread.Sleep(Math.Max(1, (int)(30 - deltaTime)));
            deltaTime = MathF.Min((float)sw.Elapsed.TotalSeconds - oldTime, 0.25f);
            oldTime = (float)sw.Elapsed.TotalSeconds;

            // handle add player requests
            {
                int destinationSize = Players.Count + _addPlayerRequests.Count;
                Players.EnsureCapacity(destinationSize);
                foreach (AddPlayerRequest req in _addPlayerRequests.Values)
                {
                    Player newPlayer = new(req.UserId, req.UserName, req.ConnectionId, RandomPosition());
                    Players.Add(newPlayer);
                }
                _addPlayerRequests.Clear();
            }

            // handle move
            foreach (Player player in Players) player.Move(deltaTime);

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
                            toRemove.Add(bonus);
                        }

                    }
                }

                foreach (PickableBonus bonus in toRemove)
                {
                    PickableBonuses.Remove(bonus);
                }
            }

            // handle attack
            for (int i = 0; i < Players.Count - 1; ++i)
            {
                Player p1 = Players[i];
                for (int j = i + 1; j < Players.Count; ++j)
                {
                    Player p2 = Players[j];
                    Player.AttackEachOther(p1, p2);
                }
            }

            // handle dead
            for (int i = 0; i < Players.Count; ++i)
            {
                Player player = Players[i];
                if (player.Dead)
                {
                    player.DeadTime = sw.Elapsed.TotalSeconds;
                    // insert into dead players
                    DeadPlayers.Add(player);
                    // remove from players
                    Players.RemoveAt(i);
                    --i;
                }
            }

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

            // handle player respawn
            for (int i = 0; i < DeadPlayers.Count; ++i)
            {
                Player player = DeadPlayers[i];
                if (sw.Elapsed.TotalSeconds - player.DeadTime > DeadRespawnTimeInSeconds)
                {
                    // insert into players
                    AddPlayerToRandomPosition(new(player.UserId, player.UserName, player.ConnectionId));
                    // remove from dead players
                    DeadPlayers.RemoveAt(i);
                    --i;
                }
            }

            DispatchMessage();
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
        x = Math.Clamp(x, -MaxSize.X / 2, MaxSize.X / 2);
        y = Math.Clamp(y, -MaxSize.Y / 2, MaxSize.Y / 2);

        Player? player = FindPlayer(userId);
        if (player != null)
        {
            player.Destination = new(x, y);
        }
    }
}

public record AddPlayerRequest(int UserId, string UserName, string ConnectionId);