using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.LogicCenter;
using SpinBladeArena.LogicCenter.Push;
using SpinBladeArena.Users;

namespace SpinBladeArena.Hubs;

[Authorize]
public class GameHub(GameManager gameManager, CurrentUser user, UserManager userManager) : Hub<IGameHubClient>
{
    public void JoinLobby(int lobbyId)
    {
        Lobby lobby = gameManager.Lobbies[lobbyId];
        lobby.AddPlayerToRandomPosition(new (user.Id));
        Groups.AddToGroupAsync(Context.ConnectionId, lobbyId.ToString());
        lobby.EnsureStart();
    }

    public void SetDestination(int lobbyId, float x, float y)
    {
        Lobby lobby = gameManager.Lobbies[lobbyId];
        lobby.SetPlayerDestination(user.Id, x, y);
        userManager.OnUserActive(user.Id);
    }

    public override Task OnConnectedAsync()
    {
        userManager.OnUserConnected(user.Id);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        userManager.OnUserDisconnected(user.Id);
        return base.OnDisconnectedAsync(exception);
    }
}

public interface IGameHubClient
{
    Task Update(PushState pushState);
}
