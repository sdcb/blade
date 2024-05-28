namespace SpinBladeArena.LogicCenter.Push;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[DataContract]
public record PushState(long FrameId, PlayerDto[] Players, PickableBonusDto[] Bonuses, PlayerDto[] DeadPlayers)
{
    [DataMember(Name = "i"), JsonPropertyName("i")]
    public long FrameId { get; init; } = FrameId;

    [DataMember(Name = "p"), JsonPropertyName("p")]
    public PlayerDto[] Players { get; set; } = Players;

    [DataMember(Name = "b"), JsonPropertyName("b")]
    public PickableBonusDto[] Bonuses { get; set; } = Bonuses;

    [DataMember(Name = "d"), JsonPropertyName("d")]
    public PlayerDto[] DeadPlayers { get; set; } = DeadPlayers;
}

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

    [DataMember(Name = "i"), JsonPropertyName("i")]
    public required float Size { get; init; }

    [DataMember(Name = "b"), JsonPropertyName("b")]
    public required BladeDto[] Blades { get; init; }
}

[DataContract]
public class PickableBonusDto
{
    [DataMember(Name = "n"), JsonPropertyName("n")]
    public required string Name { get; init; }

    [DataMember(Name = "p"), JsonPropertyName("p")]
    public required float[] Position { get; init; }
}

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