# Kopfhörer-Akku

**Kostenlose Bluetooth-Akkuanzeige für den Windows-Infobereich.**

Von **Aldikiller** · MIT-Lizenz · **0.1.0-beta.1**

Bis zu zwei Kopfhörer erhalten jeweils ein kombiniertes Batteriesymbol mit
kleinem Kopfhörerzeichen, Prozentzahl und eigenem farbigen Hintergrund.
Der Gerätename und Status erscheinen beim Darüberfahren.

![Beispielsymbole](docs/Symbole.png)

## Funktionen

- Zwei getrennte Gerätesymbole; das Programm blendet keines automatisch aus.
- Einstellbare Akkuwarnungen (1 bis 90 %, Standard 20 %) mit Wiederholschutz.
- Abfrageintervall 5 bis 300 Sekunden, Standard 15 Sekunden.
- Einstellungen per Doppelklick; Benachrichtigungstest im Einstellungsfenster.
- Verbindungsstatus wird separat vom gespeicherten Akkuwert abgefragt.
- Autostart als Windows-Verknüpfung, im Menü abschaltbar.
- Setup mit Deinstallation oder portable ZIP. Keine Administratorrechte nötig.
- Keine Werbung, Telemetrie, Konten oder Netzwerkverbindungen durch die Anwendung.

## Voraussetzungen

Windows 10 ab Version 2004 / Build 19041 oder Windows 11, **x64**, .NET Framework
4.8 und ein Bluetooth-Gerät, dessen Akkustand Windows bereitstellt.
Es werden keine Laufzeitkomponenten automatisch heruntergeladen.
Andere Architekturen und Windows-Versionen sind nicht als unterstützt getestet.

## Installation und erster Start

1. Die Datei `KopfhoererAkku-0.1.0-beta.1-Setup.exe` starten. Als Installationsziel
   ist `%LOCALAPPDATA%\Programs\KopfhoererAkku` vorgesehen.
2. Autostart ist im Setup vorausgewählt; eine Desktop-Verknüpfung ist optional.
3. Kopfhörer einschalten und verbinden. Beim ersten erfolgreichen Scan öffnet
   sich die Geräteauswahl. Bis zu zwei Einträge auswählen und übernehmen.
4. Bei mehreren Einträgen desselben Geräts den Eintrag mit Akkuwert wählen.
5. Windows kann Symbole unter **^** verstecken. Beide Symbole neben die Uhr ziehen.

Die Binärdateien sind **nicht digital signiert**. Windows kann deshalb einen
Warnhinweis anzeigen. SHA256-Prüfsummen liegen bei; sie bestätigen bei einem
Vergleich die Dateiintegrität, ersetzen aber keine Herausgebersignatur.

Die portable ZIP vollständig in einen beschreibbaren Ordner entpacken.
`portable.flag` aktiviert lokale Einstellungen im Unterordner `Einstellungen`.
Die installierte Version speichert unter `%LOCALAPPDATA%\KopfhoererAkku`.
Es werden keine persönlichen Geräte vorausgewählt oder mitgeliefert.

![Einstellungsfenster mit Beispieldaten](docs/Einstellungen.png)

## Bedeutung der Symbole

| Anzeige | Bedeutung |
| --- | --- |
| Grün / Blau mit Zahl | Windows meldet verbunden; Zahl entspricht dem Akkuwert in Prozent |
| Rot mit Zahl | Verbunden und Akku höchstens 20 %; unabhängig von der Warnschwelle |
| Grau mit × | Windows meldet getrennt |
| Gelb mit Zahl | Verbindung unbekannt; der Akkuwert kann gespeichert sein |
| Gelb mit ? | Verbindung und aktueller Akkuwert nicht verfügbar |
| Verbunden mit – | Verbindung bestätigt, aber kein Akkuwert verfügbar |

Warnungen erfolgen nur bei bestätigter Verbindung und einem vorhandenen Wert.
Eine neue Warnung ist nach Wiederverbindung, Neustart, Änderung der
Warneinstellungen oder nach Erholung auf mehr als Warnschwelle + 5 möglich.
Der Button **Test** prüft Windows-Benachrichtigungen unabhängig vom Akkustand.

## Bekannte Einschränkungen

- **Beta:** Akkuwerte wurden auf einem Rechner für JBL Live 770NC und JLab
  Rewind 2 gelesen. Das ist keine allgemeine Kompatibilitätszusage.
- Die Verbindungserkennung und echte Ein-/Ausschaltwechsel konnten in der
  eingeschränkten Entwicklungsumgebung nicht bestätigt werden. Dort war die
  klassische Bluetooth-Abfrage gesperrt; die alternative WinRT-Abfrage lief
  in einen Timeout. Dann wird ausdrücklich **unbekannt** angezeigt.
- Verbindungsstatus bedeutet Bluetooth-Verbindung, nicht aktive Audioausgabe.
- Windows kann auch nach dem Ausschalten gespeicherte Akkuwerte bereitstellen.
- Keine getrennten Links-/Rechts-/Ladecase-Werte, ANC- oder Trageerkennung.
- Keine automatische Aktualisierung und keine Hintergrund-Downloads.
- Die Symbole sind auf den von Windows vorgegebenen Platz begrenzt.

## Aktualisieren und entfernen

Ein neues Setup über die vorhandene Installation ausführen. Aktuelle
Release-Versionen unterstützen ein geordnetes Beenden durch das Setup.
Die frühere, lokal entwickelte Prototyp-Version bitte vorher selbst beenden
und deren Autostart deaktivieren; sie wird nicht automatisch migriert.

Deinstallation über **Einstellungen → Apps → Installierte Apps** oder das
Startmenü. Programmdateien, Verknüpfungen und Release-Autostart werden entfernt.
Einstellungen in `%LOCALAPPDATA%\KopfhoererAkku` bleiben bewusst erhalten und
können nach dem Beenden manuell gelöscht werden. Portable: Autostart im Menü
ausschalten, beenden und den entpackten Ordner löschen.

## Selbst bauen

Ein x64-Windows-Rechner mit .NET Framework 4.8 genügt für die portable Version:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Für das Setup wird zusätzlich [Inno Setup](https://jrsoftware.org/isinfo.php)
benötigt:

```powershell
.\build.ps1 -Installer -Iscc 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
```

Ergebnisse stehen unter `dist/`. Der Build führt die eingebauten Tests aus und
erstellt SHA256-Prüfsummen. Weitere Angaben: [Build und Tests](docs/TESTING.md).
Der Windows-Workflow baut Artefakte und erstellt bei Versions-Tags einen
**Release-Entwurf**, den der Herausgeber vor der Veröffentlichung prüft.

## Datenschutz, Lizenz und Mitarbeit

[MIT-Lizenz](LICENSE) · [Haftungshinweise](DISCLAIMER.de.md) ·
[Datenschutz](PRIVACY.md) · [Mitarbeiten](CONTRIBUTING.md) ·
[Sicherheit](SECURITY.md) · [Veröffentlichung](docs/PUBLISH.md)

Die angebotenen Downloads sind kostenlos. Die MIT-Lizenz gestattet auch
Weiterverteilung, Änderungen und kommerzielle Nutzung unter ihren Bedingungen.
Das Projekt wurde mit KI-Unterstützung entwickelt; die Icons werden aus
eigenem Zeichencode erzeugt, ohne beigefügte Produktfotos oder Markenlogos.
