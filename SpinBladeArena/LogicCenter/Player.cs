using System.Drawing;
using System.Numerics;
using SpinBladeArena.LogicCenter.Push;
using SpinBladeArena.Primitives;

namespace SpinBladeArena.LogicCenter;

public class Player(int userId, string userName, Vector2 position)
{
    public int UserId { get; } = userId;
    public string UserName { get; } = userName;
    public Vector2 Position = position;
    public float Health = 10;
    public float Size => MinSize + Health;
    public const float DefaultSize = 30;
    public const float MinSize = 20;
    public Vector2 Destination = position;

    public const float DefaultMovementSpeedPerSecond = 90;
    public const float MaxMovementSpeedPerSecond = 120;

    public float MovementSpeedPerSecond
    {
        get
        {
            float multiplier = Weapon.Count == 0 ? 2 : 1;
            return GetSuggestedMovementSpeedByPlayerSize(Size) * multiplier;
        }
    }

    private static float GetSuggestedMovementSpeedByPlayerSize(float size)
    {
        if (size < 50)
        {
            // 20~50 -> 100~60
            return 100 - (size - 20) * (40.0f / 30);
        }
        if (size < 120)
        {
            // 50~120 -> 60~30
            return 60 - (size - 50) * (30.0f / 70);
        }
        else
        {
            return 25;
        }
    }

    public PlayerWeapon Weapon = PlayerWeapon.Default;
    public double DeadTime = 0;
    public List<string> Connections { get; } = [];

    public int Score = 1;

    public bool Dead => Health <= 0;

    public float SafeDistance => Size + Weapon.LongestBladeLength;

    public Vector2 Direction => Vector2.Normalize(Destination - Position);

    // 刀数量不能超过半径除以8（向上取整）
    public int MaxBladeCount => (int)Math.Ceiling(Size / 8);

    public Circle ToCircle() => new(Position, Size);
    public Circle ToSafeDistanceCircle() => new(Position, SafeDistance);

    public virtual AddPlayerRequest CreateRespawnRequest() => new(UserId, UserName);

    public LineSegment GetBladeLineSegment(Blade blade)
    {
        float degree = blade.RotationDegree;
        float angle = MathF.PI * degree / 180;
        Vector2 bladeStart = Position + new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * Size;
        Vector2 bladeEnd = Position + new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * (Size + blade.Length);
        return new(bladeStart, bladeEnd);
    }

    public void Move(float deltaTime, RectangleF bound)
    {
        if (Dead) return;

        float maxDistance = MovementSpeedPerSecond * deltaTime;
        float distance = Vector2.Distance(Position, Destination);

        if (distance < maxDistance)
        {
            Position = Destination;
        }
        else
        {
            Position += Direction * maxDistance;
        }

        Position = new(
            Math.Clamp(Position.X, bound.Left, bound.Right),
            Math.Clamp(Position.Y, bound.Top, bound.Bottom));

        Weapon.RotateBlades(deltaTime);
    }

    public void BalanceCheck()
    {
        // 刀伤不能超过半径除以15，默认半径30，最多2伤，减肥时会掉刀伤
        for (int i = 0; i < Weapon.Count; i++)
        {
            if (Weapon[i].Damage > Size / 15)
            {
                Weapon[i].Damage = Size / 15;
            }
        }
    }

    public void BeenAttackedBalanceCheck(float damage)
    {
        // 刀数量不能超过半径除以8（向上取整），默认半径30，最多3.75->4把刀，减肥时不掉刀
        int maxBladeCount = MaxBladeCount;
        while (Weapon.Count > maxBladeCount)
        {
            Weapon.DestroyBladeAt(Weapon.Count - 1);
        }
    }

    internal bool AddBlade(int addBladeCount)
    {
        // 刀数量不能超过半径除以8（向上取值），默认半径30，最多3.75->4把刀，减肥时不掉刀
        int maxBladeCount = MaxBladeCount;
        int toAdd = addBladeCount + Weapon.Count > maxBladeCount ? maxBladeCount - Weapon.Count : addBladeCount;

        for (int i = 0; i < toAdd; ++i)
        {
            Weapon.Add(new());
        }

        Weapon.RearrangeBlades();

        return toAdd > 0;
    }

    public static PlayerHitInfo[] AttackEachOther(Player p1, Player p2)
    {
        if (p1.Dead || p2.Dead) return [];
        if (Vector2.Distance(p1.Position, p2.Position) > p1.SafeDistance + p2.SafeDistance) return [];

        PlayerHitInfo p1a2 = P1AttackP2(p1, p2);
        PlayerHitInfo p2a1 = P1AttackP2(p2, p1);
        BladeAttack(p1, p2);

        return [p1a2,  p2a1];

        static PlayerHitInfo P1AttackP2(Player p1, Player p2)
        {
            for (int i = 0; i < p1.Weapon.Count; i++)
            {
                Blade blade = p1.Weapon[i];
                LineSegment bladeLine = p1.GetBladeLineSegment(blade);

                if (bladeLine.IsIntersectingCircle(p2.ToCircle()))
                {
                    p2.Health -= blade.Damage;
                    p1.Weapon.ReverseRotationDirection();
                    return new PlayerHitInfo(p1, p2, blade.Damage);
                }
            }

            return new PlayerHitInfo(p1, p2, 0);
        }

        static void BladeAttack(Player p1, Player p2)
        {
            for (int p1i = 0; p1i < p1.Weapon.Count; p1i++)
            {
                Blade p1b = p1.Weapon[p1i];
                LineSegment bladeLine1 = p1.GetBladeLineSegment(p1b);

                for (int p2i = 0; p2i < p2.Weapon.Count; p2i++)
                {
                    Blade p2b = p2.Weapon[p2i];
                    LineSegment bladeLine2 = p2.GetBladeLineSegment(p2b);

                    if (LineSegment.IsIntersection(bladeLine1, bladeLine2))
                    {
                        // 平衡性设计：如果对方没有金刀，自己有金刀，对方的刀直接销毁，自己的金刀不消耗
                        if (p1.Weapon.IsGoldBlade(p1b) && !p2.Weapon.IsGoldBlade(p2b))
                        {
                            p2.Weapon.DestroyBladeAt(p2i--);
                        }
                        else if (!p1.Weapon.IsGoldBlade(p1b) && p2.Weapon.IsGoldBlade(p2b))
                        {
                            p1.Weapon.DestroyBladeAt(p1i--);
                        }
                        // 其它情况，伤害高的刀消耗低的刀，伤害高的刀只减少1伤害
                        else if (p1b.Damage > p2b.Damage)
                        {
                            p2.Weapon.DestroyBladeAt(p2i--);
                            p1b.Damage -= 1;
                            if (p1b.Damage == 0)
                            {
                                p1.Weapon.DestroyBladeAt(p1i--);
                            }
                        }
                        else if (p1b.Damage < p2b.Damage)
                        {
                            p1.Weapon.DestroyBladeAt(p1i--);
                            p2b.Damage -= 1;
                            if (p2b.Damage == 0)
                            {
                                p2.Weapon.DestroyBladeAt(p2i--);
                            }
                        }
                        // 伤害相等，双方刀都销毁
                        else
                        {
                            p1.Weapon.DestroyBladeAt(p1i--);
                            p2.Weapon.DestroyBladeAt(p2i--);
                        }

                        // 刀相撞后，反向旋转
                        p1.Weapon.ReverseRotationDirection();
                        p2.Weapon.ReverseRotationDirection();
                        return;
                    }
                }
            }
        }
    }

    public bool IsDangerousToPlayer(Player player, float reactionTime)
    {
        EstimatedPlayerState estimatedPlayerState = EstimateMove(reactionTime);
        Circle playerCircle = player.ToCircle();
        foreach (Blade blade in estimatedPlayerState.Blades)
        {
            LineSegment ls = GetBladeLineSegment(blade);

            if (ls.IsIntersectingCircle(playerCircle))
            {
                return true;
            }
        }

        return false;
    }

    public PlayerDto ToDto()
    {
        return new PlayerDto
        {
            UserId = UserId,
            UserName = UserName,
            Position = [Position.X, Position.Y],
            Destination = [Destination.X, Destination.Y],
            Health = Health,
            Blades = Weapon.ToDto(),
            Score = Score
        };
    }

    public EstimatedPlayerState EstimateMove(float deltaTime)
    {
        float maxDistance = MovementSpeedPerSecond * deltaTime;
        float distance = Vector2.Distance(Position, Destination);

        Vector2 position;
        if (distance < maxDistance)
        {
            position = Destination;
        }
        else
        {
            position = Position + Direction * maxDistance;
        }

        Blade[] blades = Weapon.EstimateRotateBlades(deltaTime);
        return new EstimatedPlayerState(position, blades);
    }
}

public record EstimatedPlayerState(Vector2 Position, Blade[] Blades);