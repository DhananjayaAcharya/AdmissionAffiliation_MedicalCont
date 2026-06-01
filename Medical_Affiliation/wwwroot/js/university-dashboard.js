var _pendingCollegeName = "";
var _pendingCollegeCode = "";
var _pendingUploadId = 0;
var _pendingReferenceId = "";
var _pendingEofficeNo = "";

function fetchUploads() {
    var sel = document.getElementById("collegeSelect");
    var code = sel.value;
    var name = sel.options[sel.selectedIndex] ? (sel.options[sel.selectedIndex].dataset.name || "") : "";

    if (!code) {
        alert("Please select a college first.");
        return;
    }

    document.getElementById("resultTitle").textContent = "Uploads for: " + name + " (" + code + ")";
    document.getElementById("uploadedList").style.display = "block";
    document.getElementById("uploadTableWrap").innerHTML =
        '<div style="padding:2rem; text-align:center; color:var(--muted);">' +
        '<i class="bi bi-arrow-clockwise animate-spin" style="font-size:1.4rem; display:block; margin-bottom:0.5rem;"></i>' +
        "Loading records...</div>";

    fetch(getCollegeUploadsUrl + "?collegeCode=" + encodeURIComponent(code))
        .then(function (r) {
            if (!r.ok) throw new Error("Server returned " + r.status);
            return r.json();
        })
        .then(function (data) {
            renderUploadTable(data, name, code);
        })
        .catch(function (err) {
            document.getElementById("uploadTableWrap").innerHTML =
                '<div style="color:#721c24; background:#f8d7da; border:1px solid #f5c6cb; padding:1rem; border-radius:8px; margin:1rem 0;">' +
                "Failed to load records: " + escHtml(err.message) + ". Please try again.</div>";
        });
}

function renderUploadTable(rows, collegeName, collegeCode) {
    var wrap = document.getElementById("uploadTableWrap");

    if (!rows || rows.length === 0) {
        wrap.innerHTML =
            '<div class="empty-state"><i class="bi bi-file-earmark-lock" style="font-size:2rem; display:block; margin-bottom:0.5rem;"></i>' +
            "<p>No uploaded documents found for this college.</p></div>";
        return;
    }

    var rowsHtml = rows.map(function (r, i) {
        var id = r.id || r.Id || 0;
        var fName = r.fileName || r.FileName || "UGFaculty_List.pdf";
        var uDate = r.uploadedDate || r.UploadedDate || "";
        var uBy = r.uploadedBy || r.UploadedBy || "College Admin";
        var fileUrl = r.fileUrl || r.FileUrl || "";

        if (fileUrl && fileUrl.includes(":\\\\")) {
            var fileName = fileUrl.split("\\").pop();
            fileUrl = medicalFacultyBaseUrl + fileName;
        }
        if (!fileUrl && id > 0) fileUrl = downloadUploadUrl + "?id=" + id;

        var rawStatus = r.status || r.Status || "Pending";
        var isApproved = rawStatus.toLowerCase() === "approved" || r.printedCopyUploaded === true || r.PrintedCopyUploaded === true;
        var displayStatus = isApproved ? "Approved" : rawStatus;

        var safeFileUrl = escAttr(fileUrl);
        var safeFileName = escAttr(fName);
        var safeCollegeName = escAttr(collegeName);
        var safeCollegeCode = escAttr(collegeCode);
        var safeRefId = escAttr(r.referenceId || r.ReferenceId || "");
        var safeEofficeNo = escAttr(r.eofficeNo || r.EofficeNo || "");

        return `<tr>
            <td>${i + 1}</td>
         
            <td>${formatDate(uDate)}</td>
           <td>
                <a href="javascript:void(0)"
                   onclick="showFacultyModal('${safeCollegeCode}','${safeCollegeName}')"
                   style="color:#0d6efd;text-decoration:underline;font-weight:600;">
                   ${escHtml(uBy)}
                </a>
            </td>
            <td><span class="badge-status ${isApproved ? "badge-verified" : "badge-pending"}">${isApproved ? "&#10003;" : "&#9679;"} ${escHtml(displayStatus)}</span></td>
            <td style="display:flex; gap:8px; align-items:center;">
                <button class="btn-view" onclick="viewDocument('${safeFileUrl}','${safeFileName}',${id})"><i class="bi bi-eye" style="margin-right:4px;"></i>View</button>
                <button class="btn-approve" onclick="confirmApproval('${safeCollegeName}','${safeCollegeCode}',${id},'${safeRefId}','${safeEofficeNo}')"><i class="bi bi-file-earmark-check" style="margin-right:4px;"></i>Generate Approval</button>
            </td>
        </tr>`;
    }).join("");

    wrap.innerHTML = '<table class="rguhs-table"><thead><tr><th>#</th><th>Upload Date</th><th>Uploaded By</th><th>Status</th><th>Actions</th></tr></thead><tbody>' + rowsHtml + "</tbody></table>";
}

function confirmApproval(collegeName, collegeCode, uploadId, existingReferenceId, existingEofficeNo) {
    _pendingCollegeName = collegeName;
    _pendingCollegeCode = collegeCode;
    _pendingUploadId = uploadId || 0;
    _pendingReferenceId = existingReferenceId || "";
    _pendingEofficeNo = existingEofficeNo || "";

    document.getElementById("confirmMessage").innerHTML =
        "You are about to generate an official approval letter for:<br>" +
        '<strong style="color:var(--maroon-dark);">' + escHtml(collegeName) + "</strong><br>" +
        '<span style="font-size:0.82rem; color:var(--muted);">Code: ' + escHtml(collegeCode) + "</span><br><br>" +
        '<label style="display:block; text-align:left; font-size:0.82rem; color:var(--muted); margin-bottom:4px;">Eoffice No</label>' +
        '<input id="eofficeNoInput" type="text" value="' + escHtml(_pendingEofficeNo) + '" placeholder="Enter Eoffice No" style="width:100%; padding:8px 10px; border:1px solid var(--border); border-radius:8px; margin-bottom:10px; font-size:0.9rem;" />' +
        '<label style="display:block; text-align:left; font-size:0.82rem; color:var(--muted); margin-bottom:4px;">Ref 1 Number</label>' +
        '<input id="ref1Input" type="text" value="' + escHtml(_pendingReferenceId) + '" placeholder="Enter Ref 1" style="width:100%; padding:8px 10px; border:1px solid var(--border); border-radius:8px; margin-bottom:10px; font-size:0.9rem;" />' +
        "Please ensure the submitted faculty list has been reviewed before proceeding.";

    openModal("confirmModal");
}

function proceedWithApproval() {
    var eofficeNode = document.getElementById("eofficeNoInput");
    var refNode = document.getElementById("ref1Input");

    var eofficeNo = eofficeNode ? String(eofficeNode.value || "").trim() : "";
    var refValue = refNode ? String(refNode.value || "").trim() : "";

    if (!eofficeNo) { alert("Please enter Eoffice No."); return; }
    if (!refValue) { alert("Please enter Ref 1."); return; }

    _pendingEofficeNo = eofficeNo;
    _pendingReferenceId = refValue;

    closeModal("confirmModal");
    saveReferenceAndFetchFaculty(_pendingCollegeName, _pendingCollegeCode, _pendingUploadId, _pendingReferenceId, _pendingEofficeNo);
}

function saveReferenceAndFetchFaculty(collegeName, collegeCode, uploadId, referenceId, eofficeNo) {
    if (typeof saveReferenceUrl === "undefined" || !saveReferenceUrl) {
        fetchFacultyAndBuildLetter(collegeName, collegeCode, referenceId, eofficeNo);
        return;
    }

    fetch(saveReferenceUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            id: uploadId,
            collegeCode: collegeCode,
            referenceId: referenceId,
            eofficeNo: eofficeNo
        })
    })
        .then(function (r) {
            if (!r.ok) throw new Error("Server returned " + r.status);
            return r.json();
        })
        .then(function (res) {
            if (res && res.success === false) throw new Error(res.message || "Failed to save reference.");
            fetchFacultyAndBuildLetter(collegeName, collegeCode, referenceId, eofficeNo);
        })
        .catch(function (err) {
            alert("Unable to save Ref 1 / Eoffice No: " + err.message);
        });
}

function fetchFacultyAndBuildLetter(collegeName, collegeCode, referenceId, eofficeNo) {
    fetch(getFacultyListUrl + "?collegeCode=" + encodeURIComponent(collegeCode))
        .then(function (r) {
            if (!r.ok) throw new Error("Server returned " + r.status);
            return r.json();
        })
        .then(function (faculty) {
            buildLetter(collegeName, collegeCode, faculty, referenceId, eofficeNo);
        })
        .catch(function (err) {
            alert("Failed to retrieve faculty records: " + err.message);
        });
}

function viewDocument(url, name, id) {
    document.getElementById("viewModalTitle").textContent = name || "Uploaded Document";
    var container = document.getElementById("docContainer");
    container.innerHTML = "";

    var iframeSrc = "";
    if (url && url.trim() !== "" && url !== "undefined") {
        if (url.includes(":\\\\")) {
            var fileName = url.split("\\").pop();
            iframeSrc = medicalFacultyBaseUrl + fileName;
        } else {
            iframeSrc = url;
        }
    } else if (id && id > 0) {
        iframeSrc = downloadUploadUrl + "?id=" + id;
    }

    if (iframeSrc) {
        container.innerHTML = '<iframe src="' + iframeSrc + '" style="width:100%; height:68vh; border:none; border-radius:8px;"></iframe>';
    } else {
        container.innerHTML = '<div style="text-align:center; padding:5rem 2rem; color:var(--muted);"><i class="bi bi-file-earmark-pdf" style="font-size:2.5rem; display:block; margin-bottom:1rem; color:var(--maroon);"></i><p style="font-size:0.95rem; font-weight:500;">Document path is unavailable.</p></div>';
    }

    openModal("viewModal");
}

function buildLetter(collegeName, collegeCode, facultyList, referenceId, eofficeNo) {
    var today = new Date();
    var dateStr = today.toLocaleDateString("en-IN", { day: "2-digit", month: "long", year: "numeric" });
    var ayStart = 2026;
    var ayEnd = 2027;
    var subjectText = "List of Undergraduate Teaching Faculty for the Academic Year " + ayStart + "-" + String(ayEnd).slice(-2);
    var groupedHtml = buildDepartmentSections(facultyList || []);

    var html =
        '<div class="letter-border">' +
        '<div class="letter-header-block">' +
        '<div class="letter-head-grid">' +
        '<div class="head-logo-wrap">' +
        '<img class="head-logo" src="' + rguhsLogoUrl + '" alt="RGUHS Logo" onerror="this.src=\'' + rguhsLogoFallbackUrl + '\'; this.onerror=null;" />' +
        '</div>' +
        '<div class="head-text-wrap">' +
        '<h2 class="kn-title">ರಾಜೀವ್ ಗಾಂಧಿ ಆರೋಗ್ಯ ವಿಜ್ಞಾನಗಳ ವಿಶ್ವವಿದ್ಯಾಲಯ, ಕರ್ನಾಟಕ</h2>' +
        '<h2 class="en-title">RAJIV GANDHI UNIVERSITY OF HEALTH SCIENCES, KARNATAKA</h2>' +
        '<p class="addr">4th T Block, Jayanagar, Bengaluru 560041</p>' +
        '<p class="addr">registrar@rguhs.ac.in | +91-80-29601928</p>' +
        '</div>' +
        '</div>' +
        '</div>' +

        '<div class="letter-meta-row">' +
        '<div class="letter-meta-left"><strong>Ref:</strong> ' + escHtml("RGUHS/MEDICAL/" + (eofficeNo || "_____") + "/2026-27") + '</div>' +
        '<div class="letter-meta-right"><strong>Date:</strong> ' + dateStr + '</div>' +
        '</div>' +

        '<div class="letter-to-block">' +
        '<strong>To</strong><br>' +
        'The Principal / Director,<br>' +
        '<strong>' + escHtml(collegeName) + '</strong>' +
        '</div>' +

        '<p class="letter-para" style="margin-bottom:6px;">Sir/Madam,</p>' +

        '<div class="letter-subject-block"><strong>Subject:</strong> ' + escHtml(subjectText) + '</div>' +

        '<div class="letter-ref-block">' +
        '<strong>Ref:</strong>' +
        '<ol class="ref-list" style="margin-left:14px; padding-left:18px; margin-top:6px;">' +
        '<li>Data duly verified and endorsed by the Director/Principal submitted to the University online.</li>' +
        '<li>' + escHtml(referenceId || "") + '</li>' +
        '</ol>' +
        '</div>' +

        '<p class="letter-para" style="margin-top:6px; text-indent:24px;">As per the data uploaded and endorsed by the head of the institution, the following is the list of faculty members working as undergraduate teachers in the concerned subject in compliance with the regulations of the Rajiv Gandhi University of Health Sciences (RGUHS) and the National Medical Commission (NMC) for the academic year ' + ayStart + "-" + String(ayEnd).slice(-2) + ".</p>" +
        '<p class="letter-para center-strong">' + escHtml(collegeName) + '</p>' +
        '<p class="letter-para center-strong">UG Teaching Faculty Details for the Academic Year ' + ayStart + "-" + String(ayEnd).slice(-2) + '</p>' +
        groupedHtml +
        '<p class="letter-closing-para"><br></p>' +
        '<div class="letter-sign-block"><span class="sign-name">REGISTRAR</span></div>' + '<br>' + '<br>' +
        '</div>';

    document.getElementById("letterContent").innerHTML = html;
    openModal("letterModal");
}
function printLetter() {
    var content = document.getElementById("letterContent").innerHTML;
    var googleFonts = "https://fonts.googleapis.com/css2?family=Noto+Serif+Kannada:wght@400;600;700&family=EB+Garamond:wght@400;500;600;700&family=DM+Sans:wght@300;400;500;600&display=swap";

    var css = [
        ":root{--gov-maroon:#6E1B2A;--gov-gold:#B38A2E;--gov-ink:#1E1E1E;--gov-muted:#5C5C5C;--gov-border:#D7CFC1;--gov-bg:#FFFDF9;}",
        "@page{size:A4 portrait;margin:14mm 10mm 16mm 10mm;}",
        'html,body{margin:0;padding:0;background:#fff;font-family:"Noto Serif Kannada","EB Garamond",serif;color:var(--gov-ink);font-size:10.5pt;-webkit-print-color-adjust:exact;print-color-adjust:exact;}',

        ".letter-border{position:relative;width:auto;min-height:calc(297mm - 30mm);box-sizing:border-box;padding:10mm;background:var(--gov-bg);border:1.5px solid var(--gov-maroon);overflow:visible;page-break-after:auto;}",
        ".letter-border:last-child{page-break-after:auto;}",
        '.letter-border::before{content:"";position:absolute;top:4mm;right:4mm;bottom:4mm;left:4mm;border:1px solid var(--gov-gold);pointer-events:none;z-index:1;}',
        '.letter-border::after{content:"";position:absolute;top:14mm;right:10mm;bottom:12mm;left:10mm;background-repeat:no-repeat;background-position:center 58%;background-size:270px;opacity:.03;pointer-events:none;z-index:0;}',

        ".letter-header-block,.letter-meta-row,.letter-to-block,.letter-subject-block,.letter-ref-block,.letter-para,.dept-block,.letter-sign-block{position:relative;z-index:2;}",
        ".letter-header-block{border-bottom:2px solid var(--gov-maroon);padding:5px 0 6px;margin-bottom:8px;}",
        ".letter-head-grid{display:grid;grid-template-columns:120px 1fr;column-gap:12px;align-items:center;}",
        ".head-logo-wrap{width:120px;height:120px;display:flex;align-items:center;justify-content:center;}",
        ".head-logo{width:102px;height:102px;object-fit:contain;display:block;}",
        ".head-text-wrap{text-align:center;display:block;}",
        '.kn-title{margin:0 0 2px;font-family:"Noto Serif Kannada","EB Garamond",serif;font-size:14.8pt;font-weight:700;line-height:1.08;color:var(--gov-maroon);text-align:center;}',
        '.en-title{margin:0 0 4px;font-family:"EB Garamond",serif;font-size:12.5pt;font-weight:700;line-height:1.04;letter-spacing:.1px;color:var(--gov-maroon);text-transform:uppercase;text-align:center;}',
        '.addr{margin:0;font-family:"DM Sans",sans-serif;font-size:9pt;font-weight:600;line-height:1.14;color:#5f5f5f;text-align:center;}',

        '.letter-meta-row{display:flex;justify-content:space-between;align-items:flex-start;margin:2mm 0 4mm;font-family:"DM Sans",sans-serif;font-size:9.1pt;}',
        ".letter-meta-left{text-align:left;max-width:65%;}",
        ".letter-meta-right{text-align:right;min-width:30%;}",

        ".letter-ref-block{margin:4px 0 8px;font-size:9.6pt;}",
        '.ref-list{margin:6px 0 7px 18px;padding:0;font-family:"DM Sans",sans-serif;font-size:8.8pt;line-height:1.32;}',

        ".letter-to-block{font-size:10.1pt;line-height:1.48;margin-bottom:7px;}",
        ".letter-subject-block{background:#F6F1E6;border-left:4px solid var(--gov-maroon);padding:7px 10px;margin-bottom:7px;font-size:10pt;}",
        ".letter-para,.letter-closing-para{font-size:10pt;line-height:1.48;color:var(--gov-ink);margin-bottom:5px;text-align:justify;}",
        ".center-strong{text-align:center !important;font-weight:700;margin-bottom:7px;}",

        ".dept-block{margin-bottom:5px;break-inside:auto;page-break-inside:auto;}",
        '.dept-header{font-family:"DM Sans",sans-serif;font-size:11pt;font-weight:800;color:#4A1220;border-left:4px solid var(--gov-maroon);background:#F8EFD9;padding:7px 9px;margin:8px 0 5px;}',

        ".faculty-letter-table{width:100%;border-collapse:collapse;font-family:\"DM Sans\",sans-serif;font-size:9pt;margin:0 0 9px;page-break-inside:auto;}",
        ".faculty-letter-table thead{background:transparent;color:#2E2E2E;display:table-row-group;}",
        ".faculty-letter-table th{padding:6px;font-size:8.7pt;font-weight:800;text-transform:uppercase;border:1px solid #C9B9A5;vertical-align:middle;}",
        ".faculty-letter-table td{padding:5px 6px;border:1px solid var(--gov-border);color:#2E2E2E;vertical-align:middle;font-size:8.9pt;}",
        ".faculty-letter-table td:nth-child(2){font-size:9.1pt;font-weight:700;color:#2A1F1A;}",
        ".faculty-letter-table tbody tr{height:32px;page-break-inside:avoid;}",
        ".faculty-letter-table tr:nth-child(even){background:#FCF9F2;}",

        ".letter-sign-block{margin-top:9px;padding-bottom:2mm;text-align:right;page-break-inside:avoid;}",
        ".sign-name{font-size:10.5pt;font-weight:800;color:#2A2A2A;letter-spacing:.03em;display:block;}"
    ].join("");

    css += '.letter-border::after{background-image:url("' + rguhsWatermarkUrl + '")}';

    var win = window.open("", "_blank");
    win.document.open();
    win.document.write("<!DOCTYPE html><html><head><title></title><link href=\"" + googleFonts + "\" rel=\"stylesheet\"><style>" + css + "</style></head><body>" + content + "</body></html>");
    win.document.close();
    setTimeout(function () { win.print(); }, 750);
}

function buildDepartmentSections(facultyList) {
    if (!facultyList || facultyList.length === 0) {
        return '<div class="dept-block"><table class="faculty-letter-table"><tbody><tr><td colspan="5" style="text-align:center; padding:1rem; color:var(--muted);">No faculty records found for this college.</td></tr></tbody></table></div>';
    }

    var normalized = facultyList.map(function (f, idx) {
        return {
            slNo: f.slNo || f.SlNo || (idx + 1),
            name: f.name || f.Name || "",
            designation: f.designation || f.Designation || "",
            designationCode: f.designationCode || f.DesignationCode || "",
            department: f.department || f.Department || "",
            aebasId: f.aebasAttendId || f.AEBASAttendId || f.aEBASAttendId || "",
            kmcNo: f.stateCouncilRegNo || f.StateCouncilRegNo || ""
        };
    });

    var deptMap = {};
    normalized.forEach(function (f) {
        var dept = (f.department || "General").trim();
        if (!deptMap[dept]) deptMap[dept] = [];
        deptMap[dept].push(f);
    });

    var departments = Object.keys(deptMap).sort(function (a, b) { return a.localeCompare(b); });
    var html = "";

    departments.forEach(function (deptName) {
        var rows = deptMap[deptName]
            .sort(function (a, b) {
                var da = (a.designationCode || a.designation || "").toString();
                var db = (b.designationCode || b.designation || "").toString();
                return da.localeCompare(db, undefined, { numeric: true, sensitivity: "base" });
            })
            .map(function (f, i) {
                return "<tr>" +
                    '<td style="text-align:center;">' + (i + 1) + "</td>" +
                    '<td>' + escHtml(f.name) + "</td>" +
                    '<td>' + escHtml(f.designation) + "</td>" +
                    '<td>' + escHtml(f.aebasId) + "</td>" +
                    '<td>' + escHtml(f.kmcNo) + "</td>" +
                    "</tr>";
            }).join("");

        html += '<div class="dept-block">' +
            '<div class="dept-header">Department: ' + escHtml(deptName) + "</div>" +
            '<table class="faculty-letter-table">' +
            "<thead><tr>" +
            '<th style="text-align:center; width:8%;">Sl. No.</th>' +
            '<th style="width:38%;">Name of the Faculty</th>' +
            '<th style="width:22%;">Designation</th>' +
            '<th style="width:16%;">AEBAS Attend. ID</th>' +
            '<th style="width:16%;">KMC. No</th>' +
            "</tr></thead>" +
            "<tbody>" + rows + "</tbody></table></div>";
    });

    return html;
}



function openModal(id) { document.getElementById(id).classList.add("active"); document.body.style.overflow = "hidden"; }
function closeModal(id) { document.getElementById(id).classList.remove("active"); document.body.style.overflow = ""; }

function escHtml(s) {
    if (!s) return "";
    return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
}
function escAttr(s) { return s ? String(s).replace(/'/g, "\\'") : ""; }
function formatDate(d) {
    if (!d) return "&mdash;";
    try {
        var parsed = new Date(d);
        if (isNaN(parsed.getTime())) return d;
        return parsed.toLocaleDateString("en-IN", { day: "2-digit", month: "short", year: "numeric" });
    } catch (e) { return d; }
}

function showFacultyModal(collegeCode, collegeName) {

    const title = document.getElementById("facultyModalTitle");
    const body = document.getElementById("facultyModalBody");

    if (!title || !body) {
        console.error("Faculty modal elements not found");
        return;
    }

    title.innerText = "Faculty Details - " + collegeName;

    body.innerHTML =
        '<div class="text-center p-3">Loading...</div>';

    openModal("facultyModal");

    fetch(getFacultyListUrl + "?collegeCode=" + encodeURIComponent(collegeCode))
        .then(r => {
            if (!r.ok) throw new Error("Failed to load faculty");
            return r.json();
        })
        .then(data => {

            if (!data || data.length === 0) {
                document.getElementById("facultyModalBody").innerHTML =
                    "<p>No faculty records found.</p>";
                return;
            }

            let html = `
                <table  class="faculty-table" id="facultyTable">
                    <thead>
                        <tr>
                            <th>Sl No</th>
                            <th>Name</th>
                            <th>Designation</th>
                            <th>Department</th>
                            <th>AEBAS ID</th>
                            <th>KMC No</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

            data.forEach((f, index) => {

                html += `
                    <tr>
                        <td>${index + 1}</td>
                        <td>${f.name || f.Name || ''}</td>
                        <td>${f.designation || f.Designation || ''}</td>
                        <td>${f.department || f.Department || ''}</td>
                        <td>${f.aebasAttendId || f.AEBASAttendId || ''}</td>
                        <td>${f.stateCouncilRegNo || f.StateCouncilRegNo || ''}</td>
                    </tr>
                `;
            });

            html += "</tbody></table>";

            document.getElementById("facultyModalBody").innerHTML = html;
        })
        .catch(err => {
            document.getElementById("facultyModalBody").innerHTML =
                `<div class="alert alert-danger">${err.message}</div>`;
        });
}

function filterFacultyTable() {

    const input = document.getElementById("facultySearch");
    const filter = input.value.toLowerCase();

    const table = document.getElementById("facultyTable");

    if (!table) return;

    const rows = table.getElementsByTagName("tr");

    for (let i = 1; i < rows.length; i++) {

        let found = false;

        const cells = rows[i].getElementsByTagName("td");

        for (let j = 0; j < cells.length; j++) {

            const txt = cells[j].textContent || cells[j].innerText;

            if (txt.toLowerCase().indexOf(filter) > -1) {
                found = true;
                break;
            }
        }

        rows[i].style.display = found ? "" : "none";
    }
}