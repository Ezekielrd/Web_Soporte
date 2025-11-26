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
//CARGA DE VISTAS DINÁMICAS
const viewContainer = document.getElementById('view-container');
const menuItems = document.querySelectorAll('.menu-item');

menuItems.forEach(item => {
    item.addEventListener('click', async (e) => {
        e.preventDefault();

        menuItems.forEach(x => x.classList.remove('active'));
        item.classList.add('active');

        const url = item.dataset.url;
        const view = item.dataset.view;   // 👈 usamos el data-view="Tareas"
        if (!url) return;

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
    const activeItem = document.querySelector('.menu-item.active'); // normalmente Tareas

    // Solo si el contenedor está vacío y hay un item activo con URL
    if (viewContainer && activeItem && activeItem.dataset.url && viewContainer.innerHTML.trim() === '') {
        activeItem.click(); // dispara el mismo flujo AJAX que cuando haces clic
    }
});