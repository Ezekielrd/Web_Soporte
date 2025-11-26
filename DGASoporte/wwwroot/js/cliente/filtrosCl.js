document.addEventListener('DOMContentLoaded', function () {

    function initSolicitudDataTable() {
        const $tabla = $('#tablaSolicitud');

        if (!$.fn.DataTable) {
            console.error('DataTables no está cargado');
            return;
        }

        if ($.fn.DataTable.isDataTable($tabla)) {
            return;
        }

        window.tablaSolicitudes = $tabla.DataTable({
            pageLength: 10,
            lengthChange: true,
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            }
        });
    }

    initSolicitudDataTable();
    const tabla = window.tablaSolicitudes;
    if (!tabla) return;

    const $estado = $('#estado');
    const $tipo = $('#tipoId');
    const $chkArch = $('#chkArch');

    // 🔹 Filtro personalizado
    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
        if (settings.nTable.id !== 'tablaSolicitud') return true;

        const textoTipoCol = (data[3] || '').toString().toLowerCase();
        const textoEstadoCol = (data[4] || '').toString().toLowerCase();

        const incluirArchivadas = $chkArch.prop('checked');

        // TEXTO de los options seleccionados (no el value)
        const estadoTextoFiltro = $estado.find('option:selected').text().trim().toLowerCase();
        const tipoTextoFiltro = $tipo.find('option:selected').text().trim().toLowerCase();

        // 1) Archivadas
        if (!incluirArchivadas && textoEstadoCol.includes('archivada')) {
            return false;
        }

        // 2) Estado (si NO es "Todos los estados")
        if ($estado.val()) { // tiene algo distinto de ""
            // ejemplo: estadoTextoFiltro = "enviada"
            if (!textoEstadoCol.includes(estadoTextoFiltro)) {
                return false;
            }
        }

        // 3) Tipo (si NO es "Todos los tipos")
        if ($tipo.val()) {
            if (!textoTipoCol.includes(tipoTextoFiltro)) {
                return false;
            }
        }

        return true;
    });

    function aplicarFiltros() {
        tabla.draw();
    }

    // Se filtra automáticamente al cambiar
    $estado.on('change', aplicarFiltros);
    $tipo.on('change', aplicarFiltros);
    $chkArch.on('change', aplicarFiltros);
});
