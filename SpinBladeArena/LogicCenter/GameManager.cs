using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.Hubs;

namespace SpinBladeArena.LogicCenter;

public class GameManager(IHubContext<GameHub, IGameHubClient> Hub, UserManager UserManager)
{
    public Dictionary<int, Lobby> Lobbies { get; } = [];

    public int CreateLobby(int userId)
    {
        int nextLobbyId;
        lock (Lobbies)
        {
            nextLobbyId = Lobbies.Count + 1;
            Lobbies[nextLobbyId] = new Lobby(nextLobbyId, userId, DateTime.Now, Hub, UserManager);
        }
        return nextLobbyId;
    }
}

public record User(string Name);