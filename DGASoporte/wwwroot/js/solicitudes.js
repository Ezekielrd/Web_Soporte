// wwwroot/js/solicitudes.js
(function (window) {

    let dtSolicitudes = null;

    // =========== ANIMACIÓN DE CARDS ===========
    function animateNumber(el, to) {
        const from = parseInt(el.textContent || "0", 10) || 0;
        const duration = 300;
        const start = performance.now();

        function step(now) {
            const progress = Math.min((now - start) / duration, 1);
            const value = Math.round(from + (to - from) * progress);
            el.textContent = value;
            if (progress < 1) requestAnimationFrame(step);
        }

        requestAnimationFrame(step);
    }

    function actualizarCards(root) {
        if (!dtSolicitudes) return;
        root = root || document;

        const rows = dtSolicitudes.rows({ filter: 'applied' }).nodes().toArray();

        let total = 0, pen = 0, apr = 0, rec = 0;

        rows.forEach(tr => {
            total++;
            const est = (tr.getAttribute('data-estado') || '').toLowerCase();
            if (est === 'enviada') pen++;
            else if (est === 'aprobada') apr++;
            else if (est === 'rechazada') rec++;
        });

        const cardTotal = root.querySelector('.sol-total .js-sol-num');
        const cardPen = root.querySelector('.sol-pendientes .js-sol-num');
        const cardApr = root.querySelector('.sol-aprobadas .js-sol-num');
        const cardRec = root.querySelector('.sol-rechazadas .js-sol-num');

        if (cardTotal) animateNumber(cardTotal, total);
        if (cardPen) animateNumber(cardPen, pen);
        if (cardApr) animateNumber(cardApr, apr);
        if (cardRec) animateNumber(cardRec, rec);
    }

    // =========== SEMANAS / DÍAS (últimos 30 días) ===========
    function generarSemanas30Dias() {
        const hoy = new Date();
        const inicio = new Date();
        inicio.setDate(hoy.getDate() - 30);

        const semanas = [];
        let cursor = new Date(inicio);

        while (cursor <= hoy) {
            const ini = new Date(cursor);
            const fin = new Date(cursor);
            fin.setDate(fin.getDate() + 6);
            if (fin > hoy) fin.setTime(hoy.getTime());

            semanas.push({
                ini,
                fin,
                label:
                    ini.toLocaleDateString('es-NI', { day: '2-digit', month: 'short' }) +
                    ' – ' +
                    fin.toLocaleDateString('es-NI', { day: '2-digit', month: 'short' })
            });

            cursor.setDate(cursor.getDate() + 7);
        }

        return semanas;
    }

    function cargarSemanas(root) {
        root = root || document;
        const selSemana = root.querySelector('#filtroSemana');
        if (!selSemana) return;

        selSemana.innerHTML = '';

        // 👉 Opción "Todas las semanas"
        const optTodas = document.createElement('option');
        optTodas.value = '';
        optTodas.textContent = 'Todas las semanas';
        selSemana.appendChild(optTodas);

        const semanas = generarSemanas30Dias();
        semanas.forEach((s, idx) => {
            const opt = document.createElement('option');
            opt.value = String(idx);
            opt.textContent = s.label; // "01 ene – 07 ene"
            selSemana.appendChild(opt);
        });

        cargarDias(root);
    }


    function cargarDias(root) {
        root = root || document;
        const selSemana = root.querySelector('#filtroSemana');
        const selDia = root.querySelector('#filtroDia');
        if (!selSemana || !selDia) return;

        selDia.innerHTML = '';

        // Siempre agregamos "Todos los días"
        const optTodos = document.createElement('option');
        optTodos.value = '';
        optTodos.textContent = 'Todos los días';
        selDia.appendChild(optTodos);

        const semanas = generarSemanas30Dias();
        const valueSemana = selSemana.value;

        // Si es "Todas las semanas" -> solo queda "Todos los días"
        if (!valueSemana) {
            selDia.value = '';
            return;
        }

        const index = parseInt(valueSemana, 10);
        const semana = semanas[index];
        if (!semana) return;

        let cursor = new Date(semana.ini);

        while (cursor <= semana.fin) {
            const opt = document.createElement('option');
            opt.value = cursor.toISOString().substring(0, 10); // yyyy-MM-dd
            opt.textContent = cursor.toLocaleDateString('es-NI', {
                weekday: 'short',
                day: '2-digit',
                month: 'short'
            });
            selDia.appendChild(opt);
            cursor.setDate(cursor.getDate() + 1);
        }

        // Después de cambiar la semana, dejamos seleccionado "Todos los días"
        selDia.value = '';
    }

    function initSolicitudFiltros(root) {
        root = root || document;

        const selSemana = root.querySelector('#filtroSemana');
        const selDia = root.querySelector('#filtroDia');

        if (!selSemana || !selDia) {
            console.log('[Solicitudes] No se encontraron filtros Semana/Día en este root');
            return;
        }

        // Generar semanas y días iniciales
        cargarSemanas(root);
    }

    // =========== DATATABLE + FILTROS ===========
    function initSolicitudDataTable(root) {
        root = root || document;
        console.log('[Solicitudes] initSolicitudDataTable en root:', root);

        if (!window.jQuery || !$.fn.DataTable) {
            console.error('[Solicitudes] DataTables no está cargado');
            return;
        }

        const tabla = root.querySelector('#tablaSolicitudes');
        if (!tabla) {
            console.warn('[Solicitudes] No se encontró #tablaSolicitudes en este root');
            return;
        }

        const $tabla = $(tabla);

        if ($.fn.DataTable.isDataTable($tabla)) {
            $tabla.DataTable().destroy();
        }

        dtSolicitudes = $tabla.DataTable({
            searching: true,
            paging: true,
            info: true,
            lengthChange: true,
            dom:
                "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            language: {
                url: "https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json"
            },
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50],
            columnDefs: [
                { orderable: false, targets: -1 }
            ],
            order: []
        });

        console.log('[Solicitudes] DataTable inicializado correctamente');

        // === Filtro global solo una vez, igual que en tareas.js ===
        if (!window.SolicitudesFilterRegistered) {
            window.SolicitudesFilterRegistered = true;

            $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
                if (settings.nTable.id !== 'tablaSolicitudes') return true;

                const rowData = settings.aoData[dataIndex];
                if (!rowData) return true;

                const row = rowData.nTr;
                if (!row) return true;

                const estadoRow = (row.getAttribute('data-estado') || '').toLowerCase();
                const fechaRow = row.getAttribute('data-fecha-creacion') || ''; // yyyy-MM-dd

                const estadoSel = ($('#filtroEstado').val() || '').toLowerCase();
                const diaSel = $('#filtroDia').val() || '';
                const semanaSel = $('#filtroSemana').val() || '';

                // ---- Filtro por Estado ----
                if (estadoSel && estadoRow !== estadoSel) return false;

                // ---- Filtro por Día (si se eligió un día concreto manda sobre la semana) ----
                if (diaSel) {
                    if (fechaRow !== diaSel) return false;
                    return true;
                }

                // ---- Filtro por Semana (si hay semana seleccionada y no hay día) ----
                if (semanaSel) {
                    const idx = parseInt(semanaSel, 10);
                    const semanas = generarSemanas30Dias();
                    const s = semanas[idx];
                    if (s) {
                        const pad = n => n.toString().padStart(2, '0');
                        const iniStr = `${s.ini.getFullYear()}-${pad(s.ini.getMonth() + 1)}-${pad(s.ini.getDate())}`;
                        const finStr = `${s.fin.getFullYear()}-${pad(s.fin.getMonth() + 1)}-${pad(s.fin.getDate())}`;

                        // fechaRow está en formato yyyy-MM-dd también, así que podemos comparar cadenas
                        if (fechaRow < iniStr || fechaRow > finStr) return false;
                    }
                }

                return true;
            });


            // Handlers delegados, como en tareas.js
            const redibujar = () => {
                const tablaInst = $('#tablaSolicitudes').DataTable();
                if (tablaInst) tablaInst.draw();
                actualizarCards(document.getElementById('view-container') || document);
            };

            // Cuando cambia estado o día → redibujar
            $(document).on('change', '#filtroEstado, #filtroDia', redibujar);

            // Cuando cambia semana → recalcular días y luego redibujar
            $(document).on('change', '#filtroSemana', function () {
                const rootContainer = document.getElementById('view-container') || document;
                cargarDias(rootContainer);
                redibujar();
            });
        }

        // Primera actualización
        dtSolicitudes.draw();
        actualizarCards(root);
    }

    // =========== AJAX FORM (tu código) ===========
    function initSolicitudAjaxForm(root) {
        root = root || document;

        const form = root.querySelector('form[data-ajax-solicitud="true"]');
        if (!form) return;

        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            const txtMotivo = form.querySelector('#motivoRechazo');
            const valor = (txtMotivo?.value || '').trim();

            if (!valor || valor.length < 10) {
                alert('Por favor, ingrese un motivo de rechazo con al menos 10 caracteres.');
                if (txtMotivo) txtMotivo.focus();
                return;
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
                        if (window.appModal) window.appModal.hide();

                        const solicitudesLink = document.querySelector('.menu-item[data-view="Solicitudes"]');
                        if (solicitudesLink) solicitudesLink.click();
                        return;
                    }

                    alert(result.message || 'No se pudo procesar la solicitud.');
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    }
                    return;
                }

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

    // =========== INIT GLOBAL TIPO tareas.js ===========
    function initSolicitudForm(root) {
        root = root || document;

        initSolicitudAjaxForm(root);
        initSolicitudDataTable(root);
        initSolicitudFiltros(root);

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
