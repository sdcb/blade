using SpinBladeArena.LogicCenter.AI;

namespace SpinBladeArena.Users;

public class UserManager
{
    readonly Dictionary<int, UserInfo> _users = [];
    int _nextUserId = 1;
    public int AIUserCount { get; private set; } = 0;

    public UserManager()
    {
        AIPlayer.EnsureAIUsers(this);
    }

    public UserInfo EnsureUser(string userName, string password)
    {
        if (userName.Length > 10) throw new ArgumentException("User name is too long", nameof(userName));
        UserInfo? user = _users.Values.FirstOrDefault(user => user.Password == password);

        if (user != null)
        {
            user = user with { Name = userName };
            _users[user.Id] = user;
            return user;
        }

        int userId = Interlocked.Increment(ref _nextUserId);
        UserInfo newUser = new(userId, userName, password, IsOnline: false, DateTime.Now);
        _users[userId] = newUser;
        return newUser;
    }

    public void AddAIUser(UserInfo user)
    {
        int userId = -++AIUserCount;
        _users[userId] = user with { Id = userId };
    }

    public UserInfo[] GetAllUsers() => [.. _users.Values];

    public void OnUserConnected(int userId)
    {
        UserInfo? userInfo = GetUser(userId);
        if (userInfo != null)
        {
            _users[userId] = userInfo with { IsOnline = true, LatestActive = DateTime.Now };
        }
    }

    public void OnUserDisconnected(int userId)
    {
        UserInfo? userInfo = GetUser(userId);
        if (userInfo != null)
        {
            _users[userId] = userInfo with { IsOnline = false };
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
            _users[userId] = user with { LatestActive = DateTime.Now };
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
