namespace SpinBladeArena.LogicCenter;

public class GameManager
{
    public Dictionary<int, Lobby> Lobbies { get; } = [];

    public int CreateLobby(int userId)
    {
        int nextLobbyId;
        lock (Lobbies)
        {
            nextLobbyId = Lobbies.Count + 1;
            Lobbies[nextLobbyId] = new Lobby(nextLobbyId, userId, DateTime.Now);
        }
        return nextLobbyId;
    }
}

public record User(string Name);