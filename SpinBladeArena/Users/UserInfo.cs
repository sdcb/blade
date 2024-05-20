namespace SpinBladeArena.Users;

public class UserInfo(int id, string name, string password, bool isOnline, DateTime latestActive)
{
    public int Id { get; init; } = id;

    public string Name { get; init; } = name;

    public string Password { get; init; } = password;

    public bool IsOnline { get; set; } = isOnline;

    public DateTime LatestActive { get; set; } = latestActive;

    public bool IsExpired => DateTime.Now - LatestActive > ExpirationTime;

    public static TimeSpan ExpirationTime { get; } = TimeSpan.FromMinutes(15);
}
