using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena.Pages;

public class LobbyModel(GameManager gameManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int LobbyId { get; set; }

    public Lobby Lobby { get; set; } = null!;

    public IActionResult OnGet()
    {
        if (gameManager.Lobbies.TryGetValue(LobbyId, out Lobby? value))
        {
            Lobby = value;
            return Page();
        }
        else
        {
            return RedirectToPage("/Index");
        }
    }
}
