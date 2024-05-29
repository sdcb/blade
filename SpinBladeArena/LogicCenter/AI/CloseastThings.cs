namespace SpinBladeArena.LogicCenter.AI;

public record CloseastThings(PlayerDistance[] Players, BonusDistance[] Bonuses)
{
    public Bonus? GetPerferedBonus(params string[] perferPriorities)
    {
        foreach (string perferPriority in perferPriorities)
        {
            BonusDistance? perferedBonus = Bonuses.FirstOrDefault(b => b.Bonus.Name == perferPriority);
            if (perferedBonus != null)
            {
                return perferedBonus.Bonus;
            }
        }
        return null;
    }
}