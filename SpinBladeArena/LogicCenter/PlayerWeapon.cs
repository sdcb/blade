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

        float initialAngle = Count == 0 ? 0 : this[0].RotationDegree;
        for (int i = 0; i < Count; ++i)
        {
            this[i].RotationDegree = initialAngle + 360 / Count * i;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddDamage(float damage)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].Damage += damage;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGoldBlade(Blade blade)
    {
        return blade.Damage >= 2 && Count <= 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BladeDto[] ToDto() => this.Select(x => x.ToDto()).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RotateBlades(float deltaTime)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].RotationDegree = MathF.IEEERemainder(this[i].RotationDegree + RotationDegreePerSecond * deltaTime, 360) switch
            {
                var x when x < 0 => x + 360,
                var x => x,
            };
        }
    }

    public Blade[] EstimateRotateBlades(float deltaTime)
    {
        Blade[] result = new Blade[Count];
        for (int i = 0; i < Count; ++i)
        {
            result[i] = new Blade(MathF.IEEERemainder(this[i].RotationDegree + RotationDegreePerSecond * deltaTime, 360) switch
            {
                var x when x < 0 => x + 360,
                var x => x,
            }, this[i].Damage, this[i].Length);
        }

        return result;
    }
}

public class Blade(float rotationDegree = 0, float damage = 1, float length = 40)
{
    public float RotationDegree = rotationDegree;
    public float Damage = damage;
    public float Length = length;

    public BladeDto ToDto() => new()
    {
        Angle = RotationDegree,
        Damage = Damage,
        Length = Length,
    };
}