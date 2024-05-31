using System.Numerics;

namespace SpinBladeArena.LogicCenter.AI;

public class DefensiveAIPlayer(int userId, Vector2 position, StatInfo statInfo) : AIPlayer(userId, position, statInfo)
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
        Bonus? bladeBonus = things.GetPerferedBonus(BonusType.BladeCount3, BonusType.BladeCount);
        if (Weapon.Count < 2 && bladeBonus != null)
        {
            Destination = bladeBonus.Position;
            return;
        }

        PlayerDistance? target = things.Players.FirstOrDefault(p => p.Distance < MovementSpeedPerSecond * 2);
        if (target != null)
        {
            if (Weapon.WeaponScore > target.Player.Weapon.WeaponScore)
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

        // if I have gold blade, get the closest blade bonus except BladeCount/BladeCount3/Random/Thin
        if (Weapon.Any(Weapon.IsGoldBlade))
        {
            Bonus? b = things.GetPerferedBonus(BonusType.Speed20, BonusType.Speed, BonusType.BladeLength20, BonusType.BladeLength, BonusType.Health);
            if (b != null)
            {
                Destination = b.Position;
                return;
            }
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
