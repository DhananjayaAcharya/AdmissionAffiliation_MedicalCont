console.log('language.js loaded');
function changeLanguage() {
    console.log('changeLanguage called');
    var culture = $('#languageSelect').val();
    var setLanguageUrl = $('#setLanguageUrl').val();
    if (!culture) {
        console.error('Culture not selected');
        Swal.fire({
            title: document.getElementById('languageErrorTitle').value,
            text: document.getElementById('languageErrorMessage').value,
            icon: 'error',
            confirmButtonText: document.getElementById('languageOK').value,
            confirmButtonColor: '#991b1b'
        });
        return;
    }
    if (!setLanguageUrl) {
        console.error('SetLanguage URL not found');
        Swal.fire({
            title: document.getElementById('languageErrorTitle').value,
            text: 'Language change URL is missing.',
            icon: 'error',
            confirmButtonText: document.getElementById('languageOK').value,
            confirmButtonColor: '#991b1b'
        });
        return;
    }

    var returnUrl = window.location.pathname === '/Account/GetStudentData'
        ? '/Account/CSCStudentView'
        : window.location.pathname;

    $.ajax({
        url: setLanguageUrl,
        type: 'POST',
        data: { culture: culture, returnUrl: returnUrl },
        success: function () {
            console.log('Language change successful, redirecting to:', returnUrl);
            window.location.href = returnUrl;
        },
        error: function (xhr, status, error) {
            console.error('Language change failed:', error, xhr.responseText);
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error
                ? xhr.responseJSON.error
                : document.getElementById('languageErrorMessage').value;
            Swal.fire({
                title: document.getElementById('languageErrorTitle').value,
                text: errorMessage,
                icon: 'error',
                confirmButtonText: document.getElementById('languageOK').value,
                confirmButtonColor: '#991b1b'
            });
        }
    });
}