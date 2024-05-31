namespace SpinBladeArena.LogicCenter;

public enum BonusType
{
    Health,
    Thin,
    Speed,
    Speed20,
    BladeCount,
    BladeCount3,
    BladeLength,
    BladeLength20,
    BladeDamage,
    BladeSpeed,
    BladeSpeed20,
    Random
}

public static class BonusNames
{
    private static readonly Dictionary<BonusType, string> bonusTypeToNameDictionary = new()
    {
        { BonusType.Health, "生命" },
        { BonusType.Thin, "减肥" },
        { BonusType.Speed, "移速+5" },
        { BonusType.Speed20, "移速+20" },
        { BonusType.BladeCount, "刀数" },
        { BonusType.BladeCount3, "刀数+3" },
        { BonusType.BladeLength, "刀长+5" },
        { BonusType.BladeLength20, "刀长+20" },
        { BonusType.BladeDamage, "刀伤" },
        { BonusType.BladeSpeed, "刀速+5" },
        { BonusType.BladeSpeed20, "刀速+20" },
        { BonusType.Random, "随机" }
    };

    public static string ToDisplayString(this BonusType bonusType)
    {
        if (bonusTypeToNameDictionary.TryGetValue(bonusType, out var name))
        {
            return name;
        }
        throw new ArgumentException($"Invalid BonusType: {bonusType}");
    }
}