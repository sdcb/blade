using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpinBladeArena.LogicCenter;

public class PickableBonus(string name, Vector2 position)
{
    public string Name { get; init; } = name;
    public Vector2 Position { get; init; } = position;

    public required PickableBonusApplier Apply { get; init; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NoBladeBonus(Player player)
    {
        if (player.Weapon.Count == 0)
        {
            player.Weapon.AddBlade(1);
        }
    }

    public static PickableBonus Health(Vector2 position, float healthAmount = 2) => new("生命", position)
    {
        Apply = (Player player) =>
        {
            player.Health += healthAmount;
            if (player.IsLarge)
            {
                player.Size = Math.Clamp(player.Size - 5, 20, 100);
            }
            NoBladeBonus(player);
        }
    };

    public static PickableBonus Thin(Vector2 position) => new("减肥", position)
    {
        Apply = (Player player) => player.Size = Math.Clamp(player.Size / 2, 20, 100)
    };

    public static PickableBonus Speed(Vector2 position, float speedAmount = 5) => new("移速+5", position)
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

    public static PickableBonus Speed20(Vector2 position, float speedAmount = 20) => new("移速+20", position)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float AbsAdd(float val, float addValue)
    {
        bool isPositive = val > 0;
        return isPositive ? val + addValue : val - addValue;
    }

    public static PickableBonus BladeCount(Vector2 position, int bladeCountAmount = 1) => new("刀数", position)
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

    public static PickableBonus BladeCount3(Vector2 position, int bladeCountAmount = 3) => new("刀数+3", position)
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

    public static PickableBonus BladeLength(Vector2 position, float bladeLengthAmount = 5) => new("刀长+5", position)
    {
        Apply = (Player player) =>
        {
            NoBladeBonus(player);
            player.Weapon.AddLength(bladeLengthAmount);
        }
    };

    public static PickableBonus BladeLength20(Vector2 position, float bladeLengthAmount = 20) => new("刀长+20", position)
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

    public static PickableBonus BladeDamage(Vector2 position, float bladeDamageAmount = 1) => new("刀伤", position)
    {
        Apply = (Player player) =>
        {
            NoBladeBonus(player);
            player.Weapon.AddDamage(bladeDamageAmount);
        }
    };

    public static PickableBonus BladeSpeed(Vector2 position, float rotationDegreePerSecond = 5) => new("刀速+5", position)
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

    public static PickableBonus BladeSpeed20(Vector2 position, float rotationDegreePerSecond = 20) => new("刀速+20", position)
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

    public static PickableBonus Random(Vector2 position) => new("随机", position)
    {
        Apply = (Player player) => All[System.Random.Shared.Next(All.Length)](position).Apply(player)
    };

    internal static Func<Vector2, PickableBonus>[] All =
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

    public static PickableBonus CreateRandom(Vector2 position) => All[System.Random.Shared.Next(All.Length)](position);

    public PickableBonusDto ToDto()
    {
        return new PickableBonusDto
        {
            Name = Name,
            Position = [Position.X, Position.Y]
        };
    }
}

public delegate void PickableBonusApplier(Player player);