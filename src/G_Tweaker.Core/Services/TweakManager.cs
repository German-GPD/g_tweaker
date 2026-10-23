using G_Tweaker.Core.Models;

namespace G_Tweaker.Core.Services;

public sealed class TweakManager
{
    private readonly RegistryTweakService _registry;
    private readonly HistoryStore _history;

    public TweakManager(RegistryTweakService? registry = null, HistoryStore? history = null)
    {
        _registry = registry ?? new RegistryTweakService();
        _history = history ?? new HistoryStore();
    }

    public IReadOnlyList<TweakHistoryEntry> History => _history.ReadAll().Reverse().ToArray();

    public string GetState(TweakDefinition tweak) => _registry.GetState(tweak);

    public void Apply(TweakDefinition tweak, bool enabled)
    {
        var entry = _registry.Apply(tweak, enabled);
        try
        {
            _history.Add(entry);
        }
        catch
        {
            // If persistence fails, attempt to put the registry back before reporting failure.
            _registry.Rollback(entry);
            throw;
        }
    }

    public string RollbackLatest()
    {
        var entries = _history.ReadAll().ToList();
        if (entries.Count == 0) throw new InvalidOperationException("There are no changes to roll back.");
        var latest = entries[^1];
        _registry.Rollback(latest);
        _history.RemoveLatest();
        return latest.Title;
    }
}
