namespace SpinBladeArena.LogicCenter.AI;

public record CloseastThings(PlayerDistance[] Players, BonusDistance[] Bonuses)
{
    public Bonus? GetPerferedBonus(params BonusType[] perferPriorities)
    {
        foreach (BonusType perferPriority in perferPriorities)
        {
            BonusDistance? perferedBonus = Bonuses.FirstOrDefault(b => b.Bonus.Type == perferPriority);
            if (perferedBonus != null)
            {
                return perferedBonus.Bonus;
            }
        }
        return null;
    }
}