using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpinBladeArena.LogicCenter;
using SpinBladeArena.Performance;

namespace SpinBladeArena.Pages;

public class PerfModel(GameManager gameManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int LobbyId { get; set; }

    public Lobby Lobby { get; set; } = null!;

    public PerformanceManager Perf => Lobby.PerformanceManager;

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
