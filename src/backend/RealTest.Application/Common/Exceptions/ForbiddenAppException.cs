namespace RealTest.Application.Common.Exceptions;

/// <summary>Represents an authenticated caller who is not allowed to perform the requested action.</summary>
public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message) : base(message)
    {
    }
}
