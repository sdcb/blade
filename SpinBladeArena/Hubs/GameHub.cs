using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Hubs;

[Authorize]
public class GameHub(GameManager gameManager, CurrentUser user, UserManager userManager) : Hub<IGameHubClient>
{
    public void JoinLobby(int lobbyId)
    {
        Lobby lobby = gameManager.Lobbies[lobbyId];
        lobby.AddPlayerToRandomPosition(new (user.Id, user.Name));
        Groups.AddToGroupAsync(Context.ConnectionId, lobbyId.ToString());
        lobby.EnsureStart();
    }

    public void SetDestination(int lobbyId, float x, float y)
    {
        gameManager.Lobbies[lobbyId].SetPlayerDestination(user.Id, x, y);
    }

    public override Task OnConnectedAsync()
    {
        userManager.SetUserOnline(user.Id);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        userManager.SetUserOffline(user.Id);
        return base.OnDisconnectedAsync(exception);
    }
}

public interface IGameHubClient
{
    Task Update(PlayerDto[] players, PickableBonusDto[] pickableBonuses, PlayerDto[] deadPlayers);
}
