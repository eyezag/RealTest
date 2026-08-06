namespace RealTest.Application.Common.Interfaces;

/// <summary>
/// Sends transactional auth emails. The initial foundation ships a logging placeholder
/// implementation (see RealTest.Infrastructure) — swap in a real provider later without
/// touching Application code.
/// </summary>
public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string displayName, string verificationToken, CancellationToken cancellationToken);

    Task SendPasswordResetAsync(string toEmail, string displayName, string resetToken, CancellationToken cancellationToken);
}
