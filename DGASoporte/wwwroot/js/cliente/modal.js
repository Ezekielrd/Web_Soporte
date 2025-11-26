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

        // Título y subtítulo del modal en modo detalle
        const label = document.getElementById('modalConfirmacionLabel');
        const subtitulo = document.getElementById('modalSubtitulo');
        if (label) label.textContent = 'Detalle de la Solicitud';
        if (subtitulo) subtitulo.textContent = 'Información de la solicitud seleccionada:';

        modal.show();
    });
});
