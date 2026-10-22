document.addEventListener("DOMContentLoaded", function () {

    const layout = document.querySelector(".dental-admin-layout");

    const sidebar =
        document.getElementById("dentalSidebar");

    const sidebarToggle =
        document.getElementById("dentalSidebarToggle");

    const sidebarCollapse =
        document.getElementById("dentalSidebarCollapse");

    const overlay =
        document.getElementById("dentalSidebarOverlay");


    /* =====================================================
       HEADER HAMBURGER
       Desktop:
       - Collapse / expand sidebar

       Mobile:
       - Open / close sidebar
    ====================================================== */

    sidebarToggle?.addEventListener("click", function () {

        if (window.innerWidth >= 992) {

            // Desktop
            layout?.classList.toggle("sidebar-collapsed");

        } else {

            // Mobile
            sidebar?.classList.toggle("show");
            overlay?.classList.toggle("show");

        }

    });


    /* =====================================================
       SIDEBAR COLLAPSE BUTTON
       Desktop only
    ====================================================== */

    sidebarCollapse?.addEventListener("click", function () {

        if (window.innerWidth >= 992) {

            layout?.classList.toggle("sidebar-collapsed");

        }

    });


    /* =====================================================
       MOBILE OVERLAY
    ====================================================== */

    overlay?.addEventListener("click", function () {

        sidebar?.classList.remove("show");
        overlay?.classList.remove("show");

    });


    /* =====================================================
       RESET SIDEBAR WHEN RESIZING
    ====================================================== */

    window.addEventListener("resize", function () {

        if (window.innerWidth >= 992) {

            // Remove mobile state
            sidebar?.classList.remove("show");
            overlay?.classList.remove("show");

        }

    });

});