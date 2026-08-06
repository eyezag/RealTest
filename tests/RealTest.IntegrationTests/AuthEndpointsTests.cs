using System.Net;
using System.Net.Http.Json;
using RealTest.Contracts.Auth;
using RealTest.Contracts.Common;
using Shouldly;

namespace RealTest.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class AuthEndpointsTests(ApiWebApplicationFactory factory)
{
    private const string BaseRoute = "/api/v1/auth";

    private HttpClient CreateClient() => factory.CreateClient();

    private static RegisterRequest BuildRegisterRequest(UserType userType, string emailPrefix) => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Email = TestData.UniqueEmail(emailPrefix),
        Password = TestData.ValidPassword,
        ConfirmPassword = TestData.ValidPassword,
        UserType = userType,
        AcceptTerms = true
    };

    [Fact]
    public async Task Register_WithTesterUserType_Succeeds()
    {
        var client = CreateClient();
        var request = BuildRegisterRequest(UserType.Tester, "tester");

        var response = await client.PostAsJsonAsync($"{BaseRoute}/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.ShouldNotBeNull();
        body.User.UserType.ShouldBe(UserType.Tester);
        body.User.AccountStatus.ShouldBe(AccountStatus.PendingVerification);
        response.Headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue();
        cookies!.Any(c => c.StartsWith("realtest_refresh_token=", StringComparison.Ordinal)).ShouldBeTrue();
    }

    [Fact]
    public async Task Register_WithCompanyUserType_Succeeds()
    {
        var client = CreateClient();
        var request = BuildRegisterRequest(UserType.CompanyUser, "company");

        var response = await client.PostAsJsonAsync($"{BaseRoute}/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.User.UserType.ShouldBe(UserType.CompanyUser);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = CreateClient();
        var request = BuildRegisterRequest(UserType.Tester, "dup");

        var first = await client.PostAsJsonAsync($"{BaseRoute}/register", request);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        var second = await client.PostAsJsonAsync($"{BaseRoute}/register", request);

        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithAdministratorUserType_IsRejected()
    {
        var client = CreateClient();
        var request = BuildRegisterRequest(UserType.Administrator, "admin-hopeful");

        var response = await client.PostAsJsonAsync($"{BaseRoute}/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Succeeds()
    {
        var client = CreateClient();
        var registerRequest = BuildRegisterRequest(UserType.Tester, "login-ok");
        await client.PostAsJsonAsync($"{BaseRoute}/register", registerRequest);

        var response = await client.PostAsJsonAsync($"{BaseRoute}/login", new LoginRequest
        {
            Email = registerRequest.Email,
            Password = TestData.ValidPassword
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.User.Email.ShouldBe(registerRequest.Email);
        body.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var client = CreateClient();
        var registerRequest = BuildRegisterRequest(UserType.Tester, "login-bad");
        await client.PostAsJsonAsync($"{BaseRoute}/register", registerRequest);

        var response = await client.PostAsJsonAsync($"{BaseRoute}/login", new LoginRequest
        {
            Email = registerRequest.Email,
            Password = "TotallyWrongPassword123!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync($"{BaseRoute}/login", new LoginRequest
        {
            Email = TestData.UniqueEmail("unknown"),
            Password = TestData.ValidPassword
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidCookie_RotatesTokenAndReturnsNewAccessToken()
    {
        var client = CreateClient();
        var registerRequest = BuildRegisterRequest(UserType.Tester, "refresh-ok");
        var registerResponse = await client.PostAsJsonAsync($"{BaseRoute}/register", registerRequest);
        var originalAuth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshResponse = await client.PostAsync($"{BaseRoute}/refresh", content: null);

        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var refreshedAuth = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        refreshedAuth!.AccessToken.ShouldNotBe(originalAuth!.AccessToken);
        refreshedAuth.User.Id.ShouldBe(originalAuth.User.Id);
    }

    [Fact]
    public async Task Refresh_AfterTokenWasAlreadyRotated_RejectsReuseOfOldToken()
    {
        var registerClient = CreateClient();
        var registerRequest = BuildRegisterRequest(UserType.Tester, "reuse");
        var registerResponse = await registerClient.PostAsJsonAsync($"{BaseRoute}/register", registerRequest);
        var originalCookieValue = ExtractRefreshTokenCookieValue(registerResponse);
        originalCookieValue.ShouldNotBeNullOrWhiteSpace();

        // Rotate it once (legitimately) using the same client (its cookie container carries the token).
        var rotateResponse = await registerClient.PostAsync($"{BaseRoute}/refresh", content: null);
        rotateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Replay the pre-rotation token on a fresh client with the old cookie value re-attached.
        using var replayClient = factory.CreateClient();
        replayClient.DefaultRequestHeaders.Add("Cookie", $"realtest_refresh_token={originalCookieValue}");

        var replayResponse = await replayClient.PostAsync($"{BaseRoute}/refresh", content: null);

        replayResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken_SoSubsequentRefreshFails()
    {
        var client = CreateClient();
        var registerRequest = BuildRegisterRequest(UserType.Tester, "logout");
        await client.PostAsJsonAsync($"{BaseRoute}/register", registerRequest);

        var logoutResponse = await client.PostAsync($"{BaseRoute}/logout", content: null);
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var refreshResponse = await client.PostAsync($"{BaseRoute}/refresh", content: null);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidAccessToken_ReturnsProfile()
    {
        var client = CreateClient();
        var registerRequest = BuildRegisterRequest(UserType.Tester, "me");
        var registerResponse = await client.PostAsJsonAsync($"{BaseRoute}/register", registerRequest);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var response = await client.GetAsync($"{BaseRoute}/me");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<UserSummaryResponse>();
        profile!.Email.ShouldBe(registerRequest.Email);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAccessToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"{BaseRoute}/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static string? ExtractRefreshTokenCookieValue(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return null;
        }

        var cookie = cookies.FirstOrDefault(c => c.StartsWith("realtest_refresh_token=", StringComparison.Ordinal));
        if (cookie is null)
        {
            return null;
        }

        var valueAndAttributes = cookie["realtest_refresh_token=".Length..];
        return valueAndAttributes[..valueAndAttributes.IndexOf(';')];
    }
}
