using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.Controllers;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Hubs;

[Authorize]
public class GameHub(GameManager gameManager, CurrentUser user) : Hub<IGameHubClient>
{
    public void JoinLobby(int lobbyId)
    {
        Lobby lobby = gameManager.Lobbies[lobbyId];
        lobby.AddPlayerToRandomPosition(user.Id, user.Name, Context.ConnectionId);
        Groups.AddToGroupAsync(Context.ConnectionId, lobbyId.ToString());
        lobby.Start();
    }

    public void SetDestination(int lobbyId, float x, float y)
    {
        gameManager.Lobbies[lobbyId].SetPlayerDestination(user.Id, x, y);
    }
}

public interface IGameHubClient
{
    Task Update(PlayerDto[] players, PickableBonusDto[] pickableBonuses, PlayerDto[] deadPlayers);
}
