namespace RealTest.IntegrationTests;

internal static class TestData
{
    public const string ValidPassword = "P@ssw0rd123!";

    public static string UniqueEmail(string prefix) => $"{prefix}-{Guid.NewGuid():N}@example.test";
}
