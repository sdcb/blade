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
    public required PlayerBladesDto Blades { get; init; }
}

public class PlayerBladesDto
{
    public float Length { get; init; }
    public float Damage { get; init; }
    public required float[] Angles { get; init; }
}

public class PickableBonusDto
{
    public required string Name { get; init; }
    public required float[] Position { get; init; }
}