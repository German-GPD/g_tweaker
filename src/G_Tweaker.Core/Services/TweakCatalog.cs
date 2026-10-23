using G_Tweaker.Core.Models;
using Microsoft.Win32;

namespace G_Tweaker.Core.Services;

public static class TweakCatalog
{
    public static IReadOnlyList<TweakDefinition> All { get; } =
    [
        new("privacy.diagnostic-data", "Limit diagnostic data to required",
            "Requests the required diagnostic-data level where supported by the Windows edition and policy.",
            TweakCategory.Privacy, RegistryHive.LocalMachine,
            @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
            "AllowTelemetry", RegistryValueKind.DWord, 1, 3,
            "Requires administrator rights. This does not disable all Windows diagnostics; edition and organisation policies may override it."),

        new("privacy.advertising-id", "Disable advertising ID",
            "Turns off the advertising identifier for the current Windows user.",
            TweakCategory.Privacy, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
            "Enabled", RegistryValueKind.DWord, 0, 1),

        new("input.sticky-keys", "Disable Sticky Keys shortcut",
            "Changes the current user's Sticky Keys accessibility flags. Verify behaviour after applying.",
            TweakCategory.Input, RegistryHive.CurrentUser,
            @"Control Panel\Accessibility\StickyKeys",
            "Flags", RegistryValueKind.String, "506", "510",
            "Accessibility settings can vary by Windows version and user preference."),

        new("explorer.file-extensions", "Show file extensions",
            "Shows known file extensions in File Explorer.",
            TweakCategory.Explorer, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "HideFileExt", RegistryValueKind.DWord, 0, 1),

        new("gaming.game-mode", "Enable Windows Game Mode",
            "Enables Game Mode for the current user. Performance impact varies by system.",
            TweakCategory.Gaming, RegistryHive.CurrentUser,
            @"Software\Microsoft\GameBar",
            "AutoGameModeEnabled", RegistryValueKind.DWord, 1, 0),

        new("performance.transparency", "Reduce transparency effects",
            "Disables Windows transparency effects for the current user. This is mainly a visual preference.",
            TweakCategory.Performance, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "EnableTransparency", RegistryValueKind.DWord, 0, 1)
    ];
}
