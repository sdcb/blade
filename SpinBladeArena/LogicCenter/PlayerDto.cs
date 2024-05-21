using System.Numerics;
using System.Runtime.Serialization;

namespace SpinBladeArena.LogicCenter;

[DataContract]
public class PlayerDto
{
    [DataMember(Name = "u")]
    public required int UserId { get; init; }

    [DataMember(Name = "n")]
    public required string UserName { get; init; }

    [DataMember(Name = "s")]
    public required int Score { get; init; }

    [DataMember(Name = "p")]
    public required float[] Position { get; init; }

    [DataMember(Name = "d")]
    public required float[] Destination { get; init; }

    [DataMember(Name = "h")]
    public required float Health { get; init; }

    [DataMember(Name = "i")]
    public required float Size { get; init; }

    [DataMember(Name = "b")]
    public required BladeDto[] Blades { get; init; }
}

[DataContract]
public class PickableBonusDto
{
    [DataMember(Name = "n")]
    public required string Name { get; init; }

    [DataMember(Name = "p")]
    public required float[] Position { get; init; }
}

[DataContract]
public class BladeDto
{
    [DataMember(Name = "a")]
    public required float Angle { get; init; }

    [DataMember(Name = "l")]
    public required float Length { get; init; }

    [DataMember(Name = "d")]
    public required float Damage { get; init; }
}