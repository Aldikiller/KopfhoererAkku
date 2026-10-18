# Datenschutz

Die Anwendung liest lokale Windows-Geräteeigenschaften, Bluetooth-Adressen,
Gerätenamen, Verbindungsstatus und von Windows bereitgestellte Akkuwerte.
Ohne Update-Prüfung stellt sie keine Netzwerkverbindungen her. Sie sendet keine Telemetrie.

Gespeichert werden höchstens zwei ausgewählte Gerätekennungen und lokale
Einstellungen zu Warnungen, Abfrageintervall, eigenen Anzeigenamen, Gerätefarben, Schrift, Symbolen, Batterieform, Sprache, Update-Prüfung und Widget-Anzeige. Installierte Version:
`%LOCALAPPDATA%\KopfhoererAkku`; portable Version: `Einstellungen` neben der EXE.
Autostart nutzt eine lokale Verknüpfung im Autostartordner des angemeldeten Nutzers.

Abfragezeiten und zuletzt gelesene Akkuwerte für die Geräteübersicht bleiben im Arbeitsspeicher der laufenden Anwendung und werden nicht übertragen. Die App beobachtet lokale Bluetooth-Verbindungsereignisse sowie Standby und Entsperren, um erneut abzufragen.

Die freiwillige Diagnose `--diagnose` erzeugt `Diagnose.txt` im Datenordner.
Sie kann Gerätenamen, Bluetooth-Adressen und Gerätekennungen enthalten.
Die Datei wird nicht automatisch übertragen. Vor dem Teilen persönliche
Kennungen entfernen. Diagnoseprotokolle gehören nicht in öffentliche Issues.

GitHub und die Webseite, über die die Dateien heruntergeladen werden, haben
eigene Datenschutzbedingungen. Die freiwillige Update-Prüfung verwendet die öffentliche GitHub-API unter
api.github.com/repos/Aldikiller/KopfhoererAkku/releases. Sie ist standardmäßig
aus und wird bewusst per Klick oder nach Aktivierung beim Start aufgerufen.
GitHub erhält dabei die IP-Adresse und einen User-Agent mit der Programmversion.
Gerätekennungen, eigene Namen, Farben und Akkuwerte werden nicht übertragen.
Es werden keine Konten oder Zugriffsschlüssel benötigt.
GitHubs Datenschutzbedingungen gelten für die Anfrage:
https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement

Die Prüfung lädt keine Programmupdates herunter und installiert nichts.
Die Downloadseite öffnet sich nur nach einem ausdrücklichen Klick oder einer
Bestätigung im normalen Browser. Beim Browseraufruf gelten dessen Einstellungen.

