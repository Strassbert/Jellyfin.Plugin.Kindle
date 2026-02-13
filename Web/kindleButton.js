(function () {
    'use strict';

    var PLUGIN_ID = 'E3B2B4A1-1234-4567-89AB-CDEF12345678';

    // i18n strings
    var i18n = {
        en: {
            sendToKindle: 'Send to Kindle',
            sending: 'Sending...',
            sent: 'Sent to Kindle!',
            errorSending: 'Failed to send to Kindle.',
            noEmail: 'No Kindle email configured.',
            enterEmail: 'Enter your Kindle email address:',
            save: 'Save',
            cancel: 'Cancel',
            emailPlaceholder: 'your-name@kindle.com',
            emailSaved: 'Kindle email saved.',
            fileTooLarge: 'File is too large for Kindle (max 50 MB).',
            formatNotSupported: 'This file format is not supported by Kindle.',
            settingsLink: 'Kindle Settings'
        },
        de: {
            sendToKindle: 'An Kindle senden',
            sending: 'Wird gesendet...',
            sent: 'An Kindle gesendet!',
            errorSending: 'Senden an Kindle fehlgeschlagen.',
            noEmail: 'Keine Kindle-E-Mail konfiguriert.',
            enterEmail: 'Gib deine Kindle-E-Mail-Adresse ein:',
            save: 'Speichern',
            cancel: 'Abbrechen',
            emailPlaceholder: 'dein-name@kindle.com',
            emailSaved: 'Kindle-E-Mail gespeichert.',
            fileTooLarge: 'Datei ist zu groß für Kindle (max. 50 MB).',
            formatNotSupported: 'Dieses Dateiformat wird vom Kindle nicht unterstützt.',
            settingsLink: 'Kindle Einstellungen'
        }
    };

    function getLang() {
        var lang = (navigator.language || 'en').substring(0, 2).toLowerCase();
        return i18n[lang] ? lang : 'en';
    }

    function t(key) {
        return i18n[getLang()][key] || i18n.en[key] || key;
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

    // Wait for DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPlugin);
    } else {
        initPlugin();
    }

    function initPlugin() {
        document.addEventListener('viewshow', function (e) {
            if (!e.target.classList.contains('itemDetailPage')) return;

            var itemId = new URLSearchParams(window.location.search).get('id');
            if (!itemId) return;

            ApiClient.getItem(ApiClient.getCurrentUserId(), itemId).then(function (item) {
                if (item.Type === 'Book') {
                    renderButton(item);
                }
            });
        });

        // Inject Kindle link into user preferences page
        document.addEventListener('viewshow', function (e) {
            var page = e.target;
            if (!page.querySelector || !page.querySelector('.myPreferencesMenuPage, .userProfilePage, .myPreferencesPage')) return;

            var container = page.querySelector('.listItems, .menuLinks, .paperList');
            if (!container || container.querySelector('.kindleSettingsLink')) return;

            var link = document.createElement('a');
            link.href = '#!/configurationpage?name=KindleUserSettings';
            link.className = 'kindleSettingsLink listItem-border listItem listItem-button';
            link.innerHTML = '<span class="material-icons listItemIcon listItemIcon-transparent">email</span>' +
                '<div class="listItemBody"><div class="listItemBodyText">' + t('settingsLink') + '</div></div>';
            container.appendChild(link);
        });
    }

    function renderButton(item) {
        var container = document.querySelector('.mainDetailButtons');
        if (!container || container.querySelector('.btnSendToKindle')) return;

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btnSendToKindle detailButton emby-button';
        btn.innerHTML = '<span class="material-icons detailButton-icon">send</span>' +
            '<div class="detailButton-content"><div class="detailButton-content-text">' +
            t('sendToKindle') + '</div></div>';
        btn.addEventListener('click', function () {
            sendBook(item, btn);
        });
        container.appendChild(btn);
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
