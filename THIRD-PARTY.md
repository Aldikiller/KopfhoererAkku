# Werkzeuge und Abhängigkeiten

Die Anwendung nutzt Windows-APIs und .NET Framework, die auf dem Zielsystem
vorhanden sein müssen. Es werden keine NuGet-Bibliotheken mitgeliefert.

Der Installer wird mit Inno Setup 6.7.3 erzeugt:
https://jrsoftware.org/isinfo.php

Inno Setup und die generierte Installationslaufzeit unterliegen den Bedingungen
des Herstellers: https://github.com/jrsoftware/issrc/blob/is-6_7_3/license.txt
Diese Bestandteile werden nicht als selbst entwickelte MIT-Bestandteile beansprucht.
Der Inno-Compiler selbst gehört nicht zum ausgelieferten Anwendungs- oder Quellpaket.

Die GitHub-Actions-Workflows verwenden offizielle Aktionen von GitHub unter
deren jeweiligen Lizenzen. Die Anwendung selbst benötigt weder GitHub noch
Inno Setup zum Ausführen. Die MIT-Lizenz im Projekt gilt für den eigenen
Anwendungsquellcode, die eigenen Skripte und die eigene Dokumentation.
