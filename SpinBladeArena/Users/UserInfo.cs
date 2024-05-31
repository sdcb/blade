namespace SpinBladeArena.Users;

public record UserInfo(int Id, string Name, string Password, bool IsOnline, DateTime LatestActive)
{
    public bool IsExpired => DateTime.Now - LatestActive > ExpirationTime;

    public static TimeSpan ExpirationTime { get; } = TimeSpan.FromMinutes(15);
}
