# Changelog

Alle bemerkenswerten Änderungen an diesem Projekt werden in dieser Datei dokumentiert.

## [2.0.0.0] - 2026-04-10

### 🎉 Major Features (Phase 3: Complete Refactoring)

#### Multi-Device Support
- ✨ Users can now manage multiple E-Reader devices per account
- ✨ Device-specific settings (name, email, preferred format)
- ✨ Set default device for quick sending
- ✨ Device selection dialog when multiple devices exist
- ✨ Auto-select single device when only one configured
- ✨ Full device CRUD operations (Create, Read, Update, Delete)

#### Send History & Statistics
- 📊 Complete send history for every user
- 📊 Per-user statistics (success rate, favorite format, last send)
- 📊 System-wide admin statistics (total sends, active users, etc.)
- 📊 Color-coded status display (✅ success, ❌ failed, ⏳ pending)
- 📊 Filterable and sortable history table
- 📊 Admin dashboard with key metrics

#### User Interface Improvements
- 🎯 New header button (email icon) replacing settings menu
- 🎯 Modern popup design with responsive layout
- 🎯 Device management modal
- 🎯 History display modal with table view
- 🎯 Admin statistics dashboard
- 🎯 Mobile-friendly responsive design
- 🎯 Better visual feedback and error messages

#### Admin Features
- 👮 System-wide statistics dashboard
- 👮 Clear send history (per-user or all)
- 👮 View user statistics and history
- 👮 SMTP connection test button
- 👮 Admin-only access control

#### Security & Authorization
- 🔐 User isolation - users can only access their own data
- 🔐 Authorization checks on all device endpoints
- 🔐 Authorization checks on history endpoints
- 🔐 Proper role-based access control
- 🔐 Prevent cross-user data access
- 🔐 Admin-only statistics and management

#### Technical Improvements
- 🔧 Fixed critical JSON serialization issues
- 🔧 Fixed response field name mismatches
- 🔧 Added JsonPropertyName attributes for camelCase serialization
- 🔧 Improved error handling with detailed messages
- 🔧 Enhanced logging for debugging
- 🔧 Better API documentation

### 📋 API Changes

#### New Endpoints
- `GET /Kindle/SenderEmail` - Get admin-configured sender email
- `GET /Kindle/Devices` - List user devices
- `POST /Kindle/Devices` - Add new device
- `PUT /Kindle/Devices/{id}` - Update device
- `DELETE /Kindle/Devices/{id}` - Delete device
- `GET /Kindle/History` - Get user send history
- `DELETE /Kindle/History` - Clear user history (admin)
- `DELETE /Kindle/History/All` - Clear all history (admin)
- `GET /Kindle/Statistics` - Get user statistics
- `GET /Kindle/Statistics/System` - Get system statistics (admin)

#### Modified Endpoints
- `POST /Kindle/Send` - Now supports optional `deviceId` parameter
- `POST /Kindle/Send` - Now logs to send history automatically
- `GET /Kindle/UserEmail` - Improved documentation

### 🔍 Fixes & Improvements

#### Bug Fixes
- ✅ Fixed critical response field name mismatch (history vs logs)
- ✅ Fixed JSON property case sensitivity (PascalCase vs camelCase)
- ✅ Fixed authorization bypass on device endpoints
- ✅ Fixed authorization bypass on history endpoint
- ✅ Fixed missing user validation on all device operations
- ✅ Improved error handling with detailed messages
- ✅ Fixed email validation consistency

#### Performance Improvements
- ⚡ Optimized history query with limit parameter
- ⚡ Efficient device lookup by ID
- ⚡ Reduced database queries for statistics
- ⚡ Better caching of user data

#### Code Quality
- 📝 Improved API documentation
- 📝 Better error messages for debugging
- 📝 More comprehensive logging
- 📝 Cleaner code structure
- 📝 Added XML documentation comments

### 🌍 Internationalization

- 🇬🇧 Complete English translations for all new features
- 🇩🇪 Complete German translations for all new features
- 🌍 Browser language auto-detection
- 🌍 Consistent translation strings across frontend/backend

### 📚 Documentation

- 📖 Comprehensive README with all new features
- 📖 Detailed API documentation with examples
- 📖 Troubleshooting guide for common issues
- 📖 Configuration examples for Gmail/Outlook/Custom servers
- 📖 Screenshots and usage instructions
- 📖 Admin setup guide

### ⚠️ Breaking Changes

**None** - This release is fully backward compatible with 1.x versions.

Legacy single email per user still supported, but users can now use the multi-device system.

### 🔄 Migration from v1.x

- Existing user emails are preserved
- Can still send to legacy email if no devices configured
- Automatic fallback to email-based sending
- No action required - existing setups will continue to work

### 🚀 Known Limitations

- Maximum 50MB per file (Amazon/Kindle limitation)
- No automatic format conversion (PDF↔EPUB)
- No scheduled/time-delayed sending
- E-reader specific email addresses required (vendor limitation)

### 📦 Dependencies

- .NET 9.0 runtime
- Jellyfin 10.11.0 or higher
- System.Text.Json (for JSON serialization)

### 👥 Contributors

- @Strassbert - Main developer

---

## [1.2.0.0] - 2026-02-14

### 🎯 Improvements
- Updated names and example for users
- Improved error messages
- Better logging for debugging

### 🐛 Bug Fixes
- Fixed minor UI issues

---

## [1.1.0.0] - 2026-02-14

### ✨ Initial Release
- Basic send to Kindle functionality
- SMTP configuration
- Per-user email addresses
- Support for EPUB, PDF, MOBI, AZW formats
- English and German translations
- Admin SMTP settings page
- Rate limiting (5 per minute)
- Email validation
- File size checking (50MB limit)
- OAuth2 support (optional)

---

## Version History

| Version | Date | Status | Notes |
|---------|------|--------|-------|
| 2.0.0.0 | 2026-04-10 | Latest | Multi-device, history, statistics |
| 1.2.0.0 | 2026-02-14 | Stable | UI improvements |
| 1.1.0.0 | 2026-02-14 | EOL | Initial release |

---

## Roadmap (Future)

### Potential Features
- [ ] Automatic format conversion (PDF to EPUB, etc.)
- [ ] Schedule sending at specific times
- [ ] Cloud storage integration (OneDrive, Google Drive)
- [ ] Device synchronization across multiple Jellyfin servers
- [ ] Automatic e-reader sync (refresh library)
- [ ] Advanced search/filtering in history
- [ ] Export statistics as CSV/PDF
- [ ] Backup/restore device settings
- [ ] WebDAV integration for direct device sync
- [ ] Mobile app companion

### Improvements
- [ ] Real-time progress indicators for large files
- [ ] Batch sending support
- [ ] Device templates for quick setup
- [ ] User guides with screenshots
- [ ] Docker health check integration
- [ ] Prometheus metrics export

---

## Security Policy

### Reporting Security Issues

**IMPORTANT:** Do not post security vulnerabilities to public GitHub issues!

Please report security issues to [maintainer email] with:
- Description of vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (optional)

### Supported Versions

| Version | Security | Active Support |
|---------|----------|-----------------|
| 2.0.0.0 | ✅ | ✅ |
| 1.2.0.0 | ✅ | ❌ |
| 1.1.0.0 | ❌ | ❌ |

---

## Getting Help

- 📖 [README](README.md) - Full documentation
- 🐛 [GitHub Issues](https://github.com/Strassbert/Jellyfin.Plugin.Kindle/issues) - Bug reports
- 💬 [Jellyfin Forum](https://jellyfin.org/docs/general/community) - Community help
