namespace RealTest.Application.Common.Exceptions;

/// <summary>Thrown when request validation fails. Carries field-level error messages keyed by camelCase field name.</summary>
public sealed class ValidationAppException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.AsReadOnly() is IReadOnlyDictionary<string, string[]> readOnly
            ? readOnly
            : new Dictionary<string, string[]>(errors);
    }

    public ValidationAppException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }
}
