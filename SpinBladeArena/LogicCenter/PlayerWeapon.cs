using SpinBladeArena.LogicCenter.Push;
using SpinBladeArena.Primitives;
using System.Drawing;

namespace SpinBladeArena.LogicCenter;

public class PlayerWeapon : List<Blade>
{
    public const float DefaultRotationDegreePerSecond = 10;

    public float RotationDegreePerSecond { get; set; } = DefaultRotationDegreePerSecond;

    public void AddRotationDegreePerSecond(float amountInDegree)
    {
        // 刀速不能超过玩家半径的1.5倍（但不掉速度）
        // 起始10度每秒，半径30，最大45度每秒
        float max = 1.5f * 30 / 10 * DefaultRotationDegreePerSecond;
        if (Math.Abs(RotationDegreePerSecond) <= max)
        {
            RotationDegreePerSecond = MathUtils.AbsAdd(RotationDegreePerSecond, amountInDegree);
        }
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

    internal void AddBlade(int addBladeCount, float playerSize)
    {
        // 刀数量不能超过半径除以7，默认半径30，最多4把刀，减肥时不掉刀
        int maxBladeCount = Math.Clamp((int)playerSize / 7, 1, 100);
        int toAdd = Math.Clamp(addBladeCount, 0, maxBladeCount - Count);

        for (int i = 0; i < toAdd; ++i)
        {
            Add(new());
        }

        RearrangeBlades();
    }

    private void RearrangeBlades()
    {
        float initialAngle = Count == 0 ? 0 : this[0].RotationDegree;
        for (int i = 0; i < Count; ++i)
        {
            this[i].RotationDegree = initialAngle + 360.0f / Count * i;
        }
    }

    public void AddLength(float length, float playerSize)
    {
        // 平衡性：刀长不能超过玩家半径的3倍（但不掉长度）
        // 例如：默认刀长30，玩家半径30，最大刀长90
        float maxLength = playerSize * 3;
        for (int i = 0; i < Count; ++i)
        {
            float result = this[i].Length + length;
            if (result < maxLength)
            {
                this[i].Length = result;
            }
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