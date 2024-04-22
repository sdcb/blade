using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SpinBladeArena.Controllers;

public class UserController(TokenValidationParameters _tvp) : Controller
{
    static Dictionary<string, int> _userNameMap = new();

    [Route("token")]
    public string CreateToken(string userName)
    {
        int userId = EnsureNewUser(userName);

        List<Claim> claims =
        [
            // Add any claims you need here
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, userId.ToString()),
            // other claims
        ];

        JwtSecurityToken token = new(
            issuer: _tvp.ValidIssuer,
            audience: _tvp.ValidAudience,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials((SymmetricSecurityKey)_tvp.IssuerSigningKey, SecurityAlgorithms.HmacSha256),
            claims: claims);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Route("userList")]
    public Dictionary<string, int> UserList() => _userNameMap;

    static int EnsureNewUser(string userName)
    {
        if (_userNameMap.TryGetValue(userName, out int val))
        {
            return val;
        }
        else
        {
            lock (_userNameMap)
            {
                int nextUserId = _userNameMap.Count + 1;
                _userNameMap[userName] = nextUserId;
                return nextUserId;
            }
        }
    }
}
