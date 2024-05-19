using System.Numerics;
using System.Runtime.Serialization;

namespace SpinBladeArena.LogicCenter;

[DataContract]
public class PlayerDto
{
    [DataMember(Name = "userId")]
    public required int UserId { get; init; }

    [DataMember(Name = "userName")]
    public required string UserName { get; init; }

    [DataMember(Name = "score")]
    public required int Score { get; init; }

    [DataMember(Name = "position")]
    public required float[] Position { get; init; }

    [DataMember(Name = "destination")]
    public required float[] Destination { get; init; }

    [DataMember(Name = "health")]
    public required float Health { get; init; }

    [DataMember(Name = "size")]
    public required float Size { get; init; }

    [DataMember(Name = "blades")]
    public required BladeDto[] Blades { get; init; }
}

[DataContract]
public class PickableBonusDto
{
    [DataMember(Name = "name")]
    public required string Name { get; init; }

    [DataMember(Name = "position")]
    public required float[] Position { get; init; }
}

[DataContract]
public class BladeDto
{
    [DataMember(Name = "angle")]
    public required float Angle { get; init; }

    [DataMember(Name = "length")]
    public required float Length { get; init; }

    [DataMember(Name = "damage")]
    public required float Damage { get; init; }
}