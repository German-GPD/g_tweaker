using G_Tweaker.Core.Models;
using G_Tweaker.Core.Services;
using Microsoft.Win32;
using Xunit;

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
    public void CatalogueHasUsefulCoverage()
    {
        Assert.True(TweakCatalog.All.Count >= 15, "The catalogue should cover the supported main categories.");
        Assert.Contains(TweakCatalog.All, t => t.Category == TweakCategory.Privacy);
        Assert.Contains(TweakCatalog.All, t => t.Category == TweakCategory.Performance);
        Assert.Contains(TweakCatalog.All, t => t.Category == TweakCategory.Input);
        Assert.Contains(TweakCatalog.All, t => t.Category == TweakCategory.Explorer);
        Assert.Contains(TweakCatalog.All, t => t.Category == TweakCategory.Gaming);
    }

    [Fact]
    public void EveryTweakHasRequiredMetadataAndExplicitValues()
    {
        Assert.NotEmpty(TweakCatalog.All);
        Assert.All(TweakCatalog.All, tweak =>
        {
            Assert.False(string.IsNullOrWhiteSpace(tweak.Id));
            Assert.False(string.IsNullOrWhiteSpace(tweak.Title));
            Assert.False(string.IsNullOrWhiteSpace(tweak.Description));
            Assert.False(string.IsNullOrWhiteSpace(tweak.SubKey));
            Assert.False(string.IsNullOrWhiteSpace(tweak.ValueName));
            Assert.NotNull(tweak.EnabledValue);
            Assert.NotNull(tweak.DisabledValue);
            Assert.Contains(tweak.ValueKind, new[] { RegistryValueKind.DWord, RegistryValueKind.String });
            if (tweak.Hive == RegistryHive.LocalMachine)
                Assert.False(string.IsNullOrWhiteSpace(tweak.Warning), $"System-wide tweak {tweak.Id} needs a warning.");
        });
    }

    [Fact]
    public void DiagnosticDataTweakDoesNotClaimToDisableAllTelemetry()
    {
        var tweak = Assert.Single(TweakCatalog.All.Where(t => t.Id == "privacy.diagnostic-data"));
        Assert.Equal(RegistryHive.LocalMachine, tweak.Hive);
        Assert.Contains("does not disable all", tweak.Warning!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SensitiveExplorerAndCaptureTweaksExplainTheirImpact()
    {
        var protectedFiles = Assert.Single(TweakCatalog.All.Where(t => t.Id == "explorer.super-hidden-files"));
        Assert.False(string.IsNullOrWhiteSpace(protectedFiles.Warning));
        var capture = Assert.Single(TweakCatalog.All.Where(t => t.Id == "gaming.app-capture"));
        Assert.False(string.IsNullOrWhiteSpace(capture.Warning));
    }

    [Fact]
    public void EveryTweakHasBothOnAndOffValues()
    {
        Assert.All(TweakCatalog.All, tweak =>
        {
            Assert.NotNull(tweak.EnabledValue);
            Assert.NotNull(tweak.DisabledValue);
        });
    }
}
