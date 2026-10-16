# CommandMenu

Admin-only Rust Oxide/Carbon plugin that shows a 2x3 button menu for configurable chat commands.
Rust-Oxide/Carbon-Plugin nur für Admins, das ein 2x3-Button-Menü für konfigurierbare Chat-Befehle anzeigt.

**Author:** ThornFox · **Version:** 1.0.0 · **Framework:** Oxide/uMod, compatible with Carbon

---

## English

### Installation
Copy `CommandMenu.cs` into `oxide/plugins/` or `carbon/plugins/`. The plugin loads automatically.

### Usage
```
/radmin_menu
```
Toggles the menu. Each button executes its configured chat command for the clicking admin.

### Permission
| Permission | Description |
|---|---|
| `commandmenu.admin` | Allows opening the menu and using its buttons |

```
oxide.grant group admin commandmenu.admin
```

### Configuration
```json
{
  "Config Version": "1.0.0",
  "Menu Anchor Min (e.g., 0.01 0.01 for bottom-left)": "0.01 0.01",
  "Menu Anchor Max (e.g., 0.11 0.0767 for a small menu)": "0.11 0.0767",
  "Button Font Size": 10,
  "Enable mouse cursor while menu is open (blocks mouse look)": false,
  "Show chat confirmation after a button click": true,
  "Write button clicks to log file (oxide/logs/CommandMenu)": true,
  "List of commands to display (MAX 6 recommended for 2x3 layout)": [
    { "Button Text": "Vanish", "Chat Command": "vanish" },
    { "Button Text": "TC add self", "Chat Command": "adminpve tc add self" }
  ]
}
```
Up to six buttons are shown. Older configurations are migrated automatically; a broken config file is never overwritten.

### Security
Buttons only send their index. Only commands from the config can be executed, and every click checks `commandmenu.admin`.

### Logging
Every click is written to the server console and, if enabled, to `oxide/logs/CommandMenu/`.

### Localization
English and German are included.

### Works with AdminChange
CommandMenu is the button menu for [AdminChange](https://github.com/thornfox/AdminChange). By default, AdminChange runs `/radmin_menu` when staff mode is activated or deactivated, so the menu opens and closes together with staff mode. Both plugins also work on their own.

### Changelog
- **1.0.0**
  - Permission changed to `commandmenu.admin` (fixed, no longer configurable)
  - `cmdmenu.exec` checks the permission and accepts only a button index (whitelist)
  - Config versioning and migration; existing anchors and commands are kept
  - Localization (en/de) via the Lang API
  - Menu state fixed on disconnect, reload and empty command list
  - Optional mouse cursor, configurable font size, audit log file
- **0.1.0** – Initial version

### Tested on RustRentner
All ThornFox plugins are tested extensively on [rustrentner.de](https://rustrentner.de).

### License
MIT – see [LICENSE](LICENSE).

---

## Deutsch

### Installation
`CommandMenu.cs` nach `oxide/plugins/` bzw. `carbon/plugins/` kopieren. Das Plugin lädt automatisch.

### Benutzung
```
/radmin_menu
```
Blendet das Menü ein oder aus. Jeder Button führt seinen konfigurierten Chat-Befehl für den klickenden Admin aus.

### Berechtigung
| Berechtigung | Beschreibung |
|---|---|
| `commandmenu.admin` | Erlaubt das Öffnen des Menüs und die Nutzung der Buttons |

```
oxide.grant group admin commandmenu.admin
```

### Konfiguration
Siehe Beispiel oben. Position, Schriftgröße, Mauszeiger, Chat-Bestätigung, Logdatei und bis zu sechs Buttons sind einstellbar. Alte Konfigurationen werden automatisch migriert, eine fehlerhafte Config-Datei wird nie überschrieben.

### Sicherheit
Buttons senden nur ihren Index. Ausführbar sind ausschließlich Befehle aus der Config, und jeder Klick prüft `commandmenu.admin`.

### Protokoll
Jeder Klick wird in der Serverkonsole und, falls aktiviert, in `oxide/logs/CommandMenu/` protokolliert.

### Sprachen
Englisch und Deutsch sind enthalten.

### Zusammenspiel mit AdminChange
CommandMenu ist das Button-Menü für [AdminChange](https://github.com/thornfox/AdminChange). AdminChange führt beim Aktivieren oder Beenden des Teammodus standardmäßig `/radmin_menu` aus, sodass sich das Menü zusammen mit dem Teammodus öffnet und schließt. Beide Plugins funktionieren auch einzeln.

### Änderungen
- **1.0.0**
  - Berechtigung auf `commandmenu.admin` umgestellt (fest, nicht mehr konfigurierbar)
  - `cmdmenu.exec` prüft die Berechtigung und akzeptiert nur einen Button-Index (Whitelist)
  - Config-Versionierung und Migration; bestehende Positionen und Befehle bleiben erhalten
  - Lokalisierung (en/de) über die Lang-API
  - Menüstatus bei Disconnect, Reload und leerer Befehlsliste korrigiert
  - Optionaler Mauszeiger, einstellbare Schriftgröße, Audit-Logdatei
- **0.1.0** – Erste Version

### Getestet auf RustRentner
Alle Plugins von ThornFox werden ausgiebig auf [rustrentner.de](https://rustrentner.de) getestet.

### Lizenz
MIT – siehe [LICENSE](LICENSE).
