// ═══════════════════════════════════════════════════════════
//  LIC TA/DA — Global JS
//  ltDoFetch signature: (url, gridId, year, faculty, college, callback?)
// ═══════════════════════════════════════════════════════════

var ltVerified = {};
var ltCurBtn = null;

// ── FILTER TOGGLE ────────────────────────────────────────────
function ltToggleFilter(filterId, btnId) {
    var panel = document.getElementById(filterId);
    var btn = btnId ? document.getElementById(btnId) : null;
    if (!panel) return;
    var open = panel.style.display === 'block' || panel.style.display === 'grid';
    panel.style.display = open ? 'none' : 'block';
    if (btn) {
        btn.textContent = open ? '▼ Show Filters' : '▲ Hide Filters';
    }
}

// ── FETCH GRID ───────────────────────────────────────────────
// Called by all pages as: ltDoFetch(url, gridId, year, faculty, college, callback?)
function ltDoFetch(url, gridId, year, faculty, college, callback) {
    var target = document.getElementById(gridId);
    if (!target) return;

    target.innerHTML =
        '<div style="padding:48px;text-align:center;font-family:\'JetBrains Mono\',monospace;font-size:12px;color:#8fa3be">' +
        '<div style="font-size:28px;margin-bottom:8px">⏳</div>Loading records...</div>';

    var body = 'year=' + encodeURIComponent(year || '') +
        '&faculty=' + encodeURIComponent(faculty || '') +
        '&college=' + encodeURIComponent(college || '');

    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: body
    })
        .then(function (r) { return r.text(); })
        .then(function (html) {
            target.innerHTML = html;
            // count rows for badge callback
            if (typeof callback === 'function') {
                var tmp = document.createElement('div');
                tmp.innerHTML = html;
                var rows = tmp.querySelectorAll('tbody tr');
                callback(rows.length > 0 ? rows.length : 0);
            }
        })
        .catch(function () {
            target.innerHTML =
                '<div style="padding:48px;text-align:center;color:#e03055;font-family:\'JetBrains Mono\',monospace">' +
                '⚠️ Failed to load records. Check your connection.</div>';
        });
}

// ── MODAL OPEN ───────────────────────────────────────────────
function ltOpen(phone, name, mode, btnId) {
    var overlay = document.getElementById('ltOverlay');
    var content = document.getElementById('ltModalContent');
    if (!overlay || !content) return;

    ltCurBtn = btnId || null;

    content.innerHTML =
        '<div style="padding:60px;text-align:center;font-family:\'JetBrains Mono\',monospace;' +
        'font-size:13px;color:#8fa3be"><div style="font-size:32px;margin-bottom:10px">⏳</div>Loading...</div>';

    overlay.style.display = 'flex';
    document.body.style.overflow = 'hidden';

    fetch('/LicTada/Member?phone=' + encodeURIComponent(phone || '')
        + '&name=' + encodeURIComponent(name || '')
        + '&mode=' + encodeURIComponent(mode || 'view'))
        .then(function (r) {
            if (!r.ok) throw new Error('HTTP ' + r.status);
            return r.text();
        })
        .then(function (html) {
            content.innerHTML = html;

            /* ── FIX: re-execute any <script> tags injected via innerHTML ── */
            var scripts = content.querySelectorAll('script');
            scripts.forEach(function (oldScript) {
                var newScript = document.createElement('script');
                /* Copy all attributes */
                Array.from(oldScript.attributes).forEach(function (attr) {
                    newScript.setAttribute(attr.name, attr.value);
                });
                /* Copy inline script content */
                newScript.textContent = oldScript.textContent;
                /* Replace old (dead) script with new (live) one */
                oldScript.parentNode.replaceChild(newScript, oldScript);
            });
        })
        .catch(function (e) {
            content.innerHTML =
                '<div style="padding:40px;text-align:center;color:#e03055;font-family:\'JetBrains Mono\',monospace">' +
                '⚠️ Failed to load member details. ' + (e.message || '') + '</div>';
        });
}

// ── MODAL CLOSE ──────────────────────────────────────────────
function ltClose() {
    var overlay = document.getElementById('ltOverlay');
    if (overlay) overlay.style.display = 'none';
    document.body.style.overflow = '';
    ltCurBtn = null;
}

function ltCloseOut(event) {
    if (event.target === document.getElementById('ltOverlay')) {
        ltClose();
    }
}

// ── CW BUTTON STATE ──────────────────────────────────────────
function ltMarkBtn(btn) {
    if (!btn) return;
    btn.style.background = '#e5f7f2';
    btn.style.borderColor = '#86e0c8';
    btn.style.color = '#0f9b6e';
    btn.textContent = '✅ Verified';
    btn.classList.add('done');
    btn.onclick = null;
}

// Check all 3 members verified → show badge
// btnId format: cwSm_0 / cwAc_0 / cwSe_0
function ltCheck3(btnId) {
    if (!btnId) return;
    var parts = btnId.split('_');
    if (parts.length < 2) return;
    var idx = parts[1];
    var smDone = !!ltVerified['cwSm_' + idx];
    var acDone = !!ltVerified['cwAc_' + idx];
    var seDone = !!ltVerified['cwSe_' + idx];
    if (smDone && acDone && seDone) {
        var badge = document.getElementById('cwAV_' + idx);
        if (badge) badge.style.display = 'inline-block';
    }
}

// ── TOAST ────────────────────────────────────────────────────
function ltToast(msg, type) {
    var t = document.getElementById('ltToast');
    if (!t) return;
    t.textContent = msg;
    t.className = '';
    t.style.display = 'block';
    if (type === 'ok') t.className = 'toast-ok';
    else if (type === 'err') t.className = 'toast-err';
    else t.className = 'toast-inf';
    clearTimeout(window._ltToastTimer);
    window._ltToastTimer = setTimeout(function () {
        t.style.display = 'none';
        t.className = '';
    }, 3200);
}