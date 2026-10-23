using System.Security.Principal;
using Microsoft.Win32;
using G_Tweaker.Core.Models;

namespace G_Tweaker.Core.Services;

public sealed class RegistryTweakService
{
    public RegistrySnapshot Read(TweakDefinition tweak)
    {
        using var key = OpenBaseKey(tweak.Hive).OpenSubKey(tweak.SubKey, writable: false);
        if (key is null)
            return new RegistrySnapshot(false, null, null);

        try
        {
            var value = key.GetValue(tweak.ValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (value is null)
                return new RegistrySnapshot(false, null, null);

            return new RegistrySnapshot(true, value, key.GetValueKind(tweak.ValueName));
        }
        catch (ArgumentException)
        {
            return new RegistrySnapshot(false, null, null);
        }
    }

    public string GetState(TweakDefinition tweak)
    {
        var snapshot = Read(tweak);
        if (!snapshot.Exists) return "Not configured";
        if (Equals(snapshot.Value, tweak.EnabledValue)) return "Enabled";
        if (tweak.DisabledValue is not null && Equals(snapshot.Value, tweak.DisabledValue)) return "Disabled";
        return $"Custom ({snapshot.Value})";
    }

    public TweakHistoryEntry Apply(TweakDefinition tweak, bool enable)
    {
        var value = enable ? tweak.EnabledValue : tweak.DisabledValue;
        if (value is null)
            throw new InvalidOperationException("This tweak has no defined value for that operation.");

        if (tweak.Hive == RegistryHive.LocalMachine && !IsAdministrator())
            throw new UnauthorizedAccessException("Administrator rights are required. Restart G_Tweaker elevated.");

        var previous = Read(tweak);
        using (var key = OpenBaseKey(tweak.Hive).CreateSubKey(tweak.SubKey, writable: true)
            ?? throw new InvalidOperationException("Windows could not open this registry key for writing."))
        {
            key.SetValue(tweak.ValueName, value, tweak.ValueKind);
        }

        return new TweakHistoryEntry(
            Guid.NewGuid(), DateTimeOffset.UtcNow, tweak.Id, tweak.Title,
            tweak.Hive, tweak.SubKey, tweak.ValueName, previous, value, tweak.ValueKind);
    }

    public void Rollback(TweakHistoryEntry entry)
    {
        if (entry.Hive == RegistryHive.LocalMachine && !IsAdministrator())
            throw new UnauthorizedAccessException("Administrator rights are required to roll back this change.");

        using var key = OpenBaseKey(entry.Hive).CreateSubKey(entry.SubKey, writable: true)
            ?? throw new InvalidOperationException("Windows could not open the registry key for rollback.");

        if (entry.Previous.Exists)
        {
            key.SetValue(entry.ValueName, entry.Previous.Value!, entry.Previous.Kind!.Value);
        }
        else
        {
            try { key.DeleteValue(entry.ValueName, throwOnMissingValue: false); }
            catch (UnauthorizedAccessException) { throw; }
        }
    }

    private static RegistryKey OpenBaseKey(RegistryHive hive) =>
        RegistryKey.OpenBaseKey(hive, RegistryView.Default);

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
