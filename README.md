# Kindle Share Plugin für Jellyfin

Ein Plugin für Jellyfin (10.11.X), das es ermöglicht, E-Books direkt aus der Web-Oberfläche an einen Kindle zu senden.

## Features
* **1-Klick Versand:** Button auf der Buch-Detailseite.
* **Format-Schutz:** Prüft automatisch, ob das Format (EPUB, PDF) kompatibel ist.
* **Multi-User:** Jeder Jellyfin-Nutzer kann seine eigene Kindle-Adresse hinterlegen.
* **SMTP & OAuth2:** Unterstützt Gmail, Outlook und eigene Mailserver.

## Installation
1. Gehe im Jellyfin Dashboard zu **Repositories**.
2. Füge ein neues Repository hinzu: `https://raw.githubusercontent.com/Strassbert/Jellyfin.Plugin.Kindle/main/manifest.json`
3. Wechsle zum **Katalog**, suche nach "Kindle Share" und installiere es.
4. Starte Jellyfin neu.

## Konfiguration (Admin)
1. Gehe zu **Dashboard -> Plugins -> Kindle Share**.
2. Trage deine SMTP-Daten ein (Host, Port, User, Passwort).
3. Für Gmail/Outlook: Aktiviere OAuth2 (falls konfiguriert) oder nutze ein App-Passwort.

## Nutzung (Client)
1. Öffne ein Buch in Jellyfin.
2. Klicke auf den **Senden an Kindle** Button.
3. Beim ersten Mal wirst du nach deiner Kindle-E-Mail-Adresse gefragt (z.B. `name@kindle.com`).
4. Stelle sicher, dass die Absender-E-Mail (SMTP-User) in deinem Amazon-Konto unter "Persönliche Dokumente-Einstellungen" freigegeben ist!