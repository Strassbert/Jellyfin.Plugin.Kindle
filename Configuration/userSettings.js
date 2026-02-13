define(['loading', 'emby-button', 'emby-input'], function (loading) {
    'use strict';

    var i18n = {
        en: {
            title: 'Kindle Settings',
            emailLabel: 'Kindle E-Mail',
            emailDescription: 'Books will be sent to this address. You can find your Kindle email in your Amazon account settings.',
            save: 'Save',
            saved: 'Kindle email saved.',
            error: 'Failed to save Kindle email.'
        },
        de: {
            title: 'Kindle Einstellungen',
            emailLabel: 'Kindle E-Mail-Adresse',
            emailDescription: 'Bücher werden an diese Adresse gesendet. Du findest deine Kindle-E-Mail in deinen Amazon-Kontoeinstellungen.',
            save: 'Speichern',
            saved: 'Kindle-E-Mail gespeichert.',
            error: 'Kindle-E-Mail konnte nicht gespeichert werden.'
        }
    };

    function getLang() {
        var lang = (navigator.language || 'en').substring(0, 2).toLowerCase();
        return i18n[lang] ? lang : 'en';
    }

    function applyTranslations(view) {
        var t = i18n[getLang()];
        view.querySelector('#pageTitle').textContent = t.title;
        view.querySelector('#UserKindleEmail').setAttribute('label', t.emailLabel);
        view.querySelector('#emailDescription').textContent = t.emailDescription;
        view.querySelector('#btnSaveText').textContent = t.save;
    }

    return function (view, params) {
        applyTranslations(view);

        view.addEventListener('viewshow', function () {
            loading.show();
            var userId = ApiClient.getCurrentUserId();

            ApiClient.ajax({
                type: 'GET',
                url: ApiClient.getUrl('Kindle/UserEmail', { userId: userId }),
                dataType: 'json'
            }).then(function (result) {
                view.querySelector('#UserKindleEmail').value = result.email || '';
                loading.hide();
            }).catch(function () {
                loading.hide();
            });
        });

        view.querySelector('#kindleUserForm').addEventListener('submit', function (e) {
            e.preventDefault();
            loading.show();

            var userId = ApiClient.getCurrentUserId();
            var email = view.querySelector('#UserKindleEmail').value.trim();
            var t = i18n[getLang()];

            ApiClient.ajax({
                type: 'POST',
                url: ApiClient.getUrl('Kindle/UserEmail', { userId: userId, email: email })
            }).then(function () {
                loading.hide();
                Dashboard.alert(t.saved);
            }).catch(function () {
                loading.hide();
                Dashboard.alert(t.error);
            });

            return false;
        });
    };
});
