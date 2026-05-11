namespace VirtualAdvocatePI.Api.Auth;

public sealed record AuthenticatedFirebaseUser(
    string FirebaseUid,
    string? Email,
    string? DisplayName
);
