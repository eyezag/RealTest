namespace RealTest.Application.Common.Exceptions;

/// <summary>Represents an authentication failure (bad credentials, invalid/expired token, locked-out account).</summary>
public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message) : base(message)
    {
    }
}
