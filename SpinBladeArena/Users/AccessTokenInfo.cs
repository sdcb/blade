using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace SpinBladeArena.Users;

public record AccessTokenInfo
{
    [JsonPropertyName("exp")]
    public required int Exp { get; init; }

    [JsonPropertyName("iat")]
    public required int Iat { get; init; }

    [JsonPropertyName("auth_time")]
    public required int AuthTime { get; init; }

    [JsonPropertyName("jti")]
    public required string Jti { get; init; }

    [JsonPropertyName("iss")]
    public required string Iss { get; init; }

    [JsonPropertyName("aud")]
    public required string Aud { get; init; }

    [JsonPropertyName("sub")]
    public required string Sub { get; init; }

    [JsonPropertyName("typ")]
    public required string Typ { get; init; }

    [JsonPropertyName("azp")]
    public required string Azp { get; init; }

    [JsonPropertyName("session_state")]
    public required string SessionState { get; init; }

    [JsonPropertyName("acr")]
    public required string Acr { get; init; }

    [JsonPropertyName("allowed-origins")]
    public required List<string> AllowedOrigins { get; init; }

    [JsonPropertyName("realm_access")]
    public required RealmAccess RealmAccess { get; init; }

    [JsonPropertyName("resource_access")]
    public required Dictionary<string, ResourceAccess> ResourceAccess { get; init; }

    [JsonPropertyName("scope")]
    public required string Scope { get; init; }

    [JsonPropertyName("sid")]
    public required string Sid { get; init; }

    [JsonPropertyName("email_verified")]
    public required bool EmailVerified { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("preferred_username")]
    public required string PreferredUsername { get; init; }

    [JsonPropertyName("given_name")]
    public required string GivenName { get; init; }

    [JsonPropertyName("family_name")]
    public required string FamilyName { get; init; }

    [JsonPropertyName("email")]
    public required string Email { get; init; }

    public static AccessTokenInfo Decode(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token));

        string[] parts = token.Split('.');
        if (parts.Length < 3)
            throw new ArgumentException("Invalid JWT token format.", nameof(token));

        string payload = parts[1];
        string decodedJson = Base64UrlDecode(payload);
        return JsonSerializer.Deserialize<AccessTokenInfo>(decodedJson) ?? throw new InvalidOperationException("Deserialization failed.");
    }

    private static string Base64UrlDecode(string input)
    {
        string output = input;
        output = output.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 0: break;
            case 2: output += "=="; break;
            case 3: output += "="; break;
            default: throw new FormatException("Invalid Base64 URL string.");
        }
        var base64EncodedBytes = Convert.FromBase64String(output);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}

public record RealmAccess
{
    [JsonPropertyName("roles")]
    public required List<string> Roles { get; init; }
}

public record ResourceAccess
{
    [JsonPropertyName("roles")]
    public required List<string> Roles { get; init; }
}