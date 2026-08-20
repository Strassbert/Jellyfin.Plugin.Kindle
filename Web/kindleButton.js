(function () {
    'use strict';

    var DEBUG = false;

    function log() {
        if (DEBUG && window.console) {
            console.log.apply(console, ['[E-Book Share]'].concat(Array.prototype.slice.call(arguments)));
        }
    }

    // ---------------------------------------------------------------- i18n ----

    var i18n = {
        en: {
            send: 'Send to reader',
            sending: 'Sending…',
            sent: 'Sent!',
            sentToast: 'Book sent to your reader.',
            errorSending: 'Could not send the book.',
            enterEmailTitle: 'Set up your reader',
            enterEmail: 'Enter the email address of your e-book reader. You only have to do this once.',
            emailPlaceholder: 'name@kindle.com',
            emailInvalid: 'Please enter a valid email address.',
            save: 'Save and send',
            cancel: 'Cancel',
            emailSaved: 'Address saved.',
            tooLarge: 'Too large to send (%s MB, limit %s MB)',
            notConfigured: 'The administrator has not set up a mail server yet',
            settingsLink: 'E-Book Share',
            senderHint: 'Approve %s as a sender in your reader account, otherwise the mail is dropped.'
        },
        de: {
            send: 'An Reader senden',
            sending: 'Wird gesendet…',
            sent: 'Gesendet!',
            sentToast: 'Buch an deinen Reader gesendet.',
            errorSending: 'Das Buch konnte nicht gesendet werden.',
            enterEmailTitle: 'Reader einrichten',
            enterEmail: 'Gib die E-Mail-Adresse deines E-Book-Readers ein. Das ist nur einmal nötig.',
            emailPlaceholder: 'name@kindle.com',
            emailInvalid: 'Bitte eine gültige E-Mail-Adresse eingeben.',
            save: 'Speichern und senden',
            cancel: 'Abbrechen',
            emailSaved: 'Adresse gespeichert.',
            tooLarge: 'Zu groß zum Senden (%s MB, Limit %s MB)',
            notConfigured: 'Der Administrator hat noch keinen Mailserver eingerichtet',
            settingsLink: 'E-Book Share',
            senderHint: 'Gib %s im Reader-Konto als Absender frei, sonst wird die Mail verworfen.'
        }
    };

    // Jellyfin writes the user's chosen interface language onto <html lang>.
    // navigator.language is only the fallback: it reports the browser's locale,
    // which is often not the language the user picked in Jellyfin.
    function getLang() {
        var candidates = [
            document.documentElement.getAttribute('lang'),
            navigator.language
        ];

        for (var i = 0; i < candidates.length; i++) {
            var value = candidates[i];
            if (!value) continue;
            var code = value.substring(0, 2).toLowerCase();
            if (i18n[code]) return code;
        }

        return 'en';
    }

    function t(key) {
        var lang = getLang();
        var value = (i18n[lang] && i18n[lang][key]) || i18n.en[key] || key;

        for (var i = 1; i < arguments.length; i++) {
            value = value.replace('%s', arguments[i]);
        }

        return value;
    }

    // ApiClient.ajax rejects with the fetch Response object (apiClient.js:
    // "return Promise.reject(response)"), so the body has to be read asynchronously.
    // Reading err.responseText - as this plugin did before - always yielded undefined,
    // which is why the server's specific messages never reached the user.
    function localizedError(err, fallbackKey) {
        return Promise.resolve()
            .then(function () {
                if (err && typeof err.text === 'function') return err.text();
                if (err && typeof err.responseText === 'string') return err.responseText;
                return '';
            })
            .then(function (text) {
                var body = JSON.parse(text || '{}');
                if (getLang() === 'de' && body.errorDe) return body.errorDe;
                if (body.error) return body.error;
                return t(fallbackKey);
            })
            .catch(function () {
                return t(fallbackKey);
            });
    }

    // -------------------------------------------------------------- helpers ---

    function toast(message) {
        // The toast module is the non-blocking, native-looking option. Dashboard.alert
        // opens a modal the user has to dismiss, which is far too heavy for a
        // "sent successfully" notice, so it is only the fallback.
        if (typeof window.require === 'function') {
            try {
                window.require(['toast'], function (showToast) { showToast(message); });
                return;
            } catch (e) { /* fall through */ }
        }

        if (window.Dashboard && Dashboard.alert) {
            Dashboard.alert(message);
        }
    }

    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email);
    }

    function apiUrl(path, params) {
        return window.ApiClient.getUrl(path, params || {});
    }

    function getStatus(itemId) {
        return window.ApiClient.ajax({
            type: 'GET',
            url: apiUrl('Kindle/Status', itemId ? { itemId: itemId } : {}),
            dataType: 'json'
        });
    }

    // --------------------------------------------------------------- button ---

    function buttonMarkup(icon, label) {
        return '<span class="material-icons detailButton-icon" aria-hidden="true">' + icon + '</span>' +
            '<div class="detailButton-content"><div class="detailButton-content-text">' + label + '</div></div>';
    }

    function visibleDetailButtons() {
        // Jellyfin keeps previously visited views in the DOM and only marks them
        // .hide, so an unscoped selector can append the button to a stale page.
        return document.querySelector('.itemDetailPage:not(.hide) .mainDetailButtons')
            || document.querySelector('.mainDetailButtons');
    }

    function renderButton(item, status) {
        var container = visibleDetailButtons();
        if (!container || container.querySelector('.btnSendToKindle')) return;

        // An unsupported format can never be sent, so showing a permanently dead
        // button would just be noise. Everything else renders, disabled with the
        // reason when needed, so the user learns why before clicking.
        if (status && status.Reason === 'FORMAT') {
            log('skipping button, unsupported format', status.Extension);
            return;
        }

        var blockedReason = null;
        if (status && status.Reason === 'TOO_LARGE') {
            blockedReason = t('tooLarge', status.FileSizeMb, status.MaxFileSizeMb);
        } else if (status && !status.SmtpConfigured) {
            blockedReason = t('notConfigured');
        }

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.setAttribute('is', 'emby-button');
        btn.className = 'btnSendToKindle detailButton emby-button';
        btn.innerHTML = buttonMarkup('send', t('send'));

        if (blockedReason) {
            btn.disabled = true;
            btn.title = blockedReason;
            btn.style.opacity = '0.5';
        } else {
            btn.title = t('send');
            btn.addEventListener('click', function () { startSend(item, btn, status); });
        }

        container.appendChild(btn);
        log('button rendered for', item.Name);
    }

    function startSend(item, btn, status) {
        if (status && status.HasEmail) {
            doSend(item, btn);
            return;
        }

        // Re-check rather than trusting the status captured at render time: the user
        // may have set the address in another tab since the page loaded.
        getStatus().then(function (fresh) {
            if (fresh.HasEmail) {
                doSend(item, btn);
            } else {
                showEmailDialog(item, btn, fresh);
            }
        }).catch(function () {
            showEmailDialog(item, btn, status);
        });
    }

    function doSend(item, btn) {
        var originalHtml = btn.innerHTML;

        btn.disabled = true;
        btn.innerHTML = buttonMarkup('hourglass_empty', t('sending'));

        window.ApiClient.ajax({
            type: 'POST',
            url: apiUrl('Kindle/Send', { itemId: item.Id }),
            dataType: 'json'
        }).then(function () {
            btn.innerHTML = buttonMarkup('check', t('sent'));
            toast(t('sentToast'));

            setTimeout(function () {
                btn.innerHTML = originalHtml;
                btn.disabled = false;
            }, 3000);
        }).catch(function (err) {
            localizedError(err, 'errorSending').then(toast);
            btn.innerHTML = originalHtml;
            btn.disabled = false;
        });
    }

    // --------------------------------------------------------------- dialog ---

    function showEmailDialog(item, btn, status) {
        var previousFocus = document.activeElement;

        var overlay = document.createElement('div');
        overlay.className = 'kindleDialogOverlay';
        overlay.style.cssText =
            'position:fixed;inset:0;background:rgba(0,0,0,0.7);z-index:1000000;' +
            'display:flex;align-items:center;justify-content:center;padding:1em;';

        var dialog = document.createElement('div');
        dialog.setAttribute('role', 'dialog');
        dialog.setAttribute('aria-modal', 'true');
        dialog.setAttribute('aria-label', t('enterEmailTitle'));
        dialog.style.cssText =
            'background:var(--theme-body-background,#1c1c1e);color:var(--theme-text-color,#fff);' +
            'padding:1.5em;border-radius:10px;max-width:26em;width:100%;' +
            'box-shadow:0 8px 32px rgba(0,0,0,0.6);font-size:1em;';

        var senderHint = status && status.SenderEmail
            ? '<p style="margin:0 0 1em;opacity:0.7;font-size:0.9em;">' +
              escapeHtml(t('senderHint', status.SenderEmail)) + '</p>'
            : '';

        dialog.innerHTML =
            '<h2 style="margin:0 0 0.4em;font-size:1.25em;">' + escapeHtml(t('enterEmailTitle')) + '</h2>' +
            '<p style="margin:0 0 1em;opacity:0.85;">' + escapeHtml(t('enterEmail')) + '</p>' +
            senderHint +
            '<input type="email" class="kindleEmailInput" inputmode="email" autocomplete="email" ' +
            'placeholder="' + escapeHtml(t('emailPlaceholder')) + '" ' +
            'style="width:100%;padding:0.6em;border:1px solid rgba(255,255,255,0.25);border-radius:5px;' +
            'background:rgba(255,255,255,0.08);color:inherit;font-size:1em;box-sizing:border-box;" />' +
            '<div class="kindleDialogError" style="color:#f44336;min-height:1.4em;font-size:0.85em;margin-top:0.3em;"></div>' +
            '<div style="display:flex;gap:0.5em;margin-top:0.8em;justify-content:flex-end;flex-wrap:wrap;">' +
            '<button type="button" class="kindleDialogCancel" style="padding:0.6em 1.2em;border:1px solid rgba(255,255,255,0.25);' +
            'border-radius:5px;background:transparent;color:inherit;cursor:pointer;font-size:1em;">' +
            escapeHtml(t('cancel')) + '</button>' +
            '<button type="button" class="kindleDialogSave" style="padding:0.6em 1.2em;border:none;border-radius:5px;' +
            'background:var(--theme-primary-color,#00a4dc);color:#fff;cursor:pointer;font-size:1em;">' +
            escapeHtml(t('save')) + '</button>' +
            '</div>';

        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        var input = dialog.querySelector('.kindleEmailInput');
        var errorBox = dialog.querySelector('.kindleDialogError');
        var saveBtn = dialog.querySelector('.kindleDialogSave');

        input.focus();

        function close() {
            document.removeEventListener('keydown', onKeyDown, true);
            if (overlay.parentNode) overlay.parentNode.removeChild(overlay);
            if (previousFocus && previousFocus.focus) previousFocus.focus();
        }

        function onKeyDown(e) {
            if (e.key === 'Escape') {
                e.preventDefault();
                close();
                return;
            }

            // Keep tabbing inside the dialog while it is open.
            if (e.key === 'Tab') {
                var focusable = dialog.querySelectorAll('input, button');
                var first = focusable[0];
                var last = focusable[focusable.length - 1];

                if (e.shiftKey && document.activeElement === first) {
                    e.preventDefault();
                    last.focus();
                } else if (!e.shiftKey && document.activeElement === last) {
                    e.preventDefault();
                    first.focus();
                }
            }
        }

        function submit() {
            var email = input.value.trim();

            if (!isValidEmail(email)) {
                errorBox.textContent = t('emailInvalid');
                input.style.borderColor = '#f44336';
                input.focus();
                return;
            }

            saveBtn.disabled = true;
            errorBox.textContent = '';

            window.ApiClient.ajax({
                type: 'POST',
                url: apiUrl('Kindle/UserEmail', { email: email })
            }).then(function () {
                close();
                toast(t('emailSaved'));
                doSend(item, btn);
            }).catch(function (err) {
                saveBtn.disabled = false;
                input.style.borderColor = '#f44336';
                localizedError(err, 'emailInvalid').then(function (message) {
                    errorBox.textContent = message;
                });
            });
        }

        document.addEventListener('keydown', onKeyDown, true);
        dialog.querySelector('.kindleDialogCancel').addEventListener('click', close);
        saveBtn.addEventListener('click', submit);

        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) close();
        });

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                submit();
            }
        });
    }

    function escapeHtml(value) {
        var div = document.createElement('div');
        div.textContent = value == null ? '' : String(value);
        return div.innerHTML;
    }

    // ------------------------------------------------------- page detection ---

    function getHashParam(name) {
        var hash = window.location.hash;
        var qs = hash.indexOf('?') !== -1 ? hash.substring(hash.indexOf('?')) : window.location.search;
        return new URLSearchParams(qs).get(name);
    }

    // Remembers what the current detail page resolved to, so the observer can put
    // the button back after another plugin re-renders the page without issuing a
    // second round of API calls - and so a movie page (not a Book) is decided once
    // instead of on every mutation batch.
    var handled = { itemId: null, item: null, status: null, renderable: false };

    function onDetailPage() {
        var itemId = getHashParam('id');
        if (!itemId || !window.ApiClient) return;

        var container = visibleDetailButtons();
        if (!container) return;

        if (handled.itemId === itemId) {
            if (handled.renderable && !container.querySelector('.btnSendToKindle')) {
                renderButton(handled.item, handled.status);
            }
            return;
        }

        handled = { itemId: itemId, item: null, status: null, renderable: false };

        window.ApiClient.getItem(window.ApiClient.getCurrentUserId(), itemId).then(function (item) {
            if (!item || item.Type !== 'Book' || handled.itemId !== itemId) return;

            // Ask the server whether this specific file can actually be sent, so the
            // format and size limits are enforced before the user clicks rather than
            // surfacing as an error afterwards.
            return getStatus(item.Id).then(function (status) {
                if (handled.itemId !== itemId) return;
                handled.item = item;
                handled.status = status;
                handled.renderable = status.Reason !== 'FORMAT';
                renderButton(item, status);
            }).catch(function () {
                if (handled.itemId !== itemId) return;
                handled.item = item;
                handled.status = null;
                handled.renderable = true;
                renderButton(item, null);
            });
        }).catch(function () {
            /* not an item page, or the item vanished */
        });
    }

    function addPreferencesLink() {
        var menuContainer = document.querySelector('#myPreferencesMenuPage:not(.hide) .verticalSection');
        if (!menuContainer || document.querySelector('#kindleUserPrefsLink')) return;

        var link = document.createElement('a');
        link.id = 'kindleUserPrefsLink';
        link.setAttribute('is', 'emby-linkbutton');
        link.setAttribute('data-ripple', 'false');
        link.href = '#/configurationpage?name=KindleUserSettings';
        link.className = 'listItem-border emby-button';
        link.style.display = 'block';
        link.style.padding = '0';
        link.style.margin = '0';

        link.innerHTML =
            '<div class="listItem">' +
            '<span class="material-icons listItemIcon listItemIcon-transparent" aria-hidden="true">menu_book</span>' +
            '<div class="listItemBody">' +
            '<div class="listItemBodyText">' + escapeHtml(t('settingsLink')) + '</div>' +
            '</div>' +
            '</div>';

        menuContainer.appendChild(link);
        log('preferences link injected');
    }

    // A MutationObserver reacts to the actual re-render instead of the previous
    // 500 ms polling loop, which kept a timer running for as long as the
    // preferences page was open. Other plugins re-rendering that page discard our
    // link, and the observer puts it straight back.
    function watchDom() {
        var scheduled = false;

        var observer = new MutationObserver(function () {
            if (scheduled) return;
            scheduled = true;

            window.requestAnimationFrame(function () {
                scheduled = false;
                addPreferencesLink();
                if (document.querySelector('.itemDetailPage:not(.hide) .mainDetailButtons')) {
                    onDetailPage();
                }
            });
        });

        observer.observe(document.body, { childList: true, subtree: true });
    }

    function init() {
        if (!document.body) {
            document.addEventListener('DOMContentLoaded', init);
            return;
        }

        document.addEventListener('viewshow', function (e) {
            var target = e.target;
            if (!target || !target.classList) return;

            if (target.classList.contains('itemDetailPage')) {
                onDetailPage();
            }

            addPreferencesLink();
        });

        window.addEventListener('hashchange', addPreferencesLink);

        watchDom();
        addPreferencesLink();
        onDetailPage();

        log('initialised');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
