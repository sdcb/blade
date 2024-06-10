using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SpinBladeArena.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace SpinBladeArena.Controllers;

public class UserController(TokenValidationParameters _tvp, UserManager userManager, KeycloakConfig ssoConfig, ServerUrlAccessor serverUrlAccessor) : Controller
{
    [Route("token")]
    public object CreateToken(string userName, string password)
    {
        UserInfo user = userManager.EnsureUser(userName, password);

        List<Claim> claims =
        [
            // Add any claims you need here
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
            // other claims
        ];

        JwtSecurityToken token = new(
            issuer: _tvp.ValidIssuer,
            audience: _tvp.ValidAudience,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials((SymmetricSecurityKey)_tvp.IssuerSigningKey, SecurityAlgorithms.HmacSha256),
            claims: claims);

        return new
        {
            UserId = user.Id,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    [Route("userList")]
    public UserInfo[] UserList() => userManager.GetAllUsers();

    [Route("sso-login")]
    public IActionResult SsoLogin()
    {
        string redirectUrl = BuildUrl($"{ssoConfig.ServerUrl}/realms/{ssoConfig.Realm}/protocol/openid-connect/auth", new Dictionary<string, string>
        {
            ["client_id"] = ssoConfig.ClientId,
            ["redirect_uri"] = serverUrlAccessor.ServerUrl + "/sso-landing",
            ["response_type"] = "code",
            ["scope"] = ssoConfig.Scope,
            ["state"] = Guid.NewGuid().ToString(),
        });
        return Redirect(redirectUrl);
    }

    [Route("sso-landing")]
    public async Task<IActionResult> SsoLanding(string code, string state, string session_state, [FromServices] HttpClient http)
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

        TokenResponseDto tokenResponse = (await response.Content.ReadFromJsonAsync<TokenResponseDto>())!;

        throw new NotImplementedException();
    }

    static string BuildUrl(string baseUrl, Dictionary<string, string> query)
    {
        if (query.Count == 0)
        {
            return baseUrl;
        }

        StringBuilder sb = new();
        sb.Append(baseUrl);
        sb.Append('?');
        foreach (KeyValuePair<string, string> pair in query)
        {
            sb.Append(pair.Key);
            sb.Append('=');
            sb.Append(HttpUtility.UrlEncode(pair.Value));
            sb.Append('&');
        }
        sb.Length--;
        return sb.ToString();
    }
}
