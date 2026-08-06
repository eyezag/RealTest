namespace RealTest.Application.Common.Security;

/// <summary>Custom JWT claim types shared between token issuance (Infrastructure) and authorization (Application/Api).</summary>
public static class AppClaimTypes
{
    public const string AccountStatus = "account_status";

    public const string DisplayName = "display_name";
}
