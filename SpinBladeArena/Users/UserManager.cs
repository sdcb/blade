using System.Diagnostics.CodeAnalysis;

namespace SpinBladeArena.Users;

public class UserManager
{
    readonly Dictionary<int, UserInfo> _users = [];
    int _nextUserId = 1;

    public UserInfo EnsureUser(string userName, string password)
    {
        if (userName.Length > 10) throw new ArgumentException("User name is too long", nameof(userName));
        UserInfo? user = _users.Values.FirstOrDefault(user => user.Name == userName);

        if (user != null) return user;

        int userId = Interlocked.Increment(ref _nextUserId);
        UserInfo newUser = new(userId, userName, password, isOnline: false, DateTime.Now);
        _users[userId] = newUser;
        return newUser;
    }

    public UserInfo[] GetAllUsers() => [.. _users.Values];

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

    internal void OnUserActive(int userId)
    {
        if (_users.TryGetValue(userId, out UserInfo? user))
        {
            user.LatestActive = DateTime.Now;
        }
    }

    internal bool IsUserOnline(int userId)
    {
        if (_users.TryGetValue(userId, out UserInfo? user))
        {
            return user.IsOnline;
        }
        return false;
    }
}
