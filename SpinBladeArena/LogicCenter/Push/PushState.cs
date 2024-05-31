namespace SpinBladeArena.LogicCenter.Push;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[DataContract]
public record PushState(long FrameId, PlayerDto[] Players, BonusDto[] Bonuses, PlayerDto[] DeadPlayers)
{
    [DataMember(Name = "i"), JsonPropertyName("i")]
    public long FrameId { get; init; } = FrameId;

    [DataMember(Name = "p"), JsonPropertyName("p")]
    public PlayerDto[] Players { get; set; } = Players;

    [DataMember(Name = "b"), JsonPropertyName("b")]
    public BonusDto[] Bonuses { get; set; } = Bonuses;

    [DataMember(Name = "d"), JsonPropertyName("d")]
    public PlayerDto[] DeadPlayers { get; set; } = DeadPlayers;
}
