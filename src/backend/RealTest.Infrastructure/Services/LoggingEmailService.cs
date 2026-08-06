using Microsoft.Extensions.Logging;
using RealTest.Application.Common.Interfaces;

namespace RealTest.Infrastructure.Services;

/// <summary>
/// Placeholder email delivery: logs instead of sending. Replace with a real provider
/// (e.g. SendGrid, SES, SMTP) behind the same <see cref="IEmailService"/> interface when the
/// product is ready to send real email — no Application-layer changes required.
/// </summary>
public sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    public Task SendEmailVerificationAsync(string toEmail, string displayName, string verificationToken, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[EmailService placeholder] Email verification link would be sent to {DisplayName} <{Email}>.",
            displayName,
            toEmail);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string displayName, string resetToken, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[EmailService placeholder] Password reset link would be sent to {DisplayName} <{Email}>.",
            displayName,
            toEmail);
        return Task.CompletedTask;
    }
}
