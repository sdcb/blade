namespace SpinBladeArena.LogicCenter;

public class GameManager(IServiceProvider ServiceProvider)
{
    public Dictionary<int, Lobby> Lobbies { get; } = [];

    public int CreateLobby(int userId)
    {
        int nextLobbyId;
        lock (Lobbies)
        {
            nextLobbyId = Lobbies.Count + 1;
            Lobbies[nextLobbyId] = new Lobby(nextLobbyId, userId, DateTime.Now, ServiceProvider);
        }
        return nextLobbyId;
    }
}

public record User(string Name);