// ================== MODAL GENÉRICO ==================

// variables globales para que usuarios.js / tareas.js puedan usarlas
window.appModalElement = document.getElementById('appModal');
window.appModalBody = document.getElementById('appModalBody');
window.appModal = appModalElement ? new bootstrap.Modal(appModalElement) : null;

// Delegación para cualquier enlace/botón con .js-modal-link
document.addEventListener('click', async (e) => {
    const link = e.target.closest('.js-modal-link');
    if (!link) return;

    e.preventDefault();

    const url = link.getAttribute('href') || link.dataset.url;
    if (!url) return;

    try {
        const response = await fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();
        appModalBody.innerHTML = html;

        // Re-parse unobtrusive validation en lo que se inyectó
        if (window.jQuery && window.$ && $.validator && $.validator.unobtrusive) {
            $.validator.unobtrusive.parse(appModalBody);
        }

        // Inicializar comportamiento del formulario de USUARIOS si aplica
        if (window.Usuarios && typeof window.Usuarios.initForm === 'function') {
            window.Usuarios.initForm(appModalBody);
        }

        if (window.Tareas && typeof window.Tareas.initForm === 'function') {
            window.Tareas.initForm(appModalBody);
        }

        // Inicializar comportamiento del formulario de SOLICITUDES si aplica
        if (window.Solicitudes && typeof window.Solicitudes.initForm === 'function') {
            window.Solicitudes.initForm(appModalBody);
        }

        appModal?.show();

    } catch (err) {
        console.error(err);
    }
});

// cerrar modal desde enlaces dentro del card
document.addEventListener('click', (e) => {
    const close = e.target.closest('.js-close-modal');
    if (!close) return;
    e.preventDefault();
    appModal?.hide();
});
