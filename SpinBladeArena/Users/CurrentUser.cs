using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SpinBladeArena.Users;

public class CurrentUser(IHttpContextAccessor httpContextAccessor)
{
    public int Id => int.Parse(httpContextAccessor.HttpContext!.User.Identity!.Name!);

    public string Name => ((ClaimsIdentity)httpContextAccessor.HttpContext!.User.Identity!).FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")!.Value;

}
