# E-Book Share für Jellyfin

Ein Plugin für Jellyfin 10.11.x, das E-Books direkt aus der Weboberfläche per E-Mail an
einen E-Book-Reader (Kindle, Kobo, PocketBook …) schickt.

## Funktionen

* **Ein Klick auf der Buch-Detailseite** – der Button erscheint automatisch bei Büchern.
* **Prüfung vor dem Klick** – Format und Dateigröße werden serverseitig geprüft, bevor
  der Button aktiv wird. Kein Fehlschlag erst nach dem Senden.
* **Pro Benutzer eine eigene Adresse** – niemand sieht oder ändert die Adresse eines
  anderen.
* **SMTP mit wählbarer Verschlüsselung** – STARTTLS (587), SSL/TLS ab Verbindungsaufbau
  (465), automatisch oder unverschlüsselt für lokale Relays.
* **Verbindungstest und Testnachricht** in der Admin-Oberfläche.
* **Einträge im Jellyfin-Aktivitätsfeed** bei jedem Versand.
* **Deutsch und Englisch**, abhängig von der in Jellyfin eingestellten Sprache.

## Installation

1. Im Jellyfin-Dashboard zu **Plugins → Repositories** wechseln.
2. Repository hinzufügen:
   `https://raw.githubusercontent.com/Strassbert/Jellyfin.Plugin.Kindle/main/manifest.json`
3. Unter **Katalog** nach „E-Book Share“ suchen und installieren.
4. Jellyfin neu starten.

## Konfiguration (Administrator)

**Dashboard → Plugins → E-Book Share**

1. Anbieter-Vorlage wählen (Gmail, Outlook, GMX, WEB.DE, mailbox.org) oder Host, Port
   und Verschlüsselung selbst eintragen.
2. Benutzername und Passwort eintragen. **Wichtig:** Gmail, Outlook und die meisten
   Anbieter lehnen das normale Kontopasswort ab – es wird ein *App-Passwort* benötigt.
3. Absenderadresse eintragen (leer = Benutzername wird verwendet).
4. **Verbindung testen** klicken. Der Test verwendet die *gespeicherten* Einstellungen,
   also vorher speichern.
5. **Testnachricht senden** — das ist der wichtigere Test. Ein Verbindungstest beweist
   nur, dass die Anmeldung klappt; er kann nicht erkennen, dass Amazon deine
   Absenderadresse ablehnt und die Mail kommentarlos verwirft.

Das SMTP-Passwort wird der Konfigurationsseite nie zurückgegeben. Ein leeres
Passwortfeld beim Speichern bedeutet „unverändert"; zum Entfernen gibt es eine eigene
Checkbox.

### Verschlüsselung richtig wählen

| Port | Einstellung |
|------|-------------|
| 587  | STARTTLS |
| 465  | SSL/TLS ab Verbindungsaufbau |
| andere | Automatisch |

Bis einschließlich Version 1.2.0.0 kannte das Plugin nur STARTTLS – Port 465 konnte
deshalb nie funktionieren.

## Nutzung

1. **Einstellungen → E-Book Share** öffnen und die Adresse des Readers eintragen
   (z. B. `name@kindle.com`).
2. Ein Buch öffnen und **An Reader senden** klicken.

Beim ersten Senden fragt das Plugin die Adresse direkt im Dialog ab, falls sie noch
nicht hinterlegt ist. Über **Testnachricht senden** auf derselben Seite lässt sich
prüfen, ob die Zustellung überhaupt funktioniert, ohne ein ganzes Buch zu verschicken.

> **Amazon:** Die Absenderadresse des Servers muss unter *Meine Inhalte und Geräte →
> Einstellungen → Persönliche Dokumente-Einstellungen* als genehmigter Absender
> eingetragen sein. Amazon verwirft Mails von nicht genehmigten Absendern kommentarlos.
> Die Seite „E-Book Share“ in den Benutzereinstellungen zeigt an, welche Adresse dieser
> Server verwendet.

## Grenzen

* Amazon erlaubt 50 MB pro E-Mail. Weil Anhänge beim Kodieren um rund 37 % wachsen,
  liegt die größte sendbare Datei bei etwa **36 MB**. Der Wert wird in der
  Admin-Oberfläche angezeigt und ist konfigurierbar.
* Unterstützte Formate: EPUB, PDF, TXT, DOC, DOCX, RTF, HTM(L), MOBI, AZW, AZW3, KPF
  sowie PNG, JPG, GIF, BMP. Amazon nimmt MOBI/AZW für neue Dokumente nicht mehr an,
  andere Reader schon.

## Entwicklung

```bash
dotnet build -c Release                                   # Jellyfin 10.11 (Standard)
dotnet build -c Release -p:JellyfinTarget=jf12            # Jellyfin 12 (Kompatibilitätstest)
dotnet test                                               # Unit-Tests
bash scripts/package.sh                                   # Release-Zip inkl. meta.json + Prüfsumme
python3 scripts/verify_version.py                         # Versions-/Identitätsprüfung
python3 scripts/verify_strings.py                         # Übersetzungsprüfung
```

### Jellyfin 12

Aus derselben Quelle lassen sich zwei Artefakte bauen: `jf10` (Jellyfin 10.11, .NET 9)
und `jf12` (Jellyfin 12, .NET 10). Veröffentlicht wird zurzeit **nur `jf10`** — Jellyfin
12 ist noch ein Release Candidate. Die CI kompiliert den `jf12`-Zweig trotzdem bei jedem
Build, damit ein API-Bruch sofort auffällt und nicht erst am Release-Tag.

### Übersetzungen beitragen

Alle Oberflächentexte liegen in `Localization/<sprachcode>.json` und werden von allen
drei Frontends (Button, Admin-Seite, Benutzerseite) gemeinsam genutzt. Eine neue Sprache
ist eine Kopie von `en.json` mit übersetzten Werten — keine Codeänderung nötig. Die
Datei wird automatisch eingebettet, `scripts/verify_strings.py` prüft im Build, dass
keine Schlüssel fehlen oder überflüssig sind.

### Version anheben

`<Version>`, `<AssemblyVersion>` und `<FileVersion>` in
`Jellyfin.Plugin.Kindle.csproj` **gemeinsam** ändern, committen und einen Tag mit
genau dieser Version pushen:

```bash
git tag 1.4.0.0 && git push origin 1.4.0.0
```

Der Release-Workflow baut das Zip, berechnet die MD5-Prüfsumme, trägt den Eintrag in
`manifest.json` ein und legt den GitHub-Release an.

> **Warum das erzwungen wird:** In Release 1.2.0.0 stand im Manifest 1.2.0.0, während
> die Assembly noch 1.1.0.0 auswies. Jellyfin zeigt im Dashboard die *Assembly*-Version
> an, löst `DELETE /Plugins/{id}/{version}` aber gegen `meta.json` auf, das aus dem
> Manifest erzeugt wird. Die beiden Werte passten nicht zusammen, `GetPlugin` fand
> nichts und das **Deinstallieren schlug still mit 404 fehl** – ebenso Aktivieren und
> Deaktivieren. `scripts/verify_version.py` läuft in jedem Build und bricht bei jeder
> Abweichung ab.

## Lizenz

MIT – siehe [LICENSE](LICENSE).
