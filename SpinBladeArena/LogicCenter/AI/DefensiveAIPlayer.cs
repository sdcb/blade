using System.Numerics;

namespace SpinBladeArena.LogicCenter.AI;

public class DefensiveAIPlayer(int userId, string userName, Vector2 position) : AIPlayer(userId, userName, position)
{
    public override float ReactionTimeMS => 0.12f;

    public override AIPreference Preference => AIPreference.Defensive;

    protected override void Think(Lobby lobby, CloseastThings things)
    {
        // if been attacked, run away
        PlayerDistance[] dangers = FindCloseastDangers(things);
        if (dangers.Length > 0)
        {
            RunAwayFromDangers(dangers);
            return;
        }

        // if I only have < 2 blade, get the closest blade bonus
        Bonus? bladeBonus = things.Bonuses.FirstOrDefault(b => b.Bonus.Name == BonusNames.BladeCount3 || b.Bonus.Name == BonusNames.BladeCount)?.Bonus;
        if (Weapon.Count < 2 && bladeBonus != null)
        {
            Destination = bladeBonus.Position;
            return;
        }

        PlayerDistance? target = things.Players.FirstOrDefault(p => p.Distance < MovementSpeedPerSecond * 2);
        if (target != null)
        {
            if (Score > target.Player.Score)
            {
                // if I can get someone in 2 seconds and I am strong, attack
                Destination = target.Player.Position;
                return;
            }
            else if (target.Distance < MovementSpeedPerSecond)
            {
                // if I am weak, and the target is close, run away
                RunAwayFromDangers([target]);
                return;
            }
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
