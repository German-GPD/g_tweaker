using System.Text.Json;
using Microsoft.Win32;
using G_Tweaker.Core.Models;

namespace G_Tweaker.Core.Services;

public sealed class HistoryStore
{
    private readonly string _filePath;

    public HistoryStore(string? filePath = null)
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _filePath = filePath ?? Path.Combine(local, "G_Tweaker", "history.json");
    }

    public IReadOnlyList<TweakHistoryEntry> ReadAll()
    {
        if (!File.Exists(_filePath)) return [];
        try
        {
            var rows = JsonSerializer.Deserialize<List<HistoryRow>>(File.ReadAllText(_filePath)) ?? [];
            return rows.Select(ToEntry).ToArray();
        }
        catch (Exception ex) when (ex is IOException or JsonException or FormatException or InvalidOperationException)
        {
            throw new InvalidDataException("The change history is damaged and was not loaded.", ex);
        }
    }

    public void Add(TweakHistoryEntry entry)
    {
        var entries = ReadAll().ToList();
        entries.Add(entry);
        Save(entries);
    }

    public void RemoveLatest()
    {
        var entries = ReadAll().ToList();
        if (entries.Count == 0) return;
        entries.RemoveAt(entries.Count - 1);
        Save(entries);
    }

    public void Save(IEnumerable<TweakHistoryEntry> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var rows = entries.Select(FromEntry).ToList();
        var json = JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true });
        var temp = _filePath + ".tmp";
        File.WriteAllText(temp, json);
        File.Move(temp, _filePath, overwrite: true);
    }

    private static HistoryRow FromEntry(TweakHistoryEntry e) => new()
    {
        Id = e.Id,
        Timestamp = e.Timestamp,
        TweakId = e.TweakId,
        Title = e.Title,
        Hive = (int)e.Hive,
        SubKey = e.SubKey,
        ValueName = e.ValueName,
        PreviousExists = e.Previous.Exists,
        PreviousValue = e.Previous.Exists ? Encode(e.Previous.Value!, e.Previous.Kind!.Value) : null,
        PreviousKind = e.Previous.Kind is null ? null : (int)e.Previous.Kind.Value,
        WrittenValue = Encode(e.WrittenValue, e.WrittenKind),
        WrittenKind = (int)e.WrittenKind
    };

    private static TweakHistoryEntry ToEntry(HistoryRow r)
    {
        var writtenKind = (RegistryValueKind)r.WrittenKind;
        var previousKind = r.PreviousKind is null ? null : (RegistryValueKind?)r.PreviousKind.Value;
        var previous = new RegistrySnapshot(
            r.PreviousExists,
            r.PreviousExists ? Decode(r.PreviousValue!, previousKind!.Value) : null,
            previousKind);
        return new TweakHistoryEntry(r.Id, r.Timestamp, r.TweakId, r.Title,
            (RegistryHive)r.Hive, r.SubKey, r.ValueName, previous,
            Decode(r.WrittenValue, writtenKind), writtenKind);
    }

    private static string Encode(object value, RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.Binary => Convert.ToBase64String((byte[])value),
        RegistryValueKind.MultiString => JsonSerializer.Serialize((string[])value),
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? ""
    };

    private static object Decode(string value, RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.DWord => int.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
        RegistryValueKind.QWord => long.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
        RegistryValueKind.Binary => Convert.FromBase64String(value),
        RegistryValueKind.MultiString => JsonSerializer.Deserialize<string[]>(value) ?? [],
        RegistryValueKind.String or RegistryValueKind.ExpandString => value,
        _ => throw new InvalidDataException($"Unsupported registry value kind: {kind}")
    };

    private sealed class HistoryRow
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string TweakId { get; set; } = "";
        public string Title { get; set; } = "";
        public int Hive { get; set; }
        public string SubKey { get; set; } = "";
        public string ValueName { get; set; } = "";
        public bool PreviousExists { get; set; }
        public string? PreviousValue { get; set; }
        public int? PreviousKind { get; set; }
        public string WrittenValue { get; set; } = "";
        public int WrittenKind { get; set; }
    }
}
