using System.Numerics;

namespace SpinBladeArena.LogicCenter;

public abstract class AIPlayer(int userId, string userName, Vector2 position) : Player(userId, userName, position)
{
    static int _nextAIUserId = -1;

    public abstract float ReactionTimeMS { get; }

    public abstract AIPreference Preference { get; }

    private float _accumulatedTime = 0;

    public override AddPlayerRequest CreateRespawnRequest() => new AddAIPlayerRequest(Preference, UserId, UserName);

    public static AIPlayer CreateRandom(Vector2 position, HashSet<string> knownNames)
    {
        AIPreference preference = (AIPreference)Random.Shared.Next(_names.Count);
        string userName = RandomName(preference, knownNames);
        int userId = _nextAIUserId--;
        return Create(position, preference, userName, userId);
    }

    private static AIPlayer Create(Vector2 position, AIPreference preference, string userName, int userId)
    {
        return preference switch
        {
            AIPreference.Peaceful => new PeacefulAIPlayer(userId, userName, position),
            AIPreference.Aggressive => new AggressiveAIPlayer(userId, userName, position),
            AIPreference.Defensive => new DefensiveAIPlayer(userId, userName, position),
            _ => throw new NotImplementedException()
        };
    }

    public static AIPlayer CreateRespawn(AddAIPlayerRequest req, Vector2 position)
    {
        return Create(position, req.AIPreference, req.UserName, req.UserId);
    }

    private static string RandomName(AIPreference preference, HashSet<string> knownNames)
    {
        var nameBatch = _names[preference];
        string name;
        do
        {
            name = nameBatch[Random.Shared.Next(nameBatch.Length)];
        } while (knownNames.Contains(name));

        return name;
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
        if (Weapon.Count == 0 && things.Bonuses.Any())
        {
            Position = things.Bonuses.First().Bonus.Position;
            return;
        }

        Think(lobby, things);
    }

    protected abstract void Think(Lobby lobby, CloseastThings things);

    protected CloseastThings GetCloseastThings(Lobby lobby, int maxCount = 3)
    {
        PlayerDistance[] closestPlayers = lobby.Players
            .Where(p => p.UserId != UserId)
            .Select(p => new PlayerDistance(p, Vector2.Distance(Position, p.Position) - Size - p.Size))
            .OrderBy(p => p.Distance)
            .Take(maxCount)
            .ToArray();
        BonusDistance[] closestBonuses = lobby.PickableBonuses
            .Select(b => new BonusDistance(b, Vector2.Distance(Position, b.Position) - Size))
            .OrderBy(b => b.Distance)
            .Take(maxCount)
            .ToArray();
        return new CloseastThings(closestPlayers, closestBonuses);
    }

    protected void RunAwayFromDanger(Player danger)
    {
        Destination = Position + Vector2.Normalize(Position - danger.Position) * MovementSpeedPerSecond * ReactionTimeMS * 2;
    }

    protected PlayerDistance? FindCloseastDanger(CloseastThings closestThings)
    {
        return closestThings.Players.FirstOrDefault(p => p.Player.IsDangerousToPlayer(this, ReactionTimeMS));
    }

    protected PlayerDistance? FindApprochingDanger(CloseastThings closestThings)
    {
        return closestThings.Players.FirstOrDefault(p => p.Distance < MovementSpeedPerSecond);
    }
}

public class PeacefulAIPlayer(int userId, string userName, Vector2 position) : AIPlayer(userId, userName, position)
{
    public override float ReactionTimeMS => 0.2f;

    public override AIPreference Preference => AIPreference.Peaceful;

    protected override void Think(Lobby lobby, CloseastThings things)
    {
        // if some one approach, run away
        PlayerDistance? danger = FindApprochingDanger(things);
        if (danger != null)
        {
            RunAwayFromDanger(danger.Player);
            return;
        }

        // get the closest bonus
        Bonus? closestBonus = things.Bonuses.FirstOrDefault()?.Bonus;
        if (closestBonus != null)
        {
            Destination = closestBonus.Position;
            return;
        }
    }
}

public class AggressiveAIPlayer(int userId, string userName, Vector2 position) : AIPlayer(userId, userName, position)
{
    public override float ReactionTimeMS => 0.1f;

    public override AIPreference Preference => AIPreference.Aggressive;

    protected override void Think(Lobby lobby, CloseastThings things)
    {
        // if been attacked, run away
        PlayerDistance? danger = FindCloseastDanger(things);
        if (danger != null)
        {
            RunAwayFromDanger(danger.Player);
            return;
        }

        // if I can get someone in 2 seconds, attack
        PlayerDistance? target = things.Players.FirstOrDefault(p => p.Distance < MovementSpeedPerSecond * 2);
        if (target != null)
        {
            Destination = target.Player.Position;
            return;
        }

        // get the closest bonus, prefer blade first
        Bonus? bladeBonus = things.Bonuses.FirstOrDefault(b => b.Bonus.Name == BonusNames.BladeCount3 || b.Bonus.Name == BonusNames.BladeCount)?.Bonus;
        if (bladeBonus != null)
        {
            Destination = bladeBonus.Position;
            return;
        }

        // if no blade, get the closest bonus
        Bonus? closestBonus = things.Bonuses.FirstOrDefault()?.Bonus;
        if (closestBonus != null)
        {
            Destination = closestBonus.Position;
            return;
        }
    }
}

public class DefensiveAIPlayer(int userId, string userName, Vector2 position) : AIPlayer(userId, userName, position)
{
    public override float ReactionTimeMS => 0.12f;

    public override AIPreference Preference => AIPreference.Defensive;

    protected override void Think(Lobby lobby, CloseastThings things)
    {
        // if been attacked, run away
        PlayerDistance? danger = FindCloseastDanger(things);
        if (danger != null)
        {
            RunAwayFromDanger(danger.Player);
            return;
        }

        // if I can get someone in 2 seconds
        PlayerDistance? target = things.Players.FirstOrDefault(p => p.Distance < MovementSpeedPerSecond * 2);
        if (target != null)
        {
            if (Score > target.Player.Score)
            {
                // if I am strong, attack
                Destination = target.Player.Position;
                return;
            }
            else
            {
                // if I am weak, run away
                RunAwayFromDanger(target.Player);
                return;
            }
        }

        // if I only have < 2 blade, get the closest blade bonus
        Bonus? bladeBonus = things.Bonuses.FirstOrDefault(b => b.Bonus.Name == BonusNames.BladeCount3 || b.Bonus.Name == BonusNames.BladeCount)?.Bonus;
        if (Weapon.Count < 2 && bladeBonus != null)
        {
            Destination = bladeBonus.Position;
            return;
        }

        // if I have exact 2 blade, get the closest blade bonus except BladeCount/BladeCount3/Random
        Bonus? nonBladeBonus = things.Bonuses.FirstOrDefault(b => b.Bonus.Name != BonusNames.BladeCount3 && b.Bonus.Name != BonusNames.BladeCount && b.Bonus.Name != BonusNames.Random)?.Bonus;
        if (Weapon.Count == 2 && nonBladeBonus != null)
        {
            Destination = nonBladeBonus.Position;
            return;
        }

        // if I have more than 2 blade, get the closest bonus
        Bonus? closestBonus = things.Bonuses.FirstOrDefault()?.Bonus;
        if (closestBonus != null)
        {
            Destination = closestBonus.Position;
            return;
        }
    }
}

public record PlayerDistance(Player Player, float Distance);
public record BonusDistance(Bonus Bonus, float Distance);
public record CloseastThings(PlayerDistance[] Players, BonusDistance[] Bonuses);