using SpinBladeArena.LogicCenter.Lobbies;
using System.ComponentModel.DataAnnotations;

namespace SpinBladeArena.Controllers;

public record LobbyCreateDto
{
    [Range(0, 12, ErrorMessage = "RobotCount must be between 0 and 12.")]
    public int RobotCount { get; init; }

    [Range(0, 20, ErrorMessage = "RewardCount must be between 0 and 20.")]
    public int RewardCount { get; init; }

    public FFALobbyCreateOptions ToFFA(int userId)
    {
        return new FFALobbyCreateOptions
        {
            RobotCount = RobotCount,
            RewardCount = RewardCount,
            CreateUserId = userId,
            CreateTime = DateTime.Now
        };
    }
}
