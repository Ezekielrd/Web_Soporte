// wwwroot/js/tareas.js
(function (window) {

    function initTareaWizard(root) {
        root = root || document;

        var form = root.querySelector('#wizardForm');
        if (!form) return;

        var stepPanels = root.querySelectorAll('.form-step');
        var stepIndicators = root.querySelectorAll('[data-step-indicator]');
        var btnNext = root.querySelector('#btnNext');
        var btnPrev = root.querySelector('#btnPrev');
        var btnSubmit = root.querySelector('#btnSubmit');

        var currentStep = 1;
        var totalSteps = stepPanels.length;

        function showStep(step) {
            currentStep = step;

            stepPanels.forEach(function (panel) {
                var num = parseInt(panel.getAttribute('data-step'));
                panel.classList.toggle('active', num === step);
            });

            stepIndicators.forEach(function (ind) {
                var num = parseInt(ind.getAttribute('data-step-indicator'));
                ind.classList.toggle('active', num === step);
                ind.classList.toggle('completed', num < step);
            });

            if (btnPrev) btnPrev.classList.toggle('d-none', step === 1);
            if (btnNext) btnNext.classList.toggle('d-none', step === totalSteps);
            if (btnSubmit) btnSubmit.classList.toggle('d-none', step !== totalSteps);

            if (step === totalSteps) {
                actualizarResumen(root);
            }
        }

        function setError(input, message) {
            input.classList.add('is-invalid');
            input.classList.remove('is-valid');

            if (!input.name) return;

            var selector = '[data-valmsg-for="' + input.name.replace('.', '_') + '"]';
            var span = form.querySelector(selector);
            if (!span) return;

            span.classList.remove('field-validation-valid');
            span.classList.add('field-validation-error');
            span.textContent = message || '';
        }

        function clearError(input) {
            input.classList.remove('is-invalid');
            input.classList.remove('is-valid'); // 👈 para que no se pinte verde si no toca

            if (!input.name) return;

            var selector = '[data-valmsg-for="' + input.name.replace('.', '_') + '"]';
            var span = form.querySelector(selector);
            if (!span) return;

            span.classList.remove('field-validation-error');
            span.classList.add('field-validation-valid');
            span.textContent = '';
        }

        function validarStepActual() {
            var panel = root.querySelector('.form-step[data-step="' + currentStep + '"]');
            if (!panel) return true;

            var inputs = panel.querySelectorAll('input, select, textarea');
            var valid = true;

            // Categoría / TipoServicio
            var ddlCategoria = root.querySelector('#CategoriaServicioId') || root.querySelector('#CategoriaId');
            var ddlTipoServicio = root.querySelector('#TipoServicioId');
            var ID_CATEGORIA_GENERAL = '5';

            // División / Unidad
            var ddlDivision = root.querySelector('#DivisionId');
            var ddlUnidad = root.querySelector('#UnidadId');

            // Usuario (hidden)
            var hiddenUsuario = root.querySelector('#UsuarioId');

            inputs.forEach(function (input) {

                var isTipoServicio = ddlTipoServicio &&
                    (input === ddlTipoServicio ||
                        input.id === 'TipoServicioId' ||
                        input.name === 'TipoServicioId' ||
                        (input.name && input.name.endsWith('.TipoServicioId')));

                var isDivision = ddlDivision && (input === ddlDivision ||
                    input.id === 'DivisionId' ||
                    input.name === 'DivisionId' ||
                    (input.name && input.name.endsWith('.DivisionId')));

                var isUsuario = hiddenUsuario && (
                    input === hiddenUsuario ||
                    input.id === 'UsuarioId' ||
                    input.name === 'UsuarioId' ||
                    (input.name && input.name.endsWith('.UsuarioId'))
                );

                // IGNORAR si está deshabilitado/oculto, excepto TipoServicio, División y UsuarioId
                if (!isTipoServicio && !isDivision && !isUsuario && input.disabled) {
                    clearError(input);
                    return;
                }

                if (!isTipoServicio && !isDivision && !isUsuario && input.offsetParent === null) {
                    clearError(input);
                    return;
                }

                var requeridoMsg = input.getAttribute('data-val-required');

                // Si no tiene required → nada que validar
                if (!requeridoMsg) {
                    clearError(input);
                    return;
                }

                // ===========================
                // USUARIO SOLICITANTE (hidden)
                // ===========================
                if (isUsuario) {
                    var msgUsuario = requeridoMsg || 'Debe elegir el usuario';
                    var valU = input.value;

                    if (!valU || valU.trim() === '' || valU === '0') {
                        valid = false;
                        setError(input, msgUsuario);
                    } else {
                        clearError(input);
                    }
                    return;
                }

                // ===========================
                // TIPO SERVICIO (regla especial)
                // ===========================
                if (isTipoServicio) {
                    if (!ddlCategoria) {
                        var valTS = input.value;
                        if (!valTS || valTS.trim() === '') {
                            valid = false;
                            setError(input, requeridoMsg);
                        } else {
                            clearError(input);
                        }
                        return;
                    }

                    var catVal = ddlCategoria.value;

                    if (!catVal) {
                        ddlTipoServicio.disabled = true;
                        ddlTipoServicio.classList.remove('is-invalid', 'is-valid');

                        if (ddlTipoServicio.name) {
                            var selSpanId = ddlTipoServicio.name.replace('.', '_');
                            var spanSel = form.querySelector('[data-valmsg-for="' + selSpanId + '"]');
                            if (spanSel) {
                                spanSel.classList.remove('field-validation-error');
                                spanSel.classList.add('field-validation-valid');
                                spanSel.textContent = '';
                            }
                        }
                        return;
                    }

                    if (catVal === ID_CATEGORIA_GENERAL) {
                        ddlTipoServicio.disabled = true;
                        ddlTipoServicio.classList.remove('is-invalid', 'is-valid');

                        if (ddlTipoServicio.name) {
                            var spanIdGen = ddlTipoServicio.name.replace('.', '_');
                            var spanGen = form.querySelector('[data-valmsg-for="' + spanIdGen + '"]');
                            if (spanGen) {
                                spanGen.classList.remove('field-validation-error');
                                spanGen.classList.add('field-validation-valid');
                                spanGen.textContent = '';
                            }
                        }
                        return;
                    }

                    ddlTipoServicio.disabled = false;

                    if (!ddlTipoServicio.value || ddlTipoServicio.value === '') {
                        valid = false;
                        setError(ddlTipoServicio, requeridoMsg);
                        return;
                    } else {
                        clearError(ddlTipoServicio);
                        return;
                    }
                }

                // ===========================
                // DIVISIÓN vs UNIDAD (especial)
                // ===========================
                if (isDivision) {
                    if (!ddlUnidad) {
                        var divValSimple = input.value;
                        if (!divValSimple || divValSimple.trim() === '') {
                            valid = false;
                            setError(input, requeridoMsg);
                        } else {
                            clearError(input);
                        }
                        return;
                    }

                    var unidadOpt = ddlUnidad.options[ddlUnidad.selectedIndex] || null;
                    var unidadTieneDivision = unidadOpt && unidadOpt.dataset
                        ? (unidadOpt.dataset.division || '').toString()
                        : '';

                    if (unidadOpt && !unidadTieneDivision) {
                        clearError(input);
                        return;
                    }

                    if (!unidadOpt || !ddlUnidad.value) {
                        clearError(input);
                        return;
                    }

                    var divVal = input.value;
                    if (!divVal || divVal.trim() === '') {
                        valid = false;
                        setError(input, requeridoMsg);
                    } else {
                        clearError(input);
                    }
                    return;
                }

                // ===========================
                // VALIDACIÓN NORMAL
                // ===========================
                var valor = input.value;
                if (!valor || valor.trim() === '') {
                    valid = false;
                    setError(input, requeridoMsg);
                } else {
                    clearError(input);
                }
            });

            return valid;
        }




        function actualizarResumen(rootResumen) {
            function q(sel) { return rootResumen.querySelector(sel); }

            var r = {
                titulo: q('#resumenTitulo'),
                descripcion: q('#resumenDescripcion'),
                prioridad: q('#resumenPrioridad'),
                categoria: q('#resumenCategoria'),
                unidad: q('#resumenUnidad'),
                tecnico: q('#resumenTecnico'),
                estado: q('#resumenEstado'),
                fechaLimite: q('#resumenFechaLimite'),
                fechaAsignacion: q('#resumenFechaAsignacion')
            };

            var f = {
                titulo: q('#Titulo'),
                descripcion: q('#Descripcion'),
                prioridad: q('#Prioridad'),
                categoria: q('#CategoriaId'),
                unidad: q('#UnidadId'),
                tecnico: q('#TecnicoId'),
                estado: q('#Estado'),
                fechaLimite: q('#FechaLimite'),
                fechaAsignacion: q('#FechaAsignacion')
            };

            if (r.titulo) {
                r.titulo.innerText = (f.titulo && f.titulo.value) ? f.titulo.value : '---';
            }
            if (r.descripcion) {
                r.descripcion.innerText = (f.descripcion && f.descripcion.value) ? f.descripcion.value : '---';
            }
            if (r.prioridad) {
                if (f.prioridad && f.prioridad.selectedIndex >= 0) {
                    r.prioridad.innerText = f.prioridad.options[f.prioridad.selectedIndex].text;
                } else {
                    r.prioridad.innerText = '---';
                }
            }
            if (r.categoria) {
                if (f.categoria && f.categoria.selectedIndex >= 0) {
                    r.categoria.innerText = f.categoria.options[f.categoria.selectedIndex].text;
                } else {
                    r.categoria.innerText = '---';
                }
            }
            if (r.unidad) {
                if (f.unidad && f.unidad.selectedIndex >= 0) {
                    r.unidad.innerText = f.unidad.options[f.unidad.selectedIndex].text;
                } else {
                    r.unidad.innerText = '---';
                }
            }
            if (r.tecnico) {
                if (f.tecnico && f.tecnico.value && f.tecnico.selectedIndex >= 0) {
                    r.tecnico.innerText = f.tecnico.options[f.tecnico.selectedIndex].text;
                } else {
                    r.tecnico.innerText = 'Sin técnico';
                }
            }
            if (r.estado) {
                if (f.estado && f.estado.selectedIndex >= 0) {
                    r.estado.innerText = f.estado.options[f.estado.selectedIndex].text;
                } else {
                    r.estado.innerText = '---';
                }
            }
            if (r.fechaLimite) {
                r.fechaLimite.innerText = (f.fechaLimite && f.fechaLimite.value) ? f.fechaLimite.value : '---';
            }
            if (r.fechaAsignacion) {
                r.fechaAsignacion.innerText = (f.fechaAsignacion && f.fechaAsignacion.value) ? f.fechaAsignacion.value : '---';
            }
        }

        if (btnNext) {
            btnNext.addEventListener('click', function () {
                if (!validarStepActual()) return;
                if (currentStep < totalSteps) {
                    showStep(currentStep + 1);
                }
            });
        }

        if (btnPrev) {
            btnPrev.addEventListener('click', function () {
                if (currentStep > 1) {
                    showStep(currentStep - 1);
                }
            });
        }

        showStep(1);
    }


    function initTareaAjaxForm(root) {
        // Solo necesitamos registrar una vez el handler global
        if (window.__tareaAjaxBound) return;
        window.__tareaAjaxBound = true;

        document.addEventListener('submit', async function (e) {
            const form = e.target;

            // Solo nos interesan los formularios marcados como data-ajax-tarea="true"
            if (!(form instanceof HTMLFormElement)) return;
            if (!form.matches('form[data-ajax-tarea="true"]')) return;

            e.preventDefault();

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

                        const tareasLink = document.querySelector('.menu-item[data-view="Tareas"]');
                        if (tareasLink) {
                            tareasLink.click();
                        }
                        return;
                    } else {
                        if (result.message) {
                            alert(result.message);
                        }
                        return;
                    }
                }

                // HTML con errores de validación
                const html = await response.text();
                const container = window.appModalBody || document.getElementById('appModalBody');

                if (container) {
                    container.innerHTML = html;

                    // 🔁 Re-inicializamos todo lo de la parcial
                    initTareaWizard(container);
                    initTareaAjaxForm(container);
                    initTareaCTS(container);
                    initTareaTipoServicio(container);
                    initDivisionUnidad(container);
                    initAutocompleteUsuarios(container); // 👈 AQUÍ

                    if (window.jQuery && $.validator && $.validator.unobtrusive) {
                        $.validator.unobtrusive.parse(container);
                    }
                }
            } catch (err) {
                console.error('Error al enviar formulario de tarea', err);
            }
        });
    }


    function initTareaCTS(root) {
        root = root || document;

        // Intentamos primero CategoriaServicioId, si no existe usamos CategoriaId
        const ddlCategoria = root.querySelector('#CategoriaServicioId') || root.querySelector('#CategoriaId');
        const ddlTipo = root.querySelector('#TipoServicioId');
        const grupoTipo = root.querySelector('#grupoTipoServicio');

        if (!ddlCategoria || !ddlTipo || !grupoTipo) {
            return; // esta vista no tiene esos campos
        }

        // Guarda todas las opciones originales (menos la primera de "Seleccione...")
        const opcionDefault = ddlTipo.querySelector('option[value=""]');
        const todasOpcionesTipo = Array.from(
            ddlTipo.querySelectorAll('option[data-categoria]')
        );

        // Id de la categoría GENERAL
        const ID_CATEGORIA_GENERAL = "5"; // ⚠️ AJUSTAR AL ID REAL

        function limpiarErroresTipoServicio() {
            // limpiar span de campo
            if (ddlTipo.name) {
                const spanId = ddlTipo.name.replace('.', '_');
                const span = root.querySelector('[data-valmsg-for="' + spanId + '"]');
                if (span) {
                    span.classList.remove('field-validation-error');
                    span.classList.add('field-validation-valid');
                    span.textContent = '';
                }
            }

            // limpiar summary (li que contenga "Tipo de Servicio")
            const summaryUl = root.querySelector('div[data-valmsg-summary="true"] ul');
            if (summaryUl) {
                const items = Array.from(summaryUl.querySelectorAll('li'));
                items.forEach(li => {
                    if (li.textContent && li.textContent.toLowerCase().includes('tipo de servicio')) {
                        li.remove();
                    }
                });
            }

            ddlTipo.classList.remove('is-invalid', 'is-valid');
        }

        function actualizarOpcionesTipo(categoriaId) {
            // Limpia el select y deja solo la opción por defecto
            ddlTipo.innerHTML = '';
            if (opcionDefault) {
                ddlTipo.appendChild(opcionDefault.cloneNode(true));
            }

            // Filtra las opciones por categoría
            const filtradas = todasOpcionesTipo.filter(opt =>
                opt.dataset.categoria === categoriaId
            );

            filtradas.forEach(opt => {
                ddlTipo.appendChild(opt.cloneNode(true));
            });

            // Reinicia selección
            ddlTipo.value = "";
        }

        function actualizarTipoServicio() {
            const valorCategoria = ddlCategoria.value;

            // Sin categoría seleccionada → tipo visible pero deshabilitado
            if (!valorCategoria) {
                ddlTipo.disabled = true;
                ddlTipo.value = "";
                grupoTipo.classList.remove('d-none');
                limpiarErroresTipoServicio();
                return;
            }

            // Categoría GENERAL → ocultar / deshabilitar y limpiar errores
            if (valorCategoria === ID_CATEGORIA_GENERAL) {
                ddlTipo.disabled = true;
                ddlTipo.value = "";
                grupoTipo.classList.add('d-none');
                limpiarErroresTipoServicio();
            } else {
                // Cualquier otra categoría → mostrar y filtrar
                ddlTipo.disabled = false;
                grupoTipo.classList.remove('d-none');
                actualizarOpcionesTipo(valorCategoria);
                // al cambiar de categoría, el tipo vuelve a estar vacío; limpiamos error visual
                limpiarErroresTipoServicio();
            }
        }

        // Al cargar la parcial
        actualizarTipoServicio();

        // Al cambiar la categoría
        ddlCategoria.addEventListener('change', actualizarTipoServicio);
    }

    function initTareaTipoServicio(root) {
        root = root || document;

        const sel = root.querySelector('#TipoServicioId');
        if (!sel) {
            console.log('[Tareas] No se encontró #TipoServicioId en este root', root);
            return;
        }

        const scriptMap = root.querySelector('#tipoServicio-map');
        if (!scriptMap) {
            console.log('[Tareas] No se encontró #tipoServicio-map dentro del root');
            return;
        }

        let map;
        try {
            map = JSON.parse(scriptMap.textContent);
        } catch (e) {
            console.error('[Tareas] Error parseando tipoServicio-map', e, scriptMap.textContent);
            return;
        }

        if (!map) {
            console.log('[Tareas] Mapa de tipo servicio vacío o nulo');
            return;
        }

        console.log('[Tareas] initTareaTipoServicio OK, select y mapa encontrados', { map });

        // 👉 1) Poner title en cada <option>
        Array.from(sel.options).forEach(opt => {
            const val = opt.value;
            if (!val) {
                opt.title = 'Seleccione un tipo de servicio';
            } else if (map[val]) {
                opt.title = map[val]; // descripción
            }
        });

        // 👉 2) Mantener también el title del <select> para cuando esté cerrado
        function actualizarTitle() {
            const value = sel.value;
            if (!value) {
                sel.title = 'Seleccione un tipo de servicio';
                return;
            }

            const desc = map[value] || '';
            sel.title = desc;
        }

        sel.addEventListener('change', actualizarTitle);
        actualizarTitle(); // valor inicial
    }


    async function initTareaDeleteForm(root) {
        root = root || document;

        const form = root.querySelector('form[data-ajax-delete-tarea="true"]');
        if (!form) {
            console.log('[Tareas] No se encontró form[data-ajax-delete-tarea="true"] en', root);
            return;
        }

        console.log('[Tareas] initTareaDeleteForm: enganchando submit en', form);

        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            if (!confirm('¿Seguro que desea eliminar esta tarea?')) return;

            const url = form.action;
            const formData = new FormData(form);

            try {
                const response = await fetch(url, {
                    method: 'POST',
                    body: formData,
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                const result = await response.json();

                if (result.success) {
                    if (window.appModal) {
                        window.appModal.hide();
                    }

                    // aquí tu js global ya hace tareasLink.click()
                    const tareasLink = document.querySelector('.menu-item[data-view="Tareas"]');
                    if (tareasLink) {
                        tareasLink.click();
                    }

                    return;
                } else {
                    alert(result.message || 'No se pudo eliminar la tarea.');
                }
            } catch (err) {
                console.error('Error al eliminar tarea', err);
                alert('Error al eliminar la tarea.');
            }
        });
    }

    function initTareaDataTable(root) {
        root = root || document;
        console.log('[Tareas] initTablaTareas llamado en root:', root);

        if (!window.jQuery || !$.fn.DataTable) {
            console.error('[Tareas] DataTables no está cargado');
            return;
        }

        const tabla = root.querySelector('#tablaTareas');
        if (!tabla) {
            console.warn('[Tareas] No se encontró la tabla de tareas en este root', root);
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
                url: "https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json"
            },
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50],
            columnDefs: [
                { orderable: false, targets: -1 }
            ]
        });

        console.log('[Tareas] DataTable inicializado correctamente');

        dt.on('draw', function () {
            actualizarResumenTareas(root, dt);
        });

        // ============= FILTRO GLOBAL UNA SOLA VEZ =============
        if (!window.TareasFilterRegistered) {
            window.TareasFilterRegistered = true;
            window.TareasQuickFilter = 'all'; // Todas por defecto

            $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
                // Solo aplicamos a tablaTareas
                if (settings.nTable.id !== 'tablaTareas') return true;

                const rowData = settings.aoData[dataIndex];
                if (!rowData) return true;

                const row = rowData.nTr;
                if (!row) return true;

                // ====== selects actuales ======
                const semanaSel = $('#filterSemana').val() || ''; // '' = todas
                const diaSel = $('#filterDia').val() || '';       // '' = todos
                const tecSel = $('#filterTecnico').val() || '';
                const estSel = $('#filterEstado').val() || '';
                const priSel = $('#filterPrioridad').val() || '';
                const catSel = $('#filterCategoria').val() || '';
                const quick = window.TareasQuickFilter || 'all';

                // ====== datos de fila ======
                const tecRow = (row.getAttribute('data-tecnico') || '').toString();
                const estRow = (row.getAttribute('data-estado') || '').toString();
                const priRow = (row.getAttribute('data-prioridad') || '').toString();
                const catRow = (row.getAttribute('data-categoria') || '').toString();
                const vencida = (row.getAttribute('data-vencida') || 'false').toLowerCase();

                // ✅ Fecha en ISO (yyyy-MM-dd) en tu <tr data-fecha="...">
                const rowISO = (row.getAttribute('data-fecha') || '').toString();

                // ---- filtros por selects ----
                if (tecSel && tecRow !== tecSel) return false;
                if (estSel && estRow !== estSel) return false;
                if (priSel && priRow !== priSel) return false;
                if (catSel && catRow !== catSel) return false;

                // ---- quick filters ----
                if (quick === 'vencidas' && vencida !== 'true') return false;
                if (quick === 'sin-asignar' && tecRow) return false;

                if (quick === 'hoy') {
                    if (!rowISO) return false;
                    const hoy = new Date();
                    const yyyy = hoy.getFullYear();
                    const mm = ('0' + (hoy.getMonth() + 1)).slice(-2);
                    const dd = ('0' + hoy.getDate()).slice(-2);
                    const hoyStr = `${yyyy}-${mm}-${dd}`;
                    if (rowISO !== hoyStr) return false;
                }

                // ---- filtro por día/semana (FechaCreacion) ----
                if (!rowISO) return false; // si no hay fecha, no puede pasar
                const rowDate = new Date(rowISO + 'T00:00:00');

                // Día exacto
                if (diaSel) {
                    return rowISO === diaSel;
                }

                // Semana (rango). '' = todas las semanas => pasa
                if (semanaSel !== '') {
                    if (!window.__tareasWeeks || !window.__tareasWeeks.length) return false;

                    const w = window.__tareasWeeks[parseInt(semanaSel, 10)];
                    if (!w) return false;

                    if (rowDate < w.start || rowDate > w.end) return false;
                }

                return true;
            });


            // ============= HANDLERS DELEGADOS (para recarga parcial) =============

            const redibujar = () => {
                const tablaInst = $('#tablaTareas').DataTable();
                if (tablaInst) tablaInst.draw();
            };

            // Selects (incluyendo unidad)
            $(document).on('change',
                '#filterTecnico, #filterEstado, #filterPrioridad, #filterCategoria',
                redibujar);

            // Quick filters
            $(document).on('click', '.quick-filter-btn', function () {
                $('.quick-filter-btn').removeClass('active');
                $(this).addClass('active');

                window.TareasQuickFilter = $(this).data('filter') || 'all';
                redibujar();
            });
        }

        // Por si hay filtros ya seleccionados
        dt.draw();
    }

    function actualizarResumenTareas(root, dt) {
        root = root || document;

        // Obtener instancia de DataTables
        const api = dt || ($('#tablaTareas').DataTable ? $('#tablaTareas').DataTable() : null);
        if (!api) return;

        // Filas visibles (filtro aplicado)
        const rows = api.rows({ filter: 'applied' }).nodes().toArray();

        let total = 0;
        let enEspera = 0;
        let enProceso = 0;
        let resuelta = 0;

        const mapTecnicos = {}; // { id: { asignadas, pendientes, completadas } }

        rows.forEach(function (row) {
            const tr = row;
            if (!(tr instanceof HTMLElement)) return;

            total++;

            const estado = (tr.getAttribute('data-estado') || '').toLowerCase();
            const tecnicoId = (tr.getAttribute('data-tecnico') || '').toString();

            if (estado === 'enespera') enEspera++;
            else if (estado === 'enproceso') enProceso++;
            else if (estado === 'resuelta') resuelta++;

            if (tecnicoId) {
                if (!mapTecnicos[tecnicoId]) {
                    mapTecnicos[tecnicoId] = { asignadas: 0, pendientes: 0, completadas: 0 };
                }
                mapTecnicos[tecnicoId].asignadas++;
                if (estado === 'enproceso') mapTecnicos[tecnicoId].pendientes++;
                if (estado === 'resuelta') mapTecnicos[tecnicoId].completadas++;
            }
        });

        // ---- Actualizar cards de resumen ----
        function setText(id, value) {
            const el = root.querySelector('#' + id);
            if (el) el.textContent = value;
        }

        setText('totalTasks', total);
        setText('overdueTasks', enEspera);
        setText('pendingTasks', enProceso);
        setText('completedTasks', resuelta);

        // ---- Actualizar cards de técnicos ----
        const cards = root.querySelectorAll('.tech-card');
        cards.forEach(function (card) {
            const id = (card.getAttribute('data-tech-id') || '').toString();
            const stats = mapTecnicos[id] || { asignadas: 0, pendientes: 0, completadas: 0 };

            const elAsig = card.querySelector('.js-tech-asignadas');
            const elPend = card.querySelector('.js-tech-pendientes');
            const elComp = card.querySelector('.js-tech-completadas');

            if (elAsig) elAsig.textContent = stats.asignadas;
            if (elPend) elPend.textContent = stats.pendientes;
            if (elComp) elComp.textContent = stats.completadas;

            // Badge "X tareas"
            const badgeCount = card.querySelector('.tareas-count');
            if (badgeCount) {
                badgeCount.textContent = stats.asignadas + ' tareas';
            }

            // Disponibilidad
            const badgeDisp = card.querySelector('.estado-disponibilidad');
            if (badgeDisp) {
                if (stats.asignadas === 0) {
                    badgeDisp.textContent = 'Disponible';
                    badgeDisp.classList.remove('bg-warning');
                    badgeDisp.classList.add('bg-success');
                } else {
                    badgeDisp.textContent = 'No disponible';
                    badgeDisp.classList.remove('bg-success');
                    badgeDisp.classList.add('bg-warning');
                }
            }

            // Color de la tarjeta según carga
            card.classList.remove('available', 'busy', 'overloaded');
            let statusClass;
            if (stats.asignadas === 0) statusClass = 'available';
            else if (stats.asignadas > 5) statusClass = 'overloaded';
            else statusClass = 'busy';
            card.classList.add(statusClass);
        });
    }
    function initDivisionUnidad(root) {
        root = root || document;

        const ddlDivision = root.querySelector('#DivisionId');
        const ddlUnidad = root.querySelector('#UnidadId');
        const grupoUnidad = root.querySelector('#grupoUnidad');

        if (!ddlDivision || !ddlUnidad || !grupoUnidad) {
            return; // esta vista no tiene esos controles
        }

        const opcionDefault = ddlUnidad.querySelector('option[value=""]');
        const todasOpcionesUnidad = Array.from(
            ddlUnidad.querySelectorAll('option[data-division]')
        );

        // 🔹 Guardamos la unidad que viene seleccionada del servidor (si hay)
        const unidadInicial = ddlUnidad.value; // ej. "5" cuando viene de la solicitud

        function actualizarUnidades(keepSelected) {
            const divisionId = ddlDivision.value;

            // Limpia el select y deja solo la opción por defecto
            ddlUnidad.innerHTML = '';
            if (opcionDefault) {
                ddlUnidad.appendChild(opcionDefault.cloneNode(true));
            }

            let opcionesFiltradas;

            if (!divisionId) {
                // SIN división seleccionada: solo unidades SIN división
                opcionesFiltradas = todasOpcionesUnidad.filter(
                    opt => !opt.dataset.division
                );
            } else {
                // Con división: solo unidades de esa división
                opcionesFiltradas = todasOpcionesUnidad.filter(
                    opt => opt.dataset.division === divisionId
                );
            }

            opcionesFiltradas.forEach(opt => {
                ddlUnidad.appendChild(opt.cloneNode(true));
            });

            ddlUnidad.disabled = opcionesFiltradas.length === 0;

            if (keepSelected && unidadInicial) {
                // 🔹 Primer carga: intenta dejar la unidad que venía seleccionada
                ddlUnidad.value = unidadInicial;
                // si no existe en el filtro actual, se quedará en ""
            } else {
                // Cambios normales de división: limpia selección
                ddlUnidad.value = "";
            }
        }

        // Al cargar la parcial → respetar la unidad que vino del servidor
        actualizarUnidades(true);

        // Cada vez que cambie la división → ya NO respetamos la anterior
        ddlDivision.addEventListener('change', () => actualizarUnidades(false));
    }

    function initSelectTecnicos(root) {
        root = root || document;

        const nivelSel = root.querySelector('#nivelSelect');
        const tecnicoSel = root.querySelector('#tecnicoSelect');

        // Si la vista no tiene estos elementos → no hacer nada
        if (!nivelSel || !tecnicoSel) return;

        nivelSel.addEventListener('change', () => {
            const nivelId = nivelSel.value; // string

            Array.from(tecnicoSel.options).forEach(opt => {
                if (!opt.value) {
                    // “Todos los técnicos”
                    opt.hidden = false;
                    return;
                }

                const optNivel = opt.dataset.nivel || '';

                if (!nivelId) {
                    // SIN filtro → mostrar todos
                    opt.hidden = false;
                } else {
                    // Filtrado por nivel
                    opt.hidden = optNivel !== nivelId;
                }
            });

            // Si el técnico seleccionado ya no encaja con el nivel, limpiar selección
            const sel = tecnicoSel.selectedOptions[0];
            if (sel && sel.hidden) {
                tecnicoSel.value = '';
            }
        });
    }

    function initAutocompleteUsuarios(root) {
        root = root || document;

        // Contenedor que tenga el atributo data-usuarios-url
        const container = root.querySelector('[data-usuarios-url]');
        if (!container) return;

        const input = container.querySelector('#usuarioBuscador');
        const hidden = container.querySelector('#UsuarioId');
        const lista = container.querySelector('#usuarioResultados');

        if (!input || !hidden || !lista) return;

        const searchUrl = container.getAttribute('data-usuarios-url');
        if (!searchUrl) return;

        let currentController = null;
        let debounceTimer = null;

        function limpiarLista() {
            lista.innerHTML = '';
            lista.classList.add('d-none');
        }

        input.addEventListener('input', function () {
            const q = this.value.trim();

            // limpiar selección anterior
            hidden.value = '';
            limpiarLista();

            if (q.length < 3) {
                // mínimo 3 caracteres para buscar
                return;
            }

            // debounce para no disparar muchas peticiones
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }

            debounceTimer = setTimeout(async () => {
                // cancelar petición anterior si sigue viva
                if (currentController) {
                    currentController.abort();
                }

                currentController = new AbortController();

                try {
                    const resp = await fetch(searchUrl + '?term=' + encodeURIComponent(q), {
                        method: 'GET',
                        signal: currentController.signal,
                        headers: { 'X-Requested-With': 'XMLHttpRequest' }
                    });

                    if (!resp.ok) {
                        limpiarLista();
                        return;
                    }

                    const data = await resp.json();
                    lista.innerHTML = '';

                    if (!data || !data.length) {
                        limpiarLista();
                        return;
                    }

                    data.forEach(u => {
                        const btn = document.createElement('button');
                        btn.type = 'button';
                        btn.className = 'list-group-item list-group-item-action';
                        btn.textContent = u.nombre;
                        btn.dataset.id = u.id;
                        lista.appendChild(btn);
                    });

                    lista.classList.remove('d-none');
                } catch (err) {
                    if (err.name === 'AbortError') return;
                    console.error('Error buscando usuarios', err);
                    limpiarLista();
                }
            }, 300); // 300ms de debounce
        });

        // click en una sugerencia
        lista.addEventListener('click', function (e) {
            const btn = e.target.closest('button.list-group-item');
            if (!btn) return;

            const id = btn.dataset.id;
            const nombre = btn.textContent;

            hidden.value = id;
            input.value = nombre;

            limpiarLista();

            // limpiar error si estaba
            hidden.classList.remove('is-invalid', 'is-valid');
            const span = root.querySelector('[data-valmsg-for="UsuarioId"]');
            if (span) {
                span.classList.remove('field-validation-error');
                span.classList.add('field-validation-valid');
                span.textContent = '';
            }
        });


        // si borran el texto, se limpia el hidden
        input.addEventListener('blur', function () {
            if (!this.value.trim()) {
                hidden.value = '';
                limpiarLista();
            }
        });
    }
    function initTareaFechaFiltros(root) {
        root = root || document;

        const selSemana = root.querySelector('#filterSemana');
        const selDia = root.querySelector('#filterDia');
        if (!selSemana || !selDia) return;

        function pad2(n) { return (n < 10 ? '0' : '') + n; }
        function toISO(d) { return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`; }

        function startOfWeek(d) {
            const x = new Date(d.getFullYear(), d.getMonth(), d.getDate());
            const dow = x.getDay(); // 0 dom ... 1 lun
            const diff = (dow === 0 ? -6 : 1 - dow);
            x.setDate(x.getDate() + diff);
            x.setHours(0, 0, 0, 0);
            return x;
        }

        function addDays(d, days) {
            const x = new Date(d);
            x.setDate(x.getDate() + days);
            x.setHours(0, 0, 0, 0);
            return x;
        }

        function buildWeeksLast30() {
            const today = new Date(); today.setHours(0, 0, 0, 0);
            const minDate = addDays(today, -30);

            const weeks = [];
            let cursor = startOfWeek(minDate);
            const end = startOfWeek(today);

            while (cursor <= end) {
                const wStart = new Date(cursor);
                const wEnd = addDays(wStart, 6);
                weeks.push({ start: wStart, end: wEnd });
                cursor = addDays(cursor, 7);
            }
            return weeks;
        }

        function fillSemana() {
            const prev = selSemana.value || '';

            // guardamos semanas global para que el filtro las use
            window.__tareasWeeks = buildWeeksLast30();

            selSemana.innerHTML = '';
            selSemana.appendChild(new Option('Todas las semanas', ''));

            window.__tareasWeeks.forEach((w, idx) => {
                const label = `${w.start.toLocaleDateString('es-NI')} - ${w.end.toLocaleDateString('es-NI')}`;
                selSemana.appendChild(new Option(`Semana ${idx + 1}: ${label}`, String(idx)));
            });

            // intenta restaurar si aplica
            selSemana.value = prev;
            fillDia();
        }

        function fillDia() {
            const weekIndex = selSemana.value || '';

            selDia.innerHTML = '';
            selDia.appendChild(new Option('Todos los días', ''));

            if (weekIndex === '') return; // todas las semanas => no listamos días

            const w = (window.__tareasWeeks || [])[parseInt(weekIndex, 10)];
            if (!w) return;

            const today = new Date(); today.setHours(0, 0, 0, 0);
            const minDate = addDays(today, -30);

            for (let i = 0; i < 7; i++) {
                const d = addDays(w.start, i);

                if (d < minDate) continue;
                if (d > today) continue;

                const iso = toISO(d);
                const label = d.toLocaleDateString('es-NI', { weekday: 'short', day: '2-digit', month: 'short' });
                selDia.appendChild(new Option(label, iso));
            }
        }

        function redraw() {
            if (!window.jQuery || !$.fn.DataTable) return;
            const $t = $('#tablaTareas');
            if (!$t.length) return;
            $t.DataTable().draw();
        }

        // ✅ Eventos (delegados) una sola vez
        if (!window.__tareasFechaFiltrosEventsBound) {
            window.__tareasFechaFiltrosEventsBound = true;

            $(document).on('change', '#filterSemana', function () {
                // al cambiar semana => día vuelve a "Todos" + se rellena de nuevo
                const dia = document.querySelector('#filterDia');
                if (dia) dia.value = '';
                // rellenar días usando semanas actuales
                const rootNow = document;
                const semanaNow = rootNow.querySelector('#filterSemana');
                const diaNow = rootNow.querySelector('#filterDia');
                if (semanaNow && diaNow) {        
                    initTareaFechaFiltros(document);
                }
                redraw();
            });

            $(document).on('change', '#filterDia', function () {
                redraw();
            });
        }

        // ✅ SIEMPRE rellenar al cargar la vista
        fillSemana();
    }

    // === Inicializador "tipo usuarios.js" ===
    function initTareaForm(root) {
        root = root || document;

        initTareaWizard(root);
        initTareaAjaxForm(root);
        initTareaCTS(root);
        initTareaTipoServicio(root);
        initTareaDeleteForm(root);
        initTareaDataTable(root);
        initDivisionUnidad(root);
        initSelectTecnicos(root);
        initAutocompleteUsuarios(root); 
        initTareaFechaFiltros(root);

        if (window.jQuery && $.validator && $.validator.unobtrusive) {
            $.validator.unobtrusive.parse(root);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        initTareaForm(document);
    });

    window.Tareas = window.Tareas || {};
    window.Tareas.initForm = initTareaForm;

})(window);
