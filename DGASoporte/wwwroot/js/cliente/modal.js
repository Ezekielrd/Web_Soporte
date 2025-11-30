document.addEventListener('DOMContentLoaded', function () {
    const modalEl = document.getElementById('modalConfirmacion');
    if (!modalEl) return;

    const modal = new bootstrap.Modal(modalEl);

    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.js-detalle-solicitud');
        if (!btn) return;

        // Rellenar el modal con los data-* del botón
        document.getElementById('modalTitulo').textContent = btn.dataset.titulo || '';
        document.getElementById('modalDescripcion').textContent = btn.dataset.descripcion || '';
        document.getElementById('modalUnidad').textContent = btn.dataset.unidad || '';
        document.getElementById('modalTipo').textContent = btn.dataset.tipo || '';
        document.getElementById('modalEstado').textContent = btn.dataset.estado || '';

        const label = document.getElementById('modalConfirmacionLabel');
        const subtitulo = document.getElementById('modalSubtitulo');
        if (label) label.textContent = 'Detalle de la Solicitud';
        if (subtitulo) subtitulo.textContent = 'Información de la solicitud seleccionada:';

        modal.show();
    });

    document.addEventListener('DOMContentLoaded', function () {
        const params = new URLSearchParams(window.location.search);
        const solicitudId = params.get('solicitudId');

        if (solicitudId) {
            // Abrimos directamente el modal al entrar desde la notificación
            abrirDetalleSolicitud(solicitudId);
        }
    });
});
