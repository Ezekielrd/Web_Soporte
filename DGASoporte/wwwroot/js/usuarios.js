// wwwroot/js/usuarios.js
(function (window) {

    function initSwitchActivo(root) {
        root = root || document;

        const switchEl = root.querySelector('#switchActivo');
        const label = root.querySelector('.labelEstado');

        if (!switchEl || !label) return;

        function actualizarEstado() {
            label.textContent = switchEl.checked ? 'Activo' : 'Desactivo';
        }

        actualizarEstado();
        switchEl.addEventListener('change', actualizarEstado);
    }

    function initCamposTecnico(root) {
        root = root || document;

        const rolSelect = root.querySelector('#IdRol');
        const panelTecnico = root.querySelector('#panelTecnico');

        if (!rolSelect || !panelTecnico) return;

        const idRolTecnico = 2;

        function toggleCamposTecnico() {
            const seleccionado = parseInt(rolSelect.value || '0', 10);
            const esTecnico = seleccionado === idRolTecnico;
            panelTecnico.style.display = esTecnico ? 'block' : 'none';
        }

        toggleCamposTecnico();
        rolSelect.addEventListener('change', toggleCamposTecnico);
    }

    function initPasswordEye(root) {
        root = root || document;

        const inputs = root.querySelectorAll('.secure-password');
        const icons = root.querySelectorAll('.togglePassword');

        if (!inputs.length || !icons.length) return;

        inputs.forEach((input, index) => {
            const icon = icons[index];
            if (!icon) return;

            icon.style.display = 'none';

            function toggleEye() {
                if (input.value.length > 0) {
                    icon.style.display = 'inline';
                } else {
                    icon.style.display = 'none';
                    input.dataset.allowPlain = '0';
                    input.type = 'password';
                    icon.textContent = '👁';
                }
            }

            const hide = () => {
                input.dataset.allowPlain = '0';
                input.type = 'password';
                icon.textContent = '👁';
            };

            icon.addEventListener('mousedown', () => {
                input.dataset.allowPlain = '1';
                input.type = 'text';
                icon.textContent = '👀';
            });

            icon.addEventListener('mouseup', hide);
            icon.addEventListener('mouseleave', hide);
            icon.addEventListener('blur', hide);

            input.addEventListener('input', () => {
                toggleEye();
                if (input.value.length === 0) hide();
            });

            toggleEye();
        });
    }
    //submit AJAX cuando el form está en el modal
    function initUsuarioAjaxForm(root) {
        root = root || document;

        const form = root.querySelector('form[data-ajax-usuario="true"]');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault(); //evitar el POST normal

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

                const contentType = response.headers.get('content-type') || '';

                if (contentType.includes('application/json')) {
                    const result = await response.json();
                    if (result.success) {
                        if (window.appModal) {
                            window.appModal.hide();
                        }
                        const usuariosLink = document.querySelector('.menu-item[data-view="Usuarios"]');
                        if (usuariosLink) {
                            usuariosLink.click(); // recarga la vista de usuarios en el panel
                        }
                        return;
                    }
                }

                // Si no es JSON, es HTML con errores -> reinyectar formulario
                const html = await response.text();
                if (window.appModalBody) {
                    appModalBody.innerHTML = html;
                    // re-inicializar sobre el nuevo contenido
                    initUsuarioForm(appModalBody);
                }
            } catch (err) {
                console.error('Error al enviar formulario de usuario', err);
            }
        });
    }

     async function initUsuarioDeleteForm(root) {
        root = root || document;

        const form = root.querySelector('form[data-ajax-delete-usuario="true"]');
        if (!form) return;

        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            if (!confirm('¿Seguro que desea eliminar este usuario?')) return;

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

                    const usuariosLink = document.querySelector('.menu-item[data-view="Usuarios"]');
                    if (usuariosLink) {
                        usuariosLink.click(); // recarga la vista de usuarios en el panel
                    }
                    return;
                } else {
                    alert(result.message || 'No se pudo eliminar el usuario.');
                }
            } catch (err) {
                console.error('Error al eliminar usuario', err);
                alert('Error al eliminar el usuario.');
            }
        });
    }



    function initUsuarioForm(root) {
        initSwitchActivo(root);
        initCamposTecnico(root);
        initPasswordEye(root);
        initUsuarioAjaxForm(root);
        initUsuarioDeleteForm(root);  


        if (window.Usuarios && typeof window.Usuarios.initPasswordEdicion === 'function') {
            window.Usuarios.initPasswordEdicion(root);
        }

        // Re-parse unobtrusive validation en el contenido del modal
        if (window.jQuery && $.validator && $.validator.unobtrusive) {
            $.validator.unobtrusive.parse(root);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        initUsuarioForm(document);
    });

    window.Usuarios = window.Usuarios || {};
    window.Usuarios.initForm = initUsuarioForm;


})(window);
