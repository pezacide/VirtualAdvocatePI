using System.Net.Http.Json;
using System.Text.Json.Serialization;
using VirtualAdvocatePI.Mobile.Configuration;
using VirtualAdvocatePI.Mobile.Models.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Auth;

public sealed class FirebaseAuthSessionService : IAuthSessionService
{
    private const string IdTokenKey = "firebase_id_token";
    private const string RefreshTokenKey = "firebase_refresh_token";
    private const string EmailKey = "firebase_email";
    private const string LocalIdKey = "firebase_local_id";
    private const string ExpiresAtKey = "firebase_expires_at_utc";

    private readonly HttpClient _httpClient;
    private readonly MobileAppSettings _settings;

    public FirebaseAuthSessionService(
        HttpClient httpClient,
        MobileAppSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<AuthState> GetCurrentAuthStateAsync()
    {
        var idToken = await SecureStorage.Default.GetAsync(IdTokenKey);
        var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        var email = await SecureStorage.Default.GetAsync(EmailKey);
        var localId = await SecureStorage.Default.GetAsync(LocalIdKey);
        var expiresAtRaw = await SecureStorage.Default.GetAsync(ExpiresAtKey);

        DateTimeOffset? expiresAt = null;

        if (DateTimeOffset.TryParse(expiresAtRaw, out var parsedExpiresAt))
        {
            expiresAt = parsedExpiresAt;
        }

        return new AuthState
        {
            IsSignedIn = !string.IsNullOrWhiteSpace(idToken),
            LocalId = localId,
            DisplayName = email,
            Email = email,
            IdToken = idToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expiresAt
        };
    }

    public async Task<AuthState> SignInWithEmailPasswordAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.IsFirebaseConfigured)
        {
            throw new InvalidOperationException("Firebase Web API key is not configured in MobileAppSettings.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        var request = new FirebasePasswordSignInRequest
        {
            Email = email.Trim(),
            Password = password,
            ReturnSecureToken = true
        };

        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={Uri.EscapeDataString(_settings.FirebaseWebApiKey)}";

        using var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(ToFriendlyFirebaseError(errorText));
        }

        var signInResponse = await response.Content.ReadFromJsonAsync<FirebasePasswordSignInResponse>(
            cancellationToken: cancellationToken);

        if (signInResponse is null || string.IsNullOrWhiteSpace(signInResponse.IdToken))
        {
            throw new InvalidOperationException("Firebase sign-in did not return an ID token.");
        }

        var expiresInSeconds = 3600;

        if (int.TryParse(signInResponse.ExpiresIn, out var parsedSeconds))
        {
            expiresInSeconds = parsedSeconds;
        }

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds - 60);

        await SecureStorage.Default.SetAsync(IdTokenKey, signInResponse.IdToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, signInResponse.RefreshToken ?? string.Empty);
        await SecureStorage.Default.SetAsync(EmailKey, signInResponse.Email ?? email.Trim());
        await SecureStorage.Default.SetAsync(LocalIdKey, signInResponse.LocalId ?? string.Empty);
        await SecureStorage.Default.SetAsync(ExpiresAtKey, expiresAt.ToString("O"));

        return new AuthState
        {
            IsSignedIn = true,
            LocalId = signInResponse.LocalId,
            Email = signInResponse.Email ?? email.Trim(),
            DisplayName = signInResponse.DisplayName ?? signInResponse.Email ?? email.Trim(),
            IdToken = signInResponse.IdToken,
            RefreshToken = signInResponse.RefreshToken,
            ExpiresAtUtc = expiresAt
        };
    }

    public async Task<string?> GetIdTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(IdTokenKey);
    }

    public Task SignOutAsync()
    {
        SecureStorage.Default.Remove(IdTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(EmailKey);
        SecureStorage.Default.Remove(LocalIdKey);
        SecureStorage.Default.Remove(ExpiresAtKey);

        return Task.CompletedTask;
    }

    private static string ToFriendlyFirebaseError(string errorText)
    {
        if (errorText.Contains("INVALID_LOGIN_CREDENTIALS", StringComparison.OrdinalIgnoreCase) ||
            errorText.Contains("INVALID_PASSWORD", StringComparison.OrdinalIgnoreCase) ||
            errorText.Contains("EMAIL_NOT_FOUND", StringComparison.OrdinalIgnoreCase))
        {
            return "The email or password was not accepted by Firebase.";
        }

        if (errorText.Contains("USER_DISABLED", StringComparison.OrdinalIgnoreCase))
        {
            return "This Firebase user account is disabled.";
        }

        if (errorText.Contains("TOO_MANY_ATTEMPTS_TRY_LATER", StringComparison.OrdinalIgnoreCase))
        {
            return "Too many sign-in attempts. Try again later.";
        }

        return "Firebase sign-in failed.";
    }

    private sealed class FirebasePasswordSignInRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; init; } = string.Empty;

        [JsonPropertyName("returnSecureToken")]
        public bool ReturnSecureToken { get; init; }
    }

    private sealed class FirebasePasswordSignInResponse
    {
        [JsonPropertyName("localId")]
        public string? LocalId { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; init; }

        [JsonPropertyName("idToken")]
        public string? IdToken { get; init; }

        [JsonPropertyName("registered")]
        public bool Registered { get; init; }

        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expiresIn")]
        public string? ExpiresIn { get; init; }
    }
}