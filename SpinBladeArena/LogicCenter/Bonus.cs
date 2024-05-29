using SpinBladeArena.LogicCenter.Push;
using SpinBladeArena.Primitives;
using System.Numerics;

namespace SpinBladeArena.LogicCenter;

public class Bonus(string name, Vector2 position)
{
    public string Name { get; init; } = name;
    public Vector2 Position { get; init; } = position;

    public const int FrontendRadius = 20;

    public required BonusApplier Apply { get; init; }

    public Circle ToFrontendCircle() => new(Position, FrontendRadius);

    public static Bonus Health(Vector2 position, float healthAmount = 8) => new(BonusNames.Health, position)
    {
        Apply = (Player player) =>
        {
            player.Health += healthAmount;
        }
    };

    public static Bonus Thin(Vector2 position) => new(BonusNames.Thin, position)
    {
        Apply = (Player player) =>
        {
            float oldHealth = player.Health;
            player.Health = Math.Clamp(MathF.Round(player.Health / 2), 1, 200);
        }
    };

    public static Bonus BladeCount(Vector2 position, int bladeCountAmount = 1) => new(BonusNames.BladeCount, position)
    {
        Apply = (Player player) =>
        {
            if (player.AddBlade(bladeCountAmount))
            {
                // 只有增加了刀才应用负面效果
                player.Health += 1;
                player.Weapon.AddRotationDegreePerSecond(-1, player.Size);
            }
        }
    };

    public static Bonus BladeCount3(Vector2 position, int bladeCountAmount = 3) => new(BonusNames.BladeCount3, position)
    {
        Apply = (Player player) =>
        {
            if (player.AddBlade(bladeCountAmount))
            {
                // 只有增加了刀才应用负面效果
                player.Health += 3;
                player.Weapon.AddRotationDegreePerSecond(-4, player.Size);
            }
        }
    };

    public static Bonus BladeLength(Vector2 position, float bladeLengthAmount = 5) => new(BonusNames.BladeLength, position)
    {
        Apply = (Player player) =>
        {
            if (player.Weapon.AddLength(bladeLengthAmount, player.Size))
            {
                // 只有增加了刀长才应用负面效果
                player.Health += 1;
                player.Weapon.AddRotationDegreePerSecond(-1, player.Size);
            }
        }
    };

    public static Bonus BladeLength20(Vector2 position, float bladeLengthAmount = 20) => new(BonusNames.BladeLength20, position)
    {
        Apply = (Player player) =>
        {
            if (player.Weapon.AddLength(bladeLengthAmount, player.Size))
            {
                // 只有增加了刀长才应用负面效果
                player.Health += 3;
                player.Weapon.AddRotationDegreePerSecond(-4, player.Size);
            }
        }
    };

    public static Bonus BladeDamage(Vector2 position, float bladeDamageAmount = 1) => new(BonusNames.BladeDamage, position)
    {
        Apply = (Player player) =>
        {
            if (player.Weapon.AddDamage(bladeDamageAmount, player.Size))
            {
                // 只有增加了刀伤才应用负面效果
                player.Health += 1;
                player.Weapon.AddRotationDegreePerSecond(-1, player.Size);
            }
        }
    };

    public static Bonus BladeSpeed(Vector2 position, float rotationDegreePerSecond = 5) => new(BonusNames.BladeSpeed, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddRotationDegreePerSecond(rotationDegreePerSecond, player.Size);
            player.Health += 1;
        }
    };

    public static Bonus BladeSpeed20(Vector2 position, float rotationDegreePerSecond = 20) => new(BonusNames.BladeSpeed20, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddRotationDegreePerSecond(rotationDegreePerSecond, player.Size);
            player.Health += 3;
        }
    };

    public static Bonus Random(Vector2 position) => new(BonusNames.Random, position)
    {
        Apply = (player) => CreateRandom(position)
    };

    internal static BonusFactory CreateRandomFactory()
    {
        BonusChanceDef[] All =
        [
            new (BonusNames.Health, p => Health(p)),
            new (BonusNames.BladeLength, p => BladeLength(p), 0.1f),
            new (BonusNames.BladeLength20, p => BladeLength20(p)),
            new (BonusNames.BladeDamage, p => BladeDamage(p), 0.1f),
            new (BonusNames.BladeSpeed, p => BladeSpeed(p), 0.1f),
            new (BonusNames.BladeSpeed20, p => BladeSpeed20(p)),
            new (BonusNames.Thin, p => Thin(p)),
            new (BonusNames.BladeCount, p => BladeCount(p), 0.25f),
            new (BonusNames.BladeCount3, p => BladeCount3(p)),
            //Random,
        ];
        BonusChanceDef[] haveChances = All.Where(x => x.Chance.HasValue).ToArray();
        BonusChanceDef[] noChances = All.Where(x => !x.Chance.HasValue).ToArray();
        float haveChanceTotal = haveChances.Sum(x => x.Chance!.Value);
        float noChanceTotal = 1 - haveChanceTotal;
        float fromRandom = 0;
        
        List<BonusChance> bonusChances = [];
        foreach (BonusChanceDef def in haveChances)
        {
            BonusChance bc = new(def.Name, def.Factory, fromRandom, def.Chance!.Value);
            bonusChances.Add(bc);
            fromRandom = bc.ToRandom;
        }
        foreach (BonusChanceDef def in noChances)
        {
            BonusChance bc = new(def.Name, def.Factory, fromRandom, noChanceTotal / noChances.Length);
            bonusChances.Add(bc);
            fromRandom = bc.ToRandom;
        }

        return (Vector2 position) =>
        {
            float random = System.Random.Shared.NextSingle();
            BonusChance? chosen = bonusChances.First(x => x.FromRandom <= random && random < x.ToRandom);
            return chosen.Factory(position);
        };
    }

    public static BonusFactory CreateRandom { get; } = CreateRandomFactory();

    public PickableBonusDto ToDto()
    {
        return new PickableBonusDto
        {
            Name = Name,
            Position = [Position.X, Position.Y]
        };
    }

    private record BonusChanceDef(string Name, BonusFactory Factory, float? Chance = null);
    private record BonusChance(string Name, BonusFactory Factory, float FromRandom, float Chance)
    {
        public float ToRandom => FromRandom + Chance;
    }
}

public delegate Bonus BonusFactory(Vector2 position);

public delegate void BonusApplier(Player player);
