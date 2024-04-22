using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Pages;

public class LobbyModel(ILogger<LobbyModel> logger) : PageModel
{
    private readonly ILogger<LobbyModel> _logger = logger;

    [BindProperty(SupportsGet = true)]
    public int LobbyId { get; set; }

    public Lobby Lobby { get; set; } = null!;

    public void OnGet()
    {
        Lobby = GameManager.Instance.Lobbies[LobbyId];
    }
}
