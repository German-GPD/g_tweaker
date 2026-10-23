# G_Tweaker

G_Tweaker is a Windows 10/11 tuning utility built with **C#**, **.NET 10** and **WinUI 3**.

## Features

- Fluent-style Windows interface with navigation, searchable categories and status labels
- 17 reversible registry tweaks across Privacy, Performance, Input, File Explorer and Gaming
- Previewable profiles for privacy, gaming and everyday usability
- Per-change snapshots and persistent history under `%LOCALAPPDATA%\G_Tweaker\history.json`
- Roll back the latest recorded change to its previous registry value
- Windows activation status query and shortcut to the official Activation settings
- Windows CI for tests and x64 publish artifacts

## Build requirements

- Windows 10 version 1809 or newer (Windows 11 recommended)
- .NET 10 SDK
- Visual Studio with WinUI 3 / Windows App SDK support and Windows SDK

## Build and run

Open `G_Tweaker.sln` in Visual Studio, select `G_Tweaker.App`, and run.

Or in PowerShell:

```powershell
dotnet restore G_Tweaker.sln
dotnet test tests/G_Tweaker.Tests/G_Tweaker.Tests.csproj -c Release
dotnet build src/G_Tweaker.App/G_Tweaker.App.csproj -c Release
```

## Publish for x64

```powershell
dotnet publish src/G_Tweaker.App/G_Tweaker.App.csproj -c Release -r win-x64 --self-contained true -o artifacts/G_Tweaker-win-x64
```

The output folder contains the executable and required runtime files; this is not yet a single-file installer.

## Safety and limitations

- Read the description and warning before applying any setting.
- Registry values are recorded before changes so the latest operation can be rolled back.
- Machine-wide policy tweaks require administrator rights. Run elevated only when you intend to apply them.
- Windows editions and organisation policies may ignore or override some policy values.
- The diagnostic-data tweak requests the required level where supported; it does **not** disable all Windows telemetry.
- Some settings may need sign-out, Explorer restart or a reboot before their effects appear.
- The activation page checks status and opens Microsoft's own Settings page; it does not bypass licensing.
- The catalogue deliberately avoids destructive service-disabling and security-weakening tweaks.

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
