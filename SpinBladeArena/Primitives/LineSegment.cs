using System.Numerics;

namespace SpinBladeArena.Primitives;

public readonly record struct LineSegment(in Vector2 Start, in Vector2 End)
{
    public readonly bool IsIntersectingCircle(in Circle circle)
    {
        Vector2 d = End - Start;
        Vector2 f = Start - circle.Center;

        float a = Vector2.Dot(d, d);
        float b = 2 * Vector2.Dot(f, d);
        float c = Vector2.Dot(f, f) - circle.Radius * circle.Radius;

        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            return false;
        }

        discriminant = MathF.Sqrt(discriminant);
        float t1 = (-b - discriminant) / (2 * a);
        float t2 = (-b + discriminant) / (2 * a);

        if (t1 >= 0 && t1 <= 1)
        {
            return true;
        }

        if (t2 >= 0 && t2 <= 1)
        {
            return true;
        }

        return false;
    }

    public float DistanceTo(in Circle circle)
    {
        Vector2 d = End - Start;
        Vector2 f = Start - circle.Center;

        float a = Vector2.Dot(d, d);
        float b = 2 * Vector2.Dot(f, d);
        float t = Math.Clamp(-b / (2 * a), 0, 1);  // Lowest quadratic solution

        Vector2 nearestPoint = Start + t * d;
        Vector2 nearestVector = circle.Center - nearestPoint;

        float distance = nearestVector.Length() - circle.Radius;
        return Math.Max(0, distance);  // ensure non-negative distance
    }

    public static bool IsIntersection(in LineSegment lineSegment1, in LineSegment lineSegment2)
    {
        Vector2 a = lineSegment1.End - lineSegment1.Start;
        Vector2 b = lineSegment2.Start - lineSegment2.End;
        Vector2 c = lineSegment1.Start - lineSegment2.Start;

        float alphaNumerator = b.Y * c.X - b.X * c.Y;
        float betaNumerator = a.X * c.Y - a.Y * c.X;
        float denominator = a.Y * b.X - a.X * b.Y;

        // 使用一个小的epsilon值来检测接近于0的情况
        const float epsilon = 1e-10f;
        if (Math.Abs(denominator) < epsilon)
        {
            return false; // 线段平行或共线，但不一定相交
        }

        float alpha = alphaNumerator / denominator;
        float beta = betaNumerator / denominator;

        // 检查alpha和beta是否都在0到1之间，如果是，线段相交
        return 0 <= alpha && alpha <= 1 && 0 <= beta && beta <= 1;
    }
}