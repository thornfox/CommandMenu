# CommandMenu

**Author:** ThornFox · **Version:** 1.0.0 · **Framework:** Oxide/uMod (also runs on Carbon)

## English

Admin-only UI menu with a 2x3 button grid. Each button executes a configurable chat command for the clicking admin.

- Chat command: `/radmin_menu` (toggles the menu)
- Permission: `commandmenu.admin`, e.g. `oxide.grant group admin commandmenu.admin`
- Buttons only send their index; only commands from the config can be executed.
- Every click is logged to the server console and optionally to `oxide/logs/CommandMenu`.
- Languages: English, German

### Tested on RustRentner
All ThornFox plugins are tested extensively on [rustrentner.de](https://rustrentner.de).

### License
MIT – see [LICENSE](LICENSE).

## Deutsch

Admin-Menü mit einem 2x3-Button-Raster. Jeder Button führt einen konfigurierbaren Chat-Befehl für den klickenden Admin aus.

- Chatbefehl: `/radmin_menu` (Menü ein-/ausblenden)
- Berechtigung: `commandmenu.admin`, z. B. `oxide.grant group admin commandmenu.admin`
- Buttons senden nur ihren Index; ausführbar sind ausschließlich Befehle aus der Config.
- Jeder Klick wird in der Serverkonsole und optional in `oxide/logs/CommandMenu` protokolliert.
- Sprachen: Englisch, Deutsch

### Getestet auf RustRentner
Alle Plugins von ThornFox werden ausgiebig auf [rustrentner.de](https://rustrentner.de) getestet.

### Lizenz
MIT – siehe [LICENSE](LICENSE).

## Changelog

### 1.0.0
- Permission changed to `commandmenu.admin` (fixed, no longer configurable)
- `cmdmenu.exec` now checks permission and accepts only a button index (whitelist)
- Config versioning and migration; existing anchors and commands are kept
- Broken config is no longer overwritten
- Localization (en/de) via lang files
- Menu state fixed on disconnect, reload and empty command list
- Optional mouse cursor, configurable font size, audit log file
