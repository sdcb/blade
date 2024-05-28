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
            // 设计：减少血量的同时，增加减少血量一半的速度
            float diff = oldHealth - player.Health;
            player.AddMovementSpeed(diff / 2);
        }
    };

    public static Bonus Speed(Vector2 position, float speedAmount = 5) => new(BonusNames.Speed, position)
    {
        Apply = (Player player) =>
        {
            player.AddMovementSpeed(speedAmount);
            player.Health = Math.Clamp(player.Health - 1, 1, 200);
            player.Weapon.AddRotationDegreePerSecond(-2);
        }
    };

    public static Bonus Speed20(Vector2 position, float speedAmount = 20) => new(BonusNames.Speed20, position)
    {
        Apply = (Player player) =>
        {
            player.AddMovementSpeed(speedAmount);
            player.Health = Math.Clamp(player.Health - 2, 1, 200);
            player.Weapon.AddRotationDegreePerSecond(-5);
        }
    };

    public static Bonus BladeCount(Vector2 position, int bladeCountAmount = 1) => new(BonusNames.BladeCount, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddBlade(bladeCountAmount);
            player.Health += 1;
            player.Weapon.AddRotationDegreePerSecond(-1);
            player.AddMovementSpeed(-1);
        }
    };

    public static Bonus BladeCount3(Vector2 position, int bladeCountAmount = 3) => new(BonusNames.BladeCount3, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddBlade(bladeCountAmount);
            player.Health += 3;
            player.Weapon.AddRotationDegreePerSecond(-4);
            player.AddMovementSpeed(-4);
        }
    };

    public static Bonus BladeLength(Vector2 position, float bladeLengthAmount = 5) => new(BonusNames.BladeLength, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddLength(bladeLengthAmount, player.Size);
            player.Health += 1;
            player.Weapon.AddRotationDegreePerSecond(-1);
            player.AddMovementSpeed(-1);
        }
    };

    public static Bonus BladeLength20(Vector2 position, float bladeLengthAmount = 20) => new(BonusNames.BladeLength20, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddLength(bladeLengthAmount, player.Size);
            player.Health += 3;
            player.Weapon.AddRotationDegreePerSecond(-4);
            player.AddMovementSpeed(-4);
        }
    };

    public static Bonus BladeDamage(Vector2 position, float bladeDamageAmount = 1) => new(BonusNames.BladeDamage, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddDamage(bladeDamageAmount);
            player.Health += 1;
            player.Weapon.AddRotationDegreePerSecond(-1);
            player.AddMovementSpeed(-1);
        }
    };

    public static Bonus BladeSpeed(Vector2 position, float rotationDegreePerSecond = 5) => new(BonusNames.BladeSpeed, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddRotationDegreePerSecond(rotationDegreePerSecond);
            player.Health += 1;
            player.AddMovementSpeed(-1);
        }
    };

    public static Bonus BladeSpeed20(Vector2 position, float rotationDegreePerSecond = 20) => new(BonusNames.BladeSpeed20, position)
    {
        Apply = (Player player) =>
        {
            player.Weapon.AddRotationDegreePerSecond(rotationDegreePerSecond);
            player.Health += 3;
            player.AddMovementSpeed(-5);
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
        //Random,
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
