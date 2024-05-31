using System.Numerics;

namespace SpinBladeArena.LogicCenter.AI;

public class AggressiveAIPlayer(int userId, Vector2 position) : AIPlayer(userId, position)
{
    public override float ReactionTimeMS => 0.1f;

    public override AIPreference Preference => AIPreference.Aggressive;

    protected override void Think(Lobby lobby, CloseastThings things)
    {
        // if been attacked, run away
        PlayerDistance[] danger = FindCloseastDangers(things);
        if (danger.Length > 0)
        {
            RunAwayFromDangers(danger);
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
        Bonus? bladeBonus = things.Bonuses.FirstOrDefault(b => b.Bonus.Type == BonusType.BladeCount3 || b.Bonus.Type == BonusType.BladeCount)?.Bonus;
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
