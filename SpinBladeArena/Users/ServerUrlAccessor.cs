namespace SpinBladeArena.Users;

public class ServerUrlAccessor(IHttpContextAccessor httpContextAccessor)
{
    private HttpContext HttpContext => httpContextAccessor.HttpContext!;

    public string Schema => HttpContext.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? HttpContext.Request.Scheme;

    public string Host => HttpContext.Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? HttpContext.Request.Host.ToString();

    public string ServerUrl => $"{Schema}://{Host}";
}
