using SpinBladeArena.Primitives;
using System.Numerics;

namespace SpinBladeArena.LogicCenter.AI;

public class PeacefulAIPlayer(int userId, Vector2 position) : AIPlayer(userId, position)
{
    public override float ReactionTimeMS => 0.2f;

    public override AIPreference Preference => AIPreference.Peaceful;

    protected override void Think(Lobby lobby, CloseastThings things)
    {
        // if some one approach, run away
        PlayerDistance[] dangers = FindApprochingDangers(things);
        if (dangers.Length > 0)
        {
            RunAwayFromDangers(dangers);
            return;
        }

        Circle[] dangerAreas = things.Players
            .Select(p => p.Player.ToSafeDistanceCircle())
            .ToArray();

        // get the closest bonus
        Bonus? closestBonus = things.Bonuses
            .Where(bonus => !dangerAreas.Any(danger => Vector2.Distance(danger.Center, bonus.Bonus.Position) < danger.Radius + Bonus.FrontendRadius))
            .FirstOrDefault()?.Bonus;
        if (closestBonus != null)
        {
            Destination = closestBonus.Position;
            return;
        }
    }
}
