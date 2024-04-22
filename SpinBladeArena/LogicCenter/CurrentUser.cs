namespace SpinBladeArena.LogicCenter;

public class CurrentUser(IHttpContextAccessor httpContextAccessor)
{
    public int Id => int.Parse(httpContextAccessor.HttpContext!.User.Identity!.Name!);
}
