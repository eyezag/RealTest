namespace RealTest.Application.Common.Exceptions;

/// <summary>Base type for exceptions the global exception handler knows how to translate into ProblemDetails.</summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }
}
