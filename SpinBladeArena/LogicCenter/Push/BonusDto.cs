namespace SpinBladeArena.LogicCenter.Push;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[DataContract]
public class BonusDto
{
    [DataMember(Name = "t"), JsonPropertyName("t")]
    public required int Type { get; init; }

    [DataMember(Name = "p"), JsonPropertyName("p")]
    public required float[] Position { get; init; }
}
