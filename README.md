# E-Book Share Plugin für Jellyfin

Ein Plugin für Jellyfin (10.11.X), das es ermöglicht, E-Books direkt aus der Web-Oberfläche an einen E-Book Reader zu senden.

## Features
* **1-Klick Versand:** Button auf der Buch-Detailseite.
* **Format-Schutz:** Prüft automatisch, ob das Format (EPUB, PDF) kompatibel ist.
* **Multi-User:** Jeder Jellyfin-Nutzer kann seine eigene E-Book Reader Adresse hinterlegen.
* **SMTP & OAuth2:** Unterstützt Gmail, Outlook und eigene Mailserver.

## Installation
1. Gehe im Jellyfin Dashboard zu **Repositories**.
2. Füge ein neues Repository hinzu: `https://raw.githubusercontent.com/Strassbert/Jellyfin.Plugin.Kindle/main/manifest.json`
3. Wechsle zum **Katalog**, suche nach "E-Book Share" und installiere es.
4. Starte Jellyfin neu.

## Konfiguration (Admin)
1. Gehe zu **Dashboard -> Plugins -> Kindle Share**.
2. Trage deine SMTP-Daten ein (Host, Port, User, Passwort).
3. Für Gmail/Outlook: Aktiviere OAuth2 (falls konfiguriert) oder nutze ein App-Passwort.

## Nutzung (Client)
1. Öffne ein Buch in Jellyfin.
2. Klicke auf den **Senden an E-Book Reader** Button.
3. Beim ersten Mal wirst du nach deiner E-Book Reader E-Mail-Adresse gefragt (z.B. `name@kindle.com`).
4. Stelle sicher, dass die Absender-E-Mail (SMTP-User) in deinem Amazon-Konto unter "Persönliche Dokumente-Einstellungen" freigegeben ist!

## Konfigurationsbeispiele

### Gmail (SMTP + App-Passwort)
```
Host: smtp.gmail.com
Port: 587
User: deine.email@gmail.com
Passwort: [App-Passwort aus Google Account Security]
SSL/TLS: Aktiviert
OAuth2: Deaktiviert
```

**Anleitung:**
1. Gehe zu https://myaccount.google.com/security
2. Aktiviere "2-Faktor-Authentifizierung"
3. Gehe zu https://myaccount.google.com/apppasswords
4. Generiere ein App-Passwort für "Mail"
5. Kopiere das 16-stellige Passwort hier ein

### Outlook/Hotmail (SMTP)
```
Host: smtp-mail.outlook.com
Port: 587
User: deine.email@outlook.com
Passwort: [Dein Outlook-Passwort]
SSL/TLS: Aktiviert
OAuth2: Deaktiviert
```

### Eigener Mailserver (z.B. Postfix)
```
Host: mail.deine-domain.de
Port: 587 (oder 25/465)
User: [Benutzer auf deinem Server]
Passwort: [Passwort]
SSL/TLS: Je nach Konfiguration
OAuth2: Nein (meist nicht unterstützt)
```

## Troubleshooting

### "Keine E-Book Reader E-Mail konfiguriert"
**Symptom:** Fehler beim Klick auf "Senden an E-Book Reader"  
**Lösung:**
1. Klicke auf das **Email-Icon** oben rechts (neben der Suche)
2. Gib deine E-Book Reader E-Mail-Adresse ein (z.B. `name@kindle.com`)
3. Klicke "Speichern"

### "Datei ist zu groß (>50MB)"
**Symptom:** Amazon-Limit überschritten  
**Lösung:**
- PDF-Dateien komprimieren
- EPUB-Dateien aufteilen
- Amazon akzeptiert maximal 50MB pro Datei (Limit)

### "SMTP Verbindung fehlgeschlagen"
**Symptom:** Fehler beim Versuch zu senden  
**Lösungen:**
1. **Test der SMTP-Verbindung:**
   - Admin-Panel → Kindle Share → "SMTP Verbindung testen"
   - Überprüfe die Fehlermeldung

2. **Häufige Ursachen:**
   - **Falscher Host/Port:** Überprüfe die Einstellungen deines Mailservers
   - **Falsches Passwort:** Stelle sicher, dass das Passwort korrekt ist
   - **Firewall/Port blockiert:** Port 587 muss offensein
   - **SSL/TLS-Fehler:** Aktiviere/Deaktiviere SSL entsprechend
   - **Gmail-Sperrung:** Weniger sichere Apps zulassen oder App-Passwort nutzen

3. **Überprüfe die Jellyfin-Logs:**
   - Dashboard → Logs
   - Suche nach "Kindle" oder "SMTP" Fehlern

### "E-Mail wird nicht empfangen"
**Symptom:** Plugin sagt "Gesendet", aber E-Mail kommt nicht an  
**Lösungen:**
1. **Überprüfe Spam-Ordner** (auch bei Kindle-Geräten)
2. **Genehmigte Adressen prüfen:**
   - Gehe zu https://www.amazon.de/hz/mycd/digital-console/contentlist/pdocs/dateDsc
   - Einstellungen → Persönliche Dokumente Einstellungen
   - Stelle sicher, dass die **Absender-E-Mail** (SMTP-User) freigegeben ist
3. **E-Mail-Adresse überprüfen:**
   - Stelle sicher, dass die Kindle-E-Mail korrekt eingetragen ist
   - Beispiel: `meineebooks@kindle.com` (nicht `name@kindle.de`!)

### "Dateiformat wird nicht unterstützt"
**Symptom:** Fehler beim Senden bestimmter Dateitypen  
**Unterstützte Formate:**
- E-Books: EPUB, PDF, MOBI, AZW, AZW3, TXT, DOCX
- Format automatisch überprüft vor Versand
- Nicht unterstützte Formate werden mit Fehlermeldung abgelehnt

## API-Dokumentation

### POST /Kindle/Send
Sendet ein Buch an die E-Book Reader Adresse des Benutzers

**Parameter:**
- `itemId` (erforderlich): Jellyfin Item ID
- `userId` (erforderlich): Jellyfin User ID

**Response:**
```json
{
  "message": "Sent to E-Book Reader.",
  "messageDe": "An E-Book Reader gesendet."
}
```

**Fehler:**
- 400: Ungültiges Format, Datei zu groß, keine E-Mail konfiguriert
- 429: Rate Limit überschritten (max. 5 pro Minute)
- 500: SMTP-Fehler (siehe Logs)

### GET /Kindle/UserEmail
Abrufen der E-Book Reader E-Mail-Adresse des Benutzers

**Parameter:**
- `userId` (erforderlich)

**Response:**
```json
{
  "email": "name@kindle.com"
}
```

### POST /Kindle/UserEmail
E-Book Reader E-Mail-Adresse für Benutzer speichern

**Parameter:**
- `userId` (erforderlich)
- `email` (erforderlich): Gültige E-Mail-Adresse

**Response:**
```json
{
  "message": "E-Book Reader email saved.",
  "messageDe": "E-Book Reader-E-Mail gespeichert."
}
```

### DELETE /Kindle/UserEmail
E-Book Reader E-Mail-Adresse für Benutzer löschen

**Parameter:**
- `userId` (erforderlich)

**Response:**
```json
{
  "message": "E-Book Reader email removed.",
  "messageDe": "E-Book Reader E-Mail entfernt."
}
```

### POST /Kindle/ValidateSmtp (Admin Only)
Testet die SMTP-Verbindung mit den konfigurierten Einstellungen

**Parameter:** Keine

**Response bei Erfolg:**
```json
{
  "success": true,
  "message": "SMTP connection successful!",
  "messageDe": "SMTP-Verbindung erfolgreich!"
}
```

**Response bei Fehler:**
```json
{
  "success": false,
  "message": "SMTP connection failed: [Fehlerdetails]",
  "messageDe": "SMTP-Verbindung fehlgeschlagen: [Fehlerdetails]"
}
```

## Bekannte Probleme & Einschränkungen

- **50MB Größenlimit:** Amazon Kindle akzeptiert maximal 50MB pro Datei
- **Eine E-Mail pro Benutzer:** Aktuell kann nur eine E-Mail-Adresse pro Benutzer hinterlegt werden
- **Format-Konvertierung:** Keine automatische Konvertierung zwischen Formaten (z.B. PDF→EPUB)
- **Scheduling:** Kein automatischer Versand zu bestimmten Zeiten
- **Versandhistorie:** Keine Übersicht gesendeter Dateien (nur in Jellyfin-Logs)

## Entwicklung

### Voraussetzungen
- .NET 9.0 SDK
- Visual Studio oder VS Code

### Bauen
```bash
dotnet build
```

### Testen
```bash
dotnet test
```

### Plugin für Jellyfin vorbereiten
```bash
dotnet publish -c Release -o bin/publish
```

Die Plugin-DLL ist unter `bin/publish/Jellyfin.Plugin.Kindle.dll`

### Logs überprüfen
- Dashboard → Logs
- Oder Terminal: `journalctl -u jellyfin -f` (Linux systemd)

## Sicherheit

- SMTP-Passwörter werden verschlüsselt gespeichert
- Rate Limiting schützt vor Missbrauch (5 Versendungen pro Minute)
- Alle API-Endpoints erfordern Authentifizierung
- Admin-Endpoint (`ValidateSmtp`) erfordert Administrator-Rolle

## Lizenz

[Deine Lizenz hier - z.B. MIT]
