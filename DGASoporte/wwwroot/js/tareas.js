// wwwroot/js/tareas.js
(function (window) {

    function initTareaWizard(root) {
        root = root || document;

        const form = root.querySelector('#wizardForm');
        if (!form) return;

        const stepPanels = root.querySelectorAll('.form-step');
        const stepIndicators = root.querySelectorAll('[data-step-indicator]');
        const btnNext = root.querySelector('#btnNext');
        const btnPrev = root.querySelector('#btnPrev');
        const btnSubmit = root.querySelector('#btnSubmit');

        let currentStep = 1;
        const totalSteps = stepPanels.length;

        function showStep(step) {
            currentStep = step;

            stepPanels.forEach(panel => {
                const num = parseInt(panel.getAttribute('data-step'));
                panel.classList.toggle('active', num === step);
            });

            stepIndicators.forEach(ind => {
                const num = parseInt(ind.getAttribute('data-step-indicator'));
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

        function validarStepActual() {
            const panel = root.querySelector(`.form-step[data-step="${currentStep}"]`);
            if (!panel) return true;

            const inputs = panel.querySelectorAll("input, select, textarea");
            let valid = true;

            // Usar jQuery Validate (asp-validation-for) si está disponible
            if (window.jQuery && $.validator && form) {
                const $form = $(form);
                const validator = $form.data('validator') || $form.validate();

                inputs.forEach(input => {
                    if (!input.name) return;
                    const ok = validator.element(input);
                    if (!ok) valid = false;
                });

                return valid;
            }

            // Fallback HTML5
            inputs.forEach(input => {
                if (!input.checkValidity()) {
                    valid = false;
                    input.classList.add('is-invalid');
                } else {
                    input.classList.remove('is-invalid');
                }
            });

            return valid;
        }

        function actualizarResumen(root) {
            const q = sel => root.querySelector(sel);

            const r = {
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

            const f = {
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

            if (r.titulo) r.titulo.innerText = f.titulo?.value || '---';
            if (r.descripcion) r.descripcion.innerText = f.descripcion?.value || '---';
            if (r.prioridad) r.prioridad.innerText = f.prioridad?.options[f.prioridad.selectedIndex]?.text || '---';
            if (r.categoria) r.categoria.innerText = f.categoria?.options[f.categoria.selectedIndex]?.text || '---';
            if (r.unidad) r.unidad.innerText = f.unidad?.options[f.unidad.selectedIndex]?.text || '---';
            if (r.tecnico) r.tecnico.innerText = f.tecnico?.value ? f.tecnico.options[f.tecnico.selectedIndex].text : 'Sin técnico';
            if (r.estado) r.estado.innerText = f.estado?.options[f.estado.selectedIndex]?.text || '---';
            if (r.fechaLimite) r.fechaLimite.innerText = f.fechaLimite?.value || '---';
            if (r.fechaAsignacion) r.fechaAsignacion.innerText = f.fechaAsignacion?.value || '---';
        }

        if (btnNext) {
            btnNext.addEventListener('click', function () {
                if (!validarStepActual()) return;
                if (currentStep < totalSteps) showStep(currentStep + 1);
            });
        }

        if (btnPrev) {
            btnPrev.addEventListener('click', function () {
                if (currentStep > 1) showStep(currentStep - 1);
            });
        }

        showStep(1);
    }

    function initTareaAjaxForm(root) {
        root = root || document;

        const form = root.querySelector('form[data-ajax-tarea="true"]');
        if (!form) return;

        form.addEventListener('submit', async function (e) {
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
                            tareasLink.click(); // recarga la vista de usuarios en el panel
                        }
                        return;
                    }
                }

                // HTML con errores -> reinyectar
                const html = await response.text();
                const container = window.appModalBody || document.getElementById('appModalBody');

                if (container) {
                    container.innerHTML = html;

                    initTareaWizard(container);
                    initTareaAjaxForm(container);

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

        const selCategoria = root.querySelector('#CategoriaId');
        const selTipo = root.querySelector('#TipoServicioId');

        // Si en esta vista / parcial no existen esos selects, no hacemos nada
        if (!selCategoria || !selTipo) return;

        const allOptions = Array.from(selTipo.querySelectorAll('option'))
            .filter(o => o.value !== "");

        function filtrarTipos() {
            const categoriaId = selCategoria.value;
            const selectedTipoId = selTipo.value;

            selTipo.innerHTML = '';
            selTipo.append(new Option('-- Seleccione tipo --', ''));

            if (!categoriaId) return;

            const filtrados = allOptions.filter(o =>
                o.dataset.categoriaId === categoriaId
            );

            filtrados.forEach(o => selTipo.append(o));

            if (selectedTipoId) {
                selTipo.value = selectedTipoId;
            }
        }

        selCategoria.addEventListener('change', filtrarTipos);
        filtrarTipos();
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

                    const usuariosLink = document.querySelector('.menu-item[data-view="Tareas"]');
                    if (usuariosLink) {
                        usuariosLink.click(); // recarga la vista de usuarios en el panel
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

    // === Inicializador "tipo usuarios.js" ===
    function initTareaForm(root) {
        root = root || document;

        initTareaWizard(root);
        initTareaAjaxForm(root);
        initTareaCTS(root);
        initTareaTipoServicio(root);
        initTareaDeleteForm(root);

        if (window.jQuery && $.validator && $.validator.unobtrusive) {
            $.validator.unobtrusive.parse(root);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        initTareaForm(document);
    });

    window.Tareas = window.Tareas || {};
    window.Tareas.initForm = initTareaForm; // 👈 clave: mismo nombre que Usuarios.initForm

})(window);
