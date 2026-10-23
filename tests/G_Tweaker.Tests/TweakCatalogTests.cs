using G_Tweaker.Core.Models;
using G_Tweaker.Core.Services;

namespace G_Tweaker.Tests;

public sealed class TweakCatalogTests
{
    [Fact]
    public void TweakIdsAreUnique()
    {
        var ids = TweakCatalog.All.Select(t => t.Id).ToArray();
        Assert.Equal(ids.Length, ids.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void EveryTweakHasRequiredMetadata()
    {
        Assert.NotEmpty(TweakCatalog.All);
        Assert.All(TweakCatalog.All, tweak =>
        {
            Assert.False(string.IsNullOrWhiteSpace(tweak.Id));
            Assert.False(string.IsNullOrWhiteSpace(tweak.Title));
            Assert.False(string.IsNullOrWhiteSpace(tweak.Description));
            Assert.False(string.IsNullOrWhiteSpace(tweak.SubKey));
            Assert.False(string.IsNullOrWhiteSpace(tweak.ValueName));
        });
    }

    [Fact]
    public void EveryTweakUsesAnExplicitSupportedValueKind()
    {
        Assert.All(TweakCatalog.All, tweak =>
            Assert.Contains(tweak.ValueKind, new[] { Microsoft.Win32.RegistryValueKind.DWord, Microsoft.Win32.RegistryValueKind.String }));
    }

    [Fact]
    public void SystemPolicyTweaksWarnAboutEditionAndPermissions()
    {
        var policy = Assert.Single(TweakCatalog.All.Where(t => t.Id == "privacy.diagnostic-data"));
        Assert.Equal(Microsoft.Win32.RegistryHive.LocalMachine, policy.Hive);
        Assert.False(string.IsNullOrWhiteSpace(policy.Warning));
    }
}
