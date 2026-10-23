from g_tweaker.core.tweak_engine import TWEAKS


def test_tweak_ids_are_unique():
    ids = [t.id for t in TWEAKS]
    assert len(ids) == len(set(ids))


def test_tweaks_have_required_fields():
    for tweak in TWEAKS:
        assert tweak.id and tweak.title and tweak.description and tweak.category
        assert tweak.hive in {"HKCU", "HKLM"}
        assert tweak.kind in {"DWORD", "STRING"}
