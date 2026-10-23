from __future__ import annotations
import json
import os
import platform
import subprocess
import winreg
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

DATA_DIR = Path(os.environ.get("LOCALAPPDATA", str(Path.home()))) / "G_Tweaker"
HISTORY_FILE = DATA_DIR / "history.json"

@dataclass(frozen=True)
class Tweak:
    id: str
    title: str
    description: str
    category: str
    hive: str
    key: str
    name: str
    kind: str
    enabled_value: Any
    disabled_value: Any
    warning: str = ""

TWEAKS = [
    Tweak("privacy.telemetry", "Limit optional diagnostic data",
          "Sets the diagnostic-data policy where supported. This does not mean zero telemetry.",
          "Privacy", "HKLM", r"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
          "AllowTelemetry", "DWORD", 1, None,
          "Edition and organisation policies may override this setting."),
    Tweak("privacy.advertising_id", "Disable advertising ID",
          "Disables the current user's Windows advertising identifier.",
          "Privacy", "HKCU", r"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
          "Enabled", "DWORD", 0, 1),
    Tweak("input.sticky_keys", "Disable Sticky Keys shortcut",
          "Changes the current user's Sticky Keys accessibility flags.",
          "Input", "HKCU", r"Control Panel\Accessibility\StickyKeys",
          "Flags", "STRING", "506", "510"),
    Tweak("explorer.extensions", "Show file extensions",
          "Shows known file extensions in File Explorer.",
          "Explorer", "HKCU", r"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
          "HideFileExt", "DWORD", 0, 1),
    Tweak("gaming.game_mode", "Enable Windows Game Mode",
          "Enables Game Mode for the current user; impact varies by system.",
          "Gaming", "HKCU", r"Software\Microsoft\GameBar",
          "AutoGameModeEnabled", "DWORD", 1, 0),
]
HIVES = {"HKCU": winreg.HKEY_CURRENT_USER, "HKLM": winreg.HKEY_LOCAL_MACHINE}

def read_value(tweak: Tweak) -> dict:
    try:
        with winreg.OpenKey(HIVES[tweak.hive], tweak.key, 0, winreg.KEY_READ) as key:
            value, kind = winreg.QueryValueEx(key, tweak.name)
            return {"exists": True, "value": value, "kind": kind}
    except FileNotFoundError:
        return {"exists": False, "value": None, "kind": None}

def current_state(tweak: Tweak) -> str:
    if platform.system() != "Windows":
        return "Windows only"
    state = read_value(tweak)
    if not state["exists"]:
        return "Not configured"
    if state["value"] == tweak.enabled_value:
        return "Enabled"
    if tweak.disabled_value is not None and state["value"] == tweak.disabled_value:
        return "Disabled"
    return f"Custom ({state['value']})"

def _write(tweak: Tweak, value: Any) -> None:
    if tweak.hive == "HKLM":
        import ctypes
        if not ctypes.windll.shell32.IsUserAnAdmin():
            raise PermissionError("Administrator rights required. Relaunch G_Tweaker as administrator.")
    access = winreg.KEY_SET_VALUE
    if tweak.hive == "HKLM":
        access |= getattr(winreg, "KEY_WOW64_64KEY", 0)
    with winreg.CreateKeyEx(HIVES[tweak.hive], tweak.key, 0, access) as key:
        kind = winreg.REG_DWORD if tweak.kind == "DWORD" else winreg.REG_SZ
        winreg.SetValueEx(key, tweak.name, 0, kind, value)

def _history() -> list[dict]:
    try:
        return json.loads(HISTORY_FILE.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return []

def _save(items: list[dict]) -> None:
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    temp = HISTORY_FILE.with_suffix(".tmp")
    temp.write_text(json.dumps(items, ensure_ascii=False, indent=2), encoding="utf-8")
    temp.replace(HISTORY_FILE)

def apply_tweak(tweak: Tweak, enabled: bool) -> None:
    if platform.system() != "Windows":
        raise RuntimeError("Tweaks can only be applied on Windows.")
    value = tweak.enabled_value if enabled else tweak.disabled_value
    if value is None:
        raise ValueError("This setting has no defined value for that operation.")
    before = read_value(tweak)
    _write(tweak, value)
    items = _history()
    items.append({
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "tweak_id": tweak.id, "title": tweak.title,
        "hive": tweak.hive, "key": tweak.key, "name": tweak.name,
        "previous": before, "written_value": value, "value_type": tweak.kind,
    })
    _save(items)

def rollback_latest() -> str:
    if platform.system() != "Windows":
        raise RuntimeError("Rollback is available only on Windows.")
    items = _history()
    if not items:
        raise RuntimeError("No changes to roll back.")
    record = items[-1]
    with winreg.CreateKeyEx(HIVES[record["hive"]], record["key"], 0, winreg.KEY_SET_VALUE) as key:
        previous = record["previous"]
        if previous["exists"]:
            winreg.SetValueEx(key, record["name"], 0, previous["kind"], previous["value"])
        else:
            try:
                winreg.DeleteValue(key, record["name"])
            except FileNotFoundError:
                pass
    items.pop()
    _save(items)
    return record["title"]

def history_items() -> list[dict]:
    return list(reversed(_history()))

def activation_status() -> str:
    if platform.system() != "Windows":
        return "Activation status is available on Windows only."
    script = Path(os.environ.get("WINDIR", r"C:\Windows")) / "System32" / "slmgr.vbs"
    try:
        result = subprocess.run(["cscript.exe", "//Nologo", str(script), "/xpr"],
                                capture_output=True, text=True, timeout=20,
                                creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0))
        return (result.stdout or result.stderr).strip() or "Windows returned no activation details."
    except (OSError, subprocess.TimeoutExpired) as exc:
        return f"Could not query activation status: {exc}"
