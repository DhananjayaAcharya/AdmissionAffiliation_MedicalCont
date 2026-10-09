async function fetchDentalApplicationReport() {
    const affiliationTypeId =
        document.getElementById("reportAffiliationType").value;

    const courseLevel =
        document.getElementById("reportCourseLevel").value;

    const resultDiv =
        document.getElementById("dentalReportResult");

    const errorDiv =
        document.getElementById("dentalReportError");

    const loadingDiv =
        document.getElementById("dentalReportLoading");

    const button =
        document.getElementById("btnFetchDentalReport");


    // Reset
    resultDiv.classList.add("d-none");
    errorDiv.classList.add("d-none");


    // Validation
    if (!affiliationTypeId) {

        errorDiv.textContent =
            "Please select Type of Affiliation.";

        errorDiv.classList.remove("d-none");

        return;
    }

    if (!courseLevel) {

        errorDiv.textContent =
            "Please select Course Level.";

        errorDiv.classList.remove("d-none");

        return;
    }


    loadingDiv.classList.remove("d-none");
    button.disabled = true;


    try {

        const url =
            dentalApplicationReportUrl
            + '?affiliationTypeId=' + encodeURIComponent(affiliationTypeId)
            + '&courseLevel=' + encodeURIComponent(courseLevel);


        const response = await fetch(url, {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        });


        const data = await response.json();


        if (!response.ok || !data.success) {

            errorDiv.textContent =
                data.message ||
                "Unable to fetch application status.";

            errorDiv.classList.remove("d-none");

            return;
        }


        // -------------------------------------------------
        // Display percentage
        // -------------------------------------------------

        const percentage =
            Number(data.percentage) || 0;


        document.getElementById(
            "reportPercentage"
        ).textContent = percentage + "%";


        document.getElementById(
            "reportProgressBar"
        ).style.width = percentage + "%";

        document.getElementById("reportProgressBar").textContent = percentage+"%";


        document.getElementById(
            "reportStepCount"
        ).textContent =
            `${data.completedSteps} of ${data.totalSteps} steps completed`;


        resultDiv.classList.remove("d-none");

    }
    catch (error) {

        console.error(
            "Dental Application Report:",
            error
        );

        errorDiv.textContent =
            "Unable to fetch application status.";

        errorDiv.classList.remove("d-none");

    }
    finally {

        loadingDiv.classList.add("d-none");
        button.disabled = false;
    }
}

document.getElementById("dentalApplicationReportModal").addEventListener("hidden.bs.modal", function(){
    document.getElementById("reportAffiliationType").value = "";
    document.getElementById("reportCourseLevel").value = "";


    document.getElementById("dentalReportResult").classList.add("d-none");

    document.getElementById("dentalReportError").classList.add("d-none");

    document.getElementById("dentalReportLoading").classList.add("d-none");

    document.getElementById("reportPercentage").textContent = "0%";

    document.getElementById("reportProgressBar").textContent = "0%";

    document.getElementById("reportStepCount").textContent = "0 of 0 steps completed";

    document.getElementById("btnFetchDentalReport").disabled = false;

})