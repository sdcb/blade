using Microsoft.AspNetCore.Mvc;
using SpinBladeArena.Users;
using System.Text;
using System.Web;

namespace SpinBladeArena.Controllers;

public class UserController(UserManager userManager, KeycloakConfig ssoConfig, ServerUrlAccessor serverUrlAccessor) : Controller
{
    [Route("token")]
    public ActionResult<TokenDto> CreateToken(string userName, string uid)
    {
        if (!string.IsNullOrEmpty(ssoConfig.ClientId))
        {
            return BadRequest("SSO is enabled, use /sso-login instead");
        }
        userManager.EnsureUser(userName, uid);
        return userManager.CreateToken(userName);
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
