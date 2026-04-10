# Development Guide

Guide für Entwickler, die zum Jellyfin Kindle Plugin beitragen möchten.

## 📋 Inhaltsverzeichnis

1. [Entwicklung einrichten](#entwicklung-einrichten)
2. [Projektstruktur](#projektstruktur)
3. [Technologie-Stack](#technologie-stack)
4. [Bauen & Testen](#bauen--testen)
5. [Code-Konventionen](#code-konventionen)
6. [Git Workflow](#git-workflow)
7. [Testing](#testing)
8. [Debugging](#debugging)

## 🚀 Entwicklung einrichten

### Voraussetzungen

- **OS:** Linux, macOS, oder Windows
- **.NET:** 9.0 SDK oder höher
- **Git:** Version 2.30+
- **IDE:** Visual Studio Code, Visual Studio, oder JetBrains Rider
- **Jellyfin:** 10.11.0+ (optional, für manuelles Testen)

### 1. Repository klonen

```bash
git clone https://github.com/Strassbert/Jellyfin.Plugin.Kindle.git
cd Jellyfin.Plugin.Kindle
```

### 2. Dependencies installieren

```bash
dotnet restore
```

### 3. IDE Setup

#### Visual Studio Code
```bash
code .
# Installiere C# Extension
```

#### Visual Studio
```bash
# Öffne .sln Datei
start Jellyfin.Plugin.Kindle.sln
```

#### JetBrains Rider
```bash
# Öffne mit Rider
rider .
```

## 📁 Projektstruktur

```
Jellyfin.Plugin.Kindle/
├── Api/
│   ├── KindleController.cs           # REST API Endpoints
│   ├── Requests/                     # Request DTOs
│   └── Responses/                    # Response DTOs
├── Configuration/
│   ├── PluginConfiguration.cs        # Plugin Settings
│   ├── HtmlInjectionMiddleware.cs    # Script Injection
│   └── PluginServiceRegistrator.cs   # DI Container Setup
├── Models/
│   ├── UserDevice.cs                 # Device Model
│   ├── SendLog.cs                    # History Logging
│   └── KindleFormatValidator.cs      # Format Validation
├── Services/
│   ├── KindleMailService.cs          # SMTP Mail Service
│   ├── SendHistoryService.cs         # History & Statistics
│   ├── RateLimitingService.cs        # Rate Limiting
│   └── KindleSecurityService.cs      # Password Encryption
├── Web/
│   ├── kindleButton.js               # Frontend Script
│   ├── kindleButton.css              # Frontend Styles
│   └── ClientStyles                  # CSS Injection Endpoint
├── Plugin.cs                         # Main Plugin Class
├── Jellyfin.Plugin.Kindle.csproj     # Project File
├── README.md                         # User Documentation
├── CHANGELOG.md                      # Version History
└── DEVELOPMENT.md                    # This File
```

## 🛠️ Technologie-Stack

### Backend
- **Framework:** .NET 9.0 (C#)
- **Web:** ASP.NET Core
- **JSON:** System.Text.Json
- **Logging:** Microsoft.Extensions.Logging
- **DI:** Microsoft.Extensions.DependencyInjection
- **Auth:** Jellyfin Authorization Policies

### Frontend
- **Language:** Vanilla JavaScript (ES5+)
- **CSS:** Standard CSS3 mit Responsive Design
- **i18n:** Custom implementation mit JSON
- **Build:** None (loaded directly in browser)

### Testing
- **Framework:** xUnit
- **Mocking:** Moq
- **Assertions:** Fluent Assertions

## 🔨 Bauen & Testen

### Debug Build
```bash
dotnet build
# oder
dotnet build -c Debug
```

### Release Build
```bash
dotnet publish -c Release -o bin/publish
# Plugin-DLL: bin/publish/Jellyfin.Plugin.Kindle.dll
```

### Tests ausführen
```bash
# Alle Tests
dotnet test

# Einzelne Test-Suite
dotnet test --filter Category=Services

# Mit Verbose Output
dotnet test -v detailed

# Code Coverage
dotnet test /p:CollectCoverage=true
```

### Clean Build
```bash
dotnet clean
dotnet build
```

## 📝 Code-Konventionen

### C# Style

**Naming:**
```csharp
// Public classes: PascalCase
public class KindleController { }

// Private fields: _camelCase
private readonly ILogger _logger;

// Methods: PascalCase
public async Task<IActionResult> SendToKindle() { }

// Local variables: camelCase
var sendLog = new SendLog();

// Constants: UPPER_SNAKE_CASE
private const long MaxFileSizeBytes = 50L * 1024 * 1024;
```

**Formatting:**
- Indentation: 4 spaces
- Line length: 120 characters preferred
- Braces: Allman style
```csharp
if (condition)
{
    DoSomething();
}
else
{
    DoSomethingElse();
}
```

**Comments:**
```csharp
/// <summary>
/// Does something important
/// </summary>
public void DoSomething()
{
    // Implementation
}
```

### JavaScript Style

**Naming:**
```javascript
// Classes/Constructors: PascalCase
function MyClass() { }

// Functions: camelCase
function doSomething() { }

// Constants: UPPER_SNAKE_CASE
const MAX_RETRIES = 3;

// Variables: camelCase
var userName = "John";
```

**Formatting:**
- Indentation: 4 spaces (or 2, be consistent)
- Line length: 100 characters preferred
- Use semicolons
- Use === instead of ==

```javascript
function calculateTotal(items) {
    var total = 0;
    for (var i = 0; i < items.length; i++) {
        total += items[i].price;
    }
    return total;
}
```

### CSS Style

```css
/* Class naming: kebab-case */
.kindle-settings-popup {
    padding: 1em;
    border-radius: 0.3em;
}

.kindle-popup-button {
    cursor: pointer;
}

/* Responsive design with mobile-first */
@media (max-width: 450px) {
    .kindle-settings-popup {
        width: 100%;
    }
}
```

## 📦 Git Workflow

### Branch Naming

```
feature/description      - Neue Features
fix/issue-description    - Bugfixes
refactor/improvement     - Code Refactoring
docs/documentation       - Dokumentation
test/test-description    - Tests
chore/maintenance        - Maintenance Tasks
```

### Commit Messages

**Format:**
```
[Type] Short description (max 50 chars)

Longer explanation if needed (max 72 chars per line)

- Bullet point 1
- Bullet point 2

Fixes #123
```

**Types:**
- `feat:` - Neue Feature
- `fix:` - Bugfix
- `refactor:` - Code-Umstrukturierung
- `docs:` - Dokumentation
- `test:` - Tests hinzufügen/ändern
- `chore:` - Dependencies, Config, etc.
- `perf:` - Performance-Verbesserung

**Beispiel:**
```
feat: Add multi-device support for users

Implement UserDevice model with full CRUD operations
Add device management UI in settings popup
Support device selection during book send operation

- Create GET/POST/PUT/DELETE /Kindle/Devices endpoints
- Add device list, add, edit, delete modals
- Show device selection dialog for multiple devices
- Auto-select single device

Fixes #42, Fixes #89
```

### Pull Request Process

1. Fork das Projekt
2. Erstelle Feature Branch (`git checkout -b feature/amazing`)
3. Commit Changes (`git commit -am 'feat: Add amazing feature'`)
4. Push to Branch (`git push origin feature/amazing`)
5. Öffne Pull Request auf GitHub
6. Warte auf Review

## 🧪 Testing

### Unit Tests Schreiben

```csharp
public class SendHistoryServiceTests
{
    [Fact]
    public void LogSend_WithValidLog_ShouldAddToHistory()
    {
        // Arrange
        var config = new PluginConfiguration();
        var logger = new Mock<ILogger<SendHistoryService>>();
        var service = new SendHistoryService(config, logger.Object);
        var log = new SendLog { UserId = "user1" };

        // Act
        service.LogSend(log);

        // Assert
        Assert.Single(config.SendLogs);
        Assert.Equal("user1", config.SendLogs[0].UserId);
    }
}
```

### Integration Tests

```csharp
[Collection("API Tests")]
public class KindleControllerTests
{
    private readonly KindleController _controller;

    public KindleControllerTests()
    {
        // Setup test dependencies
        var config = new PluginConfiguration();
        var mailService = new Mock<IKindleMailService>();
        _controller = new KindleController(/* deps */);
    }

    [Fact]
    public async Task SendToKindle_WithValidItem_ShouldReturn200()
    {
        // Test implementation
    }
}
```

## 🐛 Debugging

### Visual Studio Debug Session

1. Öffne `launchSettings.json`
2. Konfiguriere Jellyfin-Verbindung
3. Setze Breakpoints
4. F5 zum Starten

### Console Logging

```csharp
_logger.LogInformation("Processing send - ItemId: {ItemId}", itemId);
_logger.LogError(ex, "Failed to send book");
```

### Jellyfin Logs

```bash
# Dashboard
Jellyfin Dashboard → Logs → Suche "Kindle"

# Linux (systemd)
journalctl -u jellyfin -f | grep -i kindle

# Windows
# %appdata%\Jellyfin\config\logs\*.log
```

### Frontend Debugging

```javascript
// Browser Console
console.log('[Kindle] Debug message:', variable);

// Check API responses
ApiClient.ajax({...}).then(result => {
    console.log('API Response:', result);
});
```

**Browser DevTools:**
- F12 zum Öffnen
- Console Tab für Logs
- Network Tab für API Calls
- Sources Tab zum Step-Through Debugging

## 📚 Wichtige Dateien

### KindleController.cs
- Alle REST API Endpoints
- Request Validation
- Authorization Checks
- Error Handling

**Zu beachten:**
- Alle Queries müssen User ID validieren
- Rate Limiting überprüfen
- Proper HTTP Status Codes zurückgeben
- JSON Response Format konsistent halten

### kindleButton.js
- Frontend UI Logik
- API Integration
- i18n Strings
- Event Handling

**Zu beachten:**
- MutationObserver Cleanup
- Event Handler Memory Leaks
- API Response Field Names (camelCase!)
- Error Handling für alle API Calls

### SendHistoryService.cs
- Send Logging
- Statistics Berechnung
- History Management

**Zu beachten:**
- Thread-safe Operations
- Retention Policy anwenden
- Configuration.SaveConfiguration() nach Changes

## 🔒 Security Considerations

### Dos
- ✅ Überprüfe User Authorization auf jedem Endpoint
- ✅ Validiere alle Inputs
- ✅ Verschlüssele Passwörter
- ✅ Rate Limiting nutzen
- ✅ Detailed Error Logs, aber sichere Info in Response

### Don'ts
- ❌ Keine Passwords in Logs
- ❌ Keine direkten File System Paths in Error Messages
- ❌ Keine SQL Injection Risiken (nicht relevant hier)
- ❌ Cross-User Data Access zulassen
- ❌ Admin-Only Daten Normalusern geben

## 📖 API Documentation

Bei neuen Endpoints dokumentieren in README.md:

```markdown
### POST /Kindle/NewEndpoint
Kurze Beschreibung

**Parameter:**
- param1 (required)

**Response:**
\`\`\`json
{ "example": "response" }
\`\`\`

**Errors:**
- 400: Wenn...
- 401: Wenn nicht authentifiziert
- 403: Wenn nicht autorisiert
```

## 🚀 Releasing

### Version Bumping

Follow Semantic Versioning: MAJOR.MINOR.PATCH

```
2.0.0.0 - Major Release (Breaking Changes)
1.2.0.0 - Minor Release (New Features)
1.1.1.0 - Patch Release (Bugfixes)
```

### Release Checklist

- [ ] Alle Tests bestanden
- [ ] CHANGELOG.md aktualisiert
- [ ] manifest.json aktualisiert
- [ ] README.md überprüft
- [ ] Git Tag erstellt: `git tag v2.0.0.0`
- [ ] Release auf GitHub erstellt
- [ ] Plugin-DLL hochgeladen
- [ ] Checksum berechnet und in manifest.json eintragen

### Checksum Berechnen

```bash
# Linux/macOS
sha256sum bin/publish/Jellyfin.Plugin.Kindle.dll

# Windows
certutil -hashfile "bin\publish\Jellyfin.Plugin.Kindle.dll" SHA256
```

## 🆘 Häufige Probleme

### Build schlägt fehl
```bash
# Clean und Rebuild
dotnet clean
dotnet restore
dotnet build
```

### Tests fehlgeschlagen
```bash
# Alle Dependencies aktuell?
dotnet nuget locals all --clear

# Einzelne Tests für Debug ausführen
dotnet test --logger "console;verbosity=detailed"
```

### Frontend wird nicht geladen
- Überprüfe HtmlInjectionMiddleware
- Überprüfe Browser Console für Errors
- Überprüfe script src Pfad in HTML

## 📞 Hilfe & Support

- **Fragen:** GitHub Issues oder Jellyfin Forum
- **Bugs:** GitHub Issues mit Reproduktionsschritte
- **PRs:** Beschreibe was, warum, und wie

## 👥 Danksagungen

Danke an alle Contributors!

---

**Happy coding! 🎉**
