/* =====================================================================
   course-schedule.js
   Reads /CourseLevelSchedule/GetSchedule (dbo.Mst_CourseLevelSchedule)
   and drives: dashboard countdown card, affiliation notification modal,
   and the sidebar UG / PG / SS availability.

   Set before loading:  window.CourseScheduleUrl = '<url of GetSchedule>';
   ===================================================================== */
(function (w, d) {
    'use strict';
    if (w.CourseSchedule) return;   // loaded twice (layout + view) -> keep the first

    var MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    var DAY = 86400000;
    var TTL = 30000;                 // re-use a response for 30 s
    var cache = null, cacheAt = 0, inflight = null, clockOffset = 0;

    // ── helpers ──────────────────────────────────────────────────────────
    function esc(s) {
        return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }
    function parts(ymd) { var p = String(ymd || '').split('-'); return p.length === 3 ? { y: +p[0], m: +p[1], d: +p[2] } : null; }
    function fmtLong(ymd) { var p = parts(ymd); return p ? p.d + ' ' + MONTHS[p.m - 1] + ' ' + p.y : ''; }
    function fmtSlash(ymd) { var p = parts(ymd); return p ? ('0' + p.d).slice(-2) + '/' + ('0' + p.m).slice(-2) + '/' + p.y : ''; }
    function money(n) { return n == null ? '' : 'Rs. ' + Number(n).toLocaleString('en-IN') + '/-'; }
    function now() { return Date.now() + clockOffset; }
    function pct(from, to) { return Math.max(0, Math.min(100, ((now() - from) / (to - from)) * 100)); }

    // ── data ─────────────────────────────────────────────────────────────
    function load(force) {
        if (!force && cache && (Date.now() - cacheAt) < TTL) return Promise.resolve(cache);
        if (inflight) return inflight;

        inflight = fetch(w.CourseScheduleUrl, {
            credentials: 'same-origin',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (res) {
            if (res.redirected || res.status === 401) { w.location.reload(); throw new Error('Session expired'); }
            if (!res.ok) throw new Error('Schedule request failed (' + res.status + ')');
            return res.json();
        }).then(function (data) {
            if (!data || !data.success) throw new Error((data && data.message) || 'Schedule unavailable');
            var serverMs = Date.parse(data.serverNow);
            if (!isNaN(serverMs)) clockOffset = serverMs - Date.now();
            cache = data; cacheAt = Date.now();
            return data;
        }).then(function (data) { inflight = null; return data; },
                function (err) { inflight = null; throw err; });

        return inflight;
    }

    /** { ug: {...level}, pg: {...}, ss: {...} } */
    function map(data) {
        var m = {};
        ((data && data.levels) || []).forEach(function (l) { m[String(l.courseLevel).toLowerCase()] = l; });
        return m;
    }

    // ── dashboard: countdown card ────────────────────────────────────────
    function pickFocus(levels) {
        var live = levels.filter(function (l) { return l.isSelectable && l.phaseEndsAt; });
        live.sort(function (a, b) { return Date.parse(a.phaseEndsAt) - Date.parse(b.phaseEndsAt); });
        return live[0] || null;
    }

    function pillHtml(l) {
        var cls = { Open: 'ok', LateFine: 'warn', Closed: 'off', Disabled: 'off' }[l.status] || 'off';
        var txt = { Open: 'Open', LateFine: 'Late fine', Closed: 'Closed', Disabled: 'Disabled' }[l.status] || l.status;
        if (l.status === 'Open' && !l.hasDeadline) txt = 'Open';
        return '<span class="cs-pill cs-' + cls + '"><b>' + esc(l.courseLevel) + '</b> ' + txt + '</span>';
    }

    function renderDashboard(data) {
        var card = d.getElementById('deadlineCard');
        if (!card) return;

        var levels = (data.levels || []).filter(function (l) { return l.hasDeadline || !l.isEnabled; });
        if (!levels.length) { card.hidden = true; return; }
        card.hidden = false;

        var $ = function (id) { return d.getElementById(id); };
        var label = $('dlLabel'), days = $('dlDays'), unit = $('dlUnit'), msg = $('dlMsg'), bar = $('dlBar');
        var startLbl = $('dlStartLabel'), endLbl = $('dlEndLabel');

        // per-level status pills
        var pills = card.querySelector('.dl-levels');
        if (!pills) {
            pills = d.createElement('div');
            pills.className = 'dl-levels';
            card.querySelector('.dl-main').appendChild(pills);
        }
        pills.innerHTML = (data.levels || []).map(pillHtml).join('');

        card.classList.remove('danger', 'safe');
        var f = pickFocus(data.levels || []);

        if (!f) {
            card.classList.add('danger');
            label.textContent = 'Submission window';
            days.textContent = '0';
            unit.textContent = 'closed';
            var anyDisabled = levels.some(function (l) { return l.status === 'Disabled'; });
            msg.textContent = anyDisabled && levels.every(function (l) { return l.status === 'Disabled'; })
                ? 'Submissions are currently switched off by the university.'
                : 'The submission period has ended.';
            bar.style.width = '100%';
            var last = levels.map(function (l) { return l.fineLastDate || l.lastDate; }).filter(Boolean).sort().pop();
            startLbl.textContent = '';
            endLbl.textContent = last ? fmtLong(last) : '';
            return;
        }

        var endMs = Date.parse(f.phaseEndsAt);
        var left = Math.max(0, Math.ceil((endMs - now()) / DAY));
        days.textContent = left;
        unit.textContent = left === 1 ? 'day left' : 'days left';

        if (f.status === 'LateFine') {
            card.classList.add('danger');
            label.textContent = f.courseLevel + ' · Final date (with fine)';
            msg.textContent = 'A fine of ' + money(f.fineAmount) + ' applies. Payment will not work after ' + fmtLong(f.fineLastDate) + '.';
            bar.style.width = pct(Date.parse(f.fineStartDate + 'T00:00:00+05:30'), endMs) + '%';
            startLbl.textContent = fmtLong(f.fineStartDate);
            endLbl.textContent = fmtLong(f.fineLastDate);
        } else {
            if (left <= 3) card.classList.add('danger');
            var hasFine = !!(f.fineLastDate && f.lastDate);
            label.textContent = f.courseLevel + (hasFine ? ' · Fee-free deadline' : ' · Last date');
            msg.textContent = hasFine
                ? 'Submit by ' + fmtLong(f.lastDate) + ' to avoid the ' + money(f.fineAmount) + ' fine.'
                : 'Submit by ' + fmtLong(f.lastDate || f.fineLastDate) + '. The level closes after this date.';
            var startMs = f.startsAt ? Date.parse(f.startsAt) : endMs - 30 * DAY;
            bar.style.width = pct(startMs, endMs) + '%';
            startLbl.textContent = f.startDate ? fmtLong(f.startDate) : '';
            endLbl.textContent = fmtLong(f.lastDate || f.fineLastDate);
        }
    }

    // ── dashboard: affiliation notification (modal body) ─────────────────
    function renderNotice(data) {
        var host = d.getElementById('affNoticeBody');
        if (!host) return;

        var levels = (data.levels || []).filter(function (l) { return l.hasDeadline || !l.isEnabled; });
        if (!levels.length) {
            host.innerHTML = '<p class="mb-0">No submission deadline has been set. Applications are currently open.</p>';
            return;
        }

        var rows = '';
        var cautions = [];
        levels.forEach(function (l) {
            var tag = l.status === 'Disabled' ? ' <span class="badge bg-secondary">Disabled</span>'
                    : l.status === 'Closed' ? ' <span class="badge bg-dark">Closed</span>' : '';
            var span = (l.hasDeadline && l.isEnabled) ? (l.fineLastDate && l.lastDate ? 2 : 1) : 1;
            var first = '<td rowspan="' + span + '"><strong>' + esc(l.courseLevel) + '</strong>' + tag + '</td>';

            if (!l.isEnabled || !l.hasDeadline) {
                rows += '<tr>' + first + '<td colspan="2">' + (l.isEnabled ? 'No deadline (always open)' : 'Currently disabled') + '</td></tr>';
                return;
            }
            var freeFrom = l.startDate ? fmtSlash(l.startDate) + ' to ' : 'Up to ';
            rows += '<tr>' + first + '<td><strong>' + freeFrom + fmtSlash(l.lastDate || l.fineLastDate) +
                '</strong></td><td><span class="badge bg-success">' + (l.fineLastDate && l.lastDate ? 'Without fine' : 'Last date') + '</span></td></tr>';
            if (l.fineLastDate && l.lastDate) {
                rows += '<tr><td><strong>' + fmtSlash(l.fineStartDate) + ' to ' + fmtSlash(l.fineLastDate) +
                    '</strong></td><td><span class="badge bg-danger">With a fine of ' + esc(money(l.fineAmount) || 'the prescribed amount') + '</span></td></tr>';
                cautions.push('<strong>' + esc(l.courseLevel) + ':</strong> applications from <strong>' + fmtSlash(l.fineStartDate) +
                    ' to ' + fmtSlash(l.fineLastDate) + '</strong> attract a fine of <strong>' + esc(money(l.fineAmount)) +
                    '</strong>. <strong>Payment will not work after ' + fmtSlash(l.fineLastDate) + '.</strong>');
            }
        });

        host.innerHTML =
            '<p class="mb-3">Colleges are requested to submit their affiliation applications within the following dates:</p>' +
            '<div class="table-responsive"><table class="table table-bordered align-middle mb-3">' +
            '<thead class="table-light"><tr><th>Course level</th><th>Submission period</th><th>Applicable fee</th></tr></thead>' +
            '<tbody>' + rows + '</tbody></table></div>' +
            (cautions.length
                ? '<div class="alert alert-warning mb-0"><i class="bi bi-exclamation-triangle-fill me-2"></i><strong>Caution:</strong><ul class="mb-0 mt-1 ps-3">' +
                  cautions.map(function (c) { return '<li>' + c + '</li>'; }).join('') + '</ul></div>'
                : '');
    }

    function renderAll(data) { renderDashboard(data); renderNotice(data); }

    // ── styles for the pills (injected once) ─────────────────────────────
    (function () {
        var s = d.createElement('style');
        s.textContent =
            '.dl-levels{display:flex;gap:8px;flex-wrap:wrap;margin-top:10px}' +
            '.cs-pill{display:inline-flex;gap:6px;align-items:center;font-size:11px;font-weight:600;padding:4px 11px;border-radius:999px;border:1px solid transparent}' +
            '.cs-pill b{font-weight:800}' +
            '.cs-ok{background:#ecfdf5;color:#059669;border-color:#a7f3d0}' +
            '.cs-warn{background:#fffbeb;color:#b45309;border-color:#fde68a}' +
            '.cs-off{background:#f1f5f9;color:#64748b;border-color:#e2e8f0}' +
            '.cs-badge{margin-left:auto;font-size:9.5px;font-weight:700;text-transform:uppercase;letter-spacing:.04em;padding:2px 8px;border-radius:20px}' +
            '.cs-badge.warn{background:rgba(245,179,1,.2);color:#fcd34d}' +
            '.cs-badge.off{background:rgba(255,255,255,.12);color:rgba(255,255,255,.6)}' +
            '.affiliation-subitem{display:flex;align-items:center}';
        d.head.appendChild(s);
    })();

    // ── public API ───────────────────────────────────────────────────────
    var ready = Promise.resolve();
    w.CourseSchedule = {
        load: load,
        map: map,
        fmtLong: fmtLong,
        money: money,
        get ready() { return ready; }
    };

    // ── auto init on pages that have the dashboard hooks ─────────────────
    function init() {
        if (!d.getElementById('deadlineCard') && !d.getElementById('affNoticeBody')) return;
        ready = load().then(function (data) { renderAll(data); return data; }).catch(function (e) { console.error(e); });
        // keep the countdown and status correct while the page stays open (e.g. midnight rollover)
        setInterval(function () { load(true).then(renderAll).catch(function () { }); }, 60000);
    }
    if (d.readyState === 'loading') d.addEventListener('DOMContentLoaded', init); else init();

})(window, document);/* =====================================================================
   course-schedule.js
   Reads /CourseLevelSchedule/GetSchedule (dbo.Mst_CourseLevelSchedule)
   and drives: dashboard countdown card, affiliation notification modal,
   and the sidebar UG / PG / SS availability.

   Set before loading:  window.CourseScheduleUrl = '<url of GetSchedule>';
   ===================================================================== */
(function (w, d) {
    'use strict';
    if (w.CourseSchedule) return;   // loaded twice (layout + view) -> keep the first

    var MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    var DAY = 86400000;
    var TTL = 30000;                 // re-use a response for 30 s
    var cache = null, cacheAt = 0, inflight = null, clockOffset = 0;

    // ── helpers ──────────────────────────────────────────────────────────
    function esc(s) {
        return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }
    function parts(ymd) { var p = String(ymd || '').split('-'); return p.length === 3 ? { y: +p[0], m: +p[1], d: +p[2] } : null; }
    function fmtLong(ymd) { var p = parts(ymd); return p ? p.d + ' ' + MONTHS[p.m - 1] + ' ' + p.y : ''; }
    function fmtSlash(ymd) { var p = parts(ymd); return p ? ('0' + p.d).slice(-2) + '/' + ('0' + p.m).slice(-2) + '/' + p.y : ''; }
    function money(n) { return n == null ? '' : 'Rs. ' + Number(n).toLocaleString('en-IN') + '/-'; }
    function now() { return Date.now() + clockOffset; }
    function pct(from, to) { return Math.max(0, Math.min(100, ((now() - from) / (to - from)) * 100)); }

    // ── data ─────────────────────────────────────────────────────────────
    function load(force) {
        if (!force && cache && (Date.now() - cacheAt) < TTL) return Promise.resolve(cache);
        if (inflight) return inflight;

        inflight = fetch(w.CourseScheduleUrl, {
            credentials: 'same-origin',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (res) {
            if (res.redirected || res.status === 401) { w.location.reload(); throw new Error('Session expired'); }
            if (!res.ok) throw new Error('Schedule request failed (' + res.status + ')');
            return res.json();
        }).then(function (data) {
            if (!data || !data.success) throw new Error((data && data.message) || 'Schedule unavailable');
            var serverMs = Date.parse(data.serverNow);
            if (!isNaN(serverMs)) clockOffset = serverMs - Date.now();
            cache = data; cacheAt = Date.now();
            return data;
        }).then(function (data) { inflight = null; return data; },
                function (err) { inflight = null; throw err; });

        return inflight;
    }

    /** { ug: {...level}, pg: {...}, ss: {...} } */
    function map(data) {
        var m = {};
        ((data && data.levels) || []).forEach(function (l) { m[String(l.courseLevel).toLowerCase()] = l; });
        return m;
    }

    // ── dashboard: countdown card ────────────────────────────────────────
    function pickFocus(levels) {
        var live = levels.filter(function (l) { return l.isSelectable && l.phaseEndsAt; });
        live.sort(function (a, b) { return Date.parse(a.phaseEndsAt) - Date.parse(b.phaseEndsAt); });
        return live[0] || null;
    }

    function pillHtml(l) {
        var cls = { Open: 'ok', LateFine: 'warn', Closed: 'off', Disabled: 'off' }[l.status] || 'off';
        var txt = { Open: 'Open', LateFine: 'Late fine', Closed: 'Closed', Disabled: 'Disabled' }[l.status] || l.status;
        if (l.status === 'Open' && !l.hasDeadline) txt = 'Open';
        return '<span class="cs-pill cs-' + cls + '"><b>' + esc(l.courseLevel) + '</b> ' + txt + '</span>';
    }

    function renderDashboard(data) {
        var card = d.getElementById('deadlineCard');
        if (!card) return;

        var levels = (data.levels || []).filter(function (l) { return l.hasDeadline || !l.isEnabled; });
        if (!levels.length) { card.hidden = true; return; }
        card.hidden = false;

        var $ = function (id) { return d.getElementById(id); };
        var label = $('dlLabel'), days = $('dlDays'), unit = $('dlUnit'), msg = $('dlMsg'), bar = $('dlBar');
        var startLbl = $('dlStartLabel'), endLbl = $('dlEndLabel');

        // per-level status pills
        var pills = card.querySelector('.dl-levels');
        if (!pills) {
            pills = d.createElement('div');
            pills.className = 'dl-levels';
            card.querySelector('.dl-main').appendChild(pills);
        }
        pills.innerHTML = (data.levels || []).map(pillHtml).join('');

        card.classList.remove('danger', 'safe');
        var f = pickFocus(data.levels || []);

        if (!f) {
            card.classList.add('danger');
            label.textContent = 'Submission window';
            days.textContent = '0';
            unit.textContent = 'closed';
            var anyDisabled = levels.some(function (l) { return l.status === 'Disabled'; });
            msg.textContent = anyDisabled && levels.every(function (l) { return l.status === 'Disabled'; })
                ? 'Submissions are currently switched off by the university.'
                : 'The submission period has ended.';
            bar.style.width = '100%';
            var last = levels.map(function (l) { return l.fineLastDate || l.lastDate; }).filter(Boolean).sort().pop();
            startLbl.textContent = '';
            endLbl.textContent = last ? fmtLong(last) : '';
            return;
        }

        var endMs = Date.parse(f.phaseEndsAt);
        var left = Math.max(0, Math.ceil((endMs - now()) / DAY));
        days.textContent = left;
        unit.textContent = left === 1 ? 'day left' : 'days left';

        if (f.status === 'LateFine') {
            card.classList.add('danger');
            label.textContent = f.courseLevel + ' · Final date (with fine)';
            msg.textContent = 'A fine of ' + money(f.fineAmount) + ' applies. Payment will not work after ' + fmtLong(f.fineLastDate) + '.';
            bar.style.width = pct(Date.parse(f.fineStartDate + 'T00:00:00+05:30'), endMs) + '%';
            startLbl.textContent = fmtLong(f.fineStartDate);
            endLbl.textContent = fmtLong(f.fineLastDate);
        } else {
            if (left <= 3) card.classList.add('danger');
            var hasFine = !!(f.fineLastDate && f.lastDate);
            label.textContent = f.courseLevel + (hasFine ? ' · Fee-free deadline' : ' · Last date');
            msg.textContent = hasFine
                ? 'Submit by ' + fmtLong(f.lastDate) + ' to avoid the ' + money(f.fineAmount) + ' fine.'
                : 'Submit by ' + fmtLong(f.lastDate || f.fineLastDate) + '. The level closes after this date.';
            var startMs = f.startsAt ? Date.parse(f.startsAt) : endMs - 30 * DAY;
            bar.style.width = pct(startMs, endMs) + '%';
            startLbl.textContent = f.startDate ? fmtLong(f.startDate) : '';
            endLbl.textContent = fmtLong(f.lastDate || f.fineLastDate);
        }
    }

    // ── dashboard: affiliation notification (modal body) ─────────────────
    function renderNotice(data) {
        var host = d.getElementById('affNoticeBody');
        if (!host) return;

        var levels = (data.levels || []).filter(function (l) { return l.hasDeadline || !l.isEnabled; });
        if (!levels.length) {
            host.innerHTML = '<p class="mb-0">No submission deadline has been set. Applications are currently open.</p>';
            return;
        }

        var rows = '';
        var cautions = [];
        levels.forEach(function (l) {
            var tag = l.status === 'Disabled' ? ' <span class="badge bg-secondary">Disabled</span>'
                    : l.status === 'Closed' ? ' <span class="badge bg-dark">Closed</span>' : '';
            var span = (l.hasDeadline && l.isEnabled) ? (l.fineLastDate && l.lastDate ? 2 : 1) : 1;
            var first = '<td rowspan="' + span + '"><strong>' + esc(l.courseLevel) + '</strong>' + tag + '</td>';

            if (!l.isEnabled || !l.hasDeadline) {
                rows += '<tr>' + first + '<td colspan="2">' + (l.isEnabled ? 'No deadline (always open)' : 'Currently disabled') + '</td></tr>';
                return;
            }
            var freeFrom = l.startDate ? fmtSlash(l.startDate) + ' to ' : 'Up to ';
            rows += '<tr>' + first + '<td><strong>' + freeFrom + fmtSlash(l.lastDate || l.fineLastDate) +
                '</strong></td><td><span class="badge bg-success">' + (l.fineLastDate && l.lastDate ? 'Without fine' : 'Last date') + '</span></td></tr>';
            if (l.fineLastDate && l.lastDate) {
                rows += '<tr><td><strong>' + fmtSlash(l.fineStartDate) + ' to ' + fmtSlash(l.fineLastDate) +
                    '</strong></td><td><span class="badge bg-danger">With a fine of ' + esc(money(l.fineAmount) || 'the prescribed amount') + '</span></td></tr>';
                cautions.push('<strong>' + esc(l.courseLevel) + ':</strong> applications from <strong>' + fmtSlash(l.fineStartDate) +
                    ' to ' + fmtSlash(l.fineLastDate) + '</strong> attract a fine of <strong>' + esc(money(l.fineAmount)) +
                    '</strong>. <strong>Payment will not work after ' + fmtSlash(l.fineLastDate) + '.</strong>');
            }
        });

        host.innerHTML =
            '<p class="mb-3">Colleges are requested to submit their affiliation applications within the following dates:</p>' +
            '<div class="table-responsive"><table class="table table-bordered align-middle mb-3">' +
            '<thead class="table-light"><tr><th>Course level</th><th>Submission period</th><th>Applicable fee</th></tr></thead>' +
            '<tbody>' + rows + '</tbody></table></div>' +
            (cautions.length
                ? '<div class="alert alert-warning mb-0"><i class="bi bi-exclamation-triangle-fill me-2"></i><strong>Caution:</strong><ul class="mb-0 mt-1 ps-3">' +
                  cautions.map(function (c) { return '<li>' + c + '</li>'; }).join('') + '</ul></div>'
                : '');
    }

    function renderAll(data) { renderDashboard(data); renderNotice(data); }

    // ── styles for the pills (injected once) ─────────────────────────────
    (function () {
        var s = d.createElement('style');
        s.textContent =
            '.dl-levels{display:flex;gap:8px;flex-wrap:wrap;margin-top:10px}' +
            '.cs-pill{display:inline-flex;gap:6px;align-items:center;font-size:11px;font-weight:600;padding:4px 11px;border-radius:999px;border:1px solid transparent}' +
            '.cs-pill b{font-weight:800}' +
            '.cs-ok{background:#ecfdf5;color:#059669;border-color:#a7f3d0}' +
            '.cs-warn{background:#fffbeb;color:#b45309;border-color:#fde68a}' +
            '.cs-off{background:#f1f5f9;color:#64748b;border-color:#e2e8f0}' +
            '.cs-badge{margin-left:auto;font-size:9.5px;font-weight:700;text-transform:uppercase;letter-spacing:.04em;padding:2px 8px;border-radius:20px}' +
            '.cs-badge.warn{background:rgba(245,179,1,.2);color:#fcd34d}' +
            '.cs-badge.off{background:rgba(255,255,255,.12);color:rgba(255,255,255,.6)}' +
            '.affiliation-subitem{display:flex;align-items:center}';
        d.head.appendChild(s);
    })();

    // ── public API ───────────────────────────────────────────────────────
    var ready = Promise.resolve();
    w.CourseSchedule = {
        load: load,
        map: map,
        fmtLong: fmtLong,
        money: money,
        get ready() { return ready; }
    };

    // ── auto init on pages that have the dashboard hooks ─────────────────
    function init() {
        if (!d.getElementById('deadlineCard') && !d.getElementById('affNoticeBody')) return;
        ready = load().then(function (data) { renderAll(data); return data; }).catch(function (e) { console.error(e); });
        // keep the countdown and status correct while the page stays open (e.g. midnight rollover)
        setInterval(function () { load(true).then(renderAll).catch(function () { }); }, 60000);
    }
    if (d.readyState === 'loading') d.addEventListener('DOMContentLoaded', init); else init();

})(window, document);