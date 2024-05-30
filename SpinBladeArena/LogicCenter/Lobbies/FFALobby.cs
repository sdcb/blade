namespace SpinBladeArena.LogicCenter.Lobbies;

public sealed class FFALobby(int id, FFALobbyCreateOptions CreateOptions, IServiceProvider ServiceProvider) : Lobby(id, CreateOptions, ServiceProvider)
{
}

public sealed record FFALobbyCreateOptions : LobbyCreateOptions;

public abstract record LobbyCreateOptions
{
    public int RobotCount { get; init; }
    public int RewardCount { get; init; }
    public int CreateUserId { get; init; }
    public DateTime CreateTime { get; init; }

    public int CalculateRewardCount(int totalPlayerCount)
    {
        if (RewardCount == 0) return totalPlayerCount * 2;
        return RewardCount;
    }
}