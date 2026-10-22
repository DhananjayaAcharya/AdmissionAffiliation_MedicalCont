document.addEventListener("DOMContentLoaded", function () {
    function initializeTable(tableId, pageSizeId, infoId, paginationId) {
        const table = document.getElementById(tableId);
        const pageSizeSelect = document.getElementById(pageSizeId);
        const infoElement = document.getElementById(infoId);
        const paginationElement = document.getElementById(paginationId);

        if (!table || !pageSizeSelect || !infoElement || !paginationElement) {
            return;
        }

        const tbody = table.querySelector("tbody");
        const headers = Array.from(table.querySelectorAll("thead th[data-sort]"));

        const originalRows = Array.from(tbody.querySelectorAll("tr")).filter(row => !row.querySelector(".adf-empty"));

        let rows = [...originalRows];
        let currentPage = 1;
        let pageSize = parseInt(pageSizeSelect.value, 10) || 10;
        let sortColumn = -1;
        let sortDirection = 1;

        function getCellValue(row, index, type) {
            const cell = row.cells[index];

            if (!cell) return "";

            const rawValue = cell.dataset.value ?? cell.textContent.trim();

            if (type === "number") {
                const number = Number(rawValue);
                return Number.isFinite(number) ? number : 0;
            }

            if (type === "date") {
                const date = Date.parse(rawValue);
                return Number.isNaN(date) ? 0 : date;
            }

            return String(rawValue).toLocaleLowerCase();
        }

        function renderPagination(totalPages) {
            paginationElement.replaceChildren();

            function addButton(label, page, disabled, active) {
                const button = document.createElement("button");
                button.type = "button";
                button.textContent = label;
                button.disabled = disabled;

                if (active) {
                    button.classList.add("active");
                    button.setAttribute("aria-current", "page");
                }

                button.addEventListener("click", function () {
                    currentPage = page;
                    render();
                });

                paginationElement.appendChild(button);
            }

            addButton("‹", Math.max(1, currentPage - 1),currentPage === 1, false);

            let start = Math.max(1, currentPage - 2);
            let end = Math.min(totalPages, currentPage + 2);

            if (end - start < 4) {
                start = Math.max(1, end - 4);
                end = Math.min(totalPages, start + 4);
            }

            for (let page = start; page <= end; page++) {
                addButton(String(page), page, false, page === currentPage);
            }

            addButton("›", Math.min(totalPages, currentPage + 1), currentPage === totalPages, false);
         }

        function render() {
            pageSize = parseInt(pageSizeSelect.value, 10) || 10;

            const total = rows.length;
            const totalPages = Math.max(1, Math.ceil(total / pageSize));

            currentPage = Math.min(currentPage, totalPages);

            const startIndex = (currentPage - 1) * pageSize;
            const endIndex = Math.min(startIndex + pageSize, total);

            const fragment = document.createDocumentFragment();

            rows.forEach(function (row, index) {
                row.style.display = index >= startIndex && index < endIndex ? "" : "none";
                fragment.appendChild(row);
            });

            // Keep the original empty-state row if there are no data rows.
            const emptyRow = Array.from(tbody.querySelectorAll("tr")).find(row => row.querySelector(".adf-empty"));

            if (total === 0 && emptyRow) {
                emptyRow.style.display = "";
                fragment.appendChild(emptyRow);
            }

            tbody.appendChild(fragment);

            infoElement.textContent = total === 0
                ? "(0 entries)"
                : `(${startIndex + 1}–${endIndex} of ${total})`;

            renderPagination(totalPages);
        }

        headers.forEach(function (header, index) {
            header.setAttribute("aria-sort", "none");
            header.title = "Click to sort";

            header.addEventListener("click", function () {
                if (sortColumn === index) {
                    sortDirection *= -1;
                } else {
                    sortColumn = index;
                    sortDirection = 1;
                }

                const type = header.dataset.sort;

                rows.sort(function (a, b) {
                    const first = getCellValue(a, index, type);
                    const second = getCellValue(b, index, type);

                    let result;

                    if (typeof first === "number" &&
                        typeof second === "number") {
                        result = first - second;
                    } else {
                        result = String(first).localeCompare(
                            String(second),
                            undefined,
                            { numeric: true, sensitivity: "base" }
                        );
                    }

                    return result * sortDirection;
                });

                headers.forEach(function (item) {
                    item.setAttribute("aria-sort", "none");

                    const oldArrow = item.querySelector(".adf-sort-arrow");
                    if (oldArrow) oldArrow.remove();
                });

                header.setAttribute("aria-sort", sortDirection === 1 ? "ascending" : "descending");

                const arrow = document.createElement("span");
                arrow.className = "adf-sort-arrow";
                arrow.textContent = sortDirection === 1 ? " ▲" : " ▼";
                header.appendChild(arrow);

                currentPage = 1;
                render();
            });
        });

        pageSizeSelect.addEventListener("change", function () {
            currentPage = 1;
            render();
        });

        render();
     }

    initializeTable( "feeTable", "feePageSize", "feeTableInfo", "feePagination" );

    initializeTable( "paymentTable", "paymentPageSize", "paymentTableInfo", "paymentPagination" );

    document.querySelectorAll(".payment-amount-input").forEach(function (input) {
        input.dataset.originalValue = input.value;

        input.addEventListener("change", function () {
            const newValue = Number(input.value);
            const oldValue = Number(input.dataset.originalValue);

            if (input.value.trim() === "" ||
                !Number.isFinite(newValue) ||
                newValue < 0 ||
                newValue > 999999999999) {
                alert("Please enter a valid payment amount.");
                input.value = input.dataset.originalValue;
                return;
            }

            if (newValue === oldValue) {
                return;
            }

            if (confirm("Save the updated payment amount?")) {
                input.form.requestSubmit();
            } else {
                input.value = input.dataset.originalValue;
            }
        });
    });


    const modal = document.getElementById("updateLateSubmissionFeeModal");

    modal.addEventListener("show.bs.modal", function (event) {

        const button = event.relatedTarget;

        const id = button.getAttribute("data-id");

        const courseLevel = button.getAttribute("data-course-level");

        const activationDate = button.getAttribute("data-activation-date");

        const isActive = button.getAttribute("data-is-active") === "True";

        document.getElementById("LateFeeId").value = id;

        document.getElementById("LateFeeCourseLevel" ).value = courseLevel;

        document.getElementById("LateFeeCourseLevelDisplay").value = courseLevel;

        document.getElementById("LateFeeActivationDate").value = activationDate || "";

        document.getElementById("LateFeeIsActive").checked = isActive;
    });
});

