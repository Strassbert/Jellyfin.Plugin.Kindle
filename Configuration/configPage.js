define(['loading', 'elementQuery', 'emby-button', 'emby-input', 'emby-checkbox', 'emby-select'], function (loading) {
    'use strict';

    const PLUGIN_ID = "E3B2B4A1-1234-4567-89AB-CDEF12345678";

    function loadConfig(view, config) {
        view.querySelector('#SmtpHost').value = config.SmtpHost || '';
        view.querySelector('#SmtpPort').value = config.SmtpPort || 587;
        view.querySelector('#SmtpUser').value = config.SmtpUser || '';
        view.querySelector('#SmtpPassword').value = config.SmtpPassword || '';
        view.querySelector('#UseSsl').checked = config.UseSsl;
        loading.hide();
    }

    return function (view, params) {
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
                config.SmtpPort = view.querySelector('#SmtpPort').value;
                config.SmtpUser = view.querySelector('#SmtpUser').value;
                config.SmtpPassword = view.querySelector('#SmtpPassword').value;
                config.UseSsl = view.querySelector('#UseSsl').checked;

                ApiClient.updatePluginConfiguration(PLUGIN_ID, config).then(function (result) {
                    Dashboard.processPluginConfigurationUpdateResult(result);
                });
            });
            return false;
        });
    };
});