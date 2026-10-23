from g_tweaker.core.tweak_engine import TWEAKS

PROFILES = {
    "Privacy starter": {
        "description": "Disable the advertising ID and request a supported diagnostic-data policy.",
        "tweaks": ["privacy.advertising_id", "privacy.telemetry"],
    },
    "Gaming starter": {
        "description": "Enable Windows Game Mode.",
        "tweaks": ["gaming.game_mode"],
    },
    "Everyday usability": {
        "description": "Show file extensions and disable the Sticky Keys shortcut.",
        "tweaks": ["explorer.extensions", "input.sticky_keys"],
    },
}

def get_profile(name: str) -> dict:
    if name not in PROFILES:
        raise KeyError(f"Unknown profile: {name}")
    profile = PROFILES[name]
    by_id = {tweak.id: tweak for tweak in TWEAKS}
    return {
        "name": name,
        "description": profile["description"],
        "tweaks": [by_id[tweak_id] for tweak_id in profile["tweaks"] if tweak_id in by_id],
    }
