namespace HaloUI.Tests.E2E;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PlaywrightE2ECollection : ICollectionFixture<PlaywrightEnvironmentFixture>
{
    public const string Name = "PlaywrightE2E";
}
