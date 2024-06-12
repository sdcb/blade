using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SpinBladeArena.Users;

public class CurrentUser(IHttpContextAccessor httpContextAccessor)
{
    public int Id => int.Parse(httpContextAccessor.HttpContext!.User.Identity!.Name!);

    public string UniqueIdentifier => ((ClaimsIdentity)httpContextAccessor.HttpContext!.User.Identity!).FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

    public string Name => ((ClaimsIdentity)httpContextAccessor.HttpContext!.User.Identity!).FindFirst(ClaimTypes.NameIdentifier)!.Value;
}
