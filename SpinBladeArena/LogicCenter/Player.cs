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

    public bool IsStrong => Weapon.Count > 5;

    public bool Dead => Health <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LineSegment GetBladeLineSegment(Blade blade)
    {
        float degree = blade.Angle;
        float angle = MathF.PI * degree / 180;
        Vector2 bladeStart = Position + new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * Size;
        Vector2 bladeEnd = Position + new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * (Size + blade.Length);
        return new(bladeStart, bladeEnd);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move(float deltaTime)
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
            Vector2 Direction = Vector2.Normalize(Destination - Position);
            Position += Direction * MovementSpeedPerSecond * deltaTime;
        }

        Weapon.RotateBlades(deltaTime);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AttackEachOther(Player p1, Player p2)
    {
        if (p1.Dead || p2.Dead) return;

        P1AttackP2(p1, p2);
        P1AttackP2(p2, p1);
        BladeAttack(p1, p2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void P1AttackP2(Player p1, Player p2)
        {
            for (int i = 0; i < p1.Weapon.Count; i++)
            {
                Blade blade = p1.Weapon[i];
                LineSegment ls = p1.GetBladeLineSegment(blade);

                if (PrimitiveUtils.IsLineIntersectingCircle(ls, p2.Position, p2.Size))
                {
                    p2.Health -= blade.Damage;
                    if (p2.Health <= 0)
                    {
                        p1.Score += p2.Score / 2;
                        for (int n = 0; n < p2.Score / 2; ++n)
                        {
                            PickableBonus.Random(Vector2.Zero).Apply(p1);
                        }
                    }
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                        // 平衡性设计：伤害高的刀削伤害低的刀，伤害高的刀只减少1点伤害，伤害低的刀直接销毁
                        if (p1b.Damage > p2b.Damage)
                        {
                            p2.Weapon.DestroyBladeAt(p2i);
                            p1b.Damage -= 1;
                            if (p1b.Damage == 0)
                            {
                                p1.Weapon.DestroyBladeAt(p1i);
                                --p1i;
                            }
                        }
                        else if (p1b.Damage < p2b.Damage)
                        {
                            p1.Weapon.DestroyBladeAt(p1i);
                            p2b.Damage -= 1;
                            if (p2b.Damage == 0)
                            {
                                p2.Weapon.DestroyBladeAt(p2i);
                                --p2i;
                            }
                        }
                        else
                        {
                            p1.Weapon.DestroyBladeAt(p1i);
                            p2.Weapon.DestroyBladeAt(p2i);
                            --p1i;
                            --p2i;
                        }
                        // Reverse rotation direction
                        p1.Weapon.RotationDegreePerSecond = -p1.Weapon.RotationDegreePerSecond;
                        p1.Weapon.RotationDegreePerSecond = -p1.Weapon.RotationDegreePerSecond;
                        return;
                    }
                }
            }
        }
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
}
