namespace SpinBladeArena.LogicCenter.Lobbies;

public class ChickenDinnerLobby(int id, int createUserId, DateTime createTime, IServiceProvider ServiceProvider) : Lobby(id, createUserId, createTime, ServiceProvider)
{
}

public record ChickenDinnerLobbyOptions
{
}