using G_Tweaker.Core.Models;
using Microsoft.Win32;

namespace G_Tweaker.Core.Services;

/// <summary>
/// Registry-only tweaks with explicit on/off values. Each entry is intentionally narrow,
/// inspectable and reversible; avoid destructive service or security changes here.
/// </summary>
public static class TweakCatalog
{
    private const string AdminPolicyWarning = "Requires administrator rights. Windows edition or organisation policy may override this setting.";

    public static IReadOnlyList<TweakDefinition> All { get; } =
    [
        // Privacy
        new("privacy.diagnostic-data", "Limit diagnostic data to required",
            "Requests the required diagnostic-data level where supported by the Windows edition and policy.",
            TweakCategory.Privacy, RegistryHive.LocalMachine,
            @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
            "AllowTelemetry", RegistryValueKind.DWord, 1, 3,
            "This does not disable all Windows diagnostics. Requires administrator rights; edition and organisation policies may override it."),
        new("privacy.advertising-id", "Disable advertising ID",
            "Turns off the advertising identifier for the current Windows user.",
            TweakCategory.Privacy, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
            "Enabled", RegistryValueKind.DWord, 0, 1),
        new("privacy.tailored-experiences", "Disable tailored experiences",
            "Stops Windows from using diagnostic data to provide tailored tips and recommendations where the policy is supported.",
            TweakCategory.Privacy, RegistryHive.LocalMachine,
            @"SOFTWARE\Policies\Microsoft\Windows\CloudContent",
            "DisableTailoredExperiencesWithDiagnosticData", RegistryValueKind.DWord, 1, 0, AdminPolicyWarning),
        new("privacy.consumer-features", "Reduce suggested consumer content",
            "Disables Windows consumer experience features on supported editions; it does not remove installed apps.",
            TweakCategory.Privacy, RegistryHive.LocalMachine,
            @"SOFTWARE\Policies\Microsoft\Windows\CloudContent",
            "DisableWindowsConsumerFeatures", RegistryValueKind.DWord, 1, 0, AdminPolicyWarning),
        new("privacy.activity-publishing", "Disable activity history publishing",
            "Prevents publishing user activities to activity history where the policy is supported.",
            TweakCategory.Privacy, RegistryHive.LocalMachine,
            @"SOFTWARE\Policies\Microsoft\Windows\System",
            "PublishUserActivities", RegistryValueKind.DWord, 0, 1, AdminPolicyWarning),
        new("privacy.activity-upload", "Disable activity history upload",
            "Prevents uploading user activities to Microsoft where the policy is supported.",
            TweakCategory.Privacy, RegistryHive.LocalMachine,
            @"SOFTWARE\Policies\Microsoft\Windows\System",
            "UploadUserActivities", RegistryValueKind.DWord, 0, 1, AdminPolicyWarning),
        new("privacy.start-tracking", "Disable Start menu app tracking",
            "Stops Windows tracking launched apps for personalised Start menu suggestions.",
            TweakCategory.Privacy, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "Start_TrackProgs", RegistryValueKind.DWord, 0, 1),

        // Performance and visual effects
        new("performance.transparency", "Reduce transparency effects",
            "Disables Windows transparency effects for the current user. This is mainly a visual preference.",
            TweakCategory.Performance, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "EnableTransparency", RegistryValueKind.DWord, 0, 1),
        new("performance.visual-effects", "Prefer performance over visual effects",
            "Selects the Windows visual-effects preset for best performance. Some animations and visual polish may be reduced.",
            TweakCategory.Performance, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects",
            "VisualFXSetting", RegistryValueKind.DWord, 2, 0,
            "This changes the visual-effects preset; it does not guarantee higher FPS in games."),

        // Input and accessibility
        new("input.sticky-keys", "Disable Sticky Keys shortcut",
            "Changes the current user's Sticky Keys flags to disable the activation shortcut. Verify behaviour after applying.",
            TweakCategory.Input, RegistryHive.CurrentUser,
            @"Control Panel\Accessibility\StickyKeys",
            "Flags", RegistryValueKind.String, "506", "510",
            "Accessibility flags can vary by Windows version and user preference."),

        // File Explorer
        new("explorer.file-extensions", "Show file extensions",
            "Shows known file extensions in File Explorer.",
            TweakCategory.Explorer, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "HideFileExt", RegistryValueKind.DWord, 0, 1),
        new("explorer.hidden-files", "Show hidden files",
            "Shows files and folders marked as hidden in File Explorer.",
            TweakCategory.Explorer, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "Hidden", RegistryValueKind.DWord, 1, 2),
        new("explorer.super-hidden-files", "Show protected operating-system files",
            "Makes protected operating-system files visible in File Explorer.",
            TweakCategory.Explorer, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "ShowSuperHidden", RegistryValueKind.DWord, 1, 0,
            "System files are normally hidden to prevent accidental changes or deletion."),
        new("explorer.open-this-pc", "Open File Explorer to This PC",
            "Makes File Explorer open to This PC instead of Home/Quick access where supported.",
            TweakCategory.Explorer, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "LaunchTo", RegistryValueKind.DWord, 1, 2),

        // Gaming
        new("gaming.game-mode", "Enable Windows Game Mode",
            "Enables Game Mode for the current user. Performance impact varies by system.",
            TweakCategory.Gaming, RegistryHive.CurrentUser,
            @"Software\Microsoft\GameBar",
            "AutoGameModeEnabled", RegistryValueKind.DWord, 1, 0),
        new("gaming.game-dvr", "Disable background Game DVR capture",
            "Disables the current user's background capture setting. Manual capture behaviour may also be affected.",
            TweakCategory.Gaming, RegistryHive.CurrentUser,
            @"System\GameConfigStore",
            "GameDVR_Enabled", RegistryValueKind.DWord, 0, 1,
            "May disable background recording and affect capture features used by Xbox Game Bar."),
        new("gaming.app-capture", "Disable app capture",
            "Turns off the current user's Windows app-capture setting.",
            TweakCategory.Gaming, RegistryHive.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\GameDVR",
            "AppCaptureEnabled", RegistryValueKind.DWord, 0, 1,
            "May affect Xbox Game Bar recording and capture features.")
    ];
}
