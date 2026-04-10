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
            helpTitle: 'Example Kindle',
            helpText1: 'To find your Send to Kindle email address, go to ',
            helpLink1: 'Manage Your Content and Devices',
            helpText2: ' > Settings > Personal Document Settings.',
            helpText3: 'Only approved email addresses can send files to your Kindle library. Before sending, make sure the account you will use is listed in your ',
            helpLink2: 'Email List for Approved Personal Document',
            helpText4: ' in your Personal Document Settings.'
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
            helpTitle: 'Beispiel Kindle',
            helpText1: 'Um deine f\u00fcr Send to Kindle verwendete E-Mail-Adresse zu finden, w\u00e4hle ',
            helpLink1: 'Meine Inhalte und Ger\u00e4te verwalten',
            helpText2: ' > Einstellungen > Pers\u00f6nliche Dokumente Einstellungen.',
            helpText3: 'Nur genehmigte E-Mail-Adressen k\u00f6nnen Dateien an deine Kindle-Bibliothek senden. Vergewissere dich vor dem Senden, dass das Konto, das du verwenden wirst, in deiner ',
            helpLink2: 'E-Mail-Liste f\u00fcr genehmigte pers\u00f6nliche Dokumente',
            helpText4: ' in deinen Einstellungen f\u00fcr pers\u00f6nliche Dokumente aufgef\u00fchrt ist.'
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

    // Wait for DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPlugin);
    } else {
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

        // Inline styles for popup
        Object.assign(popup.style, {
            position: 'fixed',
            zIndex: '10000',
            backgroundColor: '#202020',
            color: '#fff',
            padding: '1em',
            borderRadius: '0.3em',
            boxShadow: '0 0 20px rgba(0,0,0,0.5)',
            minWidth: '300px',
            maxWidth: '400px',
            fontFamily: 'inherit'
        });

        // Responsive positioning
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
        emailInput.placeholder = t('emailPlaceholder');
        emailInput.value = currentEmail;

        Object.assign(emailInput.style, {
            width: '100%',
            padding: '0.6em',
            marginBottom: '1em',
            border: '1px solid rgba(255,255,255,0.2)',
            borderRadius: '5px',
            backgroundColor: 'rgba(255,255,255,0.05)',
            color: '#fff',
            fontSize: '1em',
            boxSizing: 'border-box'
        });

        var currentEmailDisplay = document.createElement('div');
        currentEmailDisplay.style.cssText = 'margin-bottom:1em;padding:0.5em;background:rgba(255,255,255,0.05);border-radius:3px;font-size:0.9em;opacity:0.8;';
        if (currentEmail) {
            currentEmailDisplay.textContent = t('currentEmail').replace('{email}', currentEmail);
        } else {
            currentEmailDisplay.textContent = t('noEmailSet');
        }

        var buttonContainer = document.createElement('div');
        Object.assign(buttonContainer.style, {
            display: 'flex',
            gap: '0.5em',
            justifyContent: 'flex-end',
            marginBottom: '1em'
        });

        var saveBtn = document.createElement('button');
        saveBtn.textContent = t('save');
        Object.assign(saveBtn.style, {
            padding: '0.5em 1.2em',
            border: 'none',
            borderRadius: '5px',
            backgroundColor: '#00a4dc',
            color: '#fff',
            cursor: 'pointer',
            fontSize: '1em'
        });

        var clearBtn = document.createElement('button');
        clearBtn.textContent = t('clear');
        Object.assign(clearBtn.style, {
            padding: '0.5em 1.2em',
            border: '1px solid rgba(255,255,255,0.2)',
            borderRadius: '5px',
            backgroundColor: 'transparent',
            color: '#fff',
            cursor: 'pointer',
            fontSize: '1em'
        });

        var cancelBtn = document.createElement('button');
        cancelBtn.textContent = t('cancel');
        Object.assign(cancelBtn.style, {
            padding: '0.5em 1.2em',
            border: '1px solid rgba(255,255,255,0.2)',
            borderRadius: '5px',
            backgroundColor: 'transparent',
            color: '#fff',
            cursor: 'pointer',
            fontSize: '1em'
        });

        buttonContainer.appendChild(clearBtn);
        buttonContainer.appendChild(cancelBtn);
        buttonContainer.appendChild(saveBtn);

        // Help section (collapsible)
        var helpSection = document.createElement('div');
        helpSection.style.cssText = 'border-top:1px solid rgba(255,255,255,0.1);padding-top:1em;margin-top:1em;';

        var helpSummary = document.createElement('div');
        helpSummary.style.cssText = 'cursor:pointer;font-weight:bold;user-select:none;display:flex;align-items:center;gap:0.5em;';
        helpSummary.innerHTML = '<span style="display:inline-block;">▸</span> ' + t('helpTitle');

        var helpContent = document.createElement('div');
        helpContent.style.display = 'none';
        helpContent.style.cssText = 'margin-top:0.8em;font-size:0.85em;opacity:0.85;line-height:1.5;';

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

        btn.innerHTML = '<span class="material-icons detailButton-icon">send</span>' +
            '<div class="detailButton-content"><div class="detailButton-content-text">' +
            t('sendToKindle') + '</div></div>';
        btn.addEventListener('click', function () {
            sendBook(item, btn);
        });
        container.appendChild(btn);
        console.log('[Kindle] Send button rendered for:', item.Name);
    }

    function sendBook(item, btn) {
        var userId = ApiClient.getCurrentUserId();

        // Check if user has Kindle email configured
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
    }

    function doSend(item, btn, userId) {
        // Disable button and show loading state
        btn.disabled = true;
        var originalHtml = btn.innerHTML;
        btn.innerHTML = '<span class="material-icons detailButton-icon">hourglass_empty</span>' +
            '<div class="detailButton-content"><div class="detailButton-content-text">' +
            t('sending') + '</div></div>';

        ApiClient.ajax({
            type: 'POST',
            url: ApiClient.getUrl('Kindle/Send', { itemId: item.Id, userId: userId }),
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
})();
