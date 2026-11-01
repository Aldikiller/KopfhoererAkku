# Headphone compatibility / Kopfhörer-Kompatibilität

BleAirDock uses the battery information provided by Windows and its Bluetooth driver. Results apply to the tested combination of headphones, Windows and driver; they do not guarantee support for every device variant.

| Model | Battery level | Connection status | One-click audio switching | Test details / source |
| --- | --- | --- | --- | --- |
| JBL Live 770NC | Reported working | Reported working | No separate per-model record | Publisher's everyday use; exact Windows and driver versions not recorded |
| JLab Rewind 2 | Reported working | Reported working | No separate per-model record | Publisher's everyday use; exact Windows and driver versions not recorded |

Documentation updated: 1 November 2026, current release 1.8.0. These are existing reports, not new tests performed today. The publisher has also reported successful general tests of later versions, but no detailed test matrix per model is available.

Other models are **not yet confirmed**, rather than known to be incompatible. Separate left/right earbud and charging-case readings are not generally supported.

## Report your headphones

[Open the compatibility form](https://github.com/Aldikiller/KopfhoererAkku/issues/new?template=compatibility.yml). Working, partly working and failing models are all useful. A GitHub account is needed to submit a report; no account is needed to use BleAirDock.

Include the full model name, app version and Windows version. Report each feature separately:

1. Battery level in the tray and widget. If missing, does Windows itself show a value?
2. Connection status after turning the headphones off and back on.
3. Display recovery after sleep.
4. One-click audio switching, including automatic or manual output mapping.
5. Both devices independently, if you use two headphones.

Mark anything you did not test as **not tested**. A cached battery value or successful Windows query does not establish a fresh measurement by the headphones. No diagnostic log is required. Do not publish Bluetooth addresses, device IDs, product IDs or unredacted logs.

Community reports will be added with a link to the original issue. Please distinguish reports from independent verification; conflicting results should retain their Windows/driver context.

## Deutsch

Die Akkuanzeige und der Verbindungsstatus wurden für **JBL Live 770NC** und **JLab Rewind 2** als funktionierend gemeldet. Genaue Windows- und Treiberversionen fehlen. Für den Audiowechsel liegt kein getrennt dokumentiertes Ergebnis je Modell vor. Andere Kopfhörer sind damit nicht als inkompatibel eingestuft, sondern bisher unbestätigt.

[Eigenes Modell melden](https://github.com/Aldikiller/KopfhoererAkku/issues/new?template=compatibility.yml): Bitte Modell, App- und Windows-Version nennen. Akkuanzeige in Infobereich und Widget, erneutes Verbinden, Rückkehr aus Standby und Audio-Direktwahl einzeln prüfen. Bei zwei Kopfhörern beide unabhängig testen. Nicht durchgeführte Tests als **nicht getestet** kennzeichnen.

Die Liste wurde am 1. November 2026 für den Dokumentationsstand 1.8.0 überarbeitet; das Datum bedeutet keinen neuen Gerätetest. Angaben gelten für die jeweilige Kombination aus Kopfhörer, Windows und Treiber. Gespeicherte Akkuwerte können veraltet sein. Keine Bluetooth-Adressen, Gerätekennungen, Produkt-IDs oder unbereinigten Protokolle veröffentlichen. Ein Protokoll ist nicht erforderlich.
