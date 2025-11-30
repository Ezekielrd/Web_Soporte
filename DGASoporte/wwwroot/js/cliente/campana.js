
        document.addEventListener('DOMContentLoaded', function () {
        const btnBell   = document.getElementById('btnNotificaciones');
        const panel     = document.getElementById('notifPanel');
        const btnCerrar = document.getElementById('btnCerrarNotif');
        const badge     = document.getElementById('notifBadge');
        const list      = document.getElementById('notifList');
        const placeholder = document.getElementById('notifPlaceholder');
        const formMarcarLeidas = document.getElementById('formMarcarLeidas');

        if (!btnBell || !panel) return;

        let panelVisible = false;

        btnBell.addEventListener('click', function () {
            panelVisible = !panelVisible;
        panel.classList.toggle('d-none', !panelVisible);

        if (panelVisible) {
            cargarResumenNotificaciones();
        }
    });

        btnCerrar.addEventListener('click', function () {
            panelVisible = false;
        panel.classList.add('d-none');
    });

        // Cerrar al hacer click fuera del panel
        document.addEventListener('click', function (e) {
        if (!panelVisible) return;

        const clickEnPanel = panel.contains(e.target);
        const clickEnBoton = btnBell.contains(e.target);

        if (!clickEnPanel && !clickEnBoton) {
            panelVisible = false;
        panel.classList.add('d-none');
        }
    });

        async function cargarResumenNotificaciones() {
        try {
            const resp = await fetch('/Notificaciones/Resumen', {
            headers: {'X-Requested-With': 'XMLHttpRequest' }
            });
        if (!resp.ok) return;

        const data = await resp.json();

            // contador
            if (data.unreadCount > 0) {
            badge.textContent = data.unreadCount;
        badge.classList.remove('d-none');
            } else {
            badge.classList.add('d-none');
            }

        // limpiar lista
        list.innerHTML = '';

        if (!data.items || data.items.length === 0) {
                const emptyItem = document.createElement('div');
        emptyItem.className = 'list-group-item text-muted text-center py-3';
        emptyItem.textContent = 'No tienes notificaciones.';
        list.appendChild(emptyItem);
        return;
            }

            data.items.forEach(n => {
                const item = document.createElement('a');
        item.className = 'list-group-item list-group-item-action';
        if (!n.leida) item.classList.add('bg-light');

        item.href = `/Notificaciones/Abrir/${n.id}`; // marcará como leída y redirigirá

        item.innerHTML = `
        <div class="d-flex justify-content-between">
            <span class="${n.leida ? '' : 'fw-semibold'}">${n.titulo}</span>
            <small class="text-muted">${n.fecha}</small>
        </div>
        <div class="text-muted text-truncate">
            ${n.mensaje}
        </div>
        `;

        list.appendChild(item);
            });
        } catch (err) {
            console.error('Error cargando resumen de notificaciones', err);
        }
    }

        // Marcar todas como leídas sin salir del panel
        if (formMarcarLeidas) {
            formMarcarLeidas.addEventListener('submit', async function (e) {
                e.preventDefault();

                const formData = new FormData(formMarcarLeidas);

                try {
                    const resp = await fetch(formMarcarLeidas.action, {
                        method: 'POST',
                        body: formData,
                        headers: { 'X-Requested-With': 'XMLHttpRequest' }
                    });

                    if (resp.ok) {
                        // Recargar panel y contador
                        await cargarResumenNotificaciones();
                    }
                } catch (err) {
                    console.error('Error marcando notificaciones como leídas', err);
                }
            });
    }

    // Carga inicial del contador (sin abrir el panel)
    (async () => {
        try {
            const resp = await fetch('/Notificaciones/Resumen', {
            headers: {'X-Requested-With': 'XMLHttpRequest' }
            });
        if (!resp.ok) return;
        const data = await resp.json();
            if (data.unreadCount > 0) {
            badge.textContent = data.unreadCount;
        badge.classList.remove('d-none');
            }
        } catch (err) {
            console.error('Error inicial del contador de notificaciones', err);
        }
    })();
});

