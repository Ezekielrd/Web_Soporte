// wwwroot/js/pausar.js
(function ($) {
    $('#formPausar').on('submit', function (e) {
        e.preventDefault();

        if (!this.checkValidity()) {
            this.classList.add('was-validated');
            return;
        }

        const url = $('#btnPausar').data('url'); // ← URL desde botón
        const data = $(this).serialize();        // id + motivo + token

        $.post(url, data)
            .done(function () {
                window.location.href = '/Tecnico/Index'; // redirige a Index
            })
            .fail(function () {
                alert('Error al pausar la tarea.');
            });
    });
})(jQuery);