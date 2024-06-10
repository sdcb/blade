using System.Text.Json.Serialization;

namespace SpinBladeArena.Users;

public record TokenResponseDto
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; init; }

    [JsonPropertyName("refresh_expires_in")]
    public required int RefreshExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; init; }

    [JsonPropertyName("id_token")]
    public required string IdToken { get; init; }

    [JsonPropertyName("not-before-policy")]
    public required int NotBeforePolicy { get; init; }

    [JsonPropertyName("session_state")]
    public required string SessionState { get; init; }

    [JsonPropertyName("scope")]
    public required string Scope { get; init; }
}