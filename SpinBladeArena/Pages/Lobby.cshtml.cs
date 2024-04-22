using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Pages;

public class LobbyModel(GameManager gameManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int LobbyId { get; set; }

    public Lobby Lobby { get; set; } = null!;

    public void OnGet()
    {
        Lobby = gameManager.Lobbies[LobbyId];
    }
}
