using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SpinBladeArena.LogicCenter.Push;

[DataContract]
public record StatInfoDto
{
    [DataMember(Name = "i"), JsonPropertyName("i")]
    public required int UserId { get; init; }

    [DataMember(Name = "s"), JsonPropertyName("s")]
    public required int Score { get; init; }

    [DataMember(Name = "k"), JsonPropertyName("k")]
    public required int Kills { get; init; }

    [DataMember(Name = "d"), JsonPropertyName("d")]
    public required int Deaths { get; init; }

    [DataMember(Name = "b"), JsonPropertyName("b")]
    public required int DestroyBlades { get; init; }
}
