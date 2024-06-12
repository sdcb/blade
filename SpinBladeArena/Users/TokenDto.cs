namespace SpinBladeArena.Users;

public record TokenDto(int UserId, string UserName, string Token, DateTime Expires);
