namespace VirtualAdvocatePI.Api.Auth;

public interface IFirebaseAuthService
{
    Task<AuthenticatedFirebaseUser?> VerifyBearerTokenAsync(HttpRequest request);
}
