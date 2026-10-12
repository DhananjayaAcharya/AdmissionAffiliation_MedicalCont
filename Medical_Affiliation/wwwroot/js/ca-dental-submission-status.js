
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

    exportButton.addEventListener("click", function () {
        if (!window.jspdf || !window.jspdf.jsPDF) {
            alert("PDF library could not be loaded. Please try again.");
            return;
        }

        const { jsPDF } = window.jspdf;
        const doc = new jsPDF({
            orientation: "landscape",
            unit: "mm",
            format: "a4"
        });

        if (typeof doc.autoTable !== "function") {
            alert("PDF table library could not be loaded. Please refresh the page.");
            return;
        }

        const getSelectedText = (id, fallback) => {
            const select = document.getElementById(id);
            return select?.selectedOptions[0]?.text || fallback;
        };

        const academicYear = getSelectedText(
            "AcademicYear", "All Academic Years"
        );

        const courseLevel = getSelectedText(
            "CourseLevel", "All Course Levels"
        );

        const affiliationType = getSelectedText(
            "AffiliationTypeId", "All Affiliation Types"
        );

        const formatDate = value => {
            if (!value) return "---";

            const date = new Date(value);

            if (Number.isNaN(date.getTime())) return "---";

            return date.toLocaleString("en-IN", {
                day: "2-digit",
                month: "long",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
                hour12: true
            });
        };

        // Report heading
        doc.setFontSize(18);
        doc.setFont("helvetica", "bold");
        doc.text("Dental Final Submission Report", 14, 16);

        doc.setFontSize(10);
        doc.setFont("helvetica", "normal");

        doc.text(`Type of Affiliation: ${affiliationType}`, 14, 25);
        doc.text(`Course Level: ${courseLevel}`, 14, 31);
        doc.text(`Academic Year: ${academicYear}`, 14, 37);
        doc.text(`Generated On: ${formatDate(new Date().toISOString())}`, 14, 43);

        // Table containing every college in the filtered report
        const tableRows = reportData.map((item, index) => [
            index + 1,
            item.CollegeName || "---",
            item.CollegeCode || "---",
            item.ApplicationNumber || "---",
            item.CourseLevel || "---",
            item.AffiliationType || "---",
            item.IsSubmitted ? "Submitted" : "Not Submitted",
            formatDate(item.SubmittedDate)
        ]);

        doc.autoTable({
            startY: 49,
            head: [[
                "Sl. No.",
                "College Name",
                "College Code",
                "Application Number",
                "Course Level",
                "Type of Affiliation",
                "Submission Status",
                "Submitted Date"
            ]],
            body: tableRows,
            theme: "grid",
            margin: { left: 10, right: 10 },
            styles: {
                fontSize: 8,
                cellPadding: 2.5,
                overflow: "linebreak",
                valign: "middle"
            },
            headStyles: {
                fillColor: [38, 55, 82],
                textColor: 255,
                fontStyle: "bold"
            },
            columnStyles: {
                0: { cellWidth: 13 },
                1: { cellWidth: 43 },
                2: { cellWidth: 22 },
                3: { cellWidth: 32 },
                4: { cellWidth: 20 },
                5: { cellWidth: 28 },
                6: { cellWidth: 25 },
                7: { cellWidth: 43 }
            },
            didDrawPage: function () {
                const pageCount = doc.internal.getNumberOfPages();

                doc.setFontSize(8);
                doc.text(
                    `Page ${doc.internal.getCurrentPageInfo().pageNumber} of ${pageCount}`,
                    doc.internal.pageSize.getWidth() - 14,
                    doc.internal.pageSize.getHeight() - 7,
                    { align: "right" }
                );
            }
        });

        const dateStamp = new Date().toISOString().slice(0, 10);
        doc.save(`Dental_Submission_Report_${dateStamp}.pdf`);
    });
});
