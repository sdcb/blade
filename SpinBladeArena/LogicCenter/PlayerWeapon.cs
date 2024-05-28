using SpinBladeArena.LogicCenter.Push;
using SpinBladeArena.Primitives;

namespace SpinBladeArena.LogicCenter;

public class PlayerWeapon : List<Blade>
{
    public const float DefaultRotationDegreePerSecond = 10;

    public float RotationDegreePerSecond { get; set; } = DefaultRotationDegreePerSecond;

    public void AddRotationDegreePerSecond(float amountInDegree)
    {
        RotationDegreePerSecond = MathUtils.AbsAdd(RotationDegreePerSecond, amountInDegree);
    }

    public void ReverseRotationDirection()
    {
        RotationDegreePerSecond = -RotationDegreePerSecond;
    }

    public void LimitRotationDegreePerSecond(float val)
    {
        if (Math.Abs(RotationDegreePerSecond) > val)
        {
            RotationDegreePerSecond = Math.Sign(RotationDegreePerSecond) * val;
        }
    }

    public float WeaponScore => this.Sum(x => x.Score) * RotationDegreePerSecond / DefaultRotationDegreePerSecond;

    public static PlayerWeapon Default => [new(Random.Shared.NextSingle() * 360)];

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

    public void AddLength(float length)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].Length = this[i].Length + length;
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

public class Blade(float rotationDegree = 0, float damage = 1, float length = Blade.DefaultLength)
{
    public const float DefaultLength = 30;

    public float RotationDegree = rotationDegree;
    public float Damage = damage;
    public float Length = length;

    public float Score => Damage / 2f + Length / 80f;

    public BladeDto ToDto() => new()
    {
        Angle = RotationDegree,
        Damage = Damage,
        Length = Length,
    };
}