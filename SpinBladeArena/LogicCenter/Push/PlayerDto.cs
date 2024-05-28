namespace SpinBladeArena.LogicCenter.Push;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[DataContract]
public class PlayerDto
{
    [DataMember(Name = "u"), JsonPropertyName("u")]
    public required int UserId { get; init; }

    [DataMember(Name = "n"), JsonPropertyName("n")]
    public required string UserName { get; init; }

    [DataMember(Name = "s"), JsonPropertyName("s")]
    public required int Score { get; init; }

    [DataMember(Name = "p"), JsonPropertyName("p")]
    public required float[] Position { get; init; }

    [DataMember(Name = "d"), JsonPropertyName("d")]
    public required float[] Destination { get; init; }

    [DataMember(Name = "h"), JsonPropertyName("h")]
    public required float Health { get; init; }

    [DataMember(Name = "b"), JsonPropertyName("b")]
    public required BladeDto[] Blades { get; init; }
}
