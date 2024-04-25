using System.Numerics;
using System.Runtime.CompilerServices;
using SpinBladeArena.Primitives;

namespace SpinBladeArena.LogicCenter;

public record struct Player(int UserId, string UserName, string ConnectionId, Vector2 Position)
{
    public float Health = 1;
    public float Size = 50;
    public Vector2 Destination = Position;
    public float MovementSpeedPerSecond = 10;
    public PlayerBlades Blades = PlayerBlades.Default;
    public double DeadTime = 0;

    public readonly bool Dead => Health <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LineSegment GetBladeLineSegment(int bladeIndex)
    {
        ref PlayerBladeInfo blade = ref Blades.Infos[bladeIndex];
        Vector2 bladeStart = Position + new Vector2(MathF.Sin(blade.RotationAngle), -MathF.Cos(blade.RotationAngle)) * Size;
        Vector2 bladeEnd = Position + new Vector2(MathF.Sin(blade.RotationAngle), -MathF.Cos(blade.RotationAngle)) * (Size + Blades.Length);
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Attack(ref Player p1, ref Player p2)
    {
        if (p1.Dead || p2.Dead) return;

        if (Vector2.Distance(p1.Position, p2.Position) > p1.Size + p1.Blades.Length + p2.Size + p2.Blades.Length) return;

        P1AttachP2(ref p1, ref p2);
        P1AttachP2(ref p2, ref p1);
        BladeAttack(ref p1, ref p2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void P1AttachP2(ref Player p1, ref Player p2)
        {
            for (int i = 0; i < p1.Blades.Count; ++i)
            {
                ref PlayerBladeInfo blade = ref p1.Blades.Infos[i];
                LineSegment ls = p1.GetBladeLineSegment(i);

                if (PrimitiveUtils.IsLineIntersectingCircle(ls, p2.Position, p2.Size))
                {
                    p2.Health -= p1.Blades.Damage;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void BladeAttack(ref Player p1, ref Player p2)
        {
            for (int p1i = 0; p1i < p1.Blades.Count; ++p1i)
            {
                ref PlayerBlades p1PlayerBlades = ref p1.Blades;
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

    public readonly PlayerDto ToDto()
    {
        return new PlayerDto
        {
            UserId = UserId,
            UserName = UserName,
            Position = [Position.X, Position.Y],
            Health = Health,
            Size = Size,
            Blades = Blades.ToDto(),
            DeadTime = DeadTime
        };
    }
}

public record struct PlayerBlades()
{
    public float RotationDegreePerSecond = 2;
    public float Length = 40;
    public float Damage = 1;
    public readonly int Count => Infos.Length;
    public PlayerBladeInfo[] Infos = [];

    public static PlayerBlades Default => new()
    {
        Infos = [new PlayerBladeInfo()]
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyBladeAt(int bladeIndex)
    {
        if (Infos.Length == 0) return;

        PlayerBladeInfo[] newBlades = new PlayerBladeInfo[Infos.Length - 1];
        for (int i = 0; i < bladeIndex; ++i)
            newBlades[i] = Infos[i];
        for (int i = bladeIndex + 1; i < Infos.Length; ++i)
            newBlades[i - 1] = Infos[i];

        Infos = newBlades;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddBlade(int bladeCountAmount)
    {
        Array.Resize(ref Infos, Infos.Length + bladeCountAmount);

        float startAngle = Infos.Length == 0 ? 0 : Infos[^1].RotationAngle;
        for (int i = 0; i < Infos.Length; ++i)
        {
            ref PlayerBladeInfo info = ref Infos[i];
            Infos[i].RotationAngle = 2 * MathF.PI / Infos.Length * i + startAngle;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PlayerBladesDto ToDto()
    {
        return new PlayerBladesDto
        {
            Length = Length,
            Damage = Damage,
            Blades = Infos.Select(bladeInfo => bladeInfo.ToDto()).ToArray()
        };
    }
}

public record struct PlayerBladeInfo
{
    public float RotationAngle;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PlayerBladeInfoDto ToDto()
    {
        return new PlayerBladeInfoDto
        {
            RotationAngle = RotationAngle
        };
    }
}
