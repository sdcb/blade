using SpinBladeArena.Primitives;
using SpinBladeArena.Users;
using System.Numerics;

namespace SpinBladeArena.LogicCenter.AI;

public abstract class AIPlayer(int userId, Vector2 position, StatInfo statInfo) : Player(userId, position, statInfo)
{
    public abstract float ReactionTimeMS { get; }

    public abstract AIPreference Preference { get; }

    private float _accumulatedTime = 0;

    public override AddPlayerRequest CreateRespawnRequest() => new AddAIPlayerRequest(Preference, UserId, StatInfo.CopyResetScore());

    public static AIPlayer CreateFor(Vector2 position, UserInfo user)
    {
        AIPreference preference = _names.First(n => n.Value.Contains(user.Name)).Key;
        return Create(position, preference, user.Id, new StatInfo());
    }

    private static AIPlayer Create(Vector2 position, AIPreference preference, int userId, StatInfo statInfo)
    {
        return preference switch
        {
            AIPreference.Peaceful => new PeacefulAIPlayer(userId, position, statInfo),
            AIPreference.Aggressive => new AggressiveAIPlayer(userId, position, statInfo),
            AIPreference.Defensive => new DefensiveAIPlayer(userId, position, statInfo),
            _ => throw new NotImplementedException()
        };
    }

    public static void EnsureAIUsers(UserManager userManager)
    {
        lock (userManager)
        {
            foreach (string[] nameBatch in _names.Values)
            {
                foreach (string name in nameBatch)
                {
                    userManager.AddAIUser(new(0, name, "", true, DateTime.Now));
                }
            }
        }
    }

    public static AIPlayer CreateRespawn(AddAIPlayerRequest req, Vector2 position, StatInfo statInfo)
    {
        return Create(position, req.AIPreference, req.UserId, statInfo);
    }

    private static readonly Dictionary<AIPreference, string[]> _names = new()
    {
        [AIPreference.Peaceful] = ["小乔", "刘禅", "袁绍", "刘表", "貂蝉", "刘备", "糜竺", "鲁肃", "陶谦", "孙尚香"],
        [AIPreference.Aggressive] = ["吕布", "曹操", "张飞", "马超", "孟获", "邢道荣", "许褚", "张郃", "魏延", "关羽"],
        [AIPreference.Defensive] = ["孙权", "赵云", "诸葛亮", "曹仁", "司马懿", "周瑜", "陆逊", "姜维", "邓艾", "钟会"]
    };

    public void Think(float deltaTime, Lobby lobby)
    {
        _accumulatedTime += deltaTime;
        if (_accumulatedTime < ReactionTimeMS) return;
        _accumulatedTime = 0;

        // common: if no weapon, goes for random bonus
        CloseastThings things = GetCloseastThings(lobby);
        if (Weapon.Count == 0 && things.Bonuses.Length != 0)
        {
            Bonus target = things.GetPerferedBonus(BonusType.BladeCount3, BonusType.BladeCount, BonusType.Health, BonusType.BladeSpeed, BonusType.BladeSpeed20)
                ?? things.Bonuses.First().Bonus;
            Destination = target.Position;
            return;
        }

        Think(lobby, things);
    }

    protected abstract void Think(Lobby lobby, CloseastThings things);

    protected CloseastThings GetCloseastThings(Lobby lobby)
    {
        PlayerDistance[] closestPlayers = lobby.Players
            .Where(p => p.UserId != UserId)
            .Select(p => new PlayerDistance(p, Vector2.Distance(Position, p.Position) - Size - p.SafeDistance))
            .OrderBy(p => p.Distance)
            .Take(4)
            .ToArray();
        IEnumerable<BonusDistance> closestBonuses = lobby.PickableBonuses
            .Select(b => new BonusDistance(b, Vector2.Distance(Position, b.Position) - Size))
            .OrderBy(b => b.Distance)
            .Take(8);
        Func<Bonus, bool> checker = CreateBonusSafeChecker(closestPlayers.Select(x => x.Player));
        BonusDistance[] safeBonuses = closestBonuses
            .Where(x => checker(x.Bonus))
            .ToArray();
        return new CloseastThings(closestPlayers, safeBonuses);
    }

    protected void RunAwayFromDangers(PlayerDistance[] dangers)
    {
        Vector2[] directions = dangers.Select(danger => Vector2.Normalize(Position - danger.Player.Position)).ToArray();
        Vector2 averageDirection = directions.Aggregate((a, b) => a + b) / dangers.Length;
        Destination = Position + averageDirection * MovementSpeedPerSecond * ReactionTimeMS * 2;
    }

    protected PlayerDistance[] FindCloseastDangers(CloseastThings closestThings)
    {
        return closestThings.Players.Where(p => p.Player.IsDangerousToPlayer(this, ReactionTimeMS)).ToArray();
    }

    protected PlayerDistance[] FindCloseastDangers2(CloseastThings closestThings)
    {
        var blades = closestThings.Players
            .SelectMany(x => x.Player.Weapon.Select(w => x.Player.GetBladeLineSegment(w)));
        throw new NotImplementedException();
    }

    protected PlayerDistance[] FindApprochingDangers(CloseastThings closestThings)
    {
        return closestThings.Players.Where(p => p.Distance < ReactionTimeMS).ToArray();
    }

    public static Func<Bonus, bool> CreateBonusSafeChecker(IEnumerable<Player> players)
    {
        LineSegment[] lines = players
            .SelectMany(p => p.Weapon.Select(w => p.GetBladeLineSegment(w)))
            .ToArray();
        return b =>
        {
            Circle bonusCircle = b.ToFrontendCircle();
            return !lines.Any(l => l.IsIntersectingCircle(bonusCircle));
        };
    }
}
