# G_Tweaker

A Windows 10/11 tweaking utility with a Windows 11-inspired Fluent interface.

> Early development. Review every change before applying it. Backups are not a substitute for a full system backup.

## Features planned
- Searchable tweak catalogue
- Privacy, input, Explorer and gaming categories
- User-created profiles
- Per-setting backups and rollback history
- Windows activation status and links to supported activation settings
- PyInstaller executable builds

## Requirements
- Windows 10/11
- Python 3.12+

## Development
```powershell
py -3.12 -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -r requirements.txt
python -m g_tweaker.main
```

## Safety
G_Tweaker does not silently disable Windows security features or bypass licensing. Tweaks should report unsupported settings clearly. Registry changes must save the previous value before applying and provide a verified rollback.

## License
MIT — see `LICENSE`.
