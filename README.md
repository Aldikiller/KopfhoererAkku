# Kopfhörer-Akku

Ein kleines Windows-Tool, das den Akkustand von bis zu zwei Bluetooth-Kopfhörern neben der Uhr anzeigt. So muss man nicht jedes Mal in den Windows-Einstellungen nachsehen.

Kostenlos und Open Source. Von **Aldikiller**.

## Download

**[Installer herunterladen](https://github.com/Aldikiller/KopfhoererAkku/releases/download/v0.1.0-beta.2/KopfhoererAkku-0.1.0-beta.2-Setup.exe)** – mit Installation und Deinstallation.

**[Portable ZIP herunterladen](https://github.com/Aldikiller/KopfhoererAkku/releases/download/v0.1.0-beta.2/KopfhoererAkku-0.1.0-beta.2-portable.zip)** – entpacken und ohne Installation starten.

[Alle Downloads](https://github.com/Aldikiller/KopfhoererAkku/releases) · [Quellcode der Beta](https://github.com/Aldikiller/KopfhoererAkku/releases/download/v0.1.0-beta.2/KopfhoererAkku-0.1.0-beta.2-Quellcode.zip) · [Prüfsummen](https://github.com/Aldikiller/KopfhoererAkku/releases/download/v0.1.0-beta.2/SHA256SUMS.txt)

Die aktuelle Version ist **0.1.0 Beta 2**. Du brauchst Windows 10 ab Version 2004 oder Windows 11, jeweils in 64 Bit, und .NET Framework 4.8.

![So sehen die Symbole aus](docs/Symbole.png)

## So geht's

1. Installer starten oder die portable ZIP vollständig entpacken.
2. Kopfhörer einschalten und mit Windows verbinden.
3. Im Tool bis zu zwei Geräte auswählen und auf **Übernehmen** klicken. Falls ein Gerät mehrmals auftaucht, den Eintrag mit Akkustand nehmen.
4. Sind die Symbole nicht neben der Uhr zu sehen, unter **^** nachschauen und herausziehen.

Ein Doppelklick auf ein Symbol öffnet die Einstellungen. Unter **Namen & Anzeige** kannst du jeden Kopfhörer umbenennen und seine Farbe auswählen. Das ändert nur die Anzeige im Tool, nicht den Bluetooth-Namen in Windows. Dort kannst du auch zwischen der Batterieanzeige und **nur Zahl mit Kopfhörerzeichen** wechseln.

Unter **Updates** kannst du nach neuen Versionen suchen und die GitHub-Downloadseite öffnen. Auf Wunsch prüft das Tool auch bei jedem Programmstart. Das ist zunächst ausgeschaltet. Es installiert nichts automatisch.

 Dort kannst du die Akkuwarnung und das Abfrageintervall ändern. Mit **Test** lässt sich die Windows-Benachrichtigung ausprobieren.

Per Rechtsklick kannst du andere Geräte auswählen, den Autostart ein- oder ausschalten und das Programm beenden. Im Installer ist der Autostart vorausgewählt.

## Was angezeigt wird

Jeder Kopfhörer hat ein eigenes Symbol. Die Zahl im Akku ist der Prozentwert. Wenn du mit der Maus darüberfährst, siehst du den Gerätenamen und den Status.

- **Deine gewählte Farbe:** verbunden (standardmäßig Grün oder Blau).
- **Rot:** verbunden und höchstens 20 % Akku.
- **Grau mit ×:** getrennt.
- **Gelb:** die Verbindung lässt sich gerade nicht feststellen. Eine angezeigte Zahl kann ein gespeicherter Wert sein.
- **? oder –:** es fehlen Daten zum Status oder Akkustand.

![Die Einstellungen](docs/Einstellungen.png)

## Was man wissen sollte

Das Tool liest die Akkuwerte aus Windows. Es funktioniert deshalb nur, wenn Windows für den Kopfhörer einen Wert bereitstellt. Die Werte können gerundet oder etwas älter sein.

Akkuwerte wurden bisher mit JBL Live 770NC und JLab Rewind 2 ausgelesen. Die Verbindungserkennung ist noch nicht auf verschiedenen Rechnern geprüft. Deshalb ist die Version als Beta gekennzeichnet. Akkuwarnungen gibt es nur, wenn die Verbindung bestätigt ist. Windows kann Benachrichtigungen bei „Nicht stören“ unterdrücken.

Die Dateien sind noch nicht digital signiert. Windows kann beim Start einen Warnhinweis anzeigen.

Es gibt keine Werbung und keine Telemetrie. Nur die freiwillige Update-Prüfung fragt GitHub ab. Dabei erhält GitHub deine IP-Adresse und die Programmversion; Gerätenamen und Akkudaten werden nicht gesendet. Deine Einstellungen bleiben auf deinem Rechner. Mehr dazu steht im [Datenschutztext](PRIVACY.md).

## Entfernen

Die installierte Version lässt sich unter **Windows-Einstellungen → Apps** deinstallieren. Die Einstellungen bleiben in `%LOCALAPPDATA%\KopfhoererAkku` erhalten und können bei Bedarf gelöscht werden.

Bei der portablen Version zuerst den Autostart ausschalten, das Tool beenden und dann den Ordner löschen.

Wer noch die frühere Prototyp-Version benutzt, sollte sie vor der Installation beenden und ihren Autostart ausschalten.

## Quellcode und Lizenz

Der Quellcode ist hier im Repository. Mit `build.ps1` lässt sich das Tool selbst bauen. Hinweise dazu stehen unter [Build und Tests](docs/TESTING.md). Fehler kannst du unter [Issues](https://github.com/Aldikiller/KopfhoererAkku/issues) melden. Bitte keine persönlichen Gerätekennungen oder unbereinigten Diagnoseprotokolle hochladen.

Das Projekt wurde mit KI-Unterstützung entwickelt und steht unter der [MIT-Lizenz](LICENSE). Du darfst es unter deren Bedingungen nutzen, verändern und weitergeben. Gewährleistung und Haftung sind soweit gesetzlich zulässig ausgeschlossen. Die [Haftungshinweise](DISCLAIMER.de.md) enthalten die Einzelheiten.

## Code signing policy

Wir bereiten einen Antrag bei SignPath Foundation für kostenlose Open-Source-Signaturen vor. Eine Zusage gibt es noch nicht. Die bisherigen Downloads sind weiterhin unsigniert. Mehr dazu steht in der [Signierrichtlinie](docs/CODE-SIGNING.md).
