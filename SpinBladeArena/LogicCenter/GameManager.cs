using SpinBladeArena.LogicCenter.Lobbies;

namespace SpinBladeArena.LogicCenter;

public class GameManager(IServiceProvider ServiceProvider)
{
    public Dictionary<int, Lobby> Lobbies { get; } = [];

    public int CreateFFALobby(int userId)
    {
        int nextLobbyId;
        lock (Lobbies)
        {
            nextLobbyId = Lobbies.Count + 1;
            Lobbies[nextLobbyId] = new FFALobby(nextLobbyId, userId, DateTime.Now, ServiceProvider);
        }
        return nextLobbyId;
    }
}

public record User(string Name);