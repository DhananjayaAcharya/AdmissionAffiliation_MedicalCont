// university-dashboard.js
// Place this file at: wwwroot/js/university-dashboard.js

var _pendingCollegeName = '';
var _pendingCollegeCode = '';

// ── FETCH UPLOADS ──────────────────────────────────────────────────────────
function fetchUploads() {
    var sel = document.getElementById('collegeSelect');
    var code = sel.value;
    var name = sel.options[sel.selectedIndex]
        ? (sel.options[sel.selectedIndex].dataset.name || '')
        : '';

    if (!code) {
        alert('Please select a college first.');
        return;
    }

    document.getElementById('resultTitle').textContent =
        'Uploads for: ' + name + ' (' + code + ')';
    document.getElementById('uploadedList').style.display = 'block';
    document.getElementById('uploadTableWrap').innerHTML =
        '<div style="padding:2rem; text-align:center; color:var(--muted);">' +
        '<i class="bi bi-arrow-clockwise animate-spin" style="font-size:1.4rem; display:block; margin-bottom:0.5rem;"></i>' +
        'Loading records...</div>';

    fetch(getCollegeUploadsUrl + '?collegeCode=' + encodeURIComponent(code))
        .then(function (r) {
            if (!r.ok) throw new Error('Server returned ' + r.status);
            return r.json();
        })
        .then(function (data) {
            renderUploadTable(data, name, code);
        })
        .catch(function (err) {
            document.getElementById('uploadTableWrap').innerHTML =
                '<div style="color:#721c24; background:#f8d7da; border:1px solid #f5c6cb;' +
                ' padding:1rem; border-radius:8px; margin:1rem 0;">' +
                'Failed to load records: ' + escHtml(err.message) + '. Please try again.</div>';
        });
}

// ── RENDER UPLOAD TABLE ─────────────────────────────────────────────────────
function renderUploadTable(rows, collegeName, collegeCode) {
    var wrap = document.getElementById('uploadTableWrap');

    if (!rows || rows.length === 0) {
        wrap.innerHTML =
            '<div class="empty-state">' +
            '<i class="bi bi-file-earmark-lock" style="font-size:2rem; display:block; margin-bottom:0.5rem;"></i>' +
            '<p>No uploaded documents found for this college.</p></div>';
        return;
    }

    var rowsHtml = rows.map(function (r, i) {
        var id = r.id || r.Id || 0;
        var fName = r.fileName || r.FileName || 'UGFaculty_List.pdf';
        var uDate = r.uploadedDate || r.UploadedDate || '';
        var uBy = r.uploadedBy || r.UploadedBy || 'College Admin';

        // ── FIX: fileUrl now comes from Url.Action("DownloadUpload") in the
        //    controller, so it is a real routed URL like /UGFaculty/DownloadUpload?id=5
        //    We fall back to the id-based URL if it is somehow missing.
        var fileUrl = r.fileUrl || r.FileUrl || '';
        if (fileUrl && fileUrl.includes(':\\')) {

            var fileName = fileUrl.split('\\').pop();

            fileUrl = medicalFacultyBaseUrl + fileName;
        }

        // FALLBACK
        if (!fileUrl && id > 0) {

            fileUrl = downloadUploadUrl + '?id=' + id;
        }

        var rawStatus = r.status || r.Status || 'Pending';
        var isApproved = rawStatus.toLowerCase() === 'approved'
            || r.printedCopyUploaded === true
            || r.PrintedCopyUploaded === true;
        var displayStatus = isApproved ? 'Approved' : rawStatus;

        return '<tr>' +
            '<td>' + (i + 1) + '</td>' +
            '<td><strong>' + escHtml(fName) + '</strong></td>' +
            '<td>' + formatDate(uDate) + '</td>' +
            '<td>' + escHtml(uBy) + '</td>' +
            '<td>' +
            '<span class="badge-status ' +
            (isApproved ? 'badge-verified' : 'badge-pending') + '">' +
            (isApproved ? '&#10003;' : '&#9679;') + ' ' +
            escHtml(displayStatus) +
            '</span>' +
            '</td>' +
            '<td style="display:flex; gap:8px; align-items:center;">' +
            '<button class="btn-view" onclick="viewDocument(\'' +
            escAttr(fileUrl) + '\',\'' + escAttr(fName) + '\',' + id + ')">' +
            '<i class="bi bi-eye" style="margin-right:4px;"></i>View' +
            '</button>' +
            '<button class="btn-approve" onclick="confirmApproval(\'' +
            escAttr(collegeName) + '\',\'' + escAttr(collegeCode) + '\')">' +
            '<i class="bi bi-file-earmark-check" style="margin-right:4px;"></i>Generate Approval' +
            '</button>' +
            '</td>' +
            '</tr>';
    }).join('');

    wrap.innerHTML =
        '<table class="rguhs-table">' +
        '<thead><tr>' +
        '<th>#</th><th>File Name</th><th>Upload Date</th>' +
        '<th>Uploaded By</th><th>Status</th><th>Actions</th>' +
        '</tr></thead>' +
        '<tbody>' + rowsHtml + '</tbody>' +
        '</table>';
}

// ── CONFIRM DIALOG ─────────────────────────────────────────────────────────
function confirmApproval(collegeName, collegeCode) {
    _pendingCollegeName = collegeName;
    _pendingCollegeCode = collegeCode;
    document.getElementById('confirmMessage').innerHTML =
        'You are about to generate an official approval letter for:<br>' +
        '<strong style="color:var(--maroon-dark);">' + escHtml(collegeName) + '</strong><br>' +
        '<span style="font-size:0.82rem; color:var(--muted);">Code: ' +
        escHtml(collegeCode) + '</span><br><br>' +
        'Please ensure the submitted faculty list has been reviewed before proceeding.';
    openModal('confirmModal');
}

function proceedWithApproval() {
    closeModal('confirmModal');
    fetchFacultyAndBuildLetter(_pendingCollegeName, _pendingCollegeCode);
}

// ── FETCH FACULTY LIST FOR LETTER ──────────────────────────────────────────
function fetchFacultyAndBuildLetter(collegeName, collegeCode) {
    fetch(getFacultyListUrl + '?collegeCode=' + encodeURIComponent(collegeCode))
        .then(function (r) {
            if (!r.ok) throw new Error('Server returned ' + r.status);
            return r.json();
        })
        .then(function (faculty) {
            buildLetter(collegeName, collegeCode, faculty);
        })
        .catch(function (err) {
            alert('Failed to retrieve faculty records: ' + err.message);
        });
}

// ── VIEW DOCUMENT ──────────────────────────────────────────────────────────
// ── VIEW DOCUMENT ──────────────────────────────────────────────────────────
function viewDocument(url, name, id) {

    debugger;

    document.getElementById('viewModalTitle').textContent =
        name || 'Uploaded Document';

    var container = document.getElementById('docContainer');

    container.innerHTML = '';

    var iframeSrc = '';

    if (url && url.trim() !== '' && url !== 'undefined') {

        // HANDLE D:\ PATH
        if (url.includes(':\\')) {

            var fileName = url.split('\\').pop();

            iframeSrc = medicalFacultyBaseUrl + fileName;
        }
        else {

            iframeSrc = url;
        }
    }
    else if (id && id > 0) {

        fileUrl = downloadUploadUrl + '?id=' + id;
    }

    console.log('PDF URL => ', iframeSrc);

    if (iframeSrc) {

        container.innerHTML =
            '<iframe src="' + iframeSrc + '"' +
            ' style="width:100%; height:68vh; border:none; border-radius:8px;">' +
            '</iframe>';
    }
    else {

        container.innerHTML =
            '<div style="text-align:center; padding:5rem 2rem; color:var(--muted);">' +
            '<i class="bi bi-file-earmark-pdf" style="font-size:2.5rem; display:block; margin-bottom:1rem; color:var(--maroon);"></i>' +
            '<p style="font-size:0.95rem; font-weight:500;">Document path is unavailable.</p>' +
            '</div>';
    }

    openModal('viewModal');
}

// ── BUILD OFFICIAL LETTER ──────────────────────────────────────────────────
function buildLetter(collegeName, collegeCode, facultyList) {
    var today = new Date();
    var dateStr = today.toLocaleDateString('en-IN', { day: '2-digit', month: 'long', year: 'numeric' });
    var yr = today.getFullYear();
    var count = (facultyList || []).length;

    var facultyRows = (facultyList || []).map(function (f, i) {
        var sl = f.slNo || f.SlNo || (i + 1);
        var name = f.name || f.Name || '';
        var designation = f.designation || f.Designation || '';
        var department = f.department || f.Department || '';
        var dob = f.dob || f.Dob || '';

        return '<tr>' +
            '<td style="text-align:center;">' + sl + '</td>' +
            '<td>' + escHtml(name) + '</td>' +
            '<td>' + escHtml(designation) + '</td>' +
            '<td>' + escHtml(department) + '</td>' +
            '<td style="text-align:center;">' + formatDate(dob) + '</td>' +
            '</tr>';
    }).join('');

    var html =
        '<div class="letter-border">' +
        '<div class="letter-header-block">' +
        '<div class="logo-row">' +
        '<div class="logo-col">' +
        '<img src="/images/rguhs_logo.png" alt="RGUHS Logo" onerror="this.style.display=\'none\'" />' +
        '</div>' +
        '<div class="text-col">' +
        '<h2>Rajiv Gandhi University of Health Sciences, Karnataka</h2>' +
        '<p class="addr">4th &ldquo;T&rdquo; Block, Jayanagar, Bangalore &ndash; 560 041' +
        ' &nbsp;|&nbsp; Phone: 080-29601933 &nbsp;|&nbsp; www.rguhs.ac.in</p>' +
        '<p class="dept-line">Department of Affiliation &ndash; Undergraduate Division</p>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '<div class="letter-date-row">Date: ' + dateStr + '</div>' +
        '<div class="letter-to-block">' +
        '<span class="lbl">To</span><br>' +
        'The Principal,<br>' +
        '<strong>' + escHtml(collegeName) + '</strong><br>' +
        'College Code: ' + escHtml(collegeCode) +
        '</div>' +
        '<div class="letter-subject-block">' +
        '<strong>Subject:</strong> Acknowledgement and Verification of Undergraduate Faculty Details ' +
        'Submitted for the Academic Year ' + yr + '&ndash;' + (yr + 1) + '.' +
        '</div>' +
        '<div class="letter-salutation">Respected Sir / Madam,</div>' +
        '<p class="letter-para">' +
        'With reference to the faculty details submitted by your institution through the RGUHS ' +
        'Affiliation Portal for the academic year ' + yr + '&ndash;' + (yr + 1) + ', this letter ' +
        'serves as a formal acknowledgement that the University has received and reviewed the ' +
        'undergraduate faculty list pertaining to your college.' +
        '</p>' +
        '<p class="letter-para">' +
        'The University has verified the credentials and records of <strong>' + count +
        ' faculty ' + (count === 1 ? 'member' : 'members') + '</strong> as submitted. ' +
        'The details on record are tabulated below for your reference and confirmation:' +
        '</p>' +
        '<table class="faculty-letter-table">' +
        '<thead><tr>' +
        '<th style="text-align:center; width:7%;">Sl. No.</th>' +
        '<th style="width:33%;">Name of the Faculty</th>' +
        '<th style="width:22%;">Designation</th>' +
        '<th style="width:24%;">Department</th>' +
        '<th style="text-align:center; width:14%;">Date of Birth</th>' +
        '</tr></thead>' +
        '<tbody>' +
        (facultyRows ||
            '<tr><td colspan="5" style="text-align:center; padding:1.5rem; color:var(--muted);">' +
            'No faculty records found for this college.</td></tr>') +
        '</tbody>' +
        '</table>' +
        '<p class="letter-closing-para">' +
        'This letter is issued solely for the purpose of acknowledging receipt of the submitted ' +
        'faculty information and does not constitute a final approval of affiliation or compliance ' +
        'clearance. The institution is advised to ensure that all faculty qualifications, ' +
        'appointment orders, and registration details are maintained and made available for ' +
        'inspection at any time as required by the University.' +
        '</p>' +
        '<p class="letter-closing-para">' +
        'Any subsequent additions, deletions, or modifications to the faculty list must be ' +
        'communicated to the Affiliation Section within the stipulated time frame. ' +
        'The University reserves the right to seek further documentation or conduct a physical ' +
        'verification as deemed necessary.' +
        '</p>' +
        '<p class="letter-closing-para">Thanking you.</p>' +
        '<div class="letter-sign-block">' +
        '<div>' +
        '<p></p>' +
        '<span class="sign-name">Director</span>' +
        '<span class="sign-title">Rajiv Gandhi University of Health Sciences<br>Karnataka, India</span>' +
        '</div>' +
        '</div>' +
        '<div class="letter-copy-block">' +
        '<strong>Copy to:</strong>' +
        '<ol>' +
        '<li>The Registrar, RGUHS &ndash; for records</li>' +
        '<li>The Deputy Director, Affiliation Section &ndash; for further action</li>' +
        '<li>File Copy &ndash; UG Affiliation Division</li>' +
        '</ol>' +
        '</div>' +
        '</div>';

    document.getElementById('letterContent').innerHTML = html;
    openModal('letterModal');
}

// ── PRINT LETTER ───────────────────────────────────────────────────────────
function printLetter() {
    var content = document.getElementById('letterContent').innerHTML;
    var googleFonts = 'https://fonts.googleapis.com/css2?family=EB+Garamond:wght@400;500;600;700&family=DM+Sans:wght@300;400;500;600&display=swap';

    var css = [
        '@page { size: A4 portrait; margin: 0; }',
        'html, body { margin:0; padding:0; background:#fff; font-family:"EB Garamond",serif; color:#1A1410; font-size:10.5pt; -webkit-print-color-adjust:exact; print-color-adjust:exact; }',
        '.letter-border { position:relative; width:210mm; min-height:297mm; box-sizing:border-box; padding:20mm 20mm 25mm 20mm; overflow:hidden; page-break-after:always; z-index:1; }',
        '.letter-border::after  { content:""; position:absolute; top:10mm; bottom:10mm; left:10mm; right:10mm; border:2px solid #7B1C2E; pointer-events:none; z-index:10; }',
        '.letter-border::before { content:""; position:absolute; top:11.5mm; bottom:11.5mm; left:11.5mm; right:11.5mm; border:1px solid #C8991A; pointer-events:none; z-index:10; }',
        '.letter-header-block { border-bottom:2px solid #7B1C2E; padding-bottom:10px; margin-bottom:15px; }',
        '.logo-row { display:table; width:100%; table-layout:fixed; }',
        '.logo-col { display:table-cell; vertical-align:middle; width:75px; padding-right:15px; }',
        '.text-col { display:table-cell; vertical-align:middle; }',
        '.logo-row img { height:65px; width:auto; display:block; }',
        'h2 { font-size:14pt; color:#5A1220; margin:0 0 2px 0; font-weight:700; text-transform:uppercase; }',
        '.addr { font-family:"DM Sans",sans-serif; font-size:8pt; color:#5A5A5A; margin:1px 0; }',
        '.dept-line { font-family:"DM Sans",sans-serif; font-size:8pt; color:#7B1C2E; font-weight:600; text-transform:uppercase; margin-top:3px; }',
        '.letter-date-row { text-align:right; font-family:"DM Sans",sans-serif; font-size:9.5pt; margin-bottom:15px; }',
        '.letter-to-block { font-size:10pt; line-height:1.5; margin-bottom:15px; }',
        '.lbl { font-size:8pt; text-transform:uppercase; letter-spacing:0.05em; color:#666; font-family:"DM Sans",sans-serif; }',
        '.letter-subject-block { background:#FAF7F2 !important; border-left:3.5px solid #7B1C2E; padding:8px 12px; margin-bottom:15px; font-size:10pt; line-height:1.45; }',
        '.letter-subject-block strong { color:#5A1220; }',
        '.letter-salutation { font-size:10pt; margin-bottom:10px; }',
        '.letter-para, .letter-closing-para { font-size:10pt; line-height:1.65; color:#2D2520; margin-bottom:12px; text-align:justify; }',
        '.faculty-letter-table { width:100%; border-collapse:collapse; font-size:8.5pt; font-family:"DM Sans",sans-serif; margin:12px 0; }',
        '.faculty-letter-table thead { background:#7B1C2E !important; color:#fff !important; display:table-header-group; }',
        '.faculty-letter-table tr { page-break-inside:avoid; }',
        '.faculty-letter-table th { padding:6px 8px; font-size:7.5pt; text-transform:uppercase; font-weight:600; color:#fff !important; border:1px solid #7B1C2E; }',
        '.faculty-letter-table td { padding:5px 8px; border:1px solid #D8CFC4; color:#3D3028; }',
        '.faculty-letter-table tr:nth-child(even) { background:#FAF7F2 !important; }',
        '.letter-sign-block { margin-top:35px; page-break-inside:avoid; text-align:right; }',
        '.letter-sign-block p { font-size:10pt; margin-bottom:45px; }',
        '.sign-name { font-size:10.5pt; font-weight:700; color:#5A1220; display:block; margin-top:5px; }',
        '.sign-title { font-size:8.5pt; color:#666; font-family:"DM Sans",sans-serif; line-height:1.3; display:block; }',
        '.letter-copy-block { margin-top:30px; border-top:1px dashed #D8CFC4; padding-top:10px; font-size:8.5pt; font-family:"DM Sans",sans-serif; color:#5A5A5A; page-break-inside:avoid; }',
        '.letter-copy-block strong { color:#7B1C2E; }',
        '.letter-copy-block ol { margin:4px 0 0; padding-left:15px; line-height:1.5; }',
        '.print-footer { position:absolute; bottom:13mm; left:20mm; right:20mm; font-family:"DM Sans",sans-serif; font-size:7.5pt; color:#8A8A8A; display:flex; justify-content:space-between; }'
    ].join('');

    var parsed = document.createElement('div');
    parsed.innerHTML = content;

    var letterContainer = parsed.querySelector('.letter-border');
    if (letterContainer) {
        var footerNode = document.createElement('div');
        footerNode.className = 'print-footer';
        footerNode.innerHTML =
            '<span>RGUHS/UG-AFFIL/' + new Date().getFullYear() + '</span>' +
            '<span style="font-style:italic;">Official University Record Document</span>';
        letterContainer.appendChild(footerNode);
    }

    var win = window.open('', '_blank');
    win.document.open();
    win.document.write(
        '<!DOCTYPE html><html><head>' +
        '<title>RGUHS Faculty Approval Letter</title>' +
        '<link href="' + googleFonts + '" rel="stylesheet">' +
        '<style>' + css + '</style>' +
        '</head><body>' + parsed.innerHTML + '</body></html>'
    );
    win.document.close();
    setTimeout(function () { win.print(); }, 750);
}

// ── MODAL UTILITIES ────────────────────────────────────────────────────────
function openModal(id) { document.getElementById(id).classList.add('active'); document.body.style.overflow = 'hidden'; }
function closeModal(id) { document.getElementById(id).classList.remove('active'); document.body.style.overflow = ''; }

function escHtml(s) {
    if (!s) return '';
    return String(s)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
function escAttr(s) {
    if (!s) return '';
    return String(s).replace(/'/g, "\\'");
}
function formatDate(d) {
    if (!d) return '&mdash;';
    try {
        var parsed = new Date(d);
        if (isNaN(parsed.getTime())) return d;
        return parsed.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
    } catch (e) { return d; }
}