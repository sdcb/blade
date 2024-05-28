using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpinBladeArena.LogicCenter;
using SpinBladeArena.LogicCenter.Push;
using SpinBladeArena.Users;

namespace SpinBladeArena.Controllers;

[Authorize]
public class LobbyController(CurrentUser user, GameManager gameManager) : Controller
{
    [HttpPost, Route("lobby")]
    public int CreateLobby()
    {
        return gameManager.CreateFFALobby(user.Id);
    }

    [AllowAnonymous, HttpGet, Route("lobby/{lobbyId}/state")]
    public ActionResult<PushState> GetLobbyState(int lobbyId)
    {
        if (gameManager.Lobbies.TryGetValue(lobbyId, out Lobby? lobby))
        {
            return lobby.PushManager.Latest;
        }
        else
        {
            return NotFound();
        }
    }
}
