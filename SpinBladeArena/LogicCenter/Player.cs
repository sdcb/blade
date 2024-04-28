using System.Numerics;
using System.Runtime.CompilerServices;
using SpinBladeArena.Primitives;

namespace SpinBladeArena.LogicCenter;

public class Player(int userId, string userName, string connectionId, Vector2 position)
{
    public int UserId { get; } = userId;
    public string UserName { get; } = userName;
    public string ConnectionId = connectionId;
    public Vector2 Position = position;
    public float Health = 1;
    public float Size = 50;
    public Vector2 Destination = position;
    public float MovementSpeedPerSecond = 50;
    public PlayerBlades Blades = PlayerBlades.Default;
    public double DeadTime = 0;

    public bool Dead => Health <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LineSegment GetBladeLineSegment(int bladeIndex)
    {
        float degree = Blades.Angles[bladeIndex];
        float angle = MathF.PI * degree / 180;
        Vector2 bladeStart = Position + new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * Size;
        Vector2 bladeEnd = Position + new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * (Size + Blades.Length);
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

        // Rotate blades
        for (int i = 0; i < Blades.Count; ++i)
        {
            Blades.Angles[i] = MathF.IEEERemainder(Blades.Angles[i] + Blades.RotationDegreePerSecond * deltaTime, 360) switch
            {
                var x when x < 0 => x + 360,
                var x => x,
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AttackEachOther(Player p1, Player p2)
    {
        if (p1.Dead || p2.Dead) return;

        if (Vector2.Distance(p1.Position, p2.Position) > p1.Size + p1.Blades.Length + p2.Size + p2.Blades.Length) return;

        P1AttackP2(p1, p2);
        P1AttackP2(p2, p1);
        BladeAttack(p1, p2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void P1AttackP2(Player p1, Player p2)
        {
            for (int i = 0; i < p1.Blades.Count; ++i)
            {
                LineSegment ls = p1.GetBladeLineSegment(i);

                if (PrimitiveUtils.IsLineIntersectingCircle(ls, p2.Position, p2.Size))
                {
                    p2.Health -= p1.Blades.Damage;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void BladeAttack(Player p1, Player p2)
        {
            for (int p1i = 0; p1i < p1.Blades.Count; ++p1i)
            {
                PlayerBlades p1PlayerBlades = p1.Blades;
                LineSegment bladeLine1 = p1.GetBladeLineSegment(p1i);

                for (int p2i = 0; p2i < p2.Blades.Count; ++p2i)
                {
                    ref PlayerBlades p2PlayerBlades = ref p2.Blades;
                    LineSegment bladeLine2 = p2.GetBladeLineSegment(p2i);

                    if (PrimitiveUtils.IsLineSegmentIntersection(bladeLine1, bladeLine2))
                    {
                        // Destroy blades
                        p1PlayerBlades.DestroyBladeAt(p1i);
                        p2PlayerBlades.DestroyBladeAt(p2i);
                        // Reverse rotation direction
                        p1PlayerBlades.RotationDegreePerSecond = -p1PlayerBlades.RotationDegreePerSecond;
                        p2PlayerBlades.RotationDegreePerSecond = -p2PlayerBlades.RotationDegreePerSecond;
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
            Blades = Blades.ToDto(),
            DeadTime = DeadTime
        };
    }
}

public class PlayerBlades
{
    public float RotationDegreePerSecond = 2;
    public float Length = 40;
    public float Damage = 1;
    public int Count => Angles.Count;
    public List<float> Angles = [];

    public static PlayerBlades Default => new()
    {
        Angles = [0]
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyBladeAt(int bladeIndex)
    {
        if (Count == 0) return;

        Angles.RemoveAt(bladeIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddBlade(int addBladeCount)
    {
        int bladeCount = Angles.Count + addBladeCount;
        float initialAngle = Angles.Count == 0 ? 0 : Angles[0];
        Angles = Enumerable.Range(0, bladeCount)
            .Select(i => initialAngle + 360 / bladeCount * i)
            .ToList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PlayerBladesDto ToDto()
    {
        return new PlayerBladesDto
        {
            Length = Length,
            Damage = Damage,
            Angles = [.. Angles]
        };
    }
}