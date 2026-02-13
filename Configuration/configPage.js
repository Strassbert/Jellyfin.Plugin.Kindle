define(['ApiClient', 'loading'], function (apiClient, loading) {
    'use strict';

    return function (view, params) {
        view.addEventListener('viewshow', function () {
            loading.show();
            const pluginId = "E3B2B4A1-1234-4567-89AB-CDEF12345678"; // Deine GUID

            apiClient.getPluginConfiguration(pluginId).then(function (config) {
                view.querySelector('#SmtpHost').value = config.SmtpHost || '';
                view.querySelector('#SmtpPort').value = config.SmtpPort || 587;
                view.querySelector('#SmtpUser').value = config.SmtpUser || '';
                view.querySelector('#SmtpPassword').value = config.SmtpPassword || '';
                view.querySelector('#UseSsl').checked = config.UseSsl;
                view.querySelector('#UseOAuth2').checked = config.UseOAuth2;
                view.querySelector('#OAuthClientId').value = config.OAuthClientId || '';
                view.querySelector('#OAuthRefreshToken').value = config.OAuthRefreshToken || '';
                
                loading.hide();
            });
        });

        view.querySelector('#kindleConfigForm').addEventListener('submit', function (e) {
            e.preventDefault();
            loading.show();

            const pluginId = "E3B2B4A1-1234-4567-89AB-CDEF12345678";
            apiClient.getPluginConfiguration(pluginId).then(function (config) {
                config.SmtpHost = view.querySelector('#SmtpHost').value;
                config.SmtpPort = view.querySelector('#SmtpPort').value;
                config.SmtpUser = view.querySelector('#SmtpUser').value;
                config.SmtpPassword = view.querySelector('#SmtpPassword').value;
                config.UseSsl = view.querySelector('#UseSsl').checked;
                config.UseOAuth2 = view.querySelector('#UseOAuth2').checked;
                config.OAuthClientId = view.querySelector('#OAuthClientId').value;
                config.OAuthRefreshToken = view.querySelector('#OAuthRefreshToken').value;

                apiClient.updatePluginConfiguration(pluginId, config).then(function () {
                    Dashboard.processPluginConfigurationUpdateResult();
                    loading.hide();
                });
            });
        });
    };
});