using System.Runtime.CompilerServices;

namespace SpinBladeArena.LogicCenter;

public class PlayerWeapon : List<Blade>
{
    public float RotationDegreePerSecond = 10;

    public static PlayerWeapon Default => [new()];

    public void DestroyBladeAt(int bladeIndex)
    {
        if (Count == 0) return;
        RemoveAt(bladeIndex);
    }

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

    public void AddLength(float length, float size)
    {
        // 平衡性调整：刀片长度不超过玩家大小的5倍
        float maxLength = size * 5;
        for (int i = 0; i < Count; ++i)
        {
            this[i].Length = Math.Min(this[i].Length + length, maxLength);
        }
    }

    public void AddDamage(float damage)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].Damage += damage;
        }
    }

    public bool IsGoldBlade(Blade blade)
    {
        return blade.Damage >= 2 && Count <= 2;
    }

    public BladeDto[] ToDto() => this.Select(x => x.ToDto()).ToArray();

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

    public float LongestBladeLength
    {
        get
        {
            float longest = 0;
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].Length > longest)
                {
                    longest = this[i].Length;
                }
            }
            return longest;
        }
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