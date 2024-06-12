using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SpinBladeArena.Users;

public class UserManager(TokenValidationParameters tvp)
{
    readonly Dictionary<int, UserInfo> _users = [];
    int _nextUserId = 1;

    public UserInfo EnsureUser(string userName, string uid)
    {
        if (userName.Length > 10) throw new ArgumentException("User name is too long", nameof(userName));
        UserInfo? user = _users.Values.FirstOrDefault(user => user.UniqueIdentifier == uid);

        if (user != null)
        {
            user.Name = userName;
            return user;
        }

        int userId = Interlocked.Increment(ref _nextUserId);
        UserInfo newUser = new(userId, userName, uid, isOnline: false, DateTime.Now);
        _users[userId] = newUser;
        return newUser;
    }

    public TokenDto CreateToken(string userName)
    {
        UserInfo user = _users.Values.Single(user => user.Name == userName);

        List<Claim> claims =
        [
            // Add any claims you need here
            new Claim(JwtRegisteredClaimNames.Sub, user.UniqueIdentifier),
            new Claim(JwtRegisteredClaimNames.Name, userName),
            new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
            // other claims
        ];

        DateTime expires = DateTime.UtcNow.AddHours(1);
        JwtSecurityToken token = new(
            issuer: tvp.ValidIssuer,
            audience: tvp.ValidAudience,
            expires: expires,
            signingCredentials: new SigningCredentials((SymmetricSecurityKey)tvp.IssuerSigningKey, SecurityAlgorithms.HmacSha256),
            claims: claims);

        return new TokenDto(user.Id, user.Name, new JwtSecurityTokenHandler().WriteToken(token), expires);
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
