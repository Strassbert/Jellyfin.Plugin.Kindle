(function () {
    const PLUGIN_ID = "E3B2B4A1-1234-4567-89AB-CDEF12345678";
    console.log("Kindle Plugin: Script loaded via Injection!");

    // Warten bis DOM bereit ist, falls Injection sehr früh passiert
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPlugin);
    } else {
        initPlugin();
    }

    function initPlugin() {
        document.addEventListener('viewshow', function (e) {
            // Logik für Detailseite
            const isDetailPage = e.target.classList.contains('itemDetailPage');
            // ... (Hier dein bestehender Button-Code rein) ...
            // Nutze den Code aus der vorherigen Antwort für addKindleButton()
             if (isDetailPage) {
                const itemId = new URLSearchParams(window.location.search).get('id');
                // Check Item Type via API...
                if(itemId) {
                    ApiClient.getItem(ApiClient.getCurrentUserId(), itemId).then(item => {
                        if (item.Type === 'Book') renderButton(item);
                    });
                }
            }
        });
    }

    function renderButton(item) {
        const container = document.querySelector('.mainDetailButtons');
        // Verhindern von Doppel-Buttons
        if (!container || container.querySelector('.btnSendToKindle')) return;

        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btnSendToKindle detailButton emby-button';
        btn.innerHTML = '<span class="material-icons detailButton-icon">send</span><div class="detailButton-content"><div>Kindle</div></div>';
        btn.onclick = () => sendBook(item);
        container.appendChild(btn);
    }

    async function sendBook(item) {
        // ... (Dein API Call Code aus der vorherigen Antwort) ...
        // Tipp: Du kannst den Code 1:1 übernehmen, ApiClient ist global verfügbar.
        const userId = ApiClient.getCurrentUserId();
        const config = await ApiClient.getPluginConfiguration(PLUGIN_ID);
        
        if (!config.UserKindleEmails[userId]) {
            alert("Bitte erstelle erst eine Konfiguration in den Plugin Einstellungen.");
            return;
        }

        ApiClient.ajax({
            type: "POST",
            url: ApiClient.getUrl("Kindle/Send", { itemId: item.Id, userId: userId })
        }).then(() => {
            alert("An Kindle gesendet!");
        }).catch(err => alert("Fehler: " + err));
    }
})();