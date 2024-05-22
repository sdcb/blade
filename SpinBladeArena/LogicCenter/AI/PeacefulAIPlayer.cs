using System.Numerics;

namespace SpinBladeArena.LogicCenter.AI;

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
