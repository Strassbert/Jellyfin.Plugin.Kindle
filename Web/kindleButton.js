(function () {
    const PLUGIN_ID = "E3B2B4A1-1234-4567-89AB-CDEF12345678";

    // Reagiert auf den Seitenwechsel im Jellyfin Web-Interface
    document.addEventListener('viewshow', function (e) {
        // Wir prüfen, ob wir uns auf der Detailseite befinden
        if (e.target.classList.contains('itemDetailPage')) {
            const itemId = new URLSearchParams(window.location.search).get('id');
            
            ApiClient.getItem(ApiClient.getCurrentUserId(), itemId).then(function (item) {
                // Nur anzeigen, wenn es ein Buch ist (Regel 4.1 Prüfung im Frontend vorab)
                if (item.MediaType === 'Book') {
                    renderKindleButton(item);
                }
            });
        }
    });

    function renderKindleButton(item) {
        const container = document.querySelector('.mainDetailButtons');
        if (!container || document.querySelector('.btnSendToKindle')) return;

        const btn = document.createElement('button');
        btn.className = 'fab btnSendToKindle emby-button';
        btn.innerHTML = '<i class="md-icon">send</i><span>An Kindle senden</span>';
        btn.style.marginLeft = "0.5em";
        btn.onclick = () => handleSendClick(item);
        
        container.appendChild(btn);
    }

    async function handleSendClick(item) {
        const userId = ApiClient.getCurrentUserId();
        const config = await ApiClient.getPluginConfiguration(PLUGIN_ID);
        const userEmail = config.UserKindleEmails[userId];

        if (!userEmail) {
            // Regel: Falls E-Mail fehlt, Eingabe fordern
            promptForEmail(userId, config);
        } else {
            sendToApi(item.Id, userId);
        }
    }

    function promptForEmail(userId, config) {
        const email = prompt("Bitte gib deine Kindle-E-Mail-Adresse ein:");
        if (email && email.includes('@')) {
            config.UserKindleEmails[userId] = email;
            ApiClient.updatePluginConfiguration(PLUGIN_ID, config).then(() => {
                alert("E-Mail gespeichert! Klicke erneut auf Senden.");
            });
        }
    }

    function sendToApi(itemId, userId) {
        const url = ApiClient.getUrl('Kindle/Send', { itemId: itemId, userId: userId });
        
        ApiClient.fetch({ url: url, type: 'POST' }).then(() => {
            alert("Buch wird an Kindle gesendet!");
        }).catch((err) => {
            alert("Fehler beim Senden: " + err);
        });
    }
})();