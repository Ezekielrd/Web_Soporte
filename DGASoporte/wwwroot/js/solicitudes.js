(function (window) {
    function initSolicitudAjaxForm(root) {
        root = root || document;

        const form = root.querySelector('form[data-ajax-solicitud="true"]');
        if (!form) return;

        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            // ✅ Validación rápida en cliente
            const txtMotivo = form.querySelector('#motivoRechazo');
            const valor = (txtMotivo?.value || '').trim();

            if (!valor || valor.length < 10) {
                alert('Por favor, ingrese un motivo de rechazo con al menos 10 caracteres.');
                if (txtMotivo) {
                    txtMotivo.focus();
                }
                return; // NO manda el fetch
            }

            const url = form.action;
            const formData = new FormData(form);

            try {
                const response = await fetch(url, {
                    method: 'POST',
                    body: formData,
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                const contentType = response.headers.get('content-type') || '';

                if (contentType.includes('application/json')) {
                    const result = await response.json();

                    if (result.success) {
                        if (window.appModal) {
                            window.appModal.hide();
                        }

                        const solicitudesLink = document.querySelector('.menu-item[data-view="Solicitudes"]');
                        if (solicitudesLink) {
                            solicitudesLink.click();
                        }
                        return;
                    }

                    // ❌ Validación / error desde el servidor
                    alert(result.message || 'No se pudo procesar la solicitud.');
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    }
                    return;
                }

                // Si el servidor devolviera HTML, aquí lo podrías reinyectar
                const html = await response.text();
                const container = window.appModalBody || document.getElementById('appModalBody');
                if (container) {
                    container.innerHTML = html;
                    initSolicitudAjaxForm(container);
                }
            } catch (err) {
                console.error('Error al enviar formulario de solicitud', err);
            }
        });
    }

    function initSolicitudDataTable(root) {
        root = root || document;
        console.log('[Solicitudes] initTablaSolicitud llamado en root:', root);

        if (!$.fn.DataTable) {
            console.error('[Solicitudes] DataTables no está cargado');
            return;
        }

        const tabla = root.querySelector('#tablaSolicitudes');
        if (!tabla) {
            console.warn('[Solicitudes] No se encontró la tabla de solicitudes en este root', root);
            return;
        }

        const $tabla = $(tabla);

        if ($.fn.DataTable.isDataTable($tabla)) {
            $tabla.DataTable().destroy();
        }

        const dt = $tabla.DataTable({
            searching: true,
            paging: true,
            info: true,
            lengthChange: true,
            dom:
                "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            language: {
                url: "https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json",
            },
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50],
            columnDefs: [
                { orderable: false, targets: -1 }
            ]
        });

        console.log('[Solicitudes] DataTable inicializado correctamente');
    }
    function initSolicitudForm(root) {
        initSolicitudAjaxForm(root);
        initSolicitudDataTable(root);

        // Re-parse unobtrusive validation en el contenido del modal
        if (window.jQuery && $.validator && $.validator.unobtrusive) {
            $.validator.unobtrusive.parse(root);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        initSolicitudForm(document);
    });

    window.Solicitudes = window.Solicitudes || {};
    window.Solicitudes.initForm = initSolicitudForm;


})(window);
