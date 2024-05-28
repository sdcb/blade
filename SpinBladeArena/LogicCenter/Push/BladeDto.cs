namespace SpinBladeArena.LogicCenter.Push;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[DataContract]
public class BladeDto
{
    [DataMember(Name = "a"), JsonPropertyName("a")]
    public required float Angle { get; init; }

    [DataMember(Name = "l"), JsonPropertyName("l")]
    public required float Length { get; init; }

    [DataMember(Name = "d"), JsonPropertyName("d")]
    public required float Damage { get; init; }
}