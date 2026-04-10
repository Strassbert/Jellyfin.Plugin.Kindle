# E-Book Share Plugin für Jellyfin

Ein modernes Plugin für Jellyfin (10.11.X+), das es ermöglicht, E-Books direkt aus der Web-Oberfläche an mehrere E-Book Reader zu senden. Mit Multi-Device-Unterstützung, Versendverlauf und Statistiken.

## 🎯 Features

### Benutzer-Features
- **🎯 1-Klick Versand:** Button auf der Buch-Detailseite
- **📱 Multi-Device Support:** Mehrere E-Reader pro Benutzer verwalten
- **📧 E-Mail-Verwaltung:** E-Reader Adressen in modernem Popup konfigurieren
- **📊 Versendverlauf:** Vollständige Geschichte aller Versendungen mit Status
- **✔️ Format-Schutz:** Automatische Überprüfung kompatible Formate (EPUB, PDF, MOBI, AZW, etc.)
- **⚡ Geräte-Auswahl:** Dialog zur Auswahl des Zielgeräts beim Versand
- **🔒 Rate Limiting:** Schutz vor Missbrauch (5 pro Minute, 50 pro Stunde)
- **🌍 Internationalisierung:** Deutsch & Englisch vollständig unterstützt

### Admin-Features
- **📈 System-Statistiken:** Dashboard mit Überblick über alle Versendungen
- **👥 Benutzer-Statistiken:** Pro-Benutzer Metriken und Verlauf
- **🗑️ Verlauf-Verwaltung:** Admin kann Versendverlauf für Benutzer oder gesamt löschen
- **🔐 SMTP-Test:** Button zum Testen der Mail-Verbindung
- **📊 Statistiken anzeigen:** Erfolgsquote, häufigste Formate, aktivste Benutzer, etc.
- **🔑 OAuth2 Support:** Optional Gmail/Outlook OAuth2 (falls konfiguriert)

### Sicherheit
- **🔐 Verschlüsselte Passwörter:** DPAPI-Verschlüsselung für SMTP-Passwörter
- **👤 Benutzer-Isolation:** Benutzer sehen nur ihre eigenen Daten
- **🛡️ Authorization Checks:** Alle Endpoints überprüfen Authentifizierung
- **🔒 Admin-Only Zugriff:** Statistiken und Verlöschung nur für Administratoren

## 📋 Installation

### Variante 1: Via Jellyfin Repository
1. Öffne Jellyfin Dashboard → **Plugins → Repositories**
2. Füge neues Repository hinzu: `https://raw.githubusercontent.com/Strassbert/Jellyfin.Plugin.Kindle/main/manifest.json`
3. Wechsle zu **Katalog**, suche "E-Book Share" und installiere es
4. Starte Jellyfin neu

### Variante 2: Manuelle Installation
1. Lade die neueste Plugin-DLL herunter
2. Kopiere sie in: `/var/lib/jellyfin/plugins/` (Linux) oder `C:\ProgramData\Jellyfin\Server\plugins\` (Windows)
3. Starte Jellyfin neu

## ⚙️ Konfiguration (Admin)

### 1. SMTP-Einstellungen
1. Gehe zu **Dashboard → Plugins → Kindle Share (Einstellungen)**
2. Trage folgende SMTP-Daten ein:
   - **SMTP Host:** smtp.example.com
   - **SMTP Port:** 587 (oder 465 für implizites SSL)
   - **Benutzername:** deine@email.com
   - **Passwort:** [Dein Passwort oder App-Passwort]
   - **SSL/TLS verwenden:** Aktiviert (meist)
   - **Absender E-Mail:** [Wird für "Von:" Feld verwendet]

### 2. SMTP-Verbindung Testen
1. Nach Eingabe der Daten: Klicke **"SMTP-Verbindung testen"**
2. Überprüfe die Fehlermeldung falls etwas schiefgeht
3. Speichern nur wenn Test erfolgreich ist

### 3. Optional: OAuth2 (Gmail/Outlook)
Falls dein System OAuth2 unterstützt:
- Aktiviere **OAuth2-Modus** und trage Client-ID/Secret ein
- Refresh-Token wird automatisch verwaltet

## 👤 Nutzung (Benutzer)

### E-Mail-Adresse konfigurieren

**Option 1: Via Header-Button (Neu!)**
1. Klicke auf das 📧 **E-Mail-Icon** oben rechts (neben Suche)
2. Settings-Popup öffnet sich
3. Trage deine E-Reader Email ein (z.B. `name@kindle.com`)
4. Klicke **"Speichern"**

**Option 2: Geräteverwaltung**
1. Klicke auf das 📧 **E-Mail-Icon** → **"Geräte verwalten"**
2. Klicke **"Gerät hinzufügen"**
3. Trage ein:
   - **Gerätename:** z.B. "Kindle Paperwhite"
   - **Email:** `name@kindle.com`
   - **Format:** EPUB/PDF/MOBI/AZW (bevorzugt)
4. Klicke **"Speichern"**

### Buch versenden

1. Öffne ein **E-Book** in Jellyfin
2. Klicke auf **"An E-Book Reader senden"** Button
3. Wenn mehrere Geräte konfiguriert: Wähle Zielgerät
4. Bestätigung: "Versendet!" oder Fehlermeldung
5. **Wichtig:** Stelle sicher, dass die Absender-E-Mail in deinen E-Reader-Einstellungen genehmigt ist!

### Versendverlauf ansehen

1. Klicke auf das 📧 **E-Mail-Icon** → **"Verlauf anzeigen"**
2. Tabelle mit allen Versendungen:
   - Buch-Titel
   - Ziel-Email
   - Datum/Zeit
   - Status (✅ Erfolgreich / ❌ Fehlgeschlagen)
   - Dateigröße
3. Admins können: **"Verlauf löschen"**

### Admin: Statistiken anzeigen

1. Klicke auf das 📧 **E-Mail-Icon** → **"Statistiken"** (nur für Admins sichtbar)
2. Dashboard zeigt:
   - **Gesamt Versendungen:** Anzahl
   - **Erfolgreich/Fehlgeschlagen:** Counts
   - **Erfolgsquote:** Prozentanteil
   - **Aktive Benutzer:** Eindeutige Benutzer
   - **Aktivster Benutzer:** Mit meisten Versendungen
   - **Häufigstes Format:** EPUB/PDF/etc.
   - **Tägliche Durchschnitt:** Versendungen/Tag
   - **Datenvolumen:** Gesamte übertragene Bytes

## 📧 Konfigurationsbeispiele

### Gmail (SMTP + App-Passwort)
```
Host: smtp.gmail.com
Port: 587
User: deine.email@gmail.com
Passwort: [16-stelliges App-Passwort]
SSL/TLS: Aktiviert
OAuth2: Deaktiviert (optional)
```

**Setup:**
1. Gehe zu https://myaccount.google.com/security
2. Aktiviere "2-Faktor-Authentifizierung"
3. Gehe zu https://myaccount.google.com/apppasswords
4. Generiere App-Passwort für "Mail"
5. Kopiere das 16-stellige Passwort

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
Port: 587 (oder 25/465 je nach Konfiguration)
User: [Mailserver-Benutzer]
Passwort: [Passwort]
SSL/TLS: Je nach Konfiguration
OAuth2: Nein (meist nicht unterstützt)
```

## 🔧 Troubleshooting

### "Keine E-Book Reader E-Mail konfiguriert"
**Symptom:** Fehler beim Klick auf "Senden an E-Book Reader"

**Lösung:**
1. Klicke auf das 📧 **E-Mail-Icon** oben rechts
2. Gib deine E-Reader Email ein (z.B. `name@kindle.com`)
3. Klicke "Speichern"

---

### "SMTP Verbindung fehlgeschlagen"
**Symptom:** Fehler beim Konfigurieren oder Testen von SMTP

**Lösungen:**

**1. Test-Button verwenden:**
- Admin-Panel → Kindle Share → "SMTP Verbindung testen"
- Überprüfe genaue Fehlermeldung

**2. Häufige Ursachen:**
- ❌ **Falscher Host/Port:** Überprüfe SMTP-Server Einstellungen
- ❌ **Falsches Passwort:** Stelle sicher, dass Passwort korrekt ist
- ❌ **Firewall/Port blockiert:** Port 587 (oder 465) muss offen sein
- ❌ **SSL/TLS-Fehler:** Aktiviere/Deaktiviere SSL entsprechend
- ❌ **Gmail-Sperrung:** "Weniger sichere Apps" zulassen oder App-Passwort nutzen

**3. Debug-Logs überprüfen:**
- Dashboard → Logs
- Suche nach "Kindle" oder "SMTP" Fehlermeldungen
- Linux: `journalctl -u jellyfin -f | grep -i kindle`

---

### "E-Mail wird nicht empfangen"
**Symptom:** Plugin sagt "Versendet", aber E-Mail kommt nicht an

**Lösungen:**

**1. Überprüfe Spam-Ordner**
- Auch auf Kindle-Geräten selbst nachschauen

**2. Genehmigte Adressen überprüfen:**
- Gehe zu https://www.amazon.de/hz/mycd/digital-console/contentlist/pdocs/dateDsc (Germany)
- Oder https://www.amazon.com/hz/mycd/digital-console/contentlist/pdocs/dateDsc (US/Int.)
- Einstellungen → "Persönliche Dokumente Einstellungen"
- **WICHTIG:** Stelle sicher, dass die **Absender-E-Mail** (SMTP-User) genehmigt ist!

**3. E-Mail-Adresse überprüfen:**
- Stelle sicher, dass die E-Reader Email korrekt ist
- Beispiel: `meineebooks@kindle.com` (nicht `name@kindle.de`!)
- Manche Reader verwenden `@kindle.de` oder `@kindle.com` - überprüfe dein Amazon-Konto

**4. Format und Größe überprüfen:**
- Datei muss unter 50MB sein
- Format muss kompatibel sein (EPUB, PDF, MOBI, etc.)

---

### "Datei ist zu groß (>50MB)"
**Symptom:** Amazon-Limit überschritten

**Lösungen:**
- PDF-Dateien komprimieren (PDF-Tools)
- EPUB-Dateien aufteilen (Calibre)
- Amazon akzeptiert maximal **50MB pro Datei**

---

### "Dateiformat wird nicht unterstützt"
**Symptom:** Fehler bei bestimmten Dateitypen

**Unterstützte Formate:**
- ✅ EPUB (E-Books)
- ✅ PDF (Documents)
- ✅ MOBI (Kindle Legacy)
- ✅ AZW / AZW3 (Kindle Format)
- ✅ TXT (Plaintext)
- ✅ DOCX (Word Documents)

**Nicht unterstützte Formate:**
- ❌ CBZ/CBR (Comics) - meist nicht unterstützt
- ❌ ZIP/RAR - Archiv-Formate
- ❌ Videos/Musik

---

### "Rate Limit überschritten"
**Symptom:** "Zu viele Anfragen. Maximum 5 Versendungen pro Minute"

**Ursache:** Du hast zu viele Versendungen zu schnell hintereinander versucht

**Lösung:**
- Warte ein paar Minuten und versuche erneut
- Limit: 5 Versendungen pro Minute, 50 pro Stunde pro Benutzer
- Schützt vor Missbrauch

---

### "Gerät nicht gefunden"
**Symptom:** "Gerät nicht gefunden oder hat keine E-Mail konfiguriert"

**Lösungen:**
1. Überprüfe, dass das Gerät noch existiert
   - Klicke auf 📧 → "Geräte verwalten"
   - Stelle sicher, dass das Gerät in der Liste ist
2. Überprüfe, dass die Email des Geräts gesetzt ist
3. Stelle sicher, dass das Gerät aktiv ist (nicht deaktiviert)

---

## 📡 API-Dokumentation

### GET /Kindle/SenderEmail
Ruft die vom Admin konfigurierte Absender-E-Mail ab

**Parameter:** Keine

**Response:**
```json
{
  "senderEmail": "admin@example.com"
}
```

---

### POST /Kindle/Send
Sendet ein Buch an den E-Reader des Benutzers

**Parameter:**
- `itemId` (erforderlich): Jellyfin Item ID
- `userId` (erforderlich): Jellyfin User ID
- `deviceId` (optional): UserDevice ID (falls mehrere Geräte)

**Response bei Erfolg:**
```json
{
  "message": "Sent to E-Book Reader.",
  "messageDe": "An E-Book Reader gesendet."
}
```

**Fehler:**
- `400`: Ungültiges Format / zu groß / keine E-Mail
- `429`: Rate Limit überschritten (5/min, 50/hour)
- `500`: SMTP Fehler (siehe Logs)

---

### GET /Kindle/UserEmail
Abrufen der E-Book Reader E-Mail des aktuellen Benutzers

**Parameter:**
- `userId` (erforderlich)

**Response:**
```json
{
  "email": "name@kindle.com"
}
```

---

### POST /Kindle/UserEmail
E-Book Reader E-Mail für Benutzer speichern

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

---

### DELETE /Kindle/UserEmail
E-Book Reader E-Mail für Benutzer löschen

**Parameter:**
- `userId` (erforderlich)

**Response:**
```json
{
  "message": "E-Book Reader email removed.",
  "messageDe": "E-Book Reader E-Mail entfernt."
}
```

---

### GET /Kindle/Devices
Alle Geräte des Benutzers abrufen

**Parameter:**
- `userId` (erforderlich)

**Response:**
```json
{
  "devices": [
    {
      "id": "device-uuid-1",
      "deviceName": "Kindle Paperwhite",
      "email": "name@kindle.com",
      "preferredFormat": "epub",
      "isDefault": true,
      "createdAt": "2024-01-15T10:00:00",
      "isActive": true
    }
  ]
}
```

---

### POST /Kindle/Devices
Neues Gerät für Benutzer hinzufügen

**Parameter:**
- `userId` (erforderlich)
- **Body (JSON):**
  ```json
  {
    "deviceName": "Mein Kindle",
    "email": "name@kindle.com",
    "preferredFormat": "epub"
  }
  ```

**Response:**
```json
{
  "message": "Device added successfully.",
  "device": { /* UserDevice Objekt */ }
}
```

---

### PUT /Kindle/Devices/{deviceId}
Bestehendes Gerät aktualisieren

**Parameter:**
- `userId` (erforderlich)
- `deviceId` (erforderlich)
- **Body (JSON):**
  ```json
  {
    "deviceName": "Neue Name",
    "email": "neue@email.com",
    "preferredFormat": "pdf",
    "isDefault": true
  }
  ```

---

### DELETE /Kindle/Devices/{deviceId}
Gerät löschen

**Parameter:**
- `userId` (erforderlich)
- `deviceId` (erforderlich)

---

### GET /Kindle/History
Versendverlauf des Benutzers abrufen

**Parameter:**
- `userId` (erforderlich)
- `limit` (optional, default 50): Max. Anzahl Einträge

**Response:**
```json
{
  "logs": [
    {
      "id": "log-uuid",
      "userId": "jellyfin-user-id",
      "itemId": "book-id",
      "fileName": "book.epub",
      "fileSizeBytes": 1048576,
      "recipientEmail": "name@kindle.com",
      "deviceId": "device-uuid",
      "status": 0,
      "sentAt": "2024-01-15T10:30:00",
      "bookTitle": "Example Book",
      "format": "epub"
    }
  ]
}
```

Status Codes:
- `0`: Erfolgreich
- `1`: Fehlgeschlagen
- `2`: Ausstehend

---

### GET /Kindle/Statistics
Statistiken des aktuellen Benutzers

**Response:**
```json
{
  "statistics": {
    "totalSends": 42,
    "successfulSends": 41,
    "failedSends": 1,
    "successRate": 97.6,
    "totalFilesSize": 524288000,
    "lastSendAt": "2024-01-15T10:30:00",
    "lastSentTo": "name@kindle.com",
    "favoriteFormat": "epub"
  }
}
```

---

### GET /Kindle/Statistics/System (Admin Only)
System-weite Statistiken (nur Admin)

**Response:**
```json
{
  "statistics": {
    "totalSends": 500,
    "successfulSends": 485,
    "failedSends": 15,
    "successRate": 97.0,
    "totalFilesSize": 52428800000,
    "uniqueUsers": 12,
    "mostActiveUser": "user123",
    "mostCommonFormat": "epub",
    "averageDailyActivity": 8.3
  }
}
```

---

### POST /Kindle/ValidateSmtp (Admin Only)
Testet die SMTP-Verbindung

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
  "message": "SMTP connection failed: [Error Details]",
  "messageDe": "SMTP-Verbindung fehlgeschlagen: [Fehlerdetails]"
}
```

---

### DELETE /Kindle/History (Admin Only)
Versendverlauf eines Benutzers löschen

**Parameter:**
- `userId` (erforderlich)

---

### DELETE /Kindle/History/All (Admin Only)
Kompletten Versendverlauf löschen

---

## ✨ Was ist neu? (Phase 3)

### Verbesserte User Experience
- 📧 **Header Button:** E-Mail-Icon oben rechts statt verstecktem Menu
- 🎯 **Moderne Popups:** Schöne, responsive Dialoge
- 📱 **Mobile-freundlich:** Vollständig responsive Design
- 🌍 **Mehrsprachig:** Deutsch + Englisch

### Multi-Device Features
- 📱 **Mehrere Geräte:** Bis zu unbegrenzt viele E-Reader pro Benutzer
- 🎯 **Geräte-Auswahl:** Dialog beim Versand wenn mehrere Geräte existieren
- 🛠️ **Geräte-Verwaltung:** UI zum Hinzufügen/Bearbeiten/Löschen von Geräten
- 🎁 **Default-Gerät:** Automatische Auswahl wenn nur ein Gerät

### Versendverlauf & Statistiken
- 📊 **Vollständiger Verlauf:** Alle Versendungen mit Datum, Status, Dateigröße
- ✅ **Status-Indikatoren:** Farbcodiert (grün=erfolg, rot=fehler, gelb=ausstehend)
- 👤 **Benutzer-Statistiken:** Pro-Benutzer Erfolgsquote, lieblingsformat, etc.
- 📈 **Admin-Dashboard:** System-weite Metriken und Überblick

### Sicherheit & Admin-Kontrolle
- 🔐 **Benutzer-Isolation:** Benutzer können nur ihre Daten sehen
- 👮 **Admin-Statistiken:** Nur Admins sehen system-weite Daten
- 🗑️ **Verlauf-Verwaltung:** Admin kann Verlauf für Benutzer oder alle löschen
- 🔒 **Authorization Checks:** Alle Endpoints validieren Benutzer-ID

## 🚀 Bekannte Probleme & Limitierungen

### Limitierungen
- **50MB Größenlimit:** Amazon Kindle akzeptiert maximal 50MB pro Datei
- **Keine Format-Konvertierung:** Keine automatische Konvertierung (z.B. PDF→EPUB)
- **Keine Scheduling:** Kein zeitgesteuerter Versand
- **Kindle-spezifische E-Mails:** Manche Reader brauchen spezielle E-Mail-Adressen

### Behobene Issues (nicht mehr bekannt)
- ✅ **Eine E-Mail pro Benutzer:** Jetzt Multi-Device Support!
- ✅ **Kein Versendverlauf:** Jetzt Vollständiger Verlauf mit Statistiken!
- ✅ **Keine Fehlerbehandlung:** Jetzt Detaillierte Fehler-Logging!

## 🔧 Entwicklung

### Voraussetzungen
- .NET 9.0 SDK
- Visual Studio oder VS Code
- Git

### Bauen
```bash
dotnet build
```

### Tests
```bash
dotnet test
```

### Plugin erstellen
```bash
dotnet publish -c Release -o bin/publish
```
Plugin-DLL: `bin/publish/Jellyfin.Plugin.Kindle.dll`

### Logs überprüfen
```bash
# Jellyfin Dashboard
Dashboard → Logs → Suche "Kindle"

# Linux (systemd)
journalctl -u jellyfin -f | grep -i kindle

# Direktes Logfile
tail -f /var/log/jellyfin/log_*.log | grep -i kindle
```

### Debug-Build
```bash
dotnet build -c Debug
```

## 🔐 Sicherheit

- **Passwort-Verschlüsselung:** DPAPI für SMTP-Passwörter
- **Authentifizierung:** Alle API-Endpoints erfordern Jellyfin-Auth
- **Authorization:** Benutzer sehen nur eigene Daten, Admins nur admin-Daten
- **Rate Limiting:** 5 Versendungen pro Minute, 50 pro Stunde
- **Email-Validierung:** RFC 5322 konforme Validierung
- **SMTP-Test:** Sichere Testfunktion mit 10s Timeout

## 📄 Lizenz

MIT License - Siehe LICENSE Datei

## 👥 Beitragen

Bug Reports und Feature Requests sind willkommen!

Bitte erstelle ein Issue mit:
- Jellyfin Version
- Plugin Version
- Fehlermeldung/Logs
- Reproduktionsschritte

## 📞 Support

- **Dokumentation:** Siehe dieses README
- **Logs:** Dashboard → Logs → Filter "Kindle"
- **Issues:** GitHub Issues auf dem Projekt
