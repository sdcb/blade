using SpinBladeArena.LogicCenter.AI;
using SpinBladeArena.LogicCenter.Lobbies;
using SpinBladeArena.Performance;
using SpinBladeArena.Primitives;
using SpinBladeArena.Users;
using System.Numerics;

namespace SpinBladeArena.LogicCenter;

public abstract partial class Lobby(int id, LobbyCreateOptions CreateOptions, IServiceProvider ServiceProvider)
{
    public int Id => id;
    public int CreateUserId => CreateOptions.CreateUserId;
    public DateTime CreateTime => CreateOptions.CreateTime;

    private readonly UserManager UserManager = ServiceProvider.GetRequiredService<UserManager>();
    public readonly PerformanceManager PerformanceManager = new();

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

    private void EnsureAIPlayers()
    {
        if (Players.OfType<AIPlayer>().Any()) return;

        int aiPlayerCount = CreateOptions.RobotCount;
        int[] userIds = RandomNumbersGenerator.GetRandomNumbers(-UserManager.AIUserCount, 0, aiPlayerCount);
        for (int i = 0; i < aiPlayerCount; ++i)
        {
            UserInfo user = UserManager.GetUser(userIds[i])!;
            Player aiPlayer = AIPlayer.CreateFor(RandomPosition(), user);
            Players.Add(aiPlayer);
        }
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

public record AddPlayerRequest(int UserId, StatInfo StatInfo);

public record AddAIPlayerRequest(AIPreference AIPreference, int UserId, StatInfo StatInfo) : AddPlayerRequest(UserId, StatInfo);