namespace SpinBladeArena.LogicCenter.Push;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[DataContract]
public class PickableBonusDto
{
    [DataMember(Name = "n"), JsonPropertyName("n")]
    public required string Name { get; init; }

    [DataMember(Name = "p"), JsonPropertyName("p")]
    public required float[] Position { get; init; }
}
