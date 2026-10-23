# G_Tweaker

A Windows 10/11 tweaking utility with a Windows 11-inspired dark Fluent interface.

> Early development: review every change before applying it. Registry tweaks can affect system behaviour.

## Current features

- Searchable tweak catalogue with Privacy, Input, Explorer and Gaming categories
- Current-state display and confirmation before changes
- Per-operation registry backup and latest-change rollback
- Windows activation status query and shortcut to Activation settings
- GitHub Actions workflow to build a Windows executable

## Planned

- Light/dark/system theme selection
- Importable and exportable profiles with preview
- More thoroughly tested privacy and performance settings
- Better rollback history management and compatibility checks

## Requirements

- Windows 10/11
- Python 3.12+

## Run from source

PowerShell:

    py -3.12 -m venv .venv
    .\.venv\Scripts\Activate.ps1
    python -m pip install -r requirements.txt
    $env:PYTHONPATH = "src"
    python -m g_tweaker.main

## Build an EXE

    python -m pip install -r requirements.txt
    python -m PyInstaller G_Tweaker.spec --clean

The executable will be written to dist/G_Tweaker.exe. You can also run the GitHub Actions workflow manually.

## Safety

See docs/SECURITY.md. G_Tweaker does not silently disable security features or bypass Windows licensing. Some settings require administrator rights, and some policies are unavailable on certain Windows editions.

## License

MIT. See LICENSE.
