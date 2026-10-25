# BleAirDock 1.7.1

- Add Restart to the tray and widget context menus. The replacement process waits for the original process to exit before starting, retaining single-instance behavior.
- Move About BleAirDock into Settings, available in both simple and advanced views. Keep update checks in Settings and remove the tray update entry.
- Combine classic and modern Windows Bluetooth connection sources on each regular scan. A positive report for the same Bluetooth address is no longer hidden by a disconnected report from the other source.
- Use one display snapshot for tray and widget. Missing or disconnected battery values remain unavailable; no fabricated percentages.
- Clearer built-in device symbols: larger headphones with solid ear pads, filled musical notes and stronger light/dark outlines. Transparent gaps are retained.
- All five languages updated. Existing settings, polling interval and audio mappings retained.

## Deutsch

Neustart ist im Rechtsklickmenü von Infobereich und Widget verfügbar. Über BleAirDock und die Update-Suche befinden sich in den Einstellungen. Die Verbindungserkennung berücksichtigt beide Windows-Bluetooth-Quellen; Widget und Infobereich erhalten denselben Anzeigestand. Die Abfrage läuft weiter im eingestellten Intervall.

The original hardware report still needs a real-device retest. Automated tests cover conflicting Windows connection sources, UI menus, five languages, tray/widget states, pulse restoration and restart process handoff. Downloads remain unsigned.
