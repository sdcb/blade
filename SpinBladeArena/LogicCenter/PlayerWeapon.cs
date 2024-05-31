using SpinBladeArena.LogicCenter.Push;
using SpinBladeArena.Primitives;

namespace SpinBladeArena.LogicCenter;

public class PlayerWeapon : List<Blade>
{
    public const float DefaultRotationDegreePerSecond = 10;

    public float RotationDegreePerSecond { get; set; } = DefaultRotationDegreePerSecond;

    public float AbsRotationDegreePerSecond => Math.Abs(RotationDegreePerSecond);

    public bool AddRotationSpeed(float amountInDegree, float playerSize)
    {
        // 刀速不能超过60（初始值10）
        float maxSpeed = 60;
        RotationDegreePerSecond = MathUtils.AbsAdd(RotationDegreePerSecond, amountInDegree);
        return LimitRotationDegreePerSecond(maxSpeed);
    }

    public void ReverseRotationDirection()
    {
        RotationDegreePerSecond = -RotationDegreePerSecond;
    }

    private bool LimitRotationDegreePerSecond(float absSpeed)
    {
        if (AbsRotationDegreePerSecond > absSpeed)
        {
            RotationDegreePerSecond = Math.Sign(RotationDegreePerSecond) * absSpeed;
            return true;
        }
        return false;
    }

    public float WeaponScore => this.Sum(x => x.Score) * RotationDegreePerSecond / DefaultRotationDegreePerSecond;

    public static PlayerWeapon Default => [new(Random.Shared.NextSingle() * 360)];

    public void DestroyBladeAt(int bladeIndex)
    {
        if (Count == 0) return;
        RemoveAt(bladeIndex);
    }

    public void RearrangeBlades()
    {
        float initialAngle = Count == 0 ? 0 : this[0].RotationDegree;
        for (int i = 0; i < Count; ++i)
        {
            this[i].RotationDegree = initialAngle + 360.0f / Count * i;
        }
    }

    public bool AddLength(float length, float playerSize)
    {
        // 在玩家半径为20时，刀长倍率为6，半径为200时，刀长倍率为3，非线性递减
        float bladeLengthToPlayerSize = 4.5f * MathF.Exp(-0.02f * playerSize) + 2.9375f;
        float maxBladeLength = playerSize * bladeLengthToPlayerSize;
        bool everApplied = false;
        for (int i = 0; i < Count; ++i)
        {
            float bladeLengthTarget = this[i].Length + length;
            if (bladeLengthTarget < maxBladeLength)
            {
                everApplied = true;
                this[i].Length = bladeLengthTarget;
            }
            else if (this[i].Length < maxBladeLength)
            {
                everApplied = true;
                this[i].Length = maxBladeLength;
            }
        }
        return everApplied;
    }

    public bool AddDamage(float damage, float playerSize)
    {
        // 刀伤不能超过半径除以12，默认半径30，最多2.5伤，减肥时会掉刀伤
        float maxDamage = playerSize / 12;
        bool everApplied = false;
        for (int i = 0; i < Count; ++i)
        {
            float destDamage = this[i].Damage + damage;
            if (destDamage < maxDamage)
            {
                everApplied = true;
                this[i].Damage = destDamage;
            }
            else if (this[i].Damage < maxDamage)
            {
                everApplied = true;
                this[i].Damage = maxDamage;
            }
        }
        return everApplied;
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