using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SpinBladeArena.LogicCenter;

public class CurrentUser(IHttpContextAccessor httpContextAccessor)
{
    public int Id => int.Parse(httpContextAccessor.HttpContext!.User.Identity!.Name!);

    public string Name => ((ClaimsIdentity)httpContextAccessor.HttpContext!.User.Identity!).FindFirst(JwtRegisteredClaimNames.Sub)!.Value;
}
