using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpinBladeArena.LogicCenter;

public class PickableBonus(string name, Vector2 position)
{
    public string Name { get; init; } = name;
    public Vector2 Position { get; init; } = position;

    public required PickableBonusApplier Apply { get; init; }

    public static PickableBonus Health(Vector2 position, float healthAmount = 1) => new("生命", position)
    {
        Apply = (Player player) => player.Health += healthAmount
    };

    public static PickableBonus Speed(Vector2 position, float speedAmount = 2) => new("移动速度", position)
    {
        Apply = (Player player) => AbsAdd(player.MovementSpeedPerSecond, speedAmount)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float AbsAdd(float val, float addValue)
    {
        bool isPositive = val > 0;
        return isPositive ? val + addValue : val - addValue;
    }

    public static PickableBonus BladeCount(Vector2 position, int bladeCountAmount = 1) => new("刀数", position)
    {
        Apply = (Player player) => player.Blades.AddBlade(bladeCountAmount)
    };

    public static PickableBonus BladeLength(Vector2 position, float bladeLengthAmount = 5) => new("刀长", position)
    {
        Apply = (Player player) => player.Blades.Length += bladeLengthAmount
    };

    public static PickableBonus BladeDamage(Vector2 position, float bladeDamageAmount = 1) => new("刀伤", position)
    {
        Apply = (Player player) => player.Blades.Damage += bladeDamageAmount
    };

    public static PickableBonus BladeSpeed(Vector2 position, float rotationDegreePerSecond = 2) => new("刀速", position)
    {
        Apply = (Player player) => player.Blades.RotationDegreePerSecond += rotationDegreePerSecond
    };

    public static PickableBonus Random(Vector2 position) => new("随机", position)
    {
        Apply = (Player player) => All[System.Random.Shared.Next(All.Length)](position).Apply(player)
    };

    internal static Func<Vector2, PickableBonus>[] All =
    [
        p => Health(p),
        p => Speed(p),
        p => BladeCount(p),
        p => BladeLength(p),
        p => BladeDamage(p),
        p => BladeSpeed(p),
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