using SpinBladeArena.Primitives;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpinBladeArena.LogicCenter;

public class Bonus(string name, Vector2 position)
{
    public string Name { get; init; } = name;
    public Vector2 Position { get; init; } = position;

    public const int FrontendRadius = 20;

    public required BonusApplier Apply { get; init; }

    public Circle ToFrontendCircle() => new(Position, FrontendRadius);

    
    public static void NoBladeBonus(Player player)
    {
        if (player.Weapon.Count == 0)
        {
            player.Weapon.AddBlade(1);
        }
    }

    public static Bonus Health(Vector2 position, float healthAmount = 2) => new(BonusNames.Health, position)
    {
        Apply = (Player player) =>
        {
            player.Health += healthAmount;
            if (player.IsLarge)
            {
                player.Size -= 5;
            }
            NoBladeBonus(player);
        }
    };

    public static Bonus Thin(Vector2 position) => new(BonusNames.Thin, position)
    {
        Apply = (Player player) => player.Size = Math.Clamp(MathF.Round(player.Size / 2), 20, 200)
    };

    public static Bonus Speed(Vector2 position, float speedAmount = 5) => new(BonusNames.Speed, position)
    {
        Apply = (Player player) =>
        {
            player.MovementSpeedPerSecond = AbsAdd(player.MovementSpeedPerSecond, speedAmount);
            if (player.IsStrong)
            {
                player.Size = Math.Clamp(player.Size + 5, 20, 200);
            }
            NoBladeBonus(player);
        }
    };

    public static Bonus Speed20(Vector2 position, float speedAmount = 20) => new(BonusNames.Speed20, position)
    {
        Apply = (Player player) =>
        {
            player.MovementSpeedPerSecond = AbsAdd(player.MovementSpeedPerSecond, speedAmount);
            if (player.IsStrong)
            {
                player.Size = Math.Clamp(player.Size + 20, 20, 200);
            }
            NoBladeBonus(player);
        }
    };

    
    static float AbsAdd(float val, float addValue)
    {
        bool isPositive = val > 0;
        return isPositive ? val + addValue : val - addValue;
    }

    public static Bonus BladeCount(Vector2 position, int bladeCountAmount = 1) => new(BonusNames.BladeCount, position)
    {
        Apply = (Player player) =>
        {
            NoBladeBonus(player);
            player.Weapon.AddBlade(bladeCountAmount);
            if (player.IsStrong)
            {
                player.Size = Math.Clamp(player.Size + 10, 20, 200);
                player.MovementSpeedPerSecond = Math.Clamp(player.MovementSpeedPerSecond - 10, 10, 100);
                player.Weapon.RotationDegreePerSecond = Math.Clamp(player.Weapon.RotationDegreePerSecond - 2, 1, 10);
            }
        }
    };

    public static Bonus BladeCount3(Vector2 position, int bladeCountAmount = 3) => new(BonusNames.BladeCount3, position)
    {
        Apply = (Player player) =>
        {
            NoBladeBonus(player);
            player.Weapon.AddBlade(bladeCountAmount);
            if (player.IsStrong)
            {
                player.Size = Math.Clamp(player.Size + 30, 20, 200);
                player.MovementSpeedPerSecond = Math.Clamp(player.MovementSpeedPerSecond - 20, 10, 100);
                player.Weapon.RotationDegreePerSecond = Math.Clamp(player.Weapon.RotationDegreePerSecond - 5, 1, 10);
            }
        }
    };

    public static Bonus BladeLength(Vector2 position, float bladeLengthAmount = 5) => new(BonusNames.BladeLength, position)
    {
        Apply = (Player player) =>
        {
            NoBladeBonus(player);
            player.Weapon.AddLength(bladeLengthAmount);
        }
    };

    public static Bonus BladeLength20(Vector2 position, float bladeLengthAmount = 20) => new(BonusNames.BladeLength20, position)
    {
        Apply = (Player player) =>
        {
            NoBladeBonus(player);
            player.Weapon.AddLength(bladeLengthAmount);
            if (player.IsStrong)
            {
                player.Size = Math.Clamp(player.Size + 30, 20, 200);
                player.MovementSpeedPerSecond = Math.Clamp(player.MovementSpeedPerSecond - 20, 10, 100);
                player.Weapon.RotationDegreePerSecond = Math.Clamp(player.Weapon.RotationDegreePerSecond - 5, 1, 10);
            }
        }
    };

    public static Bonus BladeDamage(Vector2 position, float bladeDamageAmount = 1) => new(BonusNames.BladeDamage, position)
    {
        Apply = (Player player) =>
        {
            NoBladeBonus(player);
            player.Weapon.AddDamage(bladeDamageAmount);
        }
    };

    public static Bonus BladeSpeed(Vector2 position, float rotationDegreePerSecond = 5) => new(BonusNames.BladeSpeed, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.RotationDegreePerSecond += rotationDegreePerSecond;
            if (player.IsStrong)
            {
                player.Size = Math.Clamp(player.Size + 10, 20, 200);
                player.MovementSpeedPerSecond = Math.Clamp(player.MovementSpeedPerSecond - 10, 10, 100);
            }
            NoBladeBonus(player);
        }
    };

    public static Bonus BladeSpeed20(Vector2 position, float rotationDegreePerSecond = 20) => new(BonusNames.BladeSpeed20, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.RotationDegreePerSecond += rotationDegreePerSecond;
            if (player.IsStrong)
            {
                player.Size = Math.Clamp(player.Size + 30, 20, 200);
                player.MovementSpeedPerSecond = Math.Clamp(player.MovementSpeedPerSecond - 20, 10, 100);
            }
            NoBladeBonus(player);
        }
    };

    public static Bonus Random(Vector2 position) => new(BonusNames.Random, position)
    {
        Apply = (Player player) => All[System.Random.Shared.Next(All.Length)](position).Apply(player)
    };

    internal static Func<Vector2, Bonus>[] All =
    [
        p => Health(p),
        p => Speed(p),
        p => Speed20(p),
        p => BladeCount(p),
        p => BladeLength(p),
        p => BladeLength20(p),
        p => BladeDamage(p),
        p => BladeSpeed(p),
        p => BladeSpeed20(p),
        p => Thin(p),
        p => BladeCount3(p),
        Random,
    ];

    public static Bonus CreateRandom(Vector2 position) => All[System.Random.Shared.Next(All.Length)](position);

    public PickableBonusDto ToDto()
    {
        return new PickableBonusDto
        {
            Name = Name,
            Position = [Position.X, Position.Y]
        };
    }
}

public delegate void BonusApplier(Player player);
