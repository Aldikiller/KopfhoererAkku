# BleAirDock 1.7.0

Kopfhörer-Akku is now **BleAirDock**. Same free, open-source Windows app, with the approved orange/graphite logo.

- New branding in the app, About window, installer, documentation and all five website languages.
- Download `BleAirDock-1.7.0-Setup.exe` or the portable ZIP.
- Installer identity, settings directory, audio assignments and single-instance protection remain compatible. Existing update URLs remain valid.
- Portable users: extract into the previous portable folder to retain `Einstellungen`; start BleAirDock.exe.
- Features from 1.6.5 remain included. Older screenshots are marked by their original version.

Unsigned Windows x64 freeware, MIT license. No paid certificate or Microsoft Store account required.

## Deutsch

Kopfhörer-Akku heißt jetzt **BleAirDock**. Neuer Name und neues Logo, Funktionen aus 1.6.5 bleiben enthalten. Beim Installer-Update bleiben Einstellungen und Zuordnungen bestehen. Portable: in den bisherigen Ordner entpacken, `Einstellungen` behalten und BleAirDock.exe starten. Die bisherige Repository-Adresse bleibt für alte Update-Abfragen erhalten.

---

# Kopfhörer-Akku 1.6.5

- Widget vorübergehend über Menü oder Tray ausblenden und wieder anzeigen; ein Tray-Symbol bleibt erreichbar.
- Klare Meldungen bei getrennter Verbindung, unbekanntem Status, fehlendem Windows-Akkuwert und Abfragefehlern.
- Freiwillige Lautstärkespeicherung je Kopfhörer für Wechsel über die App; erster Zielwert wählbar, Stummschaltung unverändert.
- Eigene PNG-/ICO-Gerätesymbole mit lokaler Kopie und Livevorschau; fünf SVG-/PNG-Vorlagen mitgeliefert. Tiny wahlweise nur mit Bildsymbol.

# Kopfhörer-Akku 1.6.4

- Eigene Benachrichtigungen, Warnschwellen, Leuchtwarnungen und Pulsfarben pro Kopfhörer. Alternativ allgemeine Einstellungen verwenden.
- Pulsdauer 1–10 Sekunden und Leuchtstärke 10–100 %, alternativ dauerhaftes Leuchten.
- Warnungen über das Tray-Menü für 30, 60 oder 120 Minuten pausieren und jederzeit fortsetzen. Die Pause gilt für Benachrichtigungen und Leuchten; ein manueller Leuchttest bleibt möglich. Die Pause endet beim Neustart.
- Tiny-Widget: Mouseover zeigt Name, Verbindungsstatus und verfügbaren Akkustand.
- Widget rastet optional an Bildschirmrändern ein; Einrastabstand einstellbar. Nach Änderungen der Bildschirmaufteilung wird die Position korrigiert.
- Drei unabhängige Tastenkürzel: Wechseln, Kopfhörer 1 aktivieren, Kopfhörer 2 aktivieren. Eigene Zusatztasten und Buchstaben, Erkennung doppelter oder belegter Kombinationen.
- Alle neuen Bedienelemente auf Deutsch, Englisch, Spanisch, Französisch und Ukrainisch.

Globale Tastenkürzel und reale Hardwarewechsel müssen auf einem interaktiven Windows-Desktop geprüft werden. Export/Import ist nicht enthalten.

# Kopfhörer-Akku 1.6.3

- Frei definierbares globales Tastenkürzel für den Kopfhörerwechsel. Strg, Alt, Umschalt und Win kombinierbar, Buchstaben A–Z. Belegte Kürzel werden gemeldet. Zunächst ausgeschaltet.
- Sprachabhängiger Vorschlag: Strg+Alt+K (DE), H (EN), A (ES), C (FR), Y/Н (UK). Gespeicherte Kürzel bleiben bei Sprachwechsel erhalten.

- Neues Standard-Widget mit dunkler Fläche, runden Ecken und größerer Darstellung von Namen, Akkustand und Status.
- Tiny-Vorlage: zwei anklickbare Akkusymbole nebeneinander, standardmäßig 40 Pixel. Größe, Anordnung, Farben und Transparenz bleiben einstellbar.
- Vorlagen unter Anzeige & Widget. Bestehende Gestaltung bleibt erhalten, bis eine Vorlage ausgewählt wird.

# Kopfhörer-Akku 1.6.2

- Getrennte Geräte zeigen einen Strich statt Prozent; unbekannte Verbindungen zeigen keine gespeicherte Prozentzahl als aktuellen Wert. Mouseover kennzeichnet alte Werte als zuletzt bekannt.
- Allgemeine Einstellungen kompakter angeordnet; Sprachauswahl vollständig sichtbar und Reiter scrollbar.

- Optionale Leuchtwarnung im Infobereich: langsames Pulsieren mit frei wählbarer Farbe bei einstellbarem Akkustand (1–90 %, Standard 20 %). Zunächst ausgeschaltet.
- Testknopf zeigt die Pulsfarbe für 30 Sekunden am echten Tray-Symbol; Änderungen sind direkt sichtbar.
- Symbole mit Supersampling und passender Windows-Symbolgröße, geglättete Vorschau.
- Eigener Reiter mit animierter 15-%-Vorschau. Keine zusätzlichen Bluetooth-Abfragen. Nur bei bestätigter Verbindung und bekanntem Akkustand.
- Verbesserte Einzelinstanz-Sperre erkennt auch frühe Projektversionen. Erneuter Start bleibt still.

## English

- Optional low-battery pulse in the system tray with a separate 1–90% threshold, default 20%, initially off. Customizable pulse color. Animated 15% preview; no extra battery queries.
- Single-instance guard also detects early prototypes. Duplicate launches exit silently.

# Kopfhörer-Akku 1.6.1

## Audioausgabe direkt im Widget wechseln

Ein Klick auf Symbol, Namen oder Prozentwert des Kopfhörers im Desktop-Widget wählt dessen Audioausgabe. Das grüne Häkchen zeigt die aktive Ausgabe, auch bei ausgeblendeten Gerätesymbolen.

- Ziehen verschiebt das Widget, ohne die Audioausgabe zu wechseln.
- Doppelklick öffnet nur die Einstellungen.
- Auswahl auch bei gesperrter Widget-Position.
- Dieselben Zuordnungen und derselbe Schalter „Audio-Schnellauswahl“ wie im Infobereich.
- Das Akkuintervall bleibt bei 30–300 Sekunden; bestehende Einstellungen bleiben erhalten.

Mikrofon und separates Kommunikationsgerät bleiben unverändert. Apps mit eigener fester Ausgabe können diese behalten. Downloads weiterhin unsigniert.

## English

Click a headphone icon, name or percentage in the desktop widget to switch its audio output. A green check shows the active output, even with device icons hidden. Dragging moves the widget without switching audio; double-click opens settings only. Selection also works with the position locked. The widget shares the tray mappings and Audio quick selection setting. Existing settings are preserved.

# Kopfhörer-Akku 1.6.0

## Audioausgabe direkt am Akkusymbol wählen

- Ein Klick auf das jeweilige Symbol wechselt die normale Windows-Audioausgabe zu diesem Kopfhörer.
- Grüner Kreis mit weißem Häkchen für die aktive Ausgabe.
- Automatische Zuordnung bei eindeutiger Windows-Gerätekennung; manuelle Auswahl bleibt möglich und wird gespeichert.
- Audio-Schnellauswahl mit Neu laden und Diagnose. Die Geräteübersicht bleibt im Rechtsklick-Menü erreichbar; Doppelklick öffnet die Einstellungen.
- Akkuintervall standardmäßig 30 Sekunden, einstellbar von 30 bis 300 Sekunden. Keine kurzen Zusatzabfragen nach Standby.
- Normale Prüfungen laufen ohne Fortschrittsbanner. Echte Fehler bleiben sichtbar. Update-Prüfung weiterhin nur manuell oder optional beim Start.
- Fehler beim Einlesen der Windows-Audioausgänge behoben; Bluetooth-Ereignisse fremder Geräte ändern die Anzeige nicht mehr.

Mikrofon und separates Kommunikationsgerät bleiben unverändert. Apps mit fest gewählter Audioausgabe können ihre eigene Auswahl behalten. Automatische Zuordnung setzt eine eindeutige gemeinsame Windows-Gerätekennung voraus; andernfalls einmal manuell zuordnen.

## English

Click a tray battery icon to switch Windows audio to that headphone. A green circle with a white check shows the active output. Unique device matches are assigned automatically; manual assignments remain available and are preserved. Right-click for the overview or audio mapping; double-click for settings.

Battery checks default to 30 seconds (adjustable 30–300), without rapid retries or routine progress banners. App update checks remain manual or optional at startup. Microphone and the separate communications device are unchanged; apps with a fixed output may keep their own choice.

Deutsch, English, Español, Français, Українська. Bestehende Einstellungen bleiben erhalten. Installer und portable ZIP sind weiterhin unsigniert.

# 1.4.0

- Ein Klick auf ein Akkusymbol öffnet die Übersicht beider Geräte. Doppelklick öffnet weiter die Einstellungen.
- Eigene Abfragezeiten je Gerät; zuletzt gelesene Werte bleiben bei Trennung als nicht aktuell erkennbar.
- Gezielte Wiederholungsabfragen nach Standby, Entsperren und Bluetooth-Verbindungsereignissen.
- Übersicht in Deutsch, Englisch, Spanisch, Französisch und Ukrainisch.
- Windows kann Akkuwerte zwischenspeichern. Eine erfolgreiche Abfrage bestätigt keine neue Messung am Kopfhörer.
# 1.3.3

- Über-Dialog mit eigenem Logo und anklickbaren Links zu GitHub und Webseite.
- Logo ist in der Programmdatei eingebettet und auch portabel verfügbar.

# 1.3.2

- Eigenes orange-graues Logo für Programm, Installer und Webseite.
- Mehrere Icongrößen für Windows und neues Installer-Layout.
- Keine Änderung der individuell einstellbaren Akkuanzeigen.

# 1.3.1

- Neue einfache Einstellungsansicht als Standard.
- Erweiterte Einstellungen blenden die bisherigen Detailregler ein.
- Nicht gespeicherte Werte bleiben beim Wechsel der Ansicht erhalten.
- Neue Beschriftungen in allen fünf Sprachen.

# 1.3.0

- Spanische, französische und ukrainische Oberfläche und Installertexte.
- Sprachauswahl mit fünf Sprachen; Deutsch bleibt Standard.
- Übersetzte Webseiten und Kurzanleitungen.
- Sprachwechsel, Speicherung und Neustart für alle fünf Sprachen geprüft.

# 1.1.0

## 1.2.0

- Sprachwahl Deutsch/Englisch in den Einstellungen, dauerhaft gespeichert.
- Englische Menüs, Akkuwarnungen, Statusanzeigen, Livevorschau und Widget-Eigenschaften.
- Bestehende Gerätenamen und Einstellungen bleiben erhalten.

- Frei platzierbares Desktop-Widget zusätzlich zur Anzeige im Infobereich.
- Auswahl zwischen Infobereich, Widget oder beidem.
- Einstellbar: Größe, Position, Hintergrund- und Gesamtdeckkraft, Farben, Schriften, Symbolgröße, Abstände, Rahmen, Rundung, Anordnung und sichtbare Inhalte.
- Position verschieben und speichern; Position sperren und Vordergrundverhalten wählen.
- Livevorschau mit transparentem Schachbretthintergrund.

# 1.0.2

- Livevorschau auf jeder Einstellungsseite, mit hellem und dunklem Hintergrund.
- Vorschau mit aktuellem Akkustand oder frei wählbarem Beispielwert.
- Zeichen pro Gerät: Kopfhörer, Musiknote, Lautsprecher, eigener Buchstabe oder keines.
- Zeichen ohne Hintergrundfläche; der Akku bleibt durch die Zwischenräume sichtbar.

# 1.0.1

- Transparenter Hintergrund hinter dem Kopfhörerzeichen.
- Feine Kontrastkontur für helle und dunkle Taskleisten.

# 1.0.0

- Erste Version ohne Beta-Zusatz.
- Rundung, Rahmenstärke, Breite und Höhe der Batterie getrennt einstellbar.
- Arial als Standard für neue Einstellungen; installierte Schriftarten wählbar.
- Bestehende Einstellungen und frühere Batterieformen werden übernommen.

# 0.1.0-beta.3

- Schriftart und Schriftgröße getrennt einstellbar.
- Eigene Schriftfarbe oder automatische Kontrastfarbe.
- Drei Batterieformen mit direkter Vorschau.
- Batteriefarbe weiterhin pro Gerät einstellbar.

# Änderungen

## 0.1.0-beta.2 — 2026-10-17

- Eigene Namen und Farben pro Gerät.
- Alternative Anzeige: nur Zahl mit Kopfhörerzeichen.
- Vorschau und Rücksetzen auf Standard in den Einstellungen.
- Freiwillige Update-Prüfung über GitHub, manuell oder beim Programmstart.
- Versionsvergleich mit Beta-Nummern; keine automatische Installation.
- Alte Einstellungen bleiben kompatibel; Profile sind an Gerätekennungen gebunden.
- Datenschutzangaben um die optionale GitHub-Abfrage ergänzt.

## 0.1.0-beta.1 — 2026-10-17

- Erste zur Weitergabe vorbereitete Open-Source-Version.
- Kombinierte Symbole für bis zu zwei Bluetooth-Kopfhörer.
- Einstellbare Akkuwarnungen und Abfrageintervalle.
- Verbindungserkennung mit explizitem unbekanntem Status.
- Setup ohne Administratorpflicht, Startmenüeintrag, optionaler Autostart und Deinstallation.
- Portable ZIP, lokale Einstellungen und Diagnose auf Anfrage.
- MIT-Lizenz, Datenschutz- und Haftungshinweise, Build- und GitHub-Unterlagen.
- Gerätewechsel und echte Bluetooth-Verbindung noch nicht über verschiedene Rechner validiert.
