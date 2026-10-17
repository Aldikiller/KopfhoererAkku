# Auf GitHub veröffentlichen

Vorgesehenes Konto: **Aldikiller**. Repository-Vorschlag: **KopfhoererAkku**.
Diese Unterlagen bedeuten noch nicht, dass das Repository bereits online ist.

1. Mit dem gewünschten Konto auf GitHub anmelden und ein **öffentliches**,
   leeres Repository `KopfhoererAkku` erstellen. Keine vorgefertigte README oder
   Lizenz hinzufügen; beides ist hier enthalten.
2. Nur diesen Projektordner hochladen. Die persönliche Prototyp-Anwendung,
   Geräteauswahl, Diagnoseprotokolle und andere Arbeitsordner nicht hochladen.
3. Mit authentifizierter GitHub CLI alternativ im Projektordner ausführen:

```powershell
gh auth status
gh repo create Aldikiller/KopfhoererAkku --public --source . --remote origin --push
```

4. Unter Actions den erfolgreichen Build prüfen. Anschließend versionieren:

```powershell
git tag v0.1.0-beta.1
git push origin v0.1.0-beta.1
```

5. Der Workflow erstellt einen **unveröffentlichten Prerelease-Entwurf** mit
   Setup, portabler ZIP und Prüfsummen. Den Testumfang prüfen, danach manuell
   veröffentlichen. Alternativ die lokal gebauten Dateien an einen neuen
   Release-Entwurf anhängen und `docs/RELEASE-NOTES.md` als Beschreibung verwenden.

6. GitHub-Themen z. B. `windows`, `bluetooth`, `battery`, `headphones`, `system-tray`.
   Private vulnerability reporting unter Security aktivieren, falls verfügbar.

Die MIT-Lizenz gestattet auch Dritten kommerzielle Weiterverteilung. Wenn die
Software dauerhaft ausschließlich nichtkommerziell nutzbar sein soll, passt
das nicht zur hier gewählten MIT-Open-Source-Lizenz.

Für eine spätere stabile Version: Praxistests abschließen und gegebenenfalls
eine echte Codesignatur beschaffen. Es wird kein Zertifikat oder Schlüssel
mitgeliefert und keine Vertrauenswürdigkeit durch eine Signatur vorgetäuscht.
