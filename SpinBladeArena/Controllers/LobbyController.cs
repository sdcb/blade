﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Controllers;

[Authorize]
public class LobbyController(CurrentUser user, GameManager gameManager) : Controller
{
    [HttpPost, Route("lobby")]
    public int CreateLobby()
    {
        return gameManager.CreateLobby(user.Id);
    }
}
