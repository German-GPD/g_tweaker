# Security model

- No tweak is applied without a user click and confirmation.
- The previous registry value is recorded before each change in %LOCALAPPDATA%\G_Tweaker\history.json.
- HKLM writes require the app to already be elevated; the app does not silently self-elevate.
- Test on a disposable Windows virtual machine before relying on system tweaks.
- Rollback restores the recorded previous value for the latest operation. It cannot guarantee other software or Windows policy has not changed the setting since.
- Activation support only queries status and opens official Windows activation settings. It does not bypass licensing.
- Diagnostic-data policies vary by Windows edition and organisation management.
