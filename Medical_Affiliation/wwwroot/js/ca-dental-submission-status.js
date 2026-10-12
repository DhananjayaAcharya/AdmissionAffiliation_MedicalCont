
document.addEventListener("DOMContentLoaded", function () {
    const table = document.getElementById("dr-submissions-table");
    const tbody = document.getElementById("dr-submissions-body");
    const searchInput = document.getElementById("dr-table-search");
    const clearSearchButton = document.getElementById("dr-clear-search");
    const pageSizeSelect = document.getElementById("dr-page-size-select");
    const pagination = document.getElementById("dr-pagination");
    const tableInfo = document.getElementById("dr-table-info");
    const recordCount = document.getElementById("dr-record-count");
    const noResults = document.getElementById("dr-no-results");

    if (!table || !tbody) return;

    const rows = Array.from(
        tbody.querySelectorAll("tr[data-submission-row]")
    );

    let currentPage = 1;
    let pageSize = Number(pageSizeSelect.value) || 10;
    let filteredRows = [...rows];

    function getPageCount() {
        return Math.ceil(filteredRows.length / pageSize);
    }

    function filterRows() {
        const searchTerm = searchInput.value
            .trim()
            .toLocaleLowerCase();

        filteredRows = rows.filter(row =>
            row.textContent.toLocaleLowerCase().includes(searchTerm)
        );

        currentPage = 1;
        clearSearchButton.hidden = searchTerm.length === 0;
        renderTable();
    }

    function createPageItem(label, page, options = {}) {
        const li = document.createElement("li");
        li.className = "page-item";

        if (options.active) li.classList.add("active");
        if (options.disabled) li.classList.add("disabled");

        const button = document.createElement("button");
        button.type = "button";
        button.className = "page-link";
        button.textContent = label;

        if (options.active) {
            button.setAttribute("aria-current", "page");
        }

        if (options.disabled) {
            button.disabled = true;
        } else {
            button.addEventListener("click", function () {
                currentPage = page;
                renderTable();
            });
        }

        li.appendChild(button);
        return li;
    }

    function renderPagination() {
        pagination.replaceChildren();

        const totalPages = getPageCount();

        pagination.appendChild(
            createPageItem("‹", currentPage - 1, {
                disabled: currentPage <= 1
            })
        );

        if (totalPages > 0) {
            // Show a compact range of page numbers
            let startPage = Math.max(1, currentPage - 2);
            let endPage = Math.min(totalPages, startPage + 4);

            startPage = Math.max(1, endPage - 4);

            if (startPage > 1) {
                pagination.appendChild(
                    createPageItem("1", 1)
                );

                if (startPage > 2) {
                    const dots = document.createElement("li");
                    dots.className = "page-item disabled";

                    const span = document.createElement("span");
                    span.className = "page-link";
                    span.textContent = "…";

                    dots.appendChild(span);
                    pagination.appendChild(dots);
                }
            }

            for (let page = startPage; page <= endPage; page++) {
                pagination.appendChild(
                    createPageItem(String(page), page, {
                        active: page === currentPage
                    })
                );
            }

            if (endPage < totalPages) {
                if (endPage < totalPages - 1) {
                    const dots = document.createElement("li");
                    dots.className = "page-item disabled";

                    const span = document.createElement("span");
                    span.className = "page-link";
                    span.textContent = "…";

                    dots.appendChild(span);
                    pagination.appendChild(dots);
                }

                pagination.appendChild(
                    createPageItem(String(totalPages), totalPages)
                );
            }
        }

        pagination.appendChild(
            createPageItem("›", currentPage + 1, {
                disabled: currentPage >= totalPages || totalPages === 0
            })
        );
    }

    function renderTable() {
        const total = filteredRows.length;
        const totalPages = getPageCount();

        if (currentPage > totalPages) {
            currentPage = Math.max(1, totalPages);
        }

        const startIndex = (currentPage - 1) * pageSize;
        const endIndex = Math.min(startIndex + pageSize, total);

        const visibleRows = new Set(
            filteredRows.slice(startIndex, endIndex)
        );

        rows.forEach(row => {
            row.hidden = !visibleRows.has(row);
        });

        // Update serial numbers across filtered results
        filteredRows.forEach((row, index) => {
            const serialCell = row.querySelector(".dr-serial");
            if (serialCell) {
                serialCell.textContent = index + 1;
            }
        });

        noResults.hidden = total !== 0;
        table.hidden = total === 0;

        recordCount.textContent = total.toLocaleString();

        if (total === 0) {
            tableInfo.textContent = "Showing 0 entries";
        } else {
            tableInfo.textContent =
                `Showing ${startIndex + 1} to ${endIndex} of ${total} entries`;
        }

        renderPagination();
    }


    let sortColumn = -1;

    let sortAscending = true;

    const headers = Array.from(
        table.querySelectorAll("thead th.dr-sortable")
    );

    headers.forEach((header, index) => {
        header.addEventListener("click", function () {
            if (sortColumn === index) {
                sortAscending = !sortAscending;
            } else {
                sortColumn = index;
                sortAscending = true;
            }

            const sortType = header.dataset.sort || "text";

            rows.sort((a, b) => {
                const aText = a.cells[index]?.textContent.trim() || "";
                const bText = b.cells[index]?.textContent.trim() || "";

                let result = 0;

                if (sortType === "number") {
                    result = Number(aText) - Number(bText);
                } else if (sortType === "date") {
                    const aDate = Date.parse(aText);
                    const bDate = Date.parse(bText);

                    result =
                        (Number.isNaN(aDate) ? 0 : aDate) -
                        (Number.isNaN(bDate) ? 0 : bDate);
                } else {
                    result = aText.localeCompare(bText, undefined, {
                        numeric: true,
                        sensitivity: "base"
                    });
                }

                return sortAscending ? result : -result;
            });

            // Reorder actual table rows
            rows.forEach(row => tbody.appendChild(row));

            // Update header indicators
            headers.forEach(h => {
                const icon = h.querySelector(".sort-icon");
                if (icon) {
                    icon.textContent = "↕";
                }
            });

            const activeIcon = header.querySelector(".sort-icon");
            if (activeIcon) {
                activeIcon.textContent = sortAscending ? "↑" : "↓";
            }

            filterRows();
        });
    });

    searchInput.addEventListener("input", filterRows);

    clearSearchButton.addEventListener("click", function () {
        searchInput.value = "";
        searchInput.focus();
        filterRows();
    });

    pageSizeSelect.addEventListener("change", function () {
        pageSize = Number(this.value) || 10;
        currentPage = 1;
        renderTable();
    });

    renderTable();

    const exportButton = document.getElementById("dr-export-pdf");

    if (!exportButton) return;

    // Data from the server-filtered report
    const reportData = window.dentalSubmissionReportData || [];

    function loadReportLogo(url) {
        return new Promise((resolve, reject) => {
            const image = new Image();

            image.onload = function () {
                try {
                    const canvas = document.createElement("canvas");
                    canvas.width = image.naturalWidth;
                    canvas.height = image.naturalHeight;

                    const context = canvas.getContext("2d");
                    context.drawImage(image, 0, 0);

                    resolve(canvas.toDataURL("image/jpeg", 0.95));
                } catch (error) {
                    reject(error);
                }
            };

            image.onerror = () => reject(
                new Error("RGUHS logo could not be loaded.")
            );

            image.src = url;
        });
    }

    exportButton.addEventListener("click", async function () {
        if (!window.jspdf || !window.jspdf.jsPDF) {
            alert("PDF library could not be loaded. Please refresh the page.");
            return;
        }

        const originalHtml = exportButton.innerHTML;

        exportButton.disabled = true;
        exportButton.innerHTML =
            '<span class="spinner-border spinner-border-sm"></span>' +
            '<span>Generating PDF...</span>';

        try {
            const { jsPDF } = window.jspdf;

            const doc = new jsPDF({
                orientation: "landscape",
                unit: "mm",
                format: "a4"
            });

            if (typeof doc.autoTable !== "function") {
                throw new Error("PDF table library could not be loaded.");
            }

            const logoData = await loadReportLogo(
                window.dentalReportLogoUrl
            );

            const pageWidth = doc.internal.pageSize.getWidth();
            const pageHeight = doc.internal.pageSize.getHeight();
            const centerX = pageWidth / 2;

            const getSelectedText = (id, fallback) => {
                const select = document.getElementById(id);
                return select?.selectedOptions?.[0]?.text || fallback;
            };

            const academicYear = getSelectedText(
                "AcademicYear",
                "All Academic Years"
            );

            const courseLevel = getSelectedText(
                "CourseLevel",
                "All Course Levels"
            );

            const affiliationType = getSelectedText(
                "AffiliationTypeId",
                "All Affiliation Types"
            );

            const formatDate = value => {
                if (!value) return "---";

                const date = new Date(value);

                if (Number.isNaN(date.getTime())) return "---";

                return date.toLocaleString("en-IN", {
                    day: "2-digit",
                    month: "short",
                    year: "numeric",
                    hour: "2-digit",
                    minute: "2-digit",
                    hour12: true
                });
            };

            const generatedOn = formatDate(new Date().toISOString());

            // Use the same RGUHS branding on every PDF page.
            
            const drawReportHeader = () => {
                doc.setFillColor(255, 255, 255);
                doc.rect(0, 0, pageWidth, 40, "F");

                // University logos
                doc.addImage(logoData, "JPEG", 12, 7, 19, 19);
                doc.addImage(
                    logoData,
                    "JPEG",
                    pageWidth - 31,
                    7,
                    19,
                    19
                );

                // University name
                doc.setTextColor(13, 27, 42);
                doc.setFont("helvetica", "bold");
                doc.setFontSize(16);
                doc.text(
                    "Rajiv Gandhi University of Health Sciences",
                    centerX,
                    13,
                    { align: "center" }
                );

                // Report title
                doc.setTextColor(122, 100, 32);
                doc.setFontSize(12);
                doc.text(
                    "Dental Final Submission Report",
                    centerX,
                    21,
                    { align: "center" }
                );

                // Address
                doc.setTextColor(55, 65, 81);
                doc.setFont("helvetica", "normal");
                doc.setFontSize(8);
                doc.text(
                    "4th 'T' Block, Jayanagar, Bengaluru - Karnataka, India",
                    centerX,
                    27,
                    { align: "center" }
                );

                // Gold divider
                doc.setDrawColor(201, 168, 76);
                doc.setLineWidth(0.8);
                doc.line(6, 31, pageWidth - 6, 31);

                // Generated date only — no filter details
                doc.setTextColor(55, 65, 81);
                doc.setFontSize(8);
                doc.text(
                    `Generated On: ${generatedOn}`,
                    centerX,
                    37,
                    { align: "center" }
                );
            };


            // Match the nine columns in the report table.
            const tableRows = reportData.map((item, index) => [
                index + 1,
                item.CollegeName || "---",
                item.ApplicationNumber || "---",
                item.AcademicYear || "---",
                item.CourseLevel || "---",
                item.AffiliationType || "---",
                item.PrincipalConsent === true
                    ? "Yes"
                    : item.PrincipalConsent === false
                        ? "No"
                        : "---",
                item.IsSubmitted ? "Submitted" : "Not Submitted",
                formatDate(item.SubmittedDate)
            ]);

            doc.autoTable({
                startY: 43,

                head: [[
                    "Sl. No.",
                    "College Name",
                    "Application Number",
                    "Academic Year",
                    "Course Level",
                    "Affiliation Type",
                    "Principal Consent",
                    "Submission Status",
                    "Submitted Date"
                ]],

                body: tableRows,

                theme: "grid",

                margin: {
                    top: 43,
                    left: 6,
                    right: 6,
                    bottom: 15
                },

                styles: {
                    font: "helvetica",
                    fontSize: 7.5,
                    cellPadding: 2.5,
                    overflow: "linebreak",
                    valign: "middle",
                    textColor: [31, 41, 55],
                    lineColor: [203, 213, 225],
                    lineWidth: 0.2
                },

                headStyles: {
                    fillColor: [13, 27, 42],
                    textColor: [255, 255, 255],
                    fontStyle: "bold",
                    halign: "center"
                },

                alternateRowStyles: {
                    fillColor: [248, 250, 252]
                },

                columnStyles: {
                    0: { cellWidth: 12, halign: "center" },
                    1: { cellWidth: 48 },
                    2: { cellWidth: 36 },
                    3: { cellWidth: 24 },
                    4: { cellWidth: 21 },
                    5: { cellWidth: 34 },
                    6: { cellWidth: 27, halign: "center" },
                    7: { cellWidth: 32 },
                    8: { cellWidth: 51 }
                },

                // Draw the university header again on every page.
                willDrawPage: function () {
                    drawReportHeader();
                },

                // Page number and official footer.
                
                didDrawPage: function () {
                    const pageNumber =
                        doc.internal.getCurrentPageInfo().pageNumber;

                    const totalPages = doc.internal.getNumberOfPages();

                    // Footer separator
                    doc.setDrawColor(201, 168, 76);
                    doc.setLineWidth(0.3);
                    doc.line(
                        8,
                        pageHeight - 12,
                        pageWidth - 8,
                        pageHeight - 12
                    );

                    // Footer text
                    doc.setFont("helvetica", "normal");
                    doc.setFontSize(7);
                    doc.setTextColor(90, 100, 110);

                    doc.text(
                        "Rajiv Gandhi University of Health Sciences - Official Report",
                        10,
                        pageHeight - 7
                    );

                    doc.text(
                        `Page ${pageNumber} of ${totalPages}`,
                        pageWidth - 10,
                        pageHeight - 7,
                        { align: "right" }
                    );

                    // Border on every page
                    doc.setDrawColor(13, 27, 42);
                    doc.setLineWidth(0.5);
                    doc.rect(
                        4,
                        4,
                        pageWidth - 8,
                        pageHeight - 8
                    );
                }

            });

            const dateStamp = new Date().toISOString().slice(0, 10);

            doc.save(`Dental_Submission_Report_${dateStamp}.pdf`);
        } catch (error) {
            console.error("PDF export failed:", error);

            alert(
                error.message ||
                "Unable to generate the PDF. Please try again."
            );
        } finally {
            exportButton.disabled = false;
            exportButton.innerHTML = originalHtml;
        }
    });

});
