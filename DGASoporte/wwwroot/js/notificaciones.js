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
            console.log('🔎 Post-Swal. tipoEvento=', tipoEvento, ' path=', path, ' result=', result);

            // 1) Si el usuario pulsa "Ver detalle" → ir al wrapper /Notificaciones/Abrir/{id}
            if (destinoUrl && result.isConfirmed) {
                window.location.href = destinoUrl;
                return;
            }

            // 2) Actualizar la campanita (contador + lista) si existe la función
            if (typeof window.reloadNotificacionesResumen === 'function') {
                console.log('🔄 Refrescando campanita...');
                window.reloadNotificacionesResumen();
            }

            // 3) Recargar la página de lista según el tipo de evento y la ruta actual
            if (tipoEvento === 'TareaAsignada') {
                if (path.includes('/tecnico')) {       // 👈 aquí usamos /Tecnico
                    console.log('🔁 Recargando página de tareas (Tecnico/Index)...');
                    window.location.reload();
                }
            }

            // 🔹 RESULTADO SOLICITUD → lista está en /Solicitud/Index
            if (tipoEvento === 'ResultadoSolicitud') {
                if (path.includes('/solicitud')) {     // 👈 aquí usamos /Solicitud
                    console.log('🔁 Recargando página de solicitudes (Solicitud/Index)...');
                    window.location.reload();
                }
            }
            if (tipoEvento === 'SolicitudCreadaAdmin' && path.includes('/adminsolicitudes')) {
                console.log('🔁 Recargando lista de solicitudes del admin...');
                window.location.reload();
            }
            if (tipoEvento === 'TareaFinalizada' && path.includes('/tarea')) {
                    console.log('🔁 Tarea finalizada, podrías recargar o actualizar aquí si lo necesitas');
                    window.location.reload(); // si quieres recargar
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
