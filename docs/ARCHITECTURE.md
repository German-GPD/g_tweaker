# G_Tweaker architecture

The active migration branch is built with C#/.NET 10 and WinUI 3.

- **G_Tweaker.App**: WinUI 3 interface, navigation, search, confirmation dialogs and profile preview.
- **G_Tweaker.Core**: tweak catalogue, registry access, state inspection and rollback orchestration.
- **HistoryStore**: writes typed registry snapshots to %LOCALAPPDATA%\G_Tweaker\history.json.
- **Tests**: validate catalogue metadata and safety invariants.

## Safety rules

1. Changes require a visible confirmation.
2. The prior registry value is captured before it is changed.
3. If persisting history fails after a write, the engine attempts to restore the previous value.
4. HKLM changes require the app to already be running elevated; elevation is never silent.
5. No security service is disabled automatically.
6. Activation features query status and open official Settings only.

The branch is an early implementation. It must pass the Windows CI build and manual VM testing before being treated as a release.
