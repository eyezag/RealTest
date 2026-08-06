namespace RealTest.IntegrationTests;

/// <summary>
/// All integration test classes share this collection so they share one Postgres container
/// (spinning one up per class would be slow) and therefore run sequentially against it,
/// avoiding cross-test data races.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<ApiWebApplicationFactory>;
