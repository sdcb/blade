using System.Numerics;

namespace SpinBladeArena.LogicCenter.AI;

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
