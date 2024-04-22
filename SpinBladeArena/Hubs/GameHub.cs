using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Hubs;

public class GameHub : Hub<IGameHubClient>
{
    public void JoinLobby(int userId, string lobbyName)
    {
        
    }
}

public interface IGameHubClient
{
}
