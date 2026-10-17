# Datenschutz

Die Anwendung liest lokale Windows-Geräteeigenschaften, Bluetooth-Adressen,
Gerätenamen, Verbindungsstatus und von Windows bereitgestellte Akkuwerte.
Sie stellt selbst keine Netzwerkverbindungen her und sendet keine Telemetrie.

Gespeichert werden höchstens zwei ausgewählte Gerätekennungen und lokale
Einstellungen zu Warnungen und Abfrageintervall. Installierte Version:
`%LOCALAPPDATA%\KopfhoererAkku`; portable Version: `Einstellungen` neben der EXE.
Autostart nutzt eine lokale Verknüpfung im Autostartordner des angemeldeten Nutzers.

Die freiwillige Diagnose `--diagnose` erzeugt `Diagnose.txt` im Datenordner.
Sie kann Gerätenamen, Bluetooth-Adressen und Gerätekennungen enthalten.
Die Datei wird nicht automatisch übertragen. Vor dem Teilen persönliche
Kennungen entfernen. Diagnoseprotokolle gehören nicht in öffentliche Issues.

GitHub und die Webseite, über die die Dateien heruntergeladen werden, haben
eigene Datenschutzbedingungen. Das Programm selbst lädt keine Updates herunter.
