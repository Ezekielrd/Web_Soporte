// wwwroot/js/reanudar.js
(function () {
    'use strict';

    const form = document.getElementById('formReanudar');
    const textarea = document.getElementById('motivoReanudar');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        // Limpia mensajes previos
        textarea.classList.remove('is-invalid');
        textarea.setCustomValidity('');

        const texto = textarea.value.trim();

        // Validación opcional: si escribe algo, debe tener al menos 5 caracteres
        if (texto.length > 0 && texto.length < 5) {
            textarea.setCustomValidity('Escribe al menos 5 caracteres.');
            textarea.classList.add('is-invalid');
            e.preventDefault(); // frena el envío
            return;
        }

        // Todo bien: permite enviar
        textarea.classList.remove('is-invalid');
        textarea.setCustomValidity('');
    });
})();