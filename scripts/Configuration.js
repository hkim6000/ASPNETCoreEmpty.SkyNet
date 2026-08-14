var ConfigurationJs = (function () {

    return {

        init: function () {
            ConfigurationJs._initTabs();
            ConfigurationJs._watchReactivation(); ConfigurationJs._activateFirstTab();
        },

        // ── Sub tab switching ─────────────────────────────────────────────
        _initTabs: function () {
            var tabs = document.getElementById('cfg_tabs');
            if (!tabs) { return; }

            tabs.addEventListener('click', function (e) {
                var tab = e.target.closest('.cfg-tab');
                if (!tab) { return; }
                ConfigurationJs._activateTab(tab);
            });
        },

        // ── Activate first tab ────────────────────────────────────────────
        _takePendingTab: function (tabs) {
            var name = window.snPendingTab || ''; if (!name) return null;
            window.snPendingTab = ''; return tabs.querySelector('[data-name="' + name + '"]');
        },
        _watchReactivation: function () {
            var wrap = document.querySelector('.cfg-wrap'); var panel = wrap ? wrap.closest('.sn-panel') : null;
            if (!panel) return;
            new MutationObserver(function () {
                if (!panel.classList.contains('sn-active')) return;
                var tabs = document.getElementById('cfg_tabs'); if (!tabs) return;
                var t = ConfigurationJs._takePendingTab(tabs); if (t) ConfigurationJs._activateTab(t);
            }).observe(panel, { attributes: true, attributeFilter: ['class'] });
        },
        _activateFirstTab: function () {
            var tabs = document.getElementById('cfg_tabs');
            if (!tabs) { return; }
            var first = ConfigurationJs._takePendingTab(tabs) || tabs.querySelector('.cfg-tab');
            if (first) { ConfigurationJs._activateTab(first); }
        },

        // ── Activate tab ──────────────────────────────────────────────────
        _activateTab: function (tab) {
            var tabs = document.getElementById('cfg_tabs');
            if (!tabs) { return; }

            // Already active — do nothing
            if (tab.classList.contains('cfg-active')) { return; }

            tabs.querySelectorAll('.cfg-tab').forEach(function (t) {
                t.classList.remove('cfg-active');
            });

            tab.classList.add('cfg-active');

            var target = tab.getAttribute('data-tab');
            var tabId = tab.getAttribute('id');
            var tabName = tab.getAttribute('data-name');
            var label = tab.querySelector('.cfg-t-label')?.textContent || '';

            ConfigurationJs._loadPanel(target, tabId, tabName, label);
        },

        // ── Load panel content ────────────────────────────────────────────
        _loadPanel: function (target, tabId, tabName, label) {
            var content = document.getElementById('cfg_content');
            if (!content || !target) { return; }

            // Tabs whose content must reflect live server state (e.g. run mode) reload every time.
            var alwaysReload = (tabName === 'Configuration_Seed');

            // Check if panel already loaded
            var existing = document.getElementById(target);
            if (existing && !alwaysReload) {
                document.querySelectorAll('.cfg-panel').forEach(function (p) { p.classList.remove('cfg-active'); });
                existing.classList.add('cfg-active');
                return;
            }
            if (existing) { existing.parentNode.removeChild(existing); }

            // Create placeholder panel
            var panel = document.createElement('section');
            panel.className = 'cfg-panel cfg-active';
            panel.id = target;
            panel.dataset.dynamic = 'true';
            panel.innerHTML = '<div style="padding:40px;color:#8C897F;">' + label + ' — loading...</div>';

            document.querySelectorAll('.cfg-panel').forEach(function (p) { p.classList.remove('cfg-active'); });
            content.appendChild(panel);

            // Load from server
            $WaitOn();
            var data = [{ key: 't', vlu: target }, { key: 'tabname', vlu: tabName }];
            $ApiRequest('Configuration/OpenTab', JSON.stringify(data));
        }

    };

})();

ConfigurationJs.init();
document.querySelector('.cfg-wrap').style.visibility = '';


