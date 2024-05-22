using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpinBladeArena.Primitives;

public static class PrimitiveUtils
{
    
    public static bool IsLineIntersectingCircle(LineSegment lineSegment, Vector2 circleCenter, float circleRadius)
    {
        Vector2 d = lineSegment.End - lineSegment.Start;
        Vector2 f = lineSegment.Start - circleCenter;

        float a = Vector2.Dot(d, d);
        float b = 2 * Vector2.Dot(f, d);
        float c = Vector2.Dot(f, f) - circleRadius * circleRadius;

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

    
    public static bool IsLineSegmentIntersection(LineSegment lineSegment1, LineSegment lineSegment2)
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
