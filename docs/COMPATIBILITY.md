# Getestete Kopfhörer / Tested headphones

Das Tool liest Akkuwerte, die Windows und der Bluetooth-Treiber bereitstellen. Ein erfolgreicher Test gilt für die jeweilige Kombination aus Gerät, Windows und Treiber. Er garantiert nicht, dass jede Variante desselben Modells funktioniert.

| Kopfhörer | Bisherige Rückmeldung | Windows / Treiber | Quelle |
| --- | --- | --- | --- |
| JBL Live 770NC | Akkuwert und Verbindung funktionieren; vom Herausgeber im Alltag verwendet | Genaue Versionen nicht dokumentiert | Rückmeldung des Herausgebers |
| JLab Rewind 2 | Akkuwert und Verbindung funktionieren; vom Herausgeber im Alltag verwendet | Genaue Versionen nicht dokumentiert | Rückmeldung des Herausgebers |

Stand: 18. Oktober 2026. Der Herausgeber hat nach dem Test von Version 1.4.0 gemeldet, dass alles zu funktionieren scheint. Einzeln dokumentierte Testprotokolle je Gerät und Windows-Version liegen dafür nicht vor.

Andere Modelle sind bisher **nicht bestätigt**. Das bedeutet nicht, dass sie nicht funktionieren. Linker/rechter Ohrhörer und Ladecase werden nicht grundsätzlich getrennt unterstützt.

## Eigenes Modell melden

[Kompatibilitätsmeldung öffnen](https://github.com/Aldikiller/KopfhoererAkku/issues/new?template=compatibility.yml)

Bitte Modell, App-Version, Windows-Version und dein Ergebnis angeben. Hilfreich sind diese kurzen Tests:

1. Kopfhörer verbinden: Werden Name und Akkuwert angezeigt?
2. Ausschalten und erneut verbinden: Stimmt die Verbindung in der Geräteübersicht?
3. Windows in Standby versetzen und fortsetzen: Kommt die Anzeige zurück?
4. Falls zwei Geräte vorhanden sind: Funktionieren beide Anzeigen unabhängig?

Nicht durchgeführte Tests bitte als solche angeben. Bei fehlenden Akkuwerten ergänzen, ob Windows selbst einen Wert zeigt. Windows kann Daten zwischenspeichern; eine erfolgreiche Abfrage ist keine bestätigte neue Messung am Kopfhörer.

Keine Bluetooth-Adressen, Gerätekennungen oder unbereinigten Diagnoseprotokolle veröffentlichen. Für diese Meldung ist kein Protokoll erforderlich.

## English

The owner has reported working battery levels and connection status with **JBL Live 770NC** and **JLab Rewind 2**. Exact Windows and driver versions were not documented. Version 1.4.0 received a positive general test report, without separate per-device test records. Other models are unconfirmed, rather than known to be incompatible.

[Report your model](https://github.com/Aldikiller/KopfhoererAkku/issues/new?template=compatibility.yml), including the app and Windows versions. Check initial connection, reconnect and resume from sleep; mark any tests you did not perform. If a battery value is missing, tell us whether Windows itself displays one. Windows may cache readings. Do not post Bluetooth addresses, device IDs or unredacted logs. No log is required.
