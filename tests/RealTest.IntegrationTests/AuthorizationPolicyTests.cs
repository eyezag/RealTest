using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RealTest.Contracts.Auth;
using RealTest.Contracts.Common;
using Shouldly;

namespace RealTest.IntegrationTests;

/// <summary>Proves the TesterOnly/CompanyUserOnly/AdministratorOnly policies actually gate access.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AuthorizationPolicyTests(ApiWebApplicationFactory factory)
{
    private const string AuthRoute = "/api/v1/auth";
    private const string SamplesRoute = "/api/v1/samples";

    private async Task<HttpClient> CreateAuthenticatedClientAsync(UserType userType, string emailPrefix)
    {
        var client = factory.CreateClient();
        var email = TestData.UniqueEmail(emailPrefix);

        var registerResponse = await client.PostAsJsonAsync($"{AuthRoute}/register", new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = TestData.ValidPassword,
            ConfirmPassword = TestData.ValidPassword,
            UserType = userType,
            AcceptTerms = true
        });

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return client;
    }

    [Fact]
    public async Task TesterOnlyEndpoint_AllowsTester_ButRejectsCompanyUser()
    {
        var testerClient = await CreateAuthenticatedClientAsync(UserType.Tester, "policy-tester");
        var companyClient = await CreateAuthenticatedClientAsync(UserType.CompanyUser, "policy-company-a");

        var testerResponse = await testerClient.GetAsync($"{SamplesRoute}/tester-only");
        var companyResponse = await companyClient.GetAsync($"{SamplesRoute}/tester-only");

        testerResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        companyResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CompanyUserOnlyEndpoint_AllowsCompanyUser_ButRejectsTester()
    {
        var companyClient = await CreateAuthenticatedClientAsync(UserType.CompanyUser, "policy-company-b");
        var testerClient = await CreateAuthenticatedClientAsync(UserType.Tester, "policy-tester-b");

        var companyResponse = await companyClient.GetAsync($"{SamplesRoute}/company-only");
        var testerResponse = await testerClient.GetAsync($"{SamplesRoute}/company-only");

        companyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        testerResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdministratorOnlyEndpoint_RejectsRegularUsers()
    {
        // Administrator accounts can't be created via public registration, so there is no
        // legitimate way to obtain an Administrator token here — which is exactly the point:
        // a Tester token must never satisfy the AdministratorOnly policy.
        var testerClient = await CreateAuthenticatedClientAsync(UserType.Tester, "policy-admin-check");

        var response = await testerClient.GetAsync($"{SamplesRoute}/admin-only");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var anonymousClient = factory.CreateClient();

        var response = await anonymousClient.GetAsync($"{SamplesRoute}/tester-only");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
