using SpinBladeArena.Primitives;
using System.Numerics;

namespace SpinBladeArena.Tests.LineSegments;

public class LineSegment_DistanceTo
{
    [Fact]
    public void CircleInMiddleOfLine()
    {
        // Setup - Circle is at the middle of line
        Vector2 start = new(0, 0);
        Vector2 end = new(10, 0);
        LineSegment line = new(start, end);
        Circle circle = new(new Vector2(5, 0), 3);

        // Action
        float distance = line.DistanceTo(circle);

        // Assert
        Assert.Equal(0, distance);
    }

    [Fact]
    public void CircleLeftOfLine()
    {
        // Setup - Circle is completely left of line, and does not intersect
        Vector2 start = new(10, 0);
        Vector2 end = new(20, 0);
        LineSegment line = new(start, end);
        Circle circle = new(new Vector2(0, 0), 3);

        // Action
        float distance = line.DistanceTo(circle);

        // Assert
        Assert.Equal(7, distance); // Since the circle does not intersect, the distance should be greater than 0
    }

    [Fact]
    public void CircleRightOfLine()
    {
        // Setup - Circle is completely right of line, and does not intersect
        Vector2 start = new(-20, 0);
        Vector2 end = new(-10, 0);
        LineSegment line = new(start, end);
        Circle circle = new(new Vector2(5, 0), 3);

        // Action
        float distance = line.DistanceTo(circle);

        // Assert
        Assert.Equal(12, distance); // Since the circle does not intersect, the distance should be greater than 0
    }

    [Fact]
    public void CircleIntersectsLine()
    {
        // Setup - The circle intersects the line
        Vector2 start = new(-10, 0);
        Vector2 end = new(10, 0);
        LineSegment line = new(start, end);
        Circle circle = new(new Vector2(5, 0), 10);

        // Action
        float distance = line.DistanceTo(circle);

        // Assert
        Assert.Equal(0, distance); // Since the circle intersects the line, the distance should be 0
    }
}