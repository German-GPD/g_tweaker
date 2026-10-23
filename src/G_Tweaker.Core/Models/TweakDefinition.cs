namespace G_Tweaker.Core.Models;

public enum TweakCategory
{
    Privacy,
    Performance,
    Input,
    Explorer,
    Gaming
}

public sealed record TweakDefinition(
    string Id,
    string Title,
    string Description,
    TweakCategory Category,
    RegistryHive Hive,
    string SubKey,
    string ValueName,
    RegistryValueKind ValueKind,
    object EnabledValue,
    object? DisabledValue,
    string? Warning = null);

public sealed record RegistrySnapshot(
    bool Exists,
    object? Value,
    RegistryValueKind? Kind);

public sealed record TweakHistoryEntry(
    Guid Id,
    DateTimeOffset Timestamp,
    string TweakId,
    string Title,
    RegistryHive Hive,
    string SubKey,
    string ValueName,
    RegistrySnapshot Previous,
    object WrittenValue,
    RegistryValueKind WrittenKind);
