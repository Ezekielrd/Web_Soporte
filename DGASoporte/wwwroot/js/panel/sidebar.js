const sidebar = document.getElementById('sidebar');
const btnToggleSidebar = document.getElementById('btnToggleSidebar');

btnToggleSidebar?.addEventListener('click', () => {
    const isCollapsed = sidebar.classList.toggle('collapsed');

    const icon = btnToggleSidebar.querySelector('i');
    if (icon) {
        icon.classList.toggle('bi-chevron-double-left', !isCollapsed);
        icon.classList.toggle('bi-chevron-double-right', isCollapsed);
    }
});

// === CARGA DE VISTAS DINÁMICAS ===
const viewContainer = document.getElementById('view-container');
const menuItems = document.querySelectorAll('.menu-item');
const LAST_VIEW_KEY = 'adminLastView'; // 👈 clave en localStorage

menuItems.forEach(item => {
    item.addEventListener('click', async (e) => {
        e.preventDefault();

        menuItems.forEach(x => x.classList.remove('active'));
        item.classList.add('active');

        const url = item.dataset.url;
        const view = item.dataset.view;  // aquí tienes "Tareas", "Usuarios", etc.

        if (!url) return;

        // Guardar último módulo seleccionado
        if (view) {
            try {
                localStorage.setItem(LAST_VIEW_KEY, view);
            } catch (ex) {
                console.warn('No se pudo guardar la vista en localStorage', ex);
            }
        }

        try {
            viewContainer.innerHTML = `
                <div class="d-flex justify-content-center align-items-center py-5">
                    <div class="spinner-border" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                </div>`;

            const response = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            const html = await response.text();
            viewContainer.innerHTML = html;

            // Reenganchar validación MVC en el contenido nuevo
            if (window.jQuery && $.validator && $.validator.unobtrusive) {
                $.validator.unobtrusive.parse(viewContainer);
            }

            // Inicializar lógica según vista
            if (view === 'Tareas' && window.Tareas && typeof window.Tareas.initForm === 'function') {
                window.Tareas.initForm(viewContainer);
            }

            if (view === 'Usuarios' && window.Usuarios && typeof window.Usuarios.initForm === 'function') {
                window.Usuarios.initForm(viewContainer);
            }

            if (view === 'Solicitudes' && window.Solicitudes && typeof window.Solicitudes.initForm === 'function') {
                window.Solicitudes.initForm(viewContainer);
            }
            if (view === 'Dashboard' && window.Dashboard && typeof window.Dashboard.init === 'function') {
                window.Dashboard.init(viewContainer);
            }

            if (view === 'Solicitudes' && typeof window.onSolicitudesViewLoaded === 'function') {
                console.log('✅ sidebar: vista Solicitudes cargada, llamando onSolicitudesViewLoaded()');
                window.onSolicitudesViewLoaded();
            }
            if (view === 'Tareas' && typeof window.onTareasViewLoaded === 'function') {
                console.log('✅ sidebar: vista Tareas cargada, llamando onTareasViewLoaded()');
                window.onTareasViewLoaded();
            }

        } catch (err) {
            console.error(err);
            viewContainer.innerHTML = `
                <div class="alert alert-danger mt-3">
                    Ocurrió un error al cargar la sección.
                </div>`;
        }
    });
});

document.addEventListener('DOMContentLoaded', () => {
    const viewContainer = document.getElementById('view-container');
    if (!viewContainer) return;

    let initialItem = null;
    let lastView = null;

    // 👉 Leer la última vista guardada
    try {
        lastView = localStorage.getItem(LAST_VIEW_KEY);
    } catch (ex) {
        console.warn('No se pudo leer la vista de localStorage', ex);
    }

    if (lastView) {
        // Buscar el menú que tenga ese data-view
        initialItem = document.querySelector(`.menu-item[data-view="${lastView}"]`);
    }

    // Si no hay nada guardado o no encuentra el item, usa el que ya viene como .active
    if (!initialItem) {
        initialItem = document.querySelector('.menu-item.active');
    }

    // Último fallback: el primer menú
    if (!initialItem) {
        initialItem = document.querySelector('.menu-item');
    }

    // Solo dispara el click si el contenedor está vacío
    if (initialItem && viewContainer.innerHTML.trim() === '') {
        initialItem.click(); // 👉 carga por AJAX el módulo correcto
    }
    const btnLogout = document.getElementById('btnLogout');
    if (btnLogout) {
        btnLogout.addEventListener('click', function (e) {
            e.preventDefault();

            try {
                localStorage.removeItem(LAST_VIEW_KEY);
            } catch (ex) {
                console.warn('No se pudo borrar adminLastView de localStorage', ex);
            }

            window.location.href = this.href;
        });
    }
});
