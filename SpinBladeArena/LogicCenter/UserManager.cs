using System.Diagnostics.CodeAnalysis;

namespace SpinBladeArena.LogicCenter;

public class UserManager
{
    readonly Dictionary<int, UserInfo> _users = [];
    int _nextUserId = 1;

    public void CreateUser(string userName)
    {
        if (userName.Length > 10) throw new ArgumentException("User name is too long", nameof(userName));
        if (_users.Values.Any(user => user.Name == userName)) throw new ArgumentException("User name already exists", nameof(userName));

        int userId = Interlocked.Increment(ref _nextUserId);
        _users[userId] = new UserInfo(userId, userName, false, DateTime.Now);
    }

    public void OnUserConnected(int user)
    {
        UserInfo? userInfo = GetUser(user);
        if (userInfo != null)
        {
            userInfo.IsOnline = true;
            userInfo.LatestActive = DateTime.Now;
        }
    }

    public void OnUserDisconnected(int user)
    {
        UserInfo? userInfo = GetUser(user);
        if (userInfo != null)
        {
            userInfo.IsOnline = false;
        }
    }

    public UserInfo? GetUser(int userId)
    {
        if (_users.TryGetValue(userId, out UserInfo? user))
        {
            return user;
        }
        return null;
    }

    internal void OnUserActive(int id)
    {
        if (_users.TryGetValue(id, out UserInfo? user))
        {
            user.LatestActive = DateTime.Now;
        }
    }
}

public class UserInfo(int id, string name, bool isOnline, DateTime latestActive)
{
    public int Id { get; init; } = id;

    public string Name { get; init; } = name;

    public bool IsOnline { get; set; } = isOnline;

    public DateTime LatestActive { get; set; } = latestActive;

    public bool IsExpired => DateTime.Now - LatestActive > ExpirationTime;

    public static TimeSpan ExpirationTime { get; } = TimeSpan.FromMinutes(15);
}
