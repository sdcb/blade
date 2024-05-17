using System.Numerics;

namespace SpinBladeArena.LogicCenter;

public class PlayerDto
{
    public required int UserId { get; init; }

    public required string UserName { get; init; }
    public required int Score { get; init; }
    public required float[] Position { get; init; }
    public required float[] Destination { get; init; }
    public required float Health { get; init; }
    public required float Size { get; init; }
    public required BladeDto[] Blades { get; init; }
}

public class PickableBonusDto
{
    public required string Name { get; init; }
    public required float[] Position { get; init; }
}

public class BladeDto
{
    public required float Angle { get; init; }
    public required float Length { get; init; }
    public required float Damage { get; init; }
}