using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpinBladeArena.LogicCenter;

public record struct PickableBonus(string Name, Vector2 Position)
{
    public PickableBonusApplier Apply;

    public static PickableBonus Health(Vector2 position, float healthAmount = 1) => new()
    {
        Name = "生命",
        Position = position,
        Apply = (ref Player player) => player.Health += healthAmount
    };

    public static PickableBonus Speed(Vector2 position, float speedAmount = 2) => new()
    {
        Name = "移动速度",
        Position = position,
        Apply = (ref Player player) => AbsAdd(player.MovementSpeedPerSecond, speedAmount)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float AbsAdd(float val, float addValue)
    {
        bool isPositive = val > 0;
        return isPositive ? val + addValue : val - addValue;
    }

    public static PickableBonus BladeCount(Vector2 position, int bladeCountAmount = 1) => new()
    {
        Name = "刀数",
        Position = position,
        Apply = (ref Player player) => player.Blades.AddBlade(bladeCountAmount)
    };

    public static PickableBonus BladeLength(Vector2 position, float bladeLengthAmount = 1) => new()
    {
        Name = "刀长",
        Position = position,
        Apply = (ref Player player) => player.Blades.Length += bladeLengthAmount
    };

    public static PickableBonus BladeDamage(Vector2 position, float bladeDamageAmount = 1) => new()
    {
        Name = "刀伤",
        Position = position,
        Apply = (ref Player player) => player.Blades.Damage += bladeDamageAmount
    };

    public static PickableBonus BladeSpeed(Vector2 position, float rotationDegreePerSecond = 2) => new()
    {
        Name = "刀速",
        Position = position,
        Apply = (ref Player player) => player.Blades.RotationDegreePerSecond += rotationDegreePerSecond
    };

    public static PickableBonus Random(Vector2 position) => new()
    {
        Name = "随机",
        Position = position,
        Apply = (ref Player player) => All[System.Random.Shared.Next(All.Length)](position).Apply(ref player)
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
}

public delegate void PickableBonusApplier(ref Player player);