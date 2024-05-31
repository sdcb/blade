using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SpinBladeArena.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SpinBladeArena.Controllers;

public class UserController(TokenValidationParameters _tvp, UserManager userManager) : Controller
{
    [Route("token")]
    public object CreateToken(string userName, string password)
    {
        UserInfo user = userManager.EnsureUser(userName, password);

        List<Claim> claims =
        [
            // Add any claims you need here
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
            // other claims
        ];

        JwtSecurityToken token = new(
            issuer: _tvp.ValidIssuer,
            audience: _tvp.ValidAudience,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials((SymmetricSecurityKey)_tvp.IssuerSigningKey, SecurityAlgorithms.HmacSha256),
            claims: claims);

        return new
        {
            UserId = user.Id,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    [HttpGet("user/{userId}/name")]
    public string? GetUserNameFromId(int userId) => userManager.GetUser(userId)?.Name;

    [Route("userList")]
    public UserInfo[] UserList() => userManager.GetAllUsers();
}
