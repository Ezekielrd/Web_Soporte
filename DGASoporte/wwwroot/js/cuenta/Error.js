
document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('loginForm');
    if (!form) return;

    var VISIBLE_MS = 3000; // <- tiempo visible

    function hideMessages() {
        document.querySelectorAll('[data-valmsg-for], .errors').forEach(function (el) {
            if (!el.textContent.trim()) return;
            setTimeout(function () {
                el.classList.add('fade-out');
                setTimeout(function () {
                    // si no quieres borrar el texto, comenta estas tres líneas
                    el.textContent = '';
                    el.classList.remove('field-validation-error');
                    el.classList.add('field-validation-valid');
                    el.classList.remove('fade-out');
                }, 500);
            }, VISIBLE_MS);
        });
    }

    form.addEventListener('submit', function () {
        setTimeout(hideMessages, 0);
    });
});
