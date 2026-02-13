define(['loading', 'elementQuery', 'emby-button', 'emby-input', 'emby-checkbox', 'emby-select'], function (loading) {
    'use strict';

    var PLUGIN_ID = 'E3B2B4A1-1234-4567-89AB-CDEF12345678';

    var i18n = {
        en: {
            pageTitle: 'Kindle Share Configuration',
            smtpSection: 'SMTP Settings',
            smtpHostDesc: 'The SMTP server of your email provider (e.g. smtp.gmail.com).',
            senderEmailDesc: 'The sender email address. Leave empty to use the username as sender.',
            useSsl: 'Use SSL/TLS',
            oauthSection: 'OAuth2 Settings',
            useOAuth2: 'Use OAuth2 instead of password authentication',
            save: 'Save'
        },
        de: {
            pageTitle: 'Kindle Share Konfiguration',
            smtpSection: 'SMTP Einstellungen',
            smtpHostDesc: 'Der SMTP-Server deines E-Mail-Anbieters (z.B. smtp.gmail.com).',
            senderEmailDesc: 'Die Absender-E-Mail-Adresse. Leer lassen, um den Benutzernamen als Absender zu verwenden.',
            useSsl: 'SSL/TLS nutzen',
            oauthSection: 'OAuth2 Einstellungen',
            useOAuth2: 'OAuth2 statt Passwort-Authentifizierung nutzen',
            save: 'Speichern'
        }
    };

    function getLang() {
        var lang = (navigator.language || 'en').substring(0, 2).toLowerCase();
        return i18n[lang] ? lang : 'en';
    }

    function applyTranslations(view) {
        var t = i18n[getLang()];
        view.querySelector('#pageTitle').textContent = t.pageTitle;
        view.querySelector('#smtpSectionTitle').textContent = t.smtpSection;
        view.querySelector('#smtpHostDesc').textContent = t.smtpHostDesc;
        view.querySelector('#senderEmailDesc').textContent = t.senderEmailDesc;
        view.querySelector('#useSslLabel').textContent = t.useSsl;
        view.querySelector('#oauthSectionTitle').textContent = t.oauthSection;
        view.querySelector('#useOAuth2Label').textContent = t.useOAuth2;
        view.querySelector('#btnSaveText').textContent = t.save;
    }

    function toggleOAuthFields(view) {
        var show = view.querySelector('#UseOAuth2').checked;
        view.querySelector('#oauthFields').style.display = show ? '' : 'none';
    }

    function loadConfig(view, config) {
        view.querySelector('#SmtpHost').value = config.SmtpHost || '';
        view.querySelector('#SmtpPort').value = config.SmtpPort || 587;
        view.querySelector('#SmtpUser').value = config.SmtpUser || '';
        view.querySelector('#SmtpPassword').value = config.SmtpPassword || '';
        view.querySelector('#SenderEmail').value = config.SenderEmail || '';
        view.querySelector('#UseSsl').checked = config.UseSsl;
        view.querySelector('#UseOAuth2').checked = config.UseOAuth2;
        view.querySelector('#OAuthClientId').value = config.OAuthClientId || '';
        view.querySelector('#OAuthClientSecret').value = config.OAuthClientSecret || '';
        view.querySelector('#OAuthRefreshToken').value = config.OAuthRefreshToken || '';
        toggleOAuthFields(view);
        loading.hide();
    }

    return function (view, params) {
        applyTranslations(view);

        view.querySelector('#UseOAuth2').addEventListener('change', function () {
            toggleOAuthFields(view);
        });

        view.addEventListener('viewshow', function () {
            loading.show();
            ApiClient.getPluginConfiguration(PLUGIN_ID).then(function (config) {
                loadConfig(view, config);
            });
        });

        view.querySelector('#kindleConfigForm').addEventListener('submit', function (e) {
            e.preventDefault();
            loading.show();

            ApiClient.getPluginConfiguration(PLUGIN_ID).then(function (config) {
                config.SmtpHost = view.querySelector('#SmtpHost').value;
                config.SmtpPort = parseInt(view.querySelector('#SmtpPort').value, 10);
                config.SmtpUser = view.querySelector('#SmtpUser').value;
                config.SmtpPassword = view.querySelector('#SmtpPassword').value;
                config.SenderEmail = view.querySelector('#SenderEmail').value;
                config.UseSsl = view.querySelector('#UseSsl').checked;
                config.UseOAuth2 = view.querySelector('#UseOAuth2').checked;
                config.OAuthClientId = view.querySelector('#OAuthClientId').value;
                config.OAuthClientSecret = view.querySelector('#OAuthClientSecret').value;
                config.OAuthRefreshToken = view.querySelector('#OAuthRefreshToken').value;

                ApiClient.updatePluginConfiguration(PLUGIN_ID, config).then(function (result) {
                    Dashboard.processPluginConfigurationUpdateResult(result);
                });
            });
            return false;
        });
    };
});
