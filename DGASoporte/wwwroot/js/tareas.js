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
            var ID_CATEGORIA_GENERAL = '5'; // ⚠️ AJUSTA AL ID REAL DE TU CATEGORÍA GENERAL

            // División / Unidad
            var ddlDivision = root.querySelector('#DivisionId');
            var ddlUnidad = root.querySelector('#UnidadId');

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

                // --- IGNORAR si está deshabilitado / oculto (pero NO TipoServicio ni División,
                // que tienen reglas especiales más abajo)
                if (!isTipoServicio && !isDivision && input.disabled) {
                    clearError(input);
                    return;
                }

                if (!isTipoServicio && !isDivision && input.offsetParent === null) {
                    clearError(input);
                    return;
                }

                var requeridoMsg = input.getAttribute('data-val-required');

                // Si no tiene required → nada que validar
                if (!requeridoMsg) {
                    clearError(input);
                    return;
                }

                // ====================================================
                // 🔥 REGLA ESPECIAL DE TIPO SERVICIO
                // ====================================================
                if (isTipoServicio) {

                    // Si no existe el select de categoría, lo validamos como campo normal requerido
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

                    // 1) SIN categoría seleccionada → deshabilitamos TipoServicio, sin verde ni rojo
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

                    // 2) Categoría GENERAL → TipoServicio NO requerido (se suele ocultar)
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

                    // 3) Categoría seleccionada y NO general → TipoServicio ES obligatorio
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

                // ====================================================
                // 🔥 REGLA ESPECIAL DE DIVISIÓN vs UNIDAD
                // ====================================================
                if (isDivision) {

                    // Si no tenemos Unidad en la vista, tratar como required normal
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

                    // Unidad seleccionada (si hay)
                    var unidadOpt = ddlUnidad.options[ddlUnidad.selectedIndex] || null;
                    var unidadTieneDivision = unidadOpt && unidadOpt.dataset
                        ? (unidadOpt.dataset.division || '').toString()
                        : '';

                    // CASO A: hay Unidad seleccionada y NO tiene division (data-division vacío)
                    // → División NO es obligatoria
                    if (unidadOpt && !unidadTieneDivision) {
                        clearError(input);
                        return;
                    }

                    // CASO B: no hay unidad seleccionada aún
                    // Dejas que la Unidad (que es [Required]) sea la que frene,
                    // podemos no exigir división todavía.
                    if (!unidadOpt || !ddlUnidad.value) {
                        clearError(input);
                        return;
                    }

                    // CASO C: Unidad seleccionada SÍ tiene division → División OBLIGATORIA
                    var divVal = input.value;
                    if (!divVal || divVal.trim() === '') {
                        valid = false;
                        setError(input, requeridoMsg);
                    } else {
                        clearError(input);
                    }
                    return;
                }

                // ====================================================
                // 🔥 VALIDACIÓN NORMAL PARA EL RESTO DE CAMPOS
                // ====================================================
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
                        // cerrar modal si existe
                        if (window.appModal) {
                            window.appModal.hide();
                        }

                        // recargar el módulo de tareas
                        const tareasLink = document.querySelector('.menu-item[data-view="Tareas"]');
                        if (tareasLink) {
                            tareasLink.click();
                        }
                        return;
                    } else {
                        // mostrar mensaje de error si viene en el JSON
                        if (result.message) {
                            alert(result.message);
                        }
                        return;
                    }
                }

                // Si por alguna razón no es JSON, tratamos como HTML (errores de validación)
                const html = await response.text();
                const container = window.appModalBody || document.getElementById('appModalBody');

                if (container) {
                    container.innerHTML = html;

                    // re-inicializamos todo lo que ya tienes
                    initTareaWizard(container);
                    initTareaAjaxForm(container);
                    initTareaCTS(container);
                    initTareaTipoServicio(container);
                    initDivisionUnidad(container);

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

        function actualizarTitle() {
            const value = sel.value;
            if (!value) {
                sel.title = 'Seleccione un tipo de servicio';
                console.log('[Tareas] actualizando title -> placeholder');
                return;
            }

            const desc = map[value] || '';
            sel.title = desc;
            console.log('[Tareas] actualizando title ->', { value, desc });
        }

        sel.addEventListener('change', actualizarTitle);
        actualizarTitle(); // valor inicial (placeholder)
    }

    async function initTareaDeleteForm(root) {
        root = root || document;

        const form = root.querySelector('form[data-ajax-delete-tarea="true"]');
        if (!form) return;

        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            if (!confirm('¿Seguro que desea eliminar esta tarea?')) return;

            const url = form.action;
            const formData = new FormData(form);

            try {
                const response = await fetch(url, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const result = await response.json();

                if (result.success) {
                    // cerrar modal
                    if (window.appModal) {
                        window.appModal.hide();
                    }

                    const tareasLink = document.querySelector('.menu-item[data-view="Tareas"]');
                    if (tareasLink) {
                        tareasLink.click(); // recarga la vista de tareas en el panel
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

                // Leemos SIEMPRE los selects actuales (no referencias viejas)
                const tecSel = $('#filterTecnico').val() || '';
                const estSel = $('#filterEstado').val() || '';
                const priSel = $('#filterPrioridad').val() || '';
                const catSel = $('#filterCategoria').val() || '';
                const quick = window.TareasQuickFilter || 'all';

                const tecRow = (row.getAttribute('data-tecnico') || '').toString();
                const estRow = (row.getAttribute('data-estado') || '').toString();
                const priRow = (row.getAttribute('data-prioridad') || '').toString();
                const catRow = (row.getAttribute('data-categoria') || '').toString();
                const vencida = (row.getAttribute('data-vencida') || 'false').toLowerCase();
                const fecha = row.getAttribute('data-fecha') || '';

                // ---- filtros por selects ----
                if (tecSel && tecRow !== tecSel) return false;
                if (estSel && estRow !== estSel) return false;
                if (priSel && priRow !== priSel) return false;
                if (catSel && catRow !== catSel) return false;

                // ---- quick filters ----
                if (quick === 'vencidas' && vencida !== 'true')
                    return false;

                if (quick === 'sin-asignar' && tecRow) // tiene técnico → no pasa
                    return false;

                if (quick === 'hoy') {
                    if (!fecha) return false;
                    const hoy = new Date();
                    const yyyy = hoy.getFullYear();
                    const mm = ('0' + (hoy.getMonth() + 1)).slice(-2);
                    const dd = ('0' + hoy.getDate()).slice(-2);
                    const hoyStr = `${yyyy}-${mm}-${dd}`;
                    if (fecha !== hoyStr) return false;
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

        function actualizarUnidades() {
            const divisionId = ddlDivision.value;

            // Limpia el select y deja solo la opción por defecto
            ddlUnidad.innerHTML = '';
            if (opcionDefault) {
                ddlUnidad.appendChild(opcionDefault.cloneNode(true));
            }

            let opcionesFiltradas;

            if (!divisionId) {
                // 🔹 SIN división seleccionada:
                // solo unidades SIN división (data-division vacío)
                opcionesFiltradas = todasOpcionesUnidad.filter(
                    opt => !opt.dataset.division // "" → falsy
                );
            } else {
                // 🔹 Con división: solo unidades que pertenezcan a esa división
                opcionesFiltradas = todasOpcionesUnidad.filter(
                    opt => opt.dataset.division === divisionId
                );
            }

            opcionesFiltradas.forEach(opt => {
                ddlUnidad.appendChild(opt.cloneNode(true));
            });

            ddlUnidad.disabled = opcionesFiltradas.length === 0;
            ddlUnidad.value = "";
        }

        // Al cargar la parcial
        actualizarUnidades();

        // Cada vez que cambie la división
        ddlDivision.addEventListener('change', actualizarUnidades);
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
