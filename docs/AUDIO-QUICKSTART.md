# Audio-Schnellauswahl · Version 1.6.1

1. Beide Kopfhörer mit Windows verbinden.
2. **Einstellungen → Bedienung → Audio-Schnellauswahl** öffnen.
3. Eindeutige aktive Ausgänge werden über die gemeinsame Windows-Gerätekennung automatisch zugeordnet und gespeichert. Eigene Anzeigenamen sind dafür nicht maßgeblich. Falls mehrere Ausgänge passen oder die Kennung fehlt, die passende Ausgabe einmal auswählen und speichern. Vorhandene Zuordnungen bleiben erhalten.
4. Ein Klick auf das jeweilige Akkusymbol schaltet die Standardausgabe für normale Audio-/Medienwiedergabe um. Die aktive Ausgabe bekommt einen grünen Kreis mit weißem Häkchen. Doppelklick öffnet weiter die Einstellungen; die Geräteübersicht bleibt unter Rechtsklick → Weitere Aktionen erreichbar.

Die Ein-Klick-Umschaltung lässt sich im selben Fenster abschalten. Dann öffnet der Einzelklick wie bisher die Übersicht. Dort gibt es pro Kopfhörer zusätzlich **Ton hier wiedergeben**.

Apps mit einer eigenen festen Geräteauswahl können ihre bisherige Ausgabe beibehalten. Das separate Standard-Kommunikationsgerät und die Mikrofonwahl werden nicht geändert. Nicht verbundene Audioausgaben werden nicht als aktiv markiert. Scheitert das Umschalten, erscheint eine Fehlermeldung.

## Akkuabfrage

Standard sind 30 Sekunden. Einstellbar sind 30 bis 300 Sekunden, auch in der einfachen Ansicht. Ältere kürzere Einstellungen werden auf 30 Sekunden angehoben. Es gibt keine schnellen zusätzlichen Akkuabfragen nach Standby oder Bluetooth-Ereignissen. **Jetzt aktualisieren** bleibt manuell möglich. Der aktive Audioausgang wird über Windows-Ereignisse aktualisiert.

## Teststand

Build und automatisierte Tests prüfen Zuordnung, gespeicherte Einstellungen, Intervallgrenzen und Aktiv-Markierung. Die reale Windows-Abfrage erkennt 29 Ausgänge; Registrierung und Abmeldung der Audioereignisse sowie das Setzen des bereits aktiven Ausgangs wurden geprüft. Der Nutzer bestätigt den manuellen Wechsel zwischen seinen Kopfhörern. Die neue automatische Zuordnung ist durch Testfälle abgesichert, aber noch nicht für jedes Modell im Alltag bestätigt.

Zum Wechsel der Windows-Standardausgabe wird die interne PolicyConfig-COM-Schnittstelle verwendet. Diese ist keine öffentlich zugesicherte Windows-API. Der Aufruf ist gekapselt; erst die bestätigte Windows-Standardausgabe wird als aktiv markiert.



## Ruhige Statusanzeige

Normale Abfragen laufen ohne Fortschrittsmeldung. Abfragealter, unbekannte oder getrennte Verbindung und echte Abfragefehler bleiben sichtbar. Bluetooth-Ereignisse anderer Geräte invalidieren die beiden ausgewählten Kopfhörer nicht. Die freiwillige GitHub-Update-Prüfung läuft nur manuell oder nach Aktivierung beim Programmstart, nicht im Akkuintervall.


## Direktwahl im Desktop-Widget ab 1.6.1

Klicke im Desktop-Widget auf das Symbol, den Namen oder den Prozentwert des gewünschten Kopfhörers. Das grüne Häkchen zeigt die aktive Audioausgabe. Ziehen verschiebt das Widget, ohne die Ausgabe zu wechseln. Doppelklick öffnet die Einstellungen. Die Auswahl funktioniert auch bei gesperrter Position. Es gilt dieselbe Zuordnung wie im Infobereich; „Audio-Schnellauswahl“ schaltet die Direktwahl für beide Ansichten ein oder aus.
