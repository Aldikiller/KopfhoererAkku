# Kopfhörer-Akku

**Version actuelle : 1.6.2**

## Nouveautés de la version 1.6.2

Alerte lumineuse facultative dans la zone de notification, avec couleur personnalisable et seuil indépendant de 1 à 90 %. Dans Alerte lumineuse, testez l’icône réelle pendant 30 secondes et ajustez la couleur en direct. Désactivée initialement ; aucune requête Bluetooth supplémentaire.

Les casques déconnectés affichent un tiret au lieu d’un pourcentage actuel. Les valeurs mémorisées sont signalées comme le dernier niveau connu au survol. Si la connexion est inconnue, une valeur mémorisée n’est pas présentée comme une mesure actuelle.

Icônes plus lisses, paramètres compacts avec un sélecteur de langue entièrement visible et meilleure protection contre les lancements multiples.

## Sélection audio aussi dans le widget · 1.6.2

Cliquez sur le symbole, le nom ou le pourcentage du casque dans le widget pour sélectionner sa sortie audio. Une coche verte indique la sortie active. Glisser déplace le widget sans changer le son ; un double clic ouvre les paramètres. La sélection fonctionne aussi lorsque la position est verrouillée. Le widget utilise les mêmes associations et le même réglage de sélection rapide que les icônes de notification.

## Changer de sortie audio en un clic

Cliquez sur une icône de batterie pour envoyer le son de Windows vers ce casque. Un cercle vert avec une coche blanche indique la sortie active. Les correspondances uniques sont attribuées automatiquement ; sinon, choisissez la sortie une fois dans la sélection audio rapide. Les associations manuelles sont conservées.

![Audio output](Audio-Direktwahl-1.6.0.png)

La batterie est vérifiée toutes les 30 secondes par défaut, avec un intervalle réglable de 30 à 300 secondes. Sans vérifications rapides supplémentaires ni messages de progression habituels. Les mises à jour sont vérifiées manuellement ou, en option, au démarrage. · Français

[Télécharger](https://github.com/Aldikiller/KopfhoererAkku/releases/latest) · [Code source](https://github.com/Aldikiller/KopfhoererAkku) · [Questions et retours](https://github.com/Aldikiller/KopfhoererAkku/discussions)

Affichez la batterie de deux casques Bluetooth au maximum près de l’horloge Windows, dans un widget de bureau ou dans les deux. Une petite application d’Aldikiller.

## Changer de langue

Ouvrez Einstellungen → Allgemein → Sprache / Language, choisissez Français puis cliquez sur Speichern. L’interface change immédiatement et conserve ce choix. L’allemand reste la langue par défaut.

## Installation

Téléchargez l’installateur ou extrayez entièrement le ZIP portable. Allumez les casques, connectez-les à Windows et choisissez jusqu’à deux appareils dans l’application. En cas de doublons, prenez l’entrée affichant un niveau de batterie.

## À votre goût

Personnalisez les noms, couleurs, symboles, polices et la forme de la batterie. L’aperçu montre les changements avant leur enregistrement.

## Widget et paramètres

Double-cliquez sur une icône pour ouvrir les paramètres ; un clic droit ouvre le menu. Dans Affichage et widget, choisissez la zone de notification, le widget ou les deux. Réglez taille, position, transparence, polices et disposition. Pour un fond transparent, mettez l’opacité du fond et l’épaisseur de bordure à zéro.

## Prévenu à temps

Alertes de batterie faible, démarrage facultatif avec Windows et recherche volontaire de mises à jour. Aucune mise à jour n’est installée automatiquement.

## Configuration et limites

Windows 10 à partir de la build 19041 ou Windows 11, x64, avec .NET Framework 4.8. Seules les données fournies par Windows et le pilote sont affichées. Testé avec JBL Live 770NC et JLab Rewind 2. Les autres modèles ne sont pas encore confirmés. Les valeurs peuvent être retardées. Les fichiers ne sont pas encore signés numériquement. Windows peut afficher un avertissement.

## Aide

Si un appareil manque, vérifiez sa connexion Windows, sélectionnez-le à nouveau et attendez la prochaine vérification. Cherchez les icônes masquées sous ^ près de l’horloge. Les alertes nécessitent une connexion confirmée ; Ne pas déranger peut les masquer.

## Confidentialité et licence

Sans publicité ni télémétrie. Les paramètres restent sur votre ordinateur. La recherche facultative de mises à jour est désactivée par défaut. Si vous l’utilisez, GitHub reçoit votre adresse IP et la version de l’application, sans noms d’appareils ni niveaux de batterie. Open source sous licence MIT, sans garantie dans les limites autorisées par la loi.

## Désinstallation

Supprimez la version installée via Paramètres Windows → Applications. Les paramètres restent dans %LOCALAPPDATA%\KopfhoererAkku. Pour la version portable, désactivez le démarrage automatique, quittez l’application puis supprimez son dossier.

Indiquez le modèle du casque, les versions de Windows et de l’application et les étapes du problème. Ne publiez pas d’adresses Bluetooth ni de données privées.

MIT: [Code source](https://github.com/Aldikiller/KopfhoererAkku/blob/main/LICENSE)

