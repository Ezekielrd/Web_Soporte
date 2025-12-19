(function () {
    if (!window.signalR) {
        console.error('❌ SignalR no está cargado. Revisa la ruta de signalr.min.js');
        return;
    }

    console.log('🔄 Construyendo conexión con /hub/notificaciones...');

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hub/notificaciones")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", function (payload) {
        console.log('📩 Notificación recibida via SignalR:', payload);

        const titulo = payload.Titulo || payload.titulo || 'Notificación';
        const mensaje = payload.Mensaje || payload.mensaje || '';
        const tipoEvento = payload.Tipo || payload.tipo || '';    
        let icono = payload.Icono || payload.icono || 'info';
        const destinoUrl = payload.Url || payload.url || null;

        if (!icono) {
            if (tipoEvento === 'ResultadoSolicitud') icono = 'success';
            else if (tipoEvento === 'TareaAsignada') icono = 'info';
            else icono = 'info';
        }

        Swal.fire({
            title: titulo,
            text: mensaje,
            icon: icono,
            timer: 6000,
            timerProgressBar: true,
            position: 'top-end',
            toast: true,
            showConfirmButton: !!destinoUrl,
            confirmButtonText: destinoUrl ? 'Ver detalle' : 'Aceptar'
        }).then(result => {

            const path = window.location.pathname.toLowerCase();
            const query = window.location.search.toLowerCase();

            console.log('🔎 Post-Swal. tipoEvento=', tipoEvento, ' result=', result);

            // ✅ 1) SI el usuario hizo clic en "Ver detalle"
            if (destinoUrl && result.isConfirmed) {
                console.log('➡️ Navegando a detalle:', destinoUrl);
                window.location.href = destinoUrl;
                return;
            }

            // ⛔ 2) SI existe destinoUrl, NO recargues NUNCA
            //     (el usuario puede entrar luego desde la campanita)
            if (destinoUrl) {
                console.log('ℹ Notificación con destinoUrl, no se recarga la página');
                return;
            }

            // 3) Actualizar campanita (solo si NO hay destinoUrl)
            if (typeof window.reloadNotificacionesResumen === 'function') {
                console.log('🔄 Refrescando campanita...');
                window.reloadNotificacionesResumen();
            }

            // 4) Recargas SOLO para eventos SIN destino
            if (tipoEvento === 'TareaAsignada') {
                if (path.includes('/tecnico')) {
                    console.log('🔁 Recargando página Técnico...');
                    window.location.reload();
                }
            }

            if (tipoEvento === 'ResultadoSolicitud') {
                if (path.includes('/solicitud')) {
                    console.log('🔁 Recargando solicitudes del cliente...');
                    window.location.reload();
                }
            }

            if (tipoEvento === 'SolicitudCreadaAdmin') {
                if (path.includes('/adminsolicitudes')) {
                    console.log('🔁 Recargando solicitudes del admin...');
                    window.location.reload();
                }
            }

            if (tipoEvento === 'TareaFinalizadaUsuario') {
                if (path.includes('/reportes/reportesolicitud')) {
                    console.log('🔁 Recargando reporte...');
                    window.location.reload();
                }
            }

            if (tipoEvento === 'CambioEstadoTareaAdmin' || tipoEvento === 'TareaFinalizadaAdmin') {
                if (path.includes('/home/index') && query.includes('view=tareas')) {
                    console.log('🔁 Recargando Gestión de Tareas...');
                    window.location.reload();
                }
            }
        });
    });


    async function startConnection() {
        try {
            await connection.start();
            console.log('✅ Conectado a SignalR /hub/notificaciones');
        } catch (err) {
            console.error('❌ Error al conectar SignalR. Reintentando en 5s...', err);
            setTimeout(startConnection, 5000);
        }
    }

    startConnection();

    // Para pruebas desde consola
    window._notifConnection = connection;
})();
