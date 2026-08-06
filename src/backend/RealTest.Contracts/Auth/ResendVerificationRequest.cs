namespace RealTest.Contracts.Auth;

public sealed class ResendVerificationRequest
{
    public string Email { get; init; } = string.Empty;
}
