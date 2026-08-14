var Configuration_UsersJs = (function () {

    function V(id) { return document.getElementById(id); }

    // ── State ─────────────────────────────────────────────────────────────────
    var _data = null;   // { Users, Roles, Sites, UseRoles, UseSites }
    var _currentUser = null;
    var _canDelete = false;
    var _canResetPwd = false;
    var _savedState = null;
    var _searchTimer = null;
    var _canSave = false;
    var _canAddUser = false;
    var _canUpload = false;

    // ── Init ──────────────────────────────────────────────────────────────────
    function init(roles, sites, perms) {
        _data = { Users: [], Roles: roles || [], Sites: sites || [], UseRoles: [], UseSites: [] };
        _canSave = perms.save;
        _canAddUser = perms.addUser;
        _canUpload = perms.uploadPhoto;
        _canDelete = perms.deleteUser || false;
        _canResetPwd = perms.resetPwd || false;
    }

    // ── Utils ─────────────────────────────────────────────────────────────────
    function safeId(s) { return String(s).replace(/[^a-z0-9]/gi, '_'); }
    function esc(s) { return String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;'); }
    function initials(fn, ln) { return ((fn || '').charAt(0) + (ln || '').charAt(0)).toUpperCase(); }

    // ── Search ────────────────────────────────────────────────────────────────
    function searchUsers(q) {
        clearTimeout(_searchTimer);
        _searchTimer = setTimeout(function () { _doSearch(q); }, 300);
    }

    function _doSearch(q) {
        $WaitOn();
        $ApiRequest('Configuration_Users/SearchUsers', JSON.stringify([{ key: 'q', vlu: q }]));
    }

    function onSearchResult(users, useRoles, useSites) {
        _data.Users = users || [];
        _data.UseRoles = useRoles || [];
        _data.UseSites = useSites || [];
        _currentUser = null;
        _savedState = null;
        _renderUserList();
        _renderEmptyEditor();
    }

    // ── User List ─────────────────────────────────────────────────────────────
    function _renderUserList() {
        var c = V('cfgu-user-list');
        c.innerHTML = '';
        if (!_data.Users || _data.Users.length === 0) {
            c.innerHTML = '<div class="cfgu-empty-list">No users found.</div>';
            return;
        }
        _data.Users.forEach(function (u) {
            var div = document.createElement('div');
            div.className = 'cfgu-user-item';
            div.id = 'cfgu-ui-' + safeId(u.USER_ID);
            var avatarHtml = u.PHOTO_LINK
                ? '<img src="' + esc(u.PHOTO_LINK) + '" style="width:28px;height:28px;border-radius:999px;object-fit:cover;" />'
                : esc(initials(u.FIRST_NAME, u.LAST_NAME));
            div.innerHTML =
                '<div class="cfgu-user-avatar">' + avatarHtml + '</div>' +
                '<div class="cfgu-user-info">' +
                '<span class="cfgu-user-name-sm">' + esc(u.FIRST_NAME + ' ' + u.LAST_NAME) + '</span>' +
                '<span class="cfgu-user-email-sm">' + esc(u.USER_EMAIL) + '</span>' +
                '</div>';
            div.onclick = function () { selectUser(u.USER_ID); };
            c.appendChild(div);
        });
    }

    // ── Select User ───────────────────────────────────────────────────────────
    function selectUser(userId) {
        document.querySelectorAll('.cfgu-user-item').forEach(function (el) { el.classList.remove('cfgu-active'); });
        var item = V('cfgu-ui-' + safeId(userId));
        if (item) item.classList.add('cfgu-active');

        $WaitOn();
        $ApiRequest('Configuration_Users/GetUser', JSON.stringify([{ key: 'userid', vlu: userId }]));
    }

    function onGetUser(user, useRoles, useSites) {
        if (!user) return;
        _currentUser = user;
        _data.UseRoles = useRoles || [];
        _data.UseSites = useSites || [];

        // Update in Users list
        for (var i = 0; i < _data.Users.length; i++) {
            if (_data.Users[i].USER_ID === user.USER_ID) { _data.Users[i] = user; break; }
        }

        _renderEditor();
        // Capture state after DOM settles
        setTimeout(function () {
            _savedState = _captureState();
            _refreshActionBar();
        }, 0);

        var body = V('cfgu-editor-body');
        if (body) body.scrollTop = 0;
    }

    // ── Editor ────────────────────────────────────────────────────────────────
    function _renderEmptyEditor() {
        V('cfgu-editor-name').textContent = 'No User Selected';
        V('cfgu-editor-desc').textContent = 'Search a user from the left panel to begin editing.';
        V('cfgu-editor-body').innerHTML = '';
        _refreshActionBar();
    }

    function _renderEditor() {
        var u = _currentUser;
        if (!u) return;

        V('cfgu-editor-name').textContent = u.FIRST_NAME + ' ' + u.LAST_NAME;
        V('cfgu-editor-desc').textContent = u.USER_EMAIL;

        var userRoles = (_data.UseRoles || []).filter(function (r) { return r.USER_ID === u.USER_ID; }).map(function (r) { return r.ROLE_ID; });
        var userSites = (_data.UseSites || []).filter(function (s) { return s.USER_ID === u.USER_ID; }).map(function (s) { return s.SITE_ID; });

        var html = '';

        // ── Avatar & Info ─────────────────────────────────────────────────
        html += '<div class="cfgu-section">';
        var avatarContent = u.PHOTO_LINK
            ? '<img src="' + esc(u.PHOTO_LINK) + '?t=' + Date.now() + '" style="width:52px;height:52px;border-radius:999px;object-fit:cover;" />'
            : esc(initials(u.FIRST_NAME, u.LAST_NAME));
        html += '<div class="cfgu-avatar-row">';
        html += '<div class="cfgu-avatar-lg" id="cfgu-avatar">' + avatarContent + '</div>';
        if (_canUpload) {
            html += '<div class="cfgu-avatar-info">';
            html += '<button class="cfgu-btn-upload" onclick="Configuration_UsersJs.triggerPhotoUpload(\'' + esc(u.USER_ID) + '\')">📷 Upload Photo</button>';
            html += '<span class="cfgu-upload-hint">JPG, PNG · Max 5MB</span>';
            html += '</div>';
        }
        html += '</div>';
        html += '<div class="cfgu-section-body"><div class="cfgu-form-grid">';
        html += _field('FIRST NAME', 'cfgu-first-name', u.FIRST_NAME, true);
        html += _field('LAST NAME', 'cfgu-last-name', u.LAST_NAME, true);
        html += _field('EMAIL', 'cfgu-email', u.USER_EMAIL, true);
        html += _field('PHONE', 'cfgu-phone', u.USER_PHONE, false);
        html += _field('USERNAME', 'cfgu-username', u.USER_NAME, true);
        html += '<div class="cfgu-form-group" style="justify-content:flex-end;">';
        html += '<label class="cfgu-form-label">STATUS</label>';
        html += '<select class="cfgu-form-select" id="cfgu-status" onchange="Configuration_UsersJs.onFieldChange()">';
        html += '<option value="1"' + (u.USER_STATUS === 1 ? ' selected' : '') + '>Active</option>';
        html += '<option value="0"' + (u.USER_STATUS === 0 ? ' selected' : '') + '>Inactive</option>';
        html += '</select>';
        html += '</div>';
        html += '<div class="cfgu-form-group">';
        html += '<label class="cfgu-form-label">MFA</label>';
        html += '<div class="cfgu-toggle-row">';
        html += '<label class="cfgu-toggle-wrap">';
        html += '<input class="cfgu-toggle-input" type="checkbox" id="cfgu-mfa"' + (u.USER_MFA === 1 ? ' checked' : '') + ' onchange="Configuration_UsersJs.onFieldChange()" />';
        html += '<span class="cfgu-toggle-track"></span>';
        html += '<span class="cfgu-toggle-thumb"></span>';
        html += '</label>';
        html += '<span class="cfgu-toggle-label">MFA</span>';
        html += '</div>';
        html += '</div>';
        html += '<div class="cfgu-form-group">';
        html += '<label class="cfgu-form-label">SMS</label>';
        html += '<div class="cfgu-toggle-row">';
        html += '<label class="cfgu-toggle-wrap">';
        html += '<input class="cfgu-toggle-input" type="checkbox" id="cfgu-sms"' + (u.USER_SMS === 1 ? ' checked' : '') + ' onchange="Configuration_UsersJs.onFieldChange()" />';
        html += '<span class="cfgu-toggle-track"></span>';
        html += '<span class="cfgu-toggle-thumb"></span>';
        html += '</label>';
        html += '<span class="cfgu-toggle-label">SMS</span>';
        html += '</div>';
        html += '</div>';
        html += '</div></div></div>';

        // ── Roles ─────────────────────────────────────────────────────────
        html += '<div class="cfgu-section">';
        html += '<div class="cfgu-section-header">';
        html += '<span class="cfgu-section-title">Roles</span>';
        html += '<span class="cfgu-section-link" onclick="Configuration_UsersJs.selectAllRoles()">Select All</span>';
        html += '</div>';
        html += '<div class="cfgu-section-body"><div class="cfgu-check-grid" id="cfgu-roles-grid">';
        (_data.Roles || []).forEach(function (r) {
            html += _checkItem('role', r.ROLE_ID, r.ROLE_NAME, userRoles.indexOf(r.ROLE_ID) >= 0);
        });
        html += '</div></div></div>';

        // ── Sites ─────────────────────────────────────────────────────────
        html += '<div class="cfgu-section">';
        html += '<div class="cfgu-section-header">';
        html += '<span class="cfgu-section-title">Sites</span>';
        html += '<span class="cfgu-section-link" onclick="Configuration_UsersJs.selectAllSites()">Select All</span>';
        html += '</div>';
        html += '<div class="cfgu-section-body"><div class="cfgu-check-grid" id="cfgu-sites-grid">';
        (_data.Sites || []).forEach(function (s) {
            html += _checkItem('site', s.SITE_ID, s.SITE_NAME, userSites.indexOf(s.SITE_ID) >= 0);
        });
        html += '</div></div></div>';

        V('cfgu-editor-body').innerHTML = html;

        // Wire change detection
        ['cfgu-first-name', 'cfgu-last-name', 'cfgu-email', 'cfgu-phone', 'cfgu-username'].forEach(function (id) {
            var el = V(id);
            if (el) el.addEventListener('input', function () { _updateAvatar(); _refreshActionBar(); });
        });
    }

    function _field(label, id, value, required) {
        return '<div class="cfgu-form-group">' +
            '<label class="cfgu-form-label">' + label + (required ? ' <span class="cfgu-req">*</span>' : '') + '</label>' +
            '<input class="cfgu-form-input" id="' + id + '" type="text" value="' + esc(value || '') + '" /></div>';
    }

    function _checkItem(type, id, label, checked) {
        return '<div class="cfgu-check-item' + (checked ? ' cfgu-checked' : '') + '" ' +
            'data-type="' + type + '" data-id="' + esc(id) + '" data-label="' + esc(label) + '" ' +
            'onclick="Configuration_UsersJs.toggleCheck(this)">' +
            '<span class="cfgu-check-box"></span><span>' + esc(label) + '</span></div>';
    }

    function _updateAvatar() {
        var fn = V('cfgu-first-name') ? V('cfgu-first-name').value : '';
        var ln = V('cfgu-last-name') ? V('cfgu-last-name').value : '';
        var av = V('cfgu-avatar');
        if (av && !av.querySelector('img')) av.textContent = initials(fn, ln);
    }

    // ── Toggle / Select All ───────────────────────────────────────────────────
    function toggleCheck(el) { el.classList.toggle('cfgu-checked'); _refreshActionBar(); }

    function selectAllRoles() {
        var grid = V('cfgu-roles-grid'); if (!grid) return;
        var items = grid.querySelectorAll('.cfgu-check-item');
        var allChecked = Array.prototype.every.call(items, function (el) { return el.classList.contains('cfgu-checked'); });
        items.forEach(function (el) { allChecked ? el.classList.remove('cfgu-checked') : el.classList.add('cfgu-checked'); });
        _refreshActionBar();
    }

    function selectAllSites() {
        var grid = V('cfgu-sites-grid'); if (!grid) return;
        var items = grid.querySelectorAll('.cfgu-check-item');
        var allChecked = Array.prototype.every.call(items, function (el) { return el.classList.contains('cfgu-checked'); });
        items.forEach(function (el) { allChecked ? el.classList.remove('cfgu-checked') : el.classList.add('cfgu-checked'); });
        _refreshActionBar();
    }

    function onFieldChange() { _refreshActionBar(); }

    // ── State Capture ─────────────────────────────────────────────────────────
    function _captureState() {
        return {
            firstName: V('cfgu-first-name') ? V('cfgu-first-name').value : '',
            lastName: V('cfgu-last-name') ? V('cfgu-last-name').value : '',
            email: V('cfgu-email') ? V('cfgu-email').value : '',
            phone: V('cfgu-phone') ? V('cfgu-phone').value : '',
            username: V('cfgu-username') ? V('cfgu-username').value : '',
            status: V('cfgu-status') ? V('cfgu-status').value : '1',
            mfa: V('cfgu-mfa') ? (V('cfgu-mfa').checked ? 1 : 0) : 0,
            sms: V('cfgu-sms') ? (V('cfgu-sms').checked ? 1 : 0) : 0,
            roles: _getCheckedIds('cfgu-roles-grid'),
            sites: _getCheckedIds('cfgu-sites-grid')
        };
    }

    function _getCheckedIds(gridId) {
        var ids = [];
        var grid = V(gridId);
        if (grid) grid.querySelectorAll('.cfgu-check-item.cfgu-checked').forEach(function (el) { ids.push(el.getAttribute('data-id')); });
        return ids.sort().join(',');
    }

    // ── Change Detection ──────────────────────────────────────────────────────
    function _getChanges() {
        if (!_savedState || !_currentUser) return [];
        var cur = _captureState();
        var changes = [];

        [
            { key: 'firstName', label: 'First Name' },
            { key: 'lastName', label: 'Last Name' },
            { key: 'email', label: 'Email' },
            { key: 'phone', label: 'Phone' },
            { key: 'username', label: 'Username' },
            { key: 'status', label: 'Status', fmt: function (v) { return v === '1' || v === 1 ? 'Active' : 'Inactive'; } },
            { key: 'mfa', label: 'MFA', fmt: function (v) { return v === 1 ? 'On' : 'Off'; } },
            { key: 'sms', label: 'SMS', fmt: function (v) { return v === 1 ? 'On' : 'Off'; } }
        ].forEach(function (f) {
            if (String(_savedState[f.key]) !== String(cur[f.key])) {
                changes.push({
                    section: 'User Info', field: f.label,
                    was: f.fmt ? f.fmt(_savedState[f.key]) : _savedState[f.key],
                    now: f.fmt ? f.fmt(cur[f.key]) : cur[f.key], type: 'changed'
                });
            }
        });

        var savedRoles = _savedState.roles ? _savedState.roles.split(',').filter(Boolean) : [];
        var curRoles = cur.roles ? cur.roles.split(',').filter(Boolean) : [];
        _diffList(savedRoles, curRoles, 'Roles', changes, _data.Roles, 'ROLE_ID', 'ROLE_NAME');

        var savedSites = _savedState.sites ? _savedState.sites.split(',').filter(Boolean) : [];
        var curSites = cur.sites ? cur.sites.split(',').filter(Boolean) : [];
        _diffList(savedSites, curSites, 'Sites', changes, _data.Sites, 'SITE_ID', 'SITE_NAME');

        return changes;
    }

    function _diffList(savedIds, curIds, section, changes, list, idKey, nameKey) {
        curIds.forEach(function (id) {
            if (savedIds.indexOf(id) < 0) {
                var item = (list || []).filter(function (x) { return x[idKey] === id; })[0];
                changes.push({ section: section, field: item ? item[nameKey] : id, was: 'Off', now: 'On', type: 'added' });
            }
        });
        savedIds.forEach(function (id) {
            if (curIds.indexOf(id) < 0) {
                var item = (list || []).filter(function (x) { return x[idKey] === id; })[0];
                changes.push({ section: section, field: item ? item[nameKey] : id, was: 'On', now: 'Off', type: 'removed' });
            }
        });
    }

    // ── Action Bar ────────────────────────────────────────────────────────────
    function _refreshActionBar() {
        if (!_currentUser || !_savedState) { _setActionBar(false, 0); return; }
        var changes = _getChanges();
        _setActionBar(changes.length > 0, changes.length);
    }

    function _setActionBar(hasCh, count) {
        var badge = V('cfgu-changes-badge');
        var discard = V('cfgu-discard-btn');
        var save = V('cfgu-save-btn');
        if (badge) badge.style.display = hasCh ? '' : 'none';
        if (discard) discard.style.display = hasCh ? '' : 'none';
        if (save) save.disabled = !hasCh;
        if (V('cfgu-changes-count')) V('cfgu-changes-count').textContent = count;
    }

    // ── Discard ───────────────────────────────────────────────────────────────
    function discardChanges() {
        if (!_currentUser) return;
        _renderEditor();
        _savedState = _captureState();
        _refreshActionBar();
    }

    // ── Review Dialog ─────────────────────────────────────────────────────────
    function openDiffDialog() {
        var changes = _getChanges();
        if (!changes.length) return;
        var u = _currentUser;

        V('cfgu-dlg-title').textContent = 'Review Changes';
        V('cfgu-dlg-subtitle').textContent = changes.length + ' change' + (changes.length > 1 ? 's' : '') + ' for ' + u.FIRST_NAME + ' ' + u.LAST_NAME;

        var html = '<div style="overflow-y:auto;max-height:380px;">' +
            '<table class="cfgu-diff-table"><thead><tr><th>Section</th><th>Field</th><th>Was</th><th>Now</th></tr></thead><tbody>';
        changes.forEach(function (c) {
            html += '<tr>' +
                '<td style="font-weight:600;color:#191815;">' + esc(c.section) + '</td>' +
                '<td style="color:#56544E;">' + esc(c.field) + '</td>' +
                '<td><span class="cfgu-diff-was">' + esc(c.was) + '</span></td>' +
                '<td><span class="cfgu-diff-now ' + c.type + '">' + esc(c.now) + '</span></td>' +
                '</tr>';
        });
        html += '</tbody></table></div>';

        V('cfgu-dlg-body').innerHTML = html;
        V('cfgu-dlg-foot').innerHTML = '';

        var cancelBtn = document.createElement('button');
        cancelBtn.className = 'cfgu-btn cfgu-btn-cancel';
        cancelBtn.textContent = 'Cancel';
        cancelBtn.onclick = _closeDialog;

        var saveBtn = document.createElement('button');
        saveBtn.className = 'cfgu-btn cfgu-btn-save';
        saveBtn.textContent = 'Confirm & Save';
        saveBtn.onclick = _commitSave;

        V('cfgu-dlg-foot').appendChild(cancelBtn);
        V('cfgu-dlg-foot').appendChild(saveBtn);
        V('cfgu-overlay').classList.add('open');
    }

    function _closeDialog() { V('cfgu-overlay').classList.remove('open'); }

    // ── Commit Save ───────────────────────────────────────────────────────────
    function _commitSave() {
        var cur = _captureState();
        if (!cur.firstName) { _closeDialog(); return; }
        if (!cur.lastName) { _closeDialog(); return; }
        if (!cur.email) { _closeDialog(); return; }
        if (!cur.username) { _closeDialog(); return; }

        _closeDialog();
        $WaitOn();
        var data = [
            { key: 'userid', vlu: _currentUser.USER_ID },
            { key: 'firstname', vlu: cur.firstName },
            { key: 'lastname', vlu: cur.lastName },
            { key: 'email', vlu: cur.email },
            { key: 'phone', vlu: cur.phone },
            { key: 'username', vlu: cur.username },
            { key: 'status', vlu: cur.status },
            { key: 'mfa', vlu: cur.mfa },
            { key: 'sms', vlu: cur.sms },
            { key: 'roles', vlu: cur.roles },
            { key: 'sites', vlu: cur.sites }
        ];
        $ApiRequest('Configuration_Users/SaveUser', JSON.stringify(data));
    }

    function onSaved(user, useRoles, useSites) {
        _currentUser = user;
        _data.UseRoles = useRoles || [];
        _data.UseSites = useSites || [];

        for (var i = 0; i < _data.Users.length; i++) {
            if (_data.Users[i].USER_ID === user.USER_ID) { _data.Users[i] = user; break; }
        }

        // Update sidebar item
        var item = V('cfgu-ui-' + safeId(user.USER_ID));
        if (item) {
            item.querySelector('.cfgu-user-name-sm').textContent = user.FIRST_NAME + ' ' + user.LAST_NAME;
            item.querySelector('.cfgu-user-email-sm').textContent = user.USER_EMAIL;
            var smAv = item.querySelector('.cfgu-user-avatar');
            if (smAv && !smAv.querySelector('img')) smAv.textContent = initials(user.FIRST_NAME, user.LAST_NAME);
        }

        V('cfgu-editor-name').textContent = user.FIRST_NAME + ' ' + user.LAST_NAME;
        V('cfgu-editor-desc').textContent = user.USER_EMAIL;

        _savedState = _captureState();
        _refreshActionBar();
    }

    // ── Add User Dialog ───────────────────────────────────────────────────────
    function openAddUserDialog() {
        ['cfgu-nu-firstname', 'cfgu-nu-lastname', 'cfgu-nu-email', 'cfgu-nu-phone', 'cfgu-nu-username'].forEach(function (id) {
            var el = V(id); if (el) el.value = '';
        });
        var mfa = V('cfgu-nu-mfa'); if (mfa) mfa.checked = false;
        var sms = V('cfgu-nu-sms'); if (sms) sms.checked = false;
        var status = V('cfgu-nu-status'); if (status) status.value = '1';

        var roleSelect = V('cfgu-nu-role');
        if (roleSelect) {
            roleSelect.innerHTML = '<option value="">-- Select Role --</option>';
            (_data.Roles || []).forEach(function (r) {
                var opt = document.createElement('option');
                opt.value = r.ROLE_ID;
                opt.textContent = r.ROLE_NAME;
                roleSelect.appendChild(opt);
            });
        }

        var siteSelect = V('cfgu-nu-site');
        if (siteSelect) {
            siteSelect.innerHTML = '<option value="">-- Select Site --</option>';
            (_data.Sites || []).forEach(function (s) {
                var opt = document.createElement('option');
                opt.value = s.SITE_ID;
                opt.textContent = s.SITE_NAME;
                siteSelect.appendChild(opt);
            });
        }

        V('cfgu-nu-overlay').classList.add('open');
        var fn = V('cfgu-nu-firstname'); if (fn) fn.focus();
    }

    function closeAddUserDialog() { V('cfgu-nu-overlay').classList.remove('open'); }

    function submitAddUser() {
        var firstName = V('cfgu-nu-firstname') ? V('cfgu-nu-firstname').value.trim() : '';
        var lastName = V('cfgu-nu-lastname') ? V('cfgu-nu-lastname').value.trim() : '';
        var email = V('cfgu-nu-email') ? V('cfgu-nu-email').value.trim() : '';
        var phone = V('cfgu-nu-phone') ? V('cfgu-nu-phone').value.trim() : '';
        var username = V('cfgu-nu-username') ? V('cfgu-nu-username').value.trim() : '';
        var mfa = V('cfgu-nu-mfa') ? (V('cfgu-nu-mfa').checked ? 1 : 0) : 0;
        var sms = V('cfgu-nu-sms') ? (V('cfgu-nu-sms').checked ? 1 : 0) : 0;
        var roleId = V('cfgu-nu-role') ? V('cfgu-nu-role').value : '';
        var status = V('cfgu-nu-status') ? V('cfgu-nu-status').value : '1';
        var siteId = V('cfgu-nu-site') ? V('cfgu-nu-site').value : '';

        if (!firstName || !lastName || !email || !username) return;

        closeAddUserDialog();
        $WaitOn();
        var data = [
            { key: 'firstname', vlu: firstName },
            { key: 'lastname', vlu: lastName },
            { key: 'email', vlu: email },
            { key: 'phone', vlu: phone },
            { key: 'username', vlu: username },
            { key: 'mfa', vlu: mfa },
            { key: 'sms', vlu: sms },
            { key: 'roleid', vlu: roleId },
            { key: 'siteid', vlu: siteId },
            { key: 'status', vlu: status }
        ];
        $ApiRequest('Configuration_Users/AddUser', JSON.stringify(data));
    }

    function onUserAdded(user, useRoles, useSites) {
        if (!user) return;
        _data.Users.unshift(user);
        _data.UseRoles = (_data.UseRoles || []).concat(useRoles || []);
        _data.UseSites = (_data.UseSites || []).concat(useSites || []);
        _renderUserList();
        selectUser(user.USER_ID);
    }

    // ── Photo Upload ──────────────────────────────────────────────────────────
    function triggerPhotoUpload(userId) {
        var input = V('cfgu-photo-input');
        if (input) { input.setAttribute('data-userid', userId); input.click(); }
    }

    function _onPhotoSelected(input) {
        if (!input.files || !input.files[0]) return;
        var file = input.files[0];
        var userId = input.getAttribute('data-userid');
        if (!userId) return;

        if (file.size > 5 * 1024 * 1024) { input.value = ''; return; }

        var reader = new FileReader();
        reader.onload = function (e) {
            var base64 = e.target.result.split(',')[1];
            $WaitOn();
            var data = [
                { key: 'userid', vlu: userId },
                { key: 'base64', vlu: base64 },
                { key: 'filetype', vlu: file.type }
            ];
            $ApiRequest('Configuration_Users/UploadPhoto', JSON.stringify(data));
        };
        reader.readAsDataURL(file);
        input.value = '';
    }

    function onPhotoUploaded(userId, photoLink) {
        var ts = '?t=' + Date.now();
        var av = V('cfgu-avatar');
        if (av) av.innerHTML = '<img src="' + photoLink + ts + '" style="width:52px;height:52px;border-radius:999px;object-fit:cover;" />';
        var item = V('cfgu-ui-' + safeId(userId));
        if (item) {
            var smAv = item.querySelector('.cfgu-user-avatar');
            if (smAv) smAv.innerHTML = '<img src="' + photoLink + ts + '" style="width:28px;height:28px;border-radius:999px;object-fit:cover;" />';
        }
        if (_currentUser && _currentUser.USER_ID === userId) _currentUser.PHOTO_LINK = photoLink;
    }

    // ── Overlays ──────────────────────────────────────────────────────────────
    // ── Delete / Reset btn rendering ─────────────────────────────
    function _renderActionBtns(show) {
        var delWrap = V('cfgu-del-btn-wrap');
        var resetWrap = V('cfgu-reset-btn-wrap');
        if (delWrap) {
            delWrap.innerHTML = (_canDelete && show)
                ? '<button class="cfgu-btn cfgu-btn-delete" onclick="Configuration_UsersJs.openDeleteDialog()">Delete User</button>'
                : '';
        }
        if (resetWrap) {
            resetWrap.innerHTML = (_canResetPwd && show)
                ? '<button class="cfgu-btn cfgu-btn-ghost" onclick="Configuration_UsersJs.openResetDialog()">Reset Password</button>'
                : '';
        }
    }

    // ── Delete User ───────────────────────────────────────────────
    function openDeleteDialog() {
        if (!_currentUser) return;
        var sub = V('cfgu-del-subtitle');
        if (sub) sub.textContent = _currentUser.FIRST_NAME + ' ' + _currentUser.LAST_NAME + ' (' + _currentUser.USER_NAME + ')';
        V('cfgu-del-overlay').classList.add('open');
    }

    function closeDeleteDialog() { V('cfgu-del-overlay').classList.remove('open'); }

    function confirmDeleteUser() {
        if (!_currentUser) return;
        closeDeleteDialog();
        $WaitOn();
        $ApiRequest('Configuration_Users/DeleteUser', JSON.stringify([{ key: 'userid', vlu: _currentUser.USER_ID }]));
    }

    function onUserDeleted(userId) {
        // Remove from data store
        if (_data.Users) _data.Users = _data.Users.filter(function (u) { return u.USER_ID !== userId; });
        if (_data.UseRoles) _data.UseRoles = _data.UseRoles.filter(function (r) { return r.USER_ID !== userId; });
        if (_data.UseSites) _data.UseSites = _data.UseSites.filter(function (s) { return s.USER_ID !== userId; });
        _currentUser = null;
        _savedState = null;
        // Refresh sidebar list
        _renderUserList();
        // Reset right panel
        _renderEmptyEditor();
        _renderActionBtns(false);
        _setActionBar(false, 0);
    }

    // ── Reset Password ────────────────────────────────────────────
    function openResetDialog() {
        if (!_currentUser) return;
        var sub = V('cfgu-reset-subtitle');
        if (sub) sub.textContent = _currentUser.FIRST_NAME + ' ' + _currentUser.LAST_NAME + ' — ' + _currentUser.USER_EMAIL;
        V('cfgu-reset-overlay').classList.add('open');
    }

    function closeResetDialog() { V('cfgu-reset-overlay').classList.remove('open'); }

    function confirmResetPassword() {
        if (!_currentUser) return;
        closeResetDialog();
        $WaitOn();
        $ApiRequest('Configuration_Users/ResetUserPassword', JSON.stringify([{ key: 'userid', vlu: _currentUser.USER_ID }]));
    }

    function onPasswordReset(type, detail) {
        var icon = V('cfgu-pwdreset-icon');
        var title = V('cfgu-pwdreset-title');
        var subtitle = V('cfgu-pwdreset-subtitle');
        var msg = V('cfgu-pwdreset-msg');
        if (type === 'success') {
            if (icon) icon.textContent = '✅';
            if (title) title.textContent = 'Email Sent';
            if (subtitle) subtitle.textContent = 'Password reset successful';
            if (msg) msg.textContent = 'Password reset email has been sent to ' + detail + '. The user will be prompted to set a new password on next login.';
        } else {
            if (icon) icon.textContent = '⚠️';
            if (icon) icon.style.background = '#FEF3C7';
            if (title) title.textContent = 'Warning';
            if (subtitle) subtitle.textContent = 'Reset flag was set but email failed';
            if (msg) msg.textContent = 'The password reset flag was set successfully, but the email could not be sent. Reason: ' + detail;
        }
        V('cfgu-pwdreset-overlay').classList.add('open');
    }

    function closePwdResetDialog() {
        V('cfgu-pwdreset-overlay').classList.remove('open');
    }

    function initOverlays() {
        var overlay = V('cfgu-overlay');
        if (overlay) overlay.addEventListener('click', function (e) { if (e.target === overlay) _closeDialog(); });

        var nuOverlay = V('cfgu-nu-overlay');
        var delOverlay = V('cfgu-del-overlay');
        var rstOverlay = V('cfgu-reset-overlay');
        if (delOverlay) delOverlay.addEventListener('click', function (e) { if (e.target === delOverlay) closeDeleteDialog(); });
        if (rstOverlay) rstOverlay.addEventListener('click', function (e) { if (e.target === rstOverlay) closeResetDialog(); });
        var pwdOverlay = V('cfgu-pwdreset-overlay');
        if (pwdOverlay) pwdOverlay.addEventListener('click', function (e) { if (e.target === pwdOverlay) closePwdResetDialog(); });
        if (nuOverlay) nuOverlay.addEventListener('click', function (e) { if (e.target === nuOverlay) closeAddUserDialog(); });

        var photoInput = V('cfgu-photo-input');
        if (photoInput) photoInput.addEventListener('change', function () { _onPhotoSelected(this); });

        var wrap = document.querySelector('.cfgu-wrap');
        if (wrap) wrap.style.visibility = 'visible';

        // Init from hidden div
        var initEl = V('cfgu-initdata');
        if (initEl) {
            try {
                var d = JSON.parse(atob(initEl.textContent.trim()));
                init(d.roles, d.sites, d.perms);
                _doSearch('');   // list all users by default on page open
            } catch (e) { }
        }
    }

    return {
        searchUsers: function (q) { searchUsers(q); },
        openDeleteDialog: function () { openDeleteDialog(); },
        closeDeleteDialog: function () { closeDeleteDialog(); },
        confirmDeleteUser: function () { confirmDeleteUser(); },
        onUserDeleted: function (id) { onUserDeleted(id); },
        openResetDialog: function () { openResetDialog(); },
        closeResetDialog: function () { closeResetDialog(); },
        confirmResetPassword: function () { confirmResetPassword(); },
        closePwdResetDialog: function () { closePwdResetDialog(); },
        onPasswordReset: function (t, d) { onPasswordReset(t, d); },
        selectUser: function (id) { selectUser(id); },
        toggleCheck: function (el) { toggleCheck(el); },
        selectAllRoles: function () { selectAllRoles(); },
        selectAllSites: function () { selectAllSites(); },
        onFieldChange: function () { onFieldChange(); },
        discardChanges: function () { discardChanges(); },
        openDiffDialog: function () { openDiffDialog(); },
        openAddUserDialog: function () { openAddUserDialog(); },
        closeAddUserDialog: function () { closeAddUserDialog(); },
        submitAddUser: function () { submitAddUser(); },
        triggerPhotoUpload: function (id) { triggerPhotoUpload(id); },
        onSearchResult: function (u, r, s) { onSearchResult(u, r, s); },
        onGetUser: function (u, r, s) { _renderActionBtns(true); onGetUser(u, r, s); },
        onSaved: function (u, r, s) { _renderActionBtns(true); onSaved(u, r, s); },
        onUserAdded: function (u, r, s) { onUserAdded(u, r, s); },
        onPhotoUploaded: function (id, l) { onPhotoUploaded(id, l); },
        initOverlays: function () { initOverlays(); }
    };

})();

Configuration_UsersJs.initOverlays();

