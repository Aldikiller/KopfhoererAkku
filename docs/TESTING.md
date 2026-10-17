# Build- und Testumfang

## Beta 2: neue Funktionen

- Warnlogik sowie 54 Status-/Farb-/Anzeigevarianten geprüft.
- Namen einschließlich Unicode und Gleichheitszeichen, Farben, Anzeige und
  Update-Einstellung gespeichert und wieder eingelesen; atomisches Überschreiben geprüft.
- Versionsvergleich mit beta.2/beta.10, stabilen Versionen, ungültigen
  Versionsnummern, unveröffentlichten Entwürfen und Downgrades geprüft.
- Update-Prüfung standardmäßig aus. Echte öffentliche GitHub-Abfrage erfolgreich.
- Drei Einstellungsseiten mit zwei synthetischen Geräten gerendert und geprüft.
- Die Bestätigungen des Herausgebers unten beziehen sich auf Beta 1.
  Eigene Namen/Farben und Update-Hinweis sind neue Funktionen von Beta 2.


## Rückmeldung des Herausgebers vom 17. Oktober 2026

Aldikiller hat bestätigt, dass die Verbindungsanzeige beim Ein- und Ausschalten
der beiden Kopfhörer funktioniert und die Gerätenamen stimmen. Auch die
Installationsprüfung wurde als passend zurückgemeldet. Genaue Angaben zu
Windows-Versionen und dem Testrechner liegen für diese Rückmeldung nicht vor.

Noch offen sind die Lesbarkeit bei verschiedenen Skalierungen sowie eine
Akkuwarnung bei echtem niedrigem Ladestand. Die digitale Signatur ist noch
nicht eingerichtet. Die unten beschriebene eingeschränkte Entwicklungsumgebung
ist vom manuellen Test des Herausgebers zu unterscheiden.

## Lokaler Stand am 17. Oktober 2026

- Release-Anwendung und reguläre Setup.exe erfolgreich kompiliert.
- Eingebaute Warn-/Einstellungs-/Symboltests bestanden.
- UI mit zwei synthetischen Geräten gerendert: zwei ausgewählt, zwei Symbole
  angelegt und zwei als sichtbar an Windows übergeben.
- Isolierter Installer-Testbuild: Erstinstallation, Überinstallation und
  Deinstallation erfolgreich (jeweils Exitcode 0); Programmdatei entfernt.
- Selbsttest auch aus dem isoliert installierten Verzeichnis bestanden.
- Normale Autostart-/Startmenü-/Apps-Registrierung und Anmeldung noch nicht
  durch einen vollständigen Installationstest außerhalb der Einschränkungen belegt.
- GitHub-Windows-Build und Versions-Tag-Workflow erfolgreich; noch keine Codesignatur.

Die Screenshots zeigen synthetische Beispieldaten, keine Kompatibilitätsmessung.

## Automatisch

`build.ps1` kompiliert die Anwendung mit dem Windows-.NET-Framework-Compiler
und referenziert die Windows-10-Metadaten aus `System32/WinMetadata`.
Danach prüft `--selftest`:

- erste Warnung, wiederholte Abfrage, Wiederverbindung und Erholung,
- inklusive Warnschwelle und fehlende Akkuwerte,
- keine Warnung bei unbekannter/getrennter Verbindung oder deaktivierten Warnungen,
- 18 Kombinationen aus Akkustand und Verbindungsstatus für die Symbolerzeugung,
- Speichern und Laden der Einstellungen.

`--ui-check --output-dir=<Ordner>` rendert das Einstellungsfenster und zählt
die an Windows als sichtbar übergebenen Symbole. Die tatsächliche Position
im Windows-Infobereich wird dadurch nicht geprüft.

Für eine Diagnose auf einem Testrechner:

```powershell
.\KopfhoererAkku.exe --diagnose
```

Die Ausgabe enthält persönliche Gerätekennungen. Nicht unbereinigt teilen.

## Vor einem stabilen Release manuell prüfen

- Frische Installation ohne Administratorrechte und ohne vorausgewählte Geräte.
- Zwei Geräte auswählen und beide Symbole aus dem Windows-Überlauf hervorziehen.
- Gerät einzeln aus-/einschalten, Bluetooth deaktivieren und wieder aktivieren.
- Reale Werte mit den Windows-Bluetooth-Einstellungen vergleichen.
- Windows-Warnung über Test-Button und durch echten niedrigen Akkustand prüfen.
- Autostart nach Ab-/Anmeldung, Ausschalten des Autostarts, erneutes Einschalten.
- Update bei laufender Anwendung und Deinstallation bei laufender Anwendung.
- Einstellungen bleiben nach Deinstallation erhalten; Autostart wird entfernt.
- Portable Version in einem separaten beschreibbaren Ordner.

Die klassische Bluetooth-Abfrage war in der eingeschränkten Entwicklungsumgebung
gesperrt; die WinRT-Abfrage lief in einen Timeout. Diese Entwicklungsprüfung allein belegt keine realen Verbindungswechsel;
die spätere Bestätigung des Herausgebers ist oben gesondert aufgeführt.

Für einen isolierten Installer-Test kann das Inno-Skript mit `/DSmokeTest=1`
gebaut werden. Dieser Testbuild überspringt Registrierung, Verknüpfungen,
Autostart und die .NET-Erkennung, erlaubt aber Dateiinstallation, Update und
Deinstallation in einem ausdrücklich gewählten Testordner. Er ersetzt nicht
den Test des normalen Setups in einer frischen Windows-VM.
