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
        gameManager.Lobbies[lobbyId].AddPlayerToRandomPosition(user.Name);
    }

    public void SetDestination(int lobbyId, float x, float y)
    {
        gameManager.Lobbies[lobbyId].SetPlayerDestination(user.Id, x, y);
    }
}

public interface IGameHubClient
{
}
