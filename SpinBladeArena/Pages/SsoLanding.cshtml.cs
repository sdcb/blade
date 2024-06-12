using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpinBladeArena.Users;

namespace SpinBladeArena.Pages;

public class SsoLandingModel : PageModel
{
    public TokenDto TokenDto { get; set; } = null!;

    public async Task<IActionResult> OnGet(string code, string state, /* string session_state, */
        [FromServices] HttpClient http, [FromServices] KeycloakConfig ssoConfig, [FromServices] ServerUrlAccessor serverUrlAccessor, [FromServices] UserManager userManager)
    {
        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("No code");
        }

        if (state != Request.Query["state"])
        {
            return BadRequest("Invalid state");
        }

        HttpResponseMessage response = await http.PostAsync($"{ssoConfig.ServerUrl}/realms/{ssoConfig.Realm}/protocol/openid-connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = ssoConfig.ClientId,
            ["client_secret"] = ssoConfig.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = serverUrlAccessor.ServerUrl + "/sso-landing",
        }));

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest("Failed to get token");
        }

        SsoTokenDto resp = (await response.Content.ReadFromJsonAsync<SsoTokenDto>())!;
        AccessTokenInfo info = AccessTokenInfo.Decode(resp.AccessToken);
        string userName = info.FamilyName + info.GivenName;
        userManager.EnsureUser(userName, info.Sub);
        TokenDto = userManager.CreateToken(userName);

        return Page();
    }
}
