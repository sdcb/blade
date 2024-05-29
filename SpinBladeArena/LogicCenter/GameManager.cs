using SpinBladeArena.LogicCenter.Lobbies;

namespace SpinBladeArena.LogicCenter;

public class GameManager(IServiceProvider ServiceProvider)
{
    public Dictionary<int, Lobby> Lobbies { get; } = [];

    public int CreateFFALobby(FFALobbyCreateOptions options)
    {
        int nextLobbyId;
        lock (Lobbies)
        {
            nextLobbyId = Lobbies.Count + 1;
            Lobbies[nextLobbyId] = new FFALobby(nextLobbyId, options, ServiceProvider);
        }
        return nextLobbyId;
    }
}

public record User(string Name);