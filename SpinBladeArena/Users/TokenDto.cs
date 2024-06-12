namespace SpinBladeArena.Users;

public record TokenDto(int UserId, string Token, DateTime Expires);
