namespace SpinBladeArena.LogicCenter.Lobbies;

public class ChickenDinnerLobby(int id, LobbyCreateOptions options, IServiceProvider ServiceProvider) : Lobby(id, options, ServiceProvider)
{
}

public record ChickenDinnerLobbyOptions : LobbyCreateOptions
{
}