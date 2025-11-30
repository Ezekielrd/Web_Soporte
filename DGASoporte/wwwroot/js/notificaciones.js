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

        // Datos básicos
        const titulo = payload.Titulo || payload.titulo || 'Notificación';
        const mensaje = payload.Mensaje || payload.mensaje || '';
        const tipoEvento = payload.Tipo || payload.tipo || '';    // "ResultadoSolicitud" / "TareaAsignada" / otros
        let icono = payload.Icono || payload.icono || '';    // icono Swal
        const destinoUrl = payload.Url || payload.url || null;  // URL generada en C#

        // Si no viene icono, damos uno por defecto según el tipo de evento
        if (!icono) {
            if (tipoEvento === 'ResultadoSolicitud') {
                icono = 'success'; // o 'warning' si quieres diferenciar en el servicio
            } else if (tipoEvento === 'TareaAsignada') {
                icono = 'info';
            } else {
                icono = 'info';
            }
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
            // 1) Si el usuario pulsó "Ver detalle" y hay URL, vamos allí
            if (destinoUrl && result.isConfirmed) {
                window.location.href = destinoUrl;
                return;
            }

            // 2) Comportamiento extra SOLO para ciertos tipos de notificación

            // 🔹 TAREA ASIGNADA → refrescar lista de tareas si el técnico está en esa vista
            if (tipoEvento === 'TareaAsignada') {
                const path = window.location.pathname.toLowerCase();

                // Ajusta '/tareas' si la ruta de la lista de tareas del técnico es otra
                if (path.startsWith('/tareas')) {
                    window.location.reload();
                }
            }

            // 🔹 Resultado de SOLICITUD → aquí NO tocamos nada
            //     - El flujo sigue siendo que la URL apunte a:
            //       "/Solicitudes?solicitudId=XYZ"
            //     - Y el JS de la vista Index de Solicitudes se encarga de abrir el modal.
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
