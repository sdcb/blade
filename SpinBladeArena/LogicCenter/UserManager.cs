namespace SpinBladeArena.LogicCenter;

public class UserManager
{
    private readonly Dictionary<int, bool> UserOnlineStatus = [];

    public void SetUserOnline(int userId)
    {
        UserOnlineStatus[userId] = true;
    }

    public void SetUserOffline(int userId)
    {
        UserOnlineStatus[userId] = false;
    }

    public bool IsUserOnline(int userId)
    {
        return UserOnlineStatus.TryGetValue(userId, out bool isOnline) && isOnline;
    }
}
