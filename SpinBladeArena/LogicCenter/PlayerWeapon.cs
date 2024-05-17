using System.Runtime.CompilerServices;

namespace SpinBladeArena.LogicCenter;

public class PlayerWeapon : List<Blade>
{
    public float RotationDegreePerSecond = 10;

    public static PlayerWeapon Default => [new()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyBladeAt(int bladeIndex)
    {
        if (Count == 0) return;
        RemoveAt(bladeIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddBlade(int addBladeCount)
    {
        for (int i = 0; i < addBladeCount; ++i)
        {
            Add(new());
        }

        float initialAngle = Count == 0 ? 0 : this[0].Angle;
        for (int i = 0; i < Count; ++i)
        {
            this[i].Angle = initialAngle + 360 / Count * i;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLength(float length)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].Length += length;
        }
    }

    public void AddDamage(float damage)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].Damage += damage;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Blade[] ToDto() => ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RotateBlades(float deltaTime)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].Angle = MathF.IEEERemainder(this[i].Angle + RotationDegreePerSecond * deltaTime, 360) switch
            {
                var x when x < 0 => x + 360,
                var x => x,
            };
        }
    }
}

public class Blade(float angle = 0, float damage = 1, float length = 40)
{
    public float Angle = angle;
    public float Damage = damage;
    public float Length = length;
}