using RealTest.Application.Authentication.Validators;
using RealTest.Contracts.Auth;
using RealTest.Contracts.Common;
using Shouldly;
using Xunit;

namespace RealTest.UnitTests.Authentication;

public sealed class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest ValidRequest() => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Email = "ada@example.com",
        Password = "P@ssw0rd123!",
        ConfirmPassword = "P@ssw0rd123!",
        UserType = UserType.Tester,
        AcceptTerms = true
    };

    [Fact]
    public void Validate_WithFullyValidRequest_Passes()
    {
        var result = _validator.Validate(ValidRequest());

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(UserType.Tester)]
    [InlineData(UserType.CompanyUser)]
    public void Validate_AllowsOnlyPublicUserTypes(UserType userType)
    {
        var baseline = ValidRequest();
        var request = new RegisterRequest
        {
            FirstName = baseline.FirstName,
            LastName = baseline.LastName,
            Email = baseline.Email,
            Password = baseline.Password,
            ConfirmPassword = baseline.ConfirmPassword,
            UserType = userType,
            AcceptTerms = baseline.AcceptTerms
        };

        var result = _validator.Validate(request);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_RejectsAdministratorUserType()
    {
        var request = new RegisterRequest
        {
            FirstName = "Evil",
            LastName = "Admin",
            Email = "admin@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            UserType = UserType.Administrator,
            AcceptTerms = true
        };

        var result = _validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RegisterRequest.UserType));
    }

    [Fact]
    public void Validate_RequiresEmail()
    {
        var baseline = ValidRequest();
        var invalid = new RegisterRequest
        {
            FirstName = baseline.FirstName,
            LastName = baseline.LastName,
            Email = string.Empty,
            Password = baseline.Password,
            ConfirmPassword = baseline.ConfirmPassword,
            UserType = baseline.UserType,
            AcceptTerms = baseline.AcceptTerms
        };

        var result = _validator.Validate(invalid);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("nouppercase123!")]
    [InlineData("NOLOWERCASE123!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecialChars123")]
    public void Validate_RejectsPasswordsThatViolatePolicy(string weakPassword)
    {
        var request = new RegisterRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Password = weakPassword,
            ConfirmPassword = weakPassword,
            UserType = UserType.Tester,
            AcceptTerms = true
        };

        var result = _validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Validate_RejectsMismatchedConfirmPassword()
    {
        var request = new RegisterRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "SomethingDifferent123!",
            UserType = UserType.Tester,
            AcceptTerms = true
        };

        var result = _validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RegisterRequest.ConfirmPassword));
    }

    [Fact]
    public void Validate_RequiresTermsAcceptance()
    {
        var request = new RegisterRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            UserType = UserType.Tester,
            AcceptTerms = false
        };

        var result = _validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RegisterRequest.AcceptTerms));
    }
}
