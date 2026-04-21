(function () {
    'use strict';

    console.log('[Kindle] Script loaded.');

    var PLUGIN_ID = 'E3B2B4A1-1234-4567-89AB-CDEF12345678';

    // i18n strings
    var i18n = {
        en: {
            sendToKindle: 'Send to E-Book Reader',
            sending: 'Sending...',
            sent: 'Sent to E-Book Reader!',
            errorSending: 'Failed to send to E-Book Reader.',
            noEmail: 'No E-Book Reader email configured.',
            enterEmail: 'Enter your E-Book Reader email address:',
            save: 'Save',
            cancel: 'Cancel',
            clear: 'Clear',
            help: 'Help',
            emailPlaceholder: 'your-name@kindle.com',
            emailSaved: 'E-Book Reader Email saved.',
            emailCleared: 'E-Book Reader Email cleared.',
            fileTooLarge: 'File is too large for E-Book Reader (max 50 MB).',
            formatNotSupported: 'This file format is not supported by E-Book Reader.',
            settingsTitle: 'E-Book Reader Settings',
            emailLabel: 'E-Book Reader Email',
            currentEmail: 'Current Email: {email}',
            noEmailSet: 'No email configured',
            senderEmailTitle: 'Sender Email Address',
            senderEmailInfo: 'Books will be sent from the following email address. You must approve this address in your e-reader settings to receive files.',
            senderEmail: 'Sender: {email}',
            noSenderEmail: 'No sender email configured',
            helpTitle: 'Example Kindle',
            helpText1: 'To find your Send to Kindle email address, go to ',
            helpLink1: 'Manage Your Content and Devices',
            helpText2: ' > Settings > Personal Document Settings.',
            helpText3: 'Only approved email addresses can send files to your Kindle library. Before sending, make sure the account you will use is listed in your ',
            helpLink2: 'Email List for Approved Personal Document',
            helpText4: ' in your Personal Document Settings.',
            manageDevices: 'Manage Devices',
            devices: 'Devices',
            deviceName: 'Device Name',
            deviceEmail: 'Email Address',
            preferredFormat: 'Preferred Format',
            setAsDefault: 'Set as Default',
            removeDevice: 'Remove',
            addDevice: 'Add Device',
            editDevice: 'Edit',
            defaultDevice: 'Default',
            noDevices: 'No devices configured yet.',
            deviceAdded: 'Device added successfully.',
            deviceUpdated: 'Device updated successfully.',
            deviceRemoved: 'Device removed successfully.',
            viewHistory: 'View History',
            sendHistory: 'Send History',
            bookTitle: 'Book',
            sentTo: 'Sent To',
            sentAt: 'Date',
            status: 'Status',
            fileSize: 'Size',
            noHistory: 'No send history yet.',
            sentSuccessfully: 'Sent',
            sentFailed: 'Failed',
            sentPending: 'Pending',
            clearHistory: 'Clear History',
            adminDashboard: 'Statistics',
            totalSends: 'Total Sends',
            successfulSends: 'Successful',
            failedSends: 'Failed',
            successRate: 'Success Rate',
            uniqueUsers: 'Unique Users',
            mostActiveUser: 'Most Active User',
            mostCommonFormat: 'Most Common Format',
            averageDailyActivity: 'Average Daily',
            totalDataTransferred: 'Total Data',
            selectDevice: 'Select Device',
            chooseDevice: 'Choose which device to send to:',
            sendToDevice: 'Send to {device}',
            sendingToDevice: 'Sending to {device}...'
        },
        de: {
            sendToKindle: 'An E-Book Reader senden',
            sending: 'Wird gesendet...',
            sent: 'An E-Book Reader gesendet!',
            errorSending: 'Senden an E-Book Reader fehlgeschlagen.',
            noEmail: 'Keine E-Book Reader E-Mail konfiguriert.',
            enterEmail: 'Gib deine E-Book Reader E-Mail-Adresse ein:',
            save: 'Speichern',
            cancel: 'Abbrechen',
            clear: 'Löschen',
            help: 'Hilfe',
            emailPlaceholder: 'dein-name@kindle.com',
            emailSaved: 'E-Book Reader E-Mail gespeichert.',
            emailCleared: 'E-Book Reader E-Mail gelöscht.',
            fileTooLarge: 'Datei ist zu gro\u00df f\u00fcr E-Book Reader (max. 50 MB).',
            formatNotSupported: 'Dieses Dateiformat wird vom E-Book Reader nicht unterst\u00fctzt.',
            settingsTitle: 'E-Book Reader Einstellungen',
            emailLabel: 'E-Book Reader E-Mail',
            currentEmail: 'Aktuelle E-Mail: {email}',
            noEmailSet: 'Keine E-Mail konfiguriert',
            senderEmailTitle: 'Absender E-Mail-Adresse',
            senderEmailInfo: 'B\u00fccher werden von der folgenden E-Mail-Adresse versendet. Du musst diese Adresse in deinen E-Reader-Einstellungen genehmigen, um Dateien zu erhalten.',
            senderEmail: 'Absender: {email}',
            noSenderEmail: 'Keine Absender-E-Mail konfiguriert',
            helpTitle: 'Beispiel Kindle',
            helpText1: 'Um deine f\u00fcr Send to Kindle verwendete E-Mail-Adresse zu finden, w\u00e4hle ',
            helpLink1: 'Meine Inhalte und Ger\u00e4te verwalten',
            helpText2: ' > Einstellungen > Pers\u00f6nliche Dokumente Einstellungen.',
            helpText3: 'Nur genehmigte E-Mail-Adressen k\u00f6nnen Dateien an deine Kindle-Bibliothek senden. Vergewissere dich vor dem Senden, dass das Konto, das du verwenden wirst, in deiner ',
            helpLink2: 'E-Mail-Liste f\u00fcr genehmigte pers\u00f6nliche Dokumente',
            helpText4: ' in deinen Einstellungen f\u00fcr pers\u00f6nliche Dokumente aufgef\u00fchrt ist.',
            manageDevices: 'Ger\u00e4te verwalten',
            devices: 'Ger\u00e4te',
            deviceName: 'Ger\u00e4tename',
            deviceEmail: 'E-Mail-Adresse',
            preferredFormat: 'Bevorzugtes Format',
            setAsDefault: 'Als Standard festlegen',
            removeDevice: 'Entfernen',
            addDevice: 'Ger\u00e4t hinzuf\u00fcgen',
            editDevice: 'Bearbeiten',
            defaultDevice: 'Standard',
            noDevices: 'Noch keine Ger\u00e4te konfiguriert.',
            deviceAdded: 'Ger\u00e4t erfolgreich hinzugef\u00fcgt.',
            deviceUpdated: 'Ger\u00e4t erfolgreich aktualisiert.',
            deviceRemoved: 'Ger\u00e4t erfolgreich entfernt.',
            viewHistory: 'Verlauf anzeigen',
            sendHistory: 'Versendverlauf',
            bookTitle: 'Buch',
            sentTo: 'Versendet an',
            sentAt: 'Datum',
            status: 'Status',
            fileSize: 'Gr\u00f6\u00dfe',
            noHistory: 'Noch kein Versendverlauf.',
            sentSuccessfully: 'Versendet',
            sentFailed: 'Fehlgeschlagen',
            sentPending: 'Ausstehend',
            clearHistory: 'Verlauf l\u00f6schen',
            adminDashboard: 'Statistiken',
            totalSends: 'Gesamte Versendungen',
            successfulSends: 'Erfolgreich',
            failedSends: 'Fehlgeschlagen',
            successRate: 'Erfolgsrate',
            uniqueUsers: 'Eindeutige Benutzer',
            mostActiveUser: 'Aktivster Benutzer',
            mostCommonFormat: 'H\u00e4ufigstes Format',
            averageDailyActivity: 'Durchschnittlich pro Tag',
            totalDataTransferred: 'Datenvolumen insgesamt',
            selectDevice: 'Ger\u00e4t w\u00e4hlen',
            chooseDevice: 'W\u00e4hle das Ger\u00e4t f\u00fcr den Versand:',
            sendToDevice: 'An {device} senden',
            sendingToDevice: 'Wird an {device} versendet...'
        }
    };

    function getLang() {
        var lang = (navigator.language || 'en').substring(0, 2).toLowerCase();
        return i18n[lang] ? lang : 'en';
    }

    function t(key) {
        return i18n[getLang()][key] || i18n.en[key] || key;
    }

    // Validate email format (RFC 5322 simplified)
    function isValidEmail(email) {
        if (!email || email.length > 254) return false;
        // Pattern: localpart@domain.tld
        var re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email) && email.indexOf('..') === -1;
    }

    function showToast(msg) {
        if (typeof Dashboard !== 'undefined' && Dashboard.alert) {
            Dashboard.alert(msg);
        } else if (typeof require === 'function') {
            require(['toast'], function (toast) {
                toast(msg);
            });
        }
    }

    // Parse query parameters from hash-based routing (Jellyfin 10.11 uses #/route?key=val)
    function getHashParam(name) {
        var hash = window.location.hash;
        var qs = hash.indexOf('?') !== -1 ? hash.substring(hash.indexOf('?')) : window.location.search;
        return new URLSearchParams(qs).get(name);
    }

    // Wait for a DOM element to appear (React pages render asynchronously)
    function waitForElement(selector, callback, maxAttempts) {
        var attempts = 0;
        var limit = maxAttempts || 20;
        var interval = setInterval(function () {
            var el = document.querySelector(selector);
            if (el) {
                clearInterval(interval);
                callback(el);
            } else if (++attempts >= limit) {
                clearInterval(interval);
            }
        }, 250);
    }

    // Load CSS stylesheet
    function loadStylesheet() {
        var link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = '/KindlePlugin/ClientStyles';
        document.head.appendChild(link);
    }

    // Wait for DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            loadStylesheet();
            initPlugin();
        });
    } else {
        loadStylesheet();
        initPlugin();
    }

    function initPlugin() {
        // Book detail page: "Send to E-Book Reader" button (legacy view, viewshow works)
        document.addEventListener('viewshow', function (e) {
            if (!e.target.classList || !e.target.classList.contains('itemDetailPage')) return;

            var itemId = getHashParam('id');
            if (!itemId) return;

            ApiClient.getItem(ApiClient.getCurrentUserId(), itemId).then(function (item) {
                if (item.Type === 'Book') {
                    renderButton(item);
                }
            });
        });

        // Header button: "E-Book Reader Settings" in top-right navigation
        // Uses MutationObserver to watch for .headerRight element
        setupHeaderButton();
    }

    function setupHeaderButton() {
        var buttonCreated = false;
        var observer = new MutationObserver(function (mutations) {
            // Early exit if button already created
            if (buttonCreated) return;

            var headerRight = document.querySelector('.headerRight');
            if (headerRight && !document.querySelector('.kindle-settings-button')) {
                var button = createHeaderButton();
                headerRight.prepend(button);
                buttonCreated = true;
                observer.disconnect(); // STOP monitoring after button creation
                console.log('[Kindle] Settings button injected. Observer stopped.');
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });

        // Also try immediately in case header is already present
        var headerRight = document.querySelector('.headerRight');
        if (headerRight && !document.querySelector('.kindle-settings-button')) {
            var button = createHeaderButton();
            headerRight.prepend(button);
            buttonCreated = true;
            observer.disconnect(); // Stop if created immediately
        }
    }

    function createHeaderButton() {
        var button = document.createElement('button');
        button.type = 'button';
        button.className = 'paper-icon-button-light headerButton kindle-settings-button';
        button.title = t('settingsTitle');
        button.setAttribute('aria-label', t('settingsTitle'));

        // Email icon SVG
        button.innerHTML = '<span class="material-icons" style="width:24px;height:24px;display:flex;align-items:center;justify-content:center;">email</span>';
        button.style.verticalAlign = 'middle';
        button.style.cursor = 'pointer';

        button.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleSettingsPopup(button);
        });

        return button;
    }

    function toggleSettingsPopup(anchorElement) {
        var existing = document.querySelector('.kindle-settings-popup');
        if (existing) {
            existing.remove();
        } else {
            createSettingsPopup(anchorElement);
        }
    }

    function createSettingsPopup(anchorElement) {
        var userId = ApiClient.getCurrentUserId();

        var popup = document.createElement('div');
        popup.className = 'kindle-settings-popup';

        // Responsive positioning (CSS handles most styling via classes)
        var rect = anchorElement.getBoundingClientRect();
        var rightPos = window.innerWidth - rect.right;

        if (window.innerWidth < 450 || (window.innerWidth - rightPos) < 320) {
            popup.style.right = '1rem';
            popup.style.left = 'auto';
        } else {
            popup.style.right = rightPos + 'px';
            popup.style.left = 'auto';
        }
        popup.style.top = (rect.bottom + 10) + 'px';

        // Fetch current email
        ApiClient.ajax({
            type: 'GET',
            url: ApiClient.getUrl('Kindle/UserEmail', { userId: userId }),
            dataType: 'json'
        }).then(function (result) {
            var currentEmail = result.email || '';
            populatePopup(popup, currentEmail, userId);
        }).catch(function () {
            populatePopup(popup, '', userId);
        });

        document.body.appendChild(popup);

        // Prevent clicks inside popup from closing it
        popup.addEventListener('click', function (e) {
            e.stopPropagation();
        });

        // Close popup when clicking outside
        var closeHandler = function (e) {
            if (!popup.contains(e.target) &&
                e.target !== anchorElement &&
                !anchorElement.contains(e.target)) {
                popup.remove();
                document.removeEventListener('click', closeHandler);
            }
        };
        // Register immediately (synchronously), not with setTimeout
        document.addEventListener('click', closeHandler, true);
    }

    function populatePopup(popup, currentEmail, userId) {
        var emailInput = document.createElement('input');
        emailInput.type = 'email';
        emailInput.className = 'kindle-popup-input';
        emailInput.placeholder = t('emailPlaceholder');
        emailInput.value = currentEmail;

        var currentEmailDisplay = document.createElement('div');
        currentEmailDisplay.className = 'kindle-popup-email-display';
        if (currentEmail) {
            currentEmailDisplay.textContent = t('currentEmail').replace('{email}', currentEmail);
        } else {
            currentEmailDisplay.textContent = t('noEmailSet');
        }

        // Sender email info section
        var senderInfoSection = document.createElement('div');
        senderInfoSection.className = 'kindle-popup-sender-info';

        var senderTitle = document.createElement('div');
        senderTitle.className = 'kindle-popup-sender-title';
        senderTitle.textContent = t('senderEmailTitle');

        var senderDescription = document.createElement('div');
        senderDescription.className = 'kindle-popup-sender-description';
        senderDescription.textContent = t('senderEmailInfo');

        var senderEmail = document.createElement('div');
        senderEmail.className = 'kindle-popup-sender-email';

        senderInfoSection.appendChild(senderTitle);
        senderInfoSection.appendChild(senderDescription);
        senderInfoSection.appendChild(senderEmail);

        // Fetch sender email
        ApiClient.ajax({
            type: 'GET',
            url: ApiClient.getUrl('Kindle/SenderEmail'),
            dataType: 'json'
        }).then(function (result) {
            if (result.senderEmail) {
                senderEmail.textContent = t('senderEmail').replace('{email}', result.senderEmail);
            } else {
                senderEmail.textContent = t('noSenderEmail');
            }
        }).catch(function () {
            senderEmail.textContent = t('noSenderEmail');
        });

        var buttonContainer = document.createElement('div');
        buttonContainer.className = 'kindle-popup-buttons';

        var saveBtn = document.createElement('button');
        saveBtn.textContent = t('save');
        saveBtn.className = 'kindle-popup-button kindle-popup-button-save';

        var clearBtn = document.createElement('button');
        clearBtn.textContent = t('clear');
        clearBtn.className = 'kindle-popup-button kindle-popup-button-secondary';

        var cancelBtn = document.createElement('button');
        cancelBtn.textContent = t('cancel');
        cancelBtn.className = 'kindle-popup-button kindle-popup-button-secondary';

        var devicesBtn = document.createElement('button');
        devicesBtn.textContent = t('manageDevices');
        devicesBtn.className = 'kindle-popup-button kindle-popup-button-secondary';
        devicesBtn.addEventListener('click', function () {
            popup.remove();
            openDeviceManagement(userId);
        });

        var historyBtn = document.createElement('button');
        historyBtn.textContent = t('viewHistory');
        historyBtn.className = 'kindle-popup-button kindle-popup-button-secondary';
        historyBtn.addEventListener('click', function () {
            popup.remove();
            openSendHistory(userId);
        });

        buttonContainer.appendChild(devicesBtn);
        buttonContainer.appendChild(historyBtn);

        // Add admin dashboard button if user is admin
        ApiClient.getUser(userId).then(function (user) {
            if (user.Policy.IsAdministrator) {
                var adminBtn = document.createElement('button');
                adminBtn.textContent = t('adminDashboard');
                adminBtn.className = 'kindle-popup-button kindle-popup-button-secondary';
                adminBtn.style.cssText = 'border-color:rgba(0,164,220,0.5);';
                adminBtn.addEventListener('click', function () {
                    popup.remove();
                    openAdminDashboard();
                });
                buttonContainer.insertBefore(adminBtn, buttonContainer.children[2]);
            }
        });

        buttonContainer.appendChild(clearBtn);
        buttonContainer.appendChild(cancelBtn);
        buttonContainer.appendChild(saveBtn);

        // Help section (collapsible)
        var helpSection = document.createElement('div');
        helpSection.className = 'kindle-popup-help-section';

        var helpSummary = document.createElement('div');
        helpSummary.className = 'kindle-popup-help-summary';
        var helpIcon = document.createElement('span');
        helpIcon.className = 'kindle-popup-help-icon';
        helpIcon.textContent = '▸';
        helpSummary.appendChild(helpIcon);
        helpSummary.appendChild(document.createTextNode(t('helpTitle')));

        var helpContent = document.createElement('div');
        helpContent.className = 'kindle-popup-help-content';
        helpContent.style.display = 'none';

        var lang = getLang();
        var helpLink1Href = lang === 'de'
            ? 'https://www.amazon.de/hz/mycd/digital-console/contentlist/pdocs/dateDsc'
            : 'https://www.amazon.com/hz/mycd/digital-console/contentlist/pdocs/dateDsc';
        var helpLink2Href = lang === 'de'
            ? 'https://www.amazon.de/hz/mycd/preferences/myx#/home/settings/payment'
            : 'https://www.amazon.com/hz/mycd/preferences/myx#/home/settings/payment';

        helpContent.innerHTML =
            '<p style="margin:0.5em 0;">' +
            t('helpText1') +
            '<a href="' + helpLink1Href + '" target="_blank" style="color:#00a4dc;text-decoration:underline;">' + t('helpLink1') + '</a>' +
            t('helpText2') +
            '</p>' +
            '<p style="margin:0.5em 0;">' +
            t('helpText3') +
            '<a href="' + helpLink2Href + '" target="_blank" style="color:#00a4dc;text-decoration:underline;">' + t('helpLink2') + '</a>' +
            t('helpText4') +
            '</p>';

        helpSummary.addEventListener('click', function () {
            var isHidden = helpContent.style.display === 'none';
            helpContent.style.display = isHidden ? 'block' : 'none';
            var icon = helpSummary.querySelector('span');
            icon.textContent = isHidden ? '▾' : '▸';
        });

        helpSection.appendChild(helpSummary);
        helpSection.appendChild(helpContent);

        popup.appendChild(emailInput);
        popup.appendChild(currentEmailDisplay);
        popup.appendChild(senderInfoSection);
        popup.appendChild(buttonContainer);
        popup.appendChild(helpSection);

        emailInput.focus();

        saveBtn.addEventListener('click', function () {
            var email = emailInput.value.trim();
            if (!isValidEmail(email)) {
                emailInput.style.borderColor = '#f44336';
                return;
            }

            ApiClient.ajax({
                type: 'POST',
                url: ApiClient.getUrl('Kindle/UserEmail', { userId: userId, email: email })
            }).then(function () {
                popup.remove();
                showToast(t('emailSaved'));
            }).catch(function () {
                emailInput.style.borderColor = '#f44336';
            });
        });

        clearBtn.addEventListener('click', function () {
            ApiClient.ajax({
                type: 'DELETE',
                url: ApiClient.getUrl('Kindle/UserEmail', { userId: userId })
            }).then(function () {
                popup.remove();
                showToast(t('emailCleared'));
            }).catch(function () {
                showToast('Failed to clear email.');
            });
        });

        cancelBtn.addEventListener('click', function () {
            popup.remove();
        });

        emailInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                saveBtn.click();
            }
            if (e.key === 'Escape') {
                popup.remove();
            }
        });
    }

    function renderButton(item) {
        var container = document.querySelector('.mainDetailButtons');
        if (!container || container.querySelector('.btnSendToKindle')) return;

        // Find an existing detail button to clone its styling
        var existingBtn = container.querySelector('.detailButton');

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.setAttribute('is', 'emby-button');
        btn.className = 'btnSendToKindle detailButton emby-button';

        // Copy computed styles from existing buttons for consistent appearance
        if (existingBtn) {
            var cs = window.getComputedStyle(existingBtn);
            btn.style.color = cs.color;
            btn.style.background = cs.background;
        }

        btn.innerHTML = '<span class="material-icons detailButton-icon">send</span>';
        btn.addEventListener('click', function () {
            sendBook(item, btn);
        });
        container.appendChild(btn);
        console.log('[Kindle] Send button rendered for:', item.Name);
    }

    function sendBook(item, btn) {
        var userId = ApiClient.getCurrentUserId();

        // First check if user has multiple devices
        ApiClient.ajax({
            type: 'GET',
            url: ApiClient.getUrl('Kindle/Devices', { userId: userId }),
            dataType: 'json'
        }).then(function (result) {
            var devices = result.devices || [];

            if (devices.length > 1) {
                // Show device selection dialog
                showDeviceSelectionDialog(item, btn, userId, devices);
            } else if (devices.length === 1) {
                // Auto-select the single device
                doSendWithDevice(item, btn, userId, devices[0].id);
            } else {
                // Fall back to email-based sending (legacy)
                ApiClient.ajax({
                    type: 'GET',
                    url: ApiClient.getUrl('Kindle/UserEmail', { userId: userId }),
                    dataType: 'json'
                }).then(function (emailResult) {
                    if (emailResult.email) {
                        doSend(item, btn, userId);
                    } else {
                        showEmailDialog(item, btn, userId);
                    }
                }).catch(function () {
                    showEmailDialog(item, btn, userId);
                });
            }
        }).catch(function () {
            // If device fetch fails, fall back to email
            ApiClient.ajax({
                type: 'GET',
                url: ApiClient.getUrl('Kindle/UserEmail', { userId: userId }),
                dataType: 'json'
            }).then(function (result) {
                if (result.email) {
                    doSend(item, btn, userId);
                } else {
                    showEmailDialog(item, btn, userId);
                }
            }).catch(function () {
                showEmailDialog(item, btn, userId);
            });
        });
    }

    function showDeviceSelectionDialog(item, btn, userId, devices) {
        var overlay = document.createElement('div');
        overlay.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.7);z-index:9999;display:flex;align-items:center;justify-content:center;';

        var dialog = document.createElement('div');
        dialog.style.cssText = 'background:var(--theme-card-background,#1c1c1e);color:var(--theme-text-color,#fff);padding:1.5em 2em;border-radius:10px;max-width:400px;width:90%;box-shadow:0 4px 20px rgba(0,0,0,0.5);';

        dialog.innerHTML = '<h3 style="margin:0 0 0.5em;">' + t('selectDevice') + '</h3>' +
            '<p style="margin:0 0 1em;opacity:0.8;">' + t('chooseDevice') + '</p>';

        var deviceButtonContainer = document.createElement('div');
        deviceButtonContainer.style.cssText = 'display:flex;flex-direction:column;gap:0.5em;margin-bottom:1em;';

        devices.forEach(function (device) {
            var deviceBtn = document.createElement('button');
            deviceBtn.style.cssText = 'padding:0.8em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:rgba(0,164,220,0.1);color:inherit;cursor:pointer;text-align:left;transition:all 0.2s ease;';
            deviceBtn.innerHTML = '<div style="font-weight:bold;">' + device.deviceName + '</div>' +
                '<div style="font-size:0.85em;opacity:0.7;">' + device.email + '</div>' +
                '<div style="font-size:0.8em;opacity:0.6;">' + (device.preferredFormat || 'epub') + '</div>';

            deviceBtn.addEventListener('mouseover', function () {
                deviceBtn.style.backgroundColor = 'rgba(0,164,220,0.2)';
                deviceBtn.style.borderColor = 'rgba(0,164,220,0.5)';
            });

            deviceBtn.addEventListener('mouseout', function () {
                deviceBtn.style.backgroundColor = 'rgba(0,164,220,0.1)';
                deviceBtn.style.borderColor = 'rgba(255,255,255,0.2)';
            });

            deviceBtn.addEventListener('click', function () {
                document.body.removeChild(overlay);
                doSendWithDevice(item, btn, userId, device.id);
            });

            deviceButtonContainer.appendChild(deviceBtn);
        });

        dialog.appendChild(deviceButtonContainer);

        var cancelBtn = document.createElement('button');
        cancelBtn.textContent = t('cancel');
        cancelBtn.style.cssText = 'width:100%;padding:0.5em 1.2em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:transparent;color:inherit;cursor:pointer;';
        cancelBtn.addEventListener('click', function () {
            document.body.removeChild(overlay);
        });

        dialog.appendChild(cancelBtn);

        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                document.body.removeChild(overlay);
            }
        });
    }

    function doSendWithDevice(item, btn, userId, deviceId) {
        // Disable button and show loading state
        btn.disabled = true;
        var originalHtml = btn.innerHTML;
        btn.innerHTML = '<span class="material-icons detailButton-icon">hourglass_empty</span>' +
            '<div class="detailButton-content"><div class="detailButton-content-text">' +
            t('sending') + '</div></div>';

        ApiClient.ajax({
            type: 'POST',
            url: ApiClient.getUrl('Kindle/Send', { itemId: item.Id, userId: userId, deviceId: deviceId }),
            dataType: 'json'
        }).then(function (result) {
            btn.innerHTML = '<span class="material-icons detailButton-icon">check</span>' +
                '<div class="detailButton-content"><div class="detailButton-content-text">' +
                t('sent') + '</div></div>';
            showToast(t('sent'));

            setTimeout(function () {
                btn.innerHTML = originalHtml;
                btn.disabled = false;
            }, 3000);
        }).catch(function (err) {
            var errorMsg = t('errorSending');
            try {
                var body = JSON.parse(err.responseText || '{}');
                var lang = getLang();
                if (lang === 'de' && body.errorDe) {
                    errorMsg = body.errorDe;
                } else if (body.error) {
                    errorMsg = body.error;
                }
            } catch (e) { /* ignore parse error */ }

            showToast(errorMsg);
            btn.innerHTML = originalHtml;
            btn.disabled = false;
        });
    }

    function doSend(item, btn, userId) {
        // Disable button and show loading state
        btn.disabled = true;
        var originalHtml = btn.innerHTML;
        btn.innerHTML = '<span class="material-icons detailButton-icon">hourglass_empty</span>';

        ApiClient.ajax({
            type: 'POST',
            url: ApiClient.getUrl('Kindle/Send', { itemId: item.Id, userId: userId }),
            dataType: 'json'
        }).then(function (result) {
            btn.innerHTML = '<span class="material-icons detailButton-icon">check</span>';
            showToast(t('sent'));

            setTimeout(function () {
                btn.innerHTML = originalHtml;
                btn.disabled = false;
            }, 3000);
        }).catch(function (err) {
            var errorMsg = t('errorSending');
            try {
                var body = JSON.parse(err.responseText || '{}');
                var lang = getLang();
                if (lang === 'de' && body.errorDe) {
                    errorMsg = body.errorDe;
                } else if (body.error) {
                    errorMsg = body.error;
                }
            } catch (e) { /* ignore parse error */ }

            showToast(errorMsg);
            btn.innerHTML = originalHtml;
            btn.disabled = false;
        });
    }

    function showEmailDialog(item, btn, userId) {
        // Create a simple inline dialog for entering Kindle email
        var overlay = document.createElement('div');
        overlay.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.7);z-index:9999;display:flex;align-items:center;justify-content:center;';

        var dialog = document.createElement('div');
        dialog.style.cssText = 'background:var(--theme-card-background,#1c1c1e);color:var(--theme-text-color,#fff);padding:1.5em 2em;border-radius:10px;max-width:400px;width:90%;box-shadow:0 4px 20px rgba(0,0,0,0.5);';
        dialog.innerHTML =
            '<h3 style="margin:0 0 0.5em;">' + t('sendToKindle') + '</h3>' +
            '<p style="margin:0 0 1em;opacity:0.8;">' + t('enterEmail') + '</p>' +
            '<input type="email" id="kindleEmailInput" placeholder="' + t('emailPlaceholder') + '" ' +
            'style="width:100%;padding:0.6em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:rgba(255,255,255,0.1);color:inherit;font-size:1em;box-sizing:border-box;" />' +
            '<div style="display:flex;gap:0.5em;margin-top:1em;justify-content:flex-end;">' +
            '<button id="kindleDialogCancel" style="padding:0.5em 1.2em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:transparent;color:inherit;cursor:pointer;">' + t('cancel') + '</button>' +
            '<button id="kindleDialogSave" style="padding:0.5em 1.2em;border:none;border-radius:5px;background:var(--theme-primary-color,#00a4dc);color:#fff;cursor:pointer;">' + t('save') + '</button>' +
            '</div>';

        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        var input = dialog.querySelector('#kindleEmailInput');
        input.focus();

        dialog.querySelector('#kindleDialogCancel').addEventListener('click', function () {
            document.body.removeChild(overlay);
        });

        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                document.body.removeChild(overlay);
            }
        });

        dialog.querySelector('#kindleDialogSave').addEventListener('click', function () {
            var email = input.value.trim();
            if (!email || !email.includes('@')) return;

            ApiClient.ajax({
                type: 'POST',
                url: ApiClient.getUrl('Kindle/UserEmail', { userId: userId, email: email })
            }).then(function () {
                document.body.removeChild(overlay);
                showToast(t('emailSaved'));
                doSend(item, btn, userId);
            }).catch(function () {
                input.style.borderColor = '#f44336';
            });
        });

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                dialog.querySelector('#kindleDialogSave').click();
            }
            if (e.key === 'Escape') {
                document.body.removeChild(overlay);
            }
        });
    }

    // Device Management Functions
    function openDeviceManagement(userId) {
        var overlay = document.createElement('div');
        overlay.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.7);z-index:10001;display:flex;align-items:center;justify-content:center;';

        var modal = document.createElement('div');
        modal.style.cssText = 'background:var(--theme-card-background,#1c1c1e);color:var(--theme-text-color,#fff);padding:2em;border-radius:10px;max-width:600px;width:90%;max-height:80vh;overflow-y:auto;box-shadow:0 4px 20px rgba(0,0,0,0.5);';
        modal.className = 'kindle-device-modal';

        var title = document.createElement('h2');
        title.textContent = t('devices');
        title.style.cssText = 'margin:0 0 1em 0;';

        var deviceList = document.createElement('div');
        deviceList.className = 'kindle-device-list';
        deviceList.style.cssText = 'margin-bottom:1em;';

        var footer = document.createElement('div');
        footer.style.cssText = 'display:flex;gap:0.5em;justify-content:flex-end;margin-top:1.5em;';

        var addBtn = document.createElement('button');
        addBtn.textContent = t('addDevice');
        addBtn.style.cssText = 'padding:0.5em 1.2em;border:none;border-radius:5px;background:var(--theme-primary-color,#00a4dc);color:#fff;cursor:pointer;';
        addBtn.addEventListener('click', function () {
            openAddDeviceDialog(userId, overlay, deviceList);
        });

        var closeBtn = document.createElement('button');
        closeBtn.textContent = t('cancel');
        closeBtn.style.cssText = 'padding:0.5em 1.2em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:transparent;color:inherit;cursor:pointer;';
        closeBtn.addEventListener('click', function () {
            document.body.removeChild(overlay);
        });

        footer.appendChild(addBtn);
        footer.appendChild(closeBtn);

        modal.appendChild(title);
        modal.appendChild(deviceList);
        modal.appendChild(footer);

        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        // Load devices
        loadDevices(userId, deviceList);

        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                document.body.removeChild(overlay);
            }
        });
    }

    function loadDevices(userId, container) {
        ApiClient.ajax({
            type: 'GET',
            url: ApiClient.getUrl('Kindle/Devices', { userId: userId }),
            dataType: 'json'
        }).then(function (result) {
            var devices = result.devices || [];
            container.innerHTML = '';

            if (devices.length === 0) {
                var emptyMsg = document.createElement('div');
                emptyMsg.style.cssText = 'text-align:center;padding:2em;opacity:0.6;';
                emptyMsg.textContent = t('noDevices');
                container.appendChild(emptyMsg);
                return;
            }

            devices.forEach(function (device) {
                var deviceItem = document.createElement('div');
                deviceItem.className = 'kindle-device-item';
                deviceItem.style.cssText = 'border:1px solid rgba(255,255,255,0.2);border-radius:5px;padding:1em;margin-bottom:0.8em;background:rgba(255,255,255,0.02);';

                var deviceHeader = document.createElement('div');
                deviceHeader.style.cssText = 'display:flex;justify-content:space-between;align-items:start;margin-bottom:0.5em;';

                var deviceNameSpan = document.createElement('span');
                deviceNameSpan.style.cssText = 'font-weight:bold;font-size:1em;';
                deviceNameSpan.textContent = device.deviceName;

                if (device.isDefault) {
                    var defaultBadge = document.createElement('span');
                    defaultBadge.style.cssText = 'background:var(--theme-primary-color,#00a4dc);color:#fff;padding:0.2em 0.6em;border-radius:3px;font-size:0.8em;';
                    defaultBadge.textContent = t('defaultDevice');
                    deviceHeader.appendChild(defaultBadge);
                }

                var actions = document.createElement('div');
                actions.style.cssText = 'display:flex;gap:0.3em;';

                var editBtn = document.createElement('button');
                editBtn.textContent = t('editDevice');
                editBtn.style.cssText = 'padding:0.3em 0.8em;border:1px solid rgba(255,255,255,0.2);border-radius:3px;background:transparent;color:inherit;cursor:pointer;font-size:0.85em;';
                editBtn.addEventListener('click', function () {
                    // Edit functionality would go here
                });

                var deleteBtn = document.createElement('button');
                deleteBtn.textContent = t('removeDevice');
                deleteBtn.style.cssText = 'padding:0.3em 0.8em;border:1px solid rgba(244,67,54,0.4);border-radius:3px;background:transparent;color:#f44336;cursor:pointer;font-size:0.85em;';
                deleteBtn.addEventListener('click', function () {
                    deleteDevice(userId, device.id, container);
                });

                actions.appendChild(editBtn);
                actions.appendChild(deleteBtn);
                deviceHeader.appendChild(deviceNameSpan);
                deviceHeader.appendChild(actions);

                var deviceEmail = document.createElement('div');
                deviceEmail.style.cssText = 'font-size:0.9em;opacity:0.8;margin-bottom:0.3em;';
                deviceEmail.textContent = t('deviceEmail') + ': ' + device.email;

                var deviceFormat = document.createElement('div');
                deviceFormat.style.cssText = 'font-size:0.9em;opacity:0.8;';
                deviceFormat.textContent = t('preferredFormat') + ': ' + (device.preferredFormat || 'epub');

                deviceItem.appendChild(deviceHeader);
                deviceItem.appendChild(deviceEmail);
                deviceItem.appendChild(deviceFormat);

                container.appendChild(deviceItem);
            });
        }).catch(function () {
            container.innerHTML = '<div style="text-align:center;padding:2em;color:#f44336;">Failed to load devices</div>';
        });
    }

    function openAddDeviceDialog(userId, parentOverlay, deviceList) {
        var overlay = document.createElement('div');
        overlay.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.7);z-index:10002;display:flex;align-items:center;justify-content:center;';

        var dialog = document.createElement('div');
        dialog.style.cssText = 'background:var(--theme-card-background,#1c1c1e);color:var(--theme-text-color,#fff);padding:1.5em 2em;border-radius:10px;max-width:400px;width:90%;box-shadow:0 4px 20px rgba(0,0,0,0.5);';

        dialog.innerHTML =
            '<h3 style="margin:0 0 1em;">' + t('addDevice') + '</h3>' +
            '<input type="text" id="deviceName" placeholder="' + t('deviceName') + '" style="width:100%;padding:0.6em;margin-bottom:0.5em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:rgba(255,255,255,0.1);color:inherit;font-size:1em;box-sizing:border-box;" />' +
            '<input type="email" id="deviceEmail" placeholder="' + t('deviceEmail') + '" style="width:100%;padding:0.6em;margin-bottom:0.5em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:rgba(255,255,255,0.1);color:inherit;font-size:1em;box-sizing:border-box;" />' +
            '<select id="deviceFormat" style="width:100%;padding:0.6em;margin-bottom:1em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:rgba(255,255,255,0.1);color:inherit;font-size:1em;box-sizing:border-box;"><option value="epub">EPUB</option><option value="pdf">PDF</option><option value="mobi">MOBI</option><option value="azw">AZW</option></select>' +
            '<div style="display:flex;gap:0.5em;justify-content:flex-end;">' +
            '<button id="addDialogCancel" style="padding:0.5em 1.2em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:transparent;color:inherit;cursor:pointer;">' + t('cancel') + '</button>' +
            '<button id="addDialogSave" style="padding:0.5em 1.2em;border:none;border-radius:5px;background:var(--theme-primary-color,#00a4dc);color:#fff;cursor:pointer;">' + t('save') + '</button>' +
            '</div>';

        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        var nameInput = dialog.querySelector('#deviceName');
        var emailInput = dialog.querySelector('#deviceEmail');
        var formatSelect = dialog.querySelector('#deviceFormat');

        nameInput.focus();

        dialog.querySelector('#addDialogCancel').addEventListener('click', function () {
            document.body.removeChild(overlay);
        });

        dialog.querySelector('#addDialogSave').addEventListener('click', function () {
            var deviceName = nameInput.value.trim();
            var deviceEmail = emailInput.value.trim();
            var format = formatSelect.value;

            if (!deviceName || !deviceEmail || !isValidEmail(deviceEmail)) {
                if (!deviceName) nameInput.style.borderColor = '#f44336';
                if (!deviceEmail) emailInput.style.borderColor = '#f44336';
                return;
            }

            ApiClient.ajax({
                type: 'POST',
                url: ApiClient.getUrl('Kindle/Devices', { userId: userId }),
                contentType: 'application/json',
                data: JSON.stringify({
                    deviceName: deviceName,
                    email: deviceEmail,
                    preferredFormat: format
                })
            }).then(function () {
                document.body.removeChild(overlay);
                showToast(t('deviceAdded'));
                loadDevices(userId, deviceList);
            }).catch(function () {
                showToast('Failed to add device.');
            });
        });

        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                document.body.removeChild(overlay);
            }
        });
    }

    function deleteDevice(userId, deviceId, deviceList) {
        if (confirm('Are you sure you want to remove this device?')) {
            ApiClient.ajax({
                type: 'DELETE',
                url: ApiClient.getUrl('Kindle/Devices/' + deviceId, { userId: userId })
            }).then(function () {
                showToast(t('deviceRemoved'));
                loadDevices(userId, deviceList);
            }).catch(function () {
                showToast('Failed to remove device.');
            });
        }
    }

    // Send History Functions
    function openSendHistory(userId) {
        var overlay = document.createElement('div');
        overlay.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.7);z-index:10001;display:flex;align-items:center;justify-content:center;';

        var modal = document.createElement('div');
        modal.style.cssText = 'background:var(--theme-card-background,#1c1c1e);color:var(--theme-text-color,#fff);padding:2em;border-radius:10px;max-width:800px;width:90%;max-height:80vh;overflow-y:auto;box-shadow:0 4px 20px rgba(0,0,0,0.5);';
        modal.className = 'kindle-history-modal';

        var title = document.createElement('h2');
        title.textContent = t('sendHistory');
        title.style.cssText = 'margin:0 0 1em 0;';

        var historyTable = document.createElement('div');
        historyTable.className = 'kindle-history-table';
        historyTable.style.cssText = 'margin-bottom:1em;';

        var footer = document.createElement('div');
        footer.style.cssText = 'display:flex;gap:0.5em;justify-content:flex-end;';

        var clearBtn = document.createElement('button');
        clearBtn.textContent = t('clearHistory');
        clearBtn.style.cssText = 'padding:0.5em 1.2em;border:1px solid rgba(244,67,54,0.4);border-radius:5px;background:transparent;color:#f44336;cursor:pointer;';
        clearBtn.addEventListener('click', function () {
            if (confirm('Clear all send history? This cannot be undone.')) {
                ApiClient.ajax({
                    type: 'DELETE',
                    url: ApiClient.getUrl('Kindle/History/All')
                }).then(function () {
                    showToast('History cleared');
                    loadSendHistory(userId, historyTable);
                }).catch(function () {
                    showToast('Failed to clear history');
                });
            }
        });

        var closeBtn = document.createElement('button');
        closeBtn.textContent = t('cancel');
        closeBtn.style.cssText = 'padding:0.5em 1.2em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:transparent;color:inherit;cursor:pointer;';
        closeBtn.addEventListener('click', function () {
            document.body.removeChild(overlay);
        });

        footer.appendChild(clearBtn);
        footer.appendChild(closeBtn);

        modal.appendChild(title);
        modal.appendChild(historyTable);
        modal.appendChild(footer);

        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        loadSendHistory(userId, historyTable);

        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                document.body.removeChild(overlay);
            }
        });
    }

    function loadSendHistory(userId, container) {
        ApiClient.ajax({
            type: 'GET',
            url: ApiClient.getUrl('Kindle/History', { userId: userId, limit: 100 }),
            dataType: 'json'
        }).then(function (result) {
            var history = result.logs || [];
            container.innerHTML = '';

            if (history.length === 0) {
                var emptyMsg = document.createElement('div');
                emptyMsg.style.cssText = 'text-align:center;padding:2em;opacity:0.6;';
                emptyMsg.textContent = t('noHistory');
                container.appendChild(emptyMsg);
                return;
            }

            // Create table-like display
            var table = document.createElement('div');
            table.style.cssText = 'display:flex;flex-direction:column;gap:0.5em;';

            // Header
            var header = document.createElement('div');
            header.style.cssText = 'display:grid;grid-template-columns:1fr 1fr 120px 100px 80px;gap:0.5em;padding:0.5em;background:rgba(0,164,220,0.1);border-radius:3px;font-weight:bold;font-size:0.9em;';
            header.innerHTML = '<div>' + t('bookTitle') + '</div><div>' + t('sentTo') + '</div><div>' + t('sentAt') + '</div><div>' + t('status') + '</div><div>' + t('fileSize') + '</div>';

            table.appendChild(header);

            // Rows
            history.forEach(function (log) {
                var row = document.createElement('div');
                row.style.cssText = 'display:grid;grid-template-columns:1fr 1fr 120px 100px 80px;gap:0.5em;padding:0.8em;border:1px solid rgba(255,255,255,0.1);border-radius:3px;font-size:0.85em;align-items:center;';

                var statusClass = log.status === 0 ? 'success' : log.status === 1 ? 'failed' : 'pending';
                var statusText = log.status === 0 ? t('sentSuccessfully') : log.status === 1 ? t('sentFailed') : t('sentPending');

                var title = document.createElement('div');
                title.style.cssText = 'white-space:nowrap;overflow:hidden;text-overflow:ellipsis;';
                title.title = log.bookTitle || log.fileName || 'Unknown';
                title.textContent = log.bookTitle || log.fileName || 'Unknown';

                var email = document.createElement('div');
                email.style.cssText = 'white-space:nowrap;overflow:hidden;text-overflow:ellipsis;opacity:0.8;font-size:0.9em;';
                email.textContent = log.recipientEmail || '';

                var date = document.createElement('div');
                date.style.cssText = 'white-space:nowrap;font-size:0.85em;opacity:0.7;';
                var logDate = new Date(log.sentAt);
                date.textContent = logDate.toLocaleDateString() + ' ' + logDate.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});

                var status = document.createElement('div');
                status.style.cssText = 'padding:0.3em 0.6em;border-radius:3px;text-align:center;font-size:0.8em;background:' +
                    (statusClass === 'success' ? 'rgba(76,175,80,0.2);color:#4caf50;' :
                     statusClass === 'failed' ? 'rgba(244,67,54,0.2);color:#f44336;' :
                     'rgba(255,193,7,0.2);color:#ffc107;');
                status.textContent = statusText;

                var size = document.createElement('div');
                size.style.cssText = 'text-align:right;opacity:0.7;';
                size.textContent = formatBytes(log.fileSizeBytes);

                row.appendChild(title);
                row.appendChild(email);
                row.appendChild(date);
                row.appendChild(status);
                row.appendChild(size);

                table.appendChild(row);
            });

            container.appendChild(table);
        }).catch(function () {
            container.innerHTML = '<div style="text-align:center;padding:2em;color:#f44336;">Failed to load history</div>';
        });
    }

    function formatBytes(bytes) {
        if (bytes === 0) return '0 B';
        var k = 1024;
        var sizes = ['B', 'KB', 'MB', 'GB'];
        var i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round((bytes / Math.pow(k, i)) * 10) / 10 + ' ' + sizes[i];
    }

    // Admin Dashboard Functions
    function openAdminDashboard() {
        // Check if user is admin
        ApiClient.getUser(ApiClient.getCurrentUserId()).then(function (user) {
            if (!user.Policy.IsAdministrator) {
                showToast('Admin access required');
                return;
            }

            var overlay = document.createElement('div');
            overlay.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.7);z-index:10001;display:flex;align-items:center;justify-content:center;';

            var modal = document.createElement('div');
            modal.style.cssText = 'background:var(--theme-card-background,#1c1c1e);color:var(--theme-text-color,#fff);padding:2em;border-radius:10px;max-width:800px;width:90%;max-height:80vh;overflow-y:auto;box-shadow:0 4px 20px rgba(0,0,0,0.5);';
            modal.className = 'kindle-admin-modal';

            var title = document.createElement('h2');
            title.textContent = t('adminDashboard');
            title.style.cssText = 'margin:0 0 1.5em 0;';

            var statsContainer = document.createElement('div');
            statsContainer.className = 'kindle-stats-container';
            statsContainer.style.cssText = 'display:grid;grid-template-columns:repeat(auto-fit,minmax(200px,1fr));gap:1em;margin-bottom:1.5em;';

            var footer = document.createElement('div');
            footer.style.cssText = 'display:flex;gap:0.5em;justify-content:flex-end;';

            var closeBtn = document.createElement('button');
            closeBtn.textContent = t('cancel');
            closeBtn.style.cssText = 'padding:0.5em 1.2em;border:1px solid rgba(255,255,255,0.2);border-radius:5px;background:transparent;color:inherit;cursor:pointer;';
            closeBtn.addEventListener('click', function () {
                document.body.removeChild(overlay);
            });

            footer.appendChild(closeBtn);

            modal.appendChild(title);
            modal.appendChild(statsContainer);
            modal.appendChild(footer);

            overlay.appendChild(modal);
            document.body.appendChild(overlay);

            loadAdminStatistics(statsContainer);

            overlay.addEventListener('click', function (e) {
                if (e.target === overlay) {
                    document.body.removeChild(overlay);
                }
            });
        }).catch(function () {
            showToast('Failed to verify admin status');
        });
    }

    function loadAdminStatistics(container) {
        ApiClient.ajax({
            type: 'GET',
            url: ApiClient.getUrl('Kindle/Statistics/System'),
            dataType: 'json'
        }).then(function (result) {
            var stats = result.statistics || {};
            container.innerHTML = '';

            var statCards = [
                { label: t('totalSends'), value: stats.totalSends || 0 },
                { label: t('successfulSends'), value: stats.successfulSends || 0 },
                { label: t('failedSends'), value: stats.failedSends || 0 },
                { label: t('successRate'), value: (stats.successRate || 0).toFixed(1) + '%' },
                { label: t('uniqueUsers'), value: stats.uniqueUsers || 0 },
                { label: t('mostActiveUser'), value: stats.mostActiveUser || 'N/A' },
                { label: t('mostCommonFormat'), value: stats.mostCommonFormat || 'unknown' },
                { label: t('averageDailyActivity'), value: (stats.averageDailyActivity || 0).toFixed(1) },
                { label: t('totalDataTransferred'), value: formatBytes(stats.totalFilesSize || 0) }
            ];

            statCards.forEach(function (card) {
                var statCard = document.createElement('div');
                statCard.style.cssText = 'background:rgba(0,164,220,0.1);border:1px solid rgba(0,164,220,0.3);border-radius:5px;padding:1em;';

                var label = document.createElement('div');
                label.style.cssText = 'font-size:0.85em;opacity:0.7;margin-bottom:0.5em;';
                label.textContent = card.label;

                var value = document.createElement('div');
                value.style.cssText = 'font-size:1.5em;font-weight:bold;color:#00a4dc;word-break:break-word;';
                value.textContent = card.value;

                statCard.appendChild(label);
                statCard.appendChild(value);
                container.appendChild(statCard);
            });
        }).catch(function () {
            container.innerHTML = '<div style="grid-column:1/-1;text-align:center;padding:2em;color:#f44336;">Failed to load statistics</div>';
        });
    }
})();
