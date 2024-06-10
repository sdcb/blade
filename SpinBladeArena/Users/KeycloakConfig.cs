namespace SpinBladeArena.Users;

public record KeycloakConfig
{
    public required string ServerUrl { get; init; }

    public required string Realm { get; init; }

    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }

    public required string Scope { get; init; }
}
