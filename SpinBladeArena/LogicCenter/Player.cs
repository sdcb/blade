using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using SpinBladeArena.Primitives;

namespace SpinBladeArena.LogicCenter;

public class Player(int userId, string userName, Vector2 position)
{
    public int UserId { get; } = userId;
    public string UserName { get; } = userName;
    public Vector2 Position = position;
    public float Health = 1;
    public float Size = 30;
    public Vector2 Destination = position;
    public float MovementSpeedPerSecond = 75;
    public PlayerWeapon Weapon = PlayerWeapon.Default;
    public double DeadTime = 0;
    public int Score = 1;
    public bool IsLarge => Size > 75;

    public bool IsStrong => Weapon.Count > 7;

    public bool Dead => Health <= 0;

    public float SafeDistance => Size + Weapon.LongestBladeLength;

    public Vector2 Direction => Vector2.Normalize(Destination - Position);

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

    public static KillingInfo AttackEachOther(Player p1, Player p2)
    {
        if (p1.Dead || p2.Dead) return new KillingInfo(p1, p2, false, false);
        if (Vector2.Distance(p1.Position, p2.Position) > p1.SafeDistance + p2.SafeDistance) return new KillingInfo(p1, p2, false, false);

        bool player2Dead = P1AttackP2(p1, p2);
        bool player1Dead = P1AttackP2(p2, p1);
        BladeAttack(p1, p2);

        return new(p1, p2, player1Dead, player2Dead);

        static bool P1AttackP2(Player p1, Player p2)
        {
            for (int i = 0; i < p1.Weapon.Count; i++)
            {
                Blade blade = p1.Weapon[i];
                LineSegment ls = p1.GetBladeLineSegment(blade);

                if (PrimitiveUtils.IsLineIntersectingCircle(ls, p2.Position, p2.Size))
                {
                    p2.Health -= blade.Damage;
                    return p2.Health <= 0;
                }
            }

            return false;
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

                    if (PrimitiveUtils.IsLineSegmentIntersection(bladeLine1, bladeLine2))
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
                        p1.Weapon.RotationDegreePerSecond = -p1.Weapon.RotationDegreePerSecond;
                        p1.Weapon.RotationDegreePerSecond = -p1.Weapon.RotationDegreePerSecond;
                        return;
                    }
                }
            }
        }
    }

    public bool IsDangerousToPlayer(Player player, float reactionTime)
    {
        EstimatedPlayerState estimatedPlayerState = EstimateMove(reactionTime);
        foreach (Blade blade in estimatedPlayerState.Blades)
        {
            LineSegment ls = GetBladeLineSegment(blade);

            if (PrimitiveUtils.IsLineIntersectingCircle(ls, player.Position, player.Size))
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
            Size = Size,
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