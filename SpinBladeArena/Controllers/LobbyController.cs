using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Controllers;

[Authorize]
public class LobbyController(CurrentUser user) : Controller
{
    [HttpPost, Route("lobby")]
    public int CreateLobby()
    {
        return GameManager.Instance.CreateLobby(user.Id);
    }
}
