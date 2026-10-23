# G_Tweaker

G_Tweaker is a Windows 10/11 configuration utility being rebuilt with **C#**, **.NET 10 LTS** and **WinUI 3**.

The active C# migration is in the `csharp-winui-migration` branch. The existing Python prototype remains on `main` until the migration has been built and tested.

## Current C# branch

- Native WinUI 3 navigation and Fluent-style cards
- Searchable tweak catalogue and categories
- Windows registry state inspection and per-change snapshots
- Latest-change rollback
- Previewable starter profiles
- Windows activation status query and official Settings shortcut
- Windows CI build and catalogue tests

## Requirements

- Windows 10 version 1809 or newer; Windows 11 recommended
- Visual Studio 2022/2026 with **.NET desktop development** and **Windows App SDK / WinUI** components
- .NET 10 SDK
- Windows SDK

## Open and run

Open `G_Tweaker.sln` in Visual Studio, select the `G_Tweaker.App` project, then run.

Or use PowerShell:

    dotnet restore G_Tweaker.sln
    dotnet build src/G_Tweaker.App/G_Tweaker.App.csproj -c Debug

## Publish x64

    dotnet publish src/G_Tweaker.App/G_Tweaker.App.csproj -c Release -r win-x64 --self-contained true -o artifacts/G_Tweaker-win-x64

The output directory contains `G_Tweaker.App.exe` and its required runtime files. It is not yet a single-file installer.

## Safety

- Review every tweak before applying it.
- Registry values are saved before changes and stored under `%LOCALAPPDATA%\G_Tweaker\history.json`.
- System-wide registry changes require running the app as administrator.
- Some policies vary by Windows edition and organisation management.
- Activation support checks status and opens official Windows settings; it does not bypass licensing.
- The C# migration is early development and must pass Windows CI and manual VM testing before release.

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
