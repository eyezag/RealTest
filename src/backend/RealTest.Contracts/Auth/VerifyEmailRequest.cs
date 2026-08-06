namespace RealTest.Contracts.Auth;

public sealed class VerifyEmailRequest
{
    public string Email { get; init; } = string.Empty;

    public string Token { get; init; } = string.Empty;
}
