
document.addEventListener("DOMContentLoaded", function () {

    const upButton = document.getElementById("caScrollUp");
    const downButton = document.getElementById("caScrollDown");

    // Dental main content scroll container
    const scrollElement = document.querySelector(".ca-shell-dental .ca-body");

    if (!upButton || !downButton || !scrollElement) {
        console.warn("CA scroll elements not found.");
        return;
    }

    // Update scroll button states
    function updateScrollButtons() {

        const currentScroll = scrollElement.scrollTop;
        const maxScroll = scrollElement.scrollHeight - scrollElement.clientHeight;

        upButton.disabled = currentScroll <= 3;
        downButton.disabled = currentScroll >= maxScroll - 3;

        upButton.classList.toggle("is-disabled", upButton.disabled);
        downButton.classList.toggle( "is-disabled", downButton.disabled);
    }

    // Scroll to an element inside ca-body
    function scrollToElement(element, alignment) {

        if (!element) return;

        const containerRect = scrollElement.getBoundingClientRect();
        const elementRect = element.getBoundingClientRect();

        let targetPosition = scrollElement.scrollTop + elementRect.top - containerRect.top;

        if (alignment === "end") {
            targetPosition += elementRect.height - scrollElement.clientHeight;
        }

        scrollElement.scrollTo({
            top: Math.max(0, targetPosition),
            behavior: "smooth"
        });
    }

    // Scroll Up
    upButton.addEventListener("click", function (event) {

        event.preventDefault();

        const header = document.getElementById("affiliation-header-wrapper");

        if (header) {
            scrollToElement(header, "start");
        } else {
            scrollElement.scrollTo({
                top: 0,
                behavior: "smooth"
            });
        }
    });

    // Scroll Down
    downButton.addEventListener("click", function (event) {

        event.preventDefault();

        const submitSection = document.getElementById("submitSection");

        if (submitSection) {
            scrollToElement(submitSection, "end");
        } else {
            scrollElement.scrollBy({
                top: 400,
                behavior: "smooth"
            });
        }
    });

    // Listen to actual content scrolling
    scrollElement.addEventListener(
        "scroll",
        updateScrollButtons,
        { passive: true }
    );

    window.addEventListener("resize", updateScrollButtons);
    window.addEventListener("load", updateScrollButtons);

    // Initial calculation
    updateScrollButtons();

});
