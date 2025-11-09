// JavaScript sin JSON para el modal - VERSIÓN SIMPLIFICADA
$(document).ready(function () {
    console.log("=== INICIANDO VERSIÓN SIN JSON ===");

    // Función para abrir modal con datos simples
    window.abrirModalConfirmacion = function () {
        console.log("=== ABRIR MODAL CONFIRMACIÓN ===");

        // 1. Obtener valores directamente del DOM
        const titulo = $('#titulo').val()?.trim() || '';
        const descripcion = $('#descripcion').val()?.trim() || '';
        const unidadTexto = $('#unidadId option:selected').text() || '';
        const tipoTexto = $('#tipo option:selected').text() || '';

        console.log("Datos obtenidos:");
        console.log("- Título:", titulo);
        console.log("- Descripción:", descripcion);
        console.log("- Unidad:", unidadTexto);
        console.log("- Tipo:", tipoTexto);

        // 2. Validación simple
        if (!titulo) {
            alert("El título es requerido");
            return false;
        }

        if (!descripcion) {
            alert("La descripción es requerida");
            return false;
        }

        if (!$('#unidadId').val() || $('#unidadId').val() === '0') {
            alert("Debe seleccionar una unidad");
            return false;
        }

        if (!$('#tipo').val() || $('#tipo').val() === '0') {
            alert("Debe seleccionar un tipo");
            return false;
        }

        // 3. Llenar el modal con HTML simple
        $('#modalTitulo').text(titulo);
        $('#modalDescripcion').text(descripcion);
        $('#modalUnidad').text(unidadTexto);
        $('#modalTipo').text(tipoTexto);

        // 4. Mostrar modal
        $('#modalConfirmacion').modal('show');

        console.log("Modal abierto correctamente");
        return true;
    };

    // Función para guardar sin JSON
    window.guardarSolicitudSinJson = function () {
        console.log("=== GUARDAR SIN JSON ===");

        // 1. Obtener datos del formulario
        const formData = {
            titulo: $('#titulo').val()?.trim(),
            descripcion: $('#descripcion').val()?.trim(),
            unidadId: parseInt($('#unidadId').val()),
            tipo: parseInt($('#tipo').val())
        };

        console.log("Datos a enviar:", formData);

        // 2. Validación
        if (!formData.titulo || !formData.descripcion || !formData.unidadId || !formData.tipo) {
            alert("Complete todos los campos antes de guardar");
            return;
        }

        // 3. Obtener token
        const token = $('input[name="__RequestVerificationToken"]').val();
        console.log("Token encontrado:", token ? "SÍ" : "NO");

        if (!token) {
            alert("Error: Token de seguridad no encontrado. Recarga la página.");
            return;
        }

        // 4. Enviar datos usando FormData en lugar de JSON
        const datosForm = new FormData();
        datosForm.append('titulo', formData.titulo);
        datosForm.append('descripcion', formData.descripcion);
        datosForm.append('unidadId', formData.unidadId);
        datosForm.append('tipo', formData.tipo);
        datosForm.append('__RequestVerificationToken', token);

        console.log("=== ENVIANDO PETICIÓN ===");

        $.ajax({
            url: '/SolicitudIncidencia/Create',
            type: 'POST',
            data: datosForm,
            processData: false,  // No procesar datos
            contentType: false,  // No establecer content-type
            success: function (response) {
                console.log("✅ ÉXITO:", response);

                if (response.exito || response.success) {
                    $('#modalConfirmacion').modal('hide');
                    alert('¡Solicitud guardada exitosamente!\n\nID: ' + (response.id || 'N/A'));

                    // Limpiar formulario
                    $('#titulo').val('');
                    $('#descripcion').val('');
                    $('#unidadId').val('0');
                    $('#tipo').val('0');
                } else {
                    console.error("Respuesta con error:", response);
                    alert('Error: ' + (response.error || 'Error desconocido'));
                }
            },
            error: function (xhr, status, error) {
                console.error("❌ ERROR AJAX:");
                console.error("- Status:", xhr.status);
                console.error("- Error:", error);
                console.error("- Response:", xhr.responseText);

                if (xhr.status === 400) {
                    alert('Error 400: Los datos no son válidos.\n\nRevisa la consola (F12) para más detalles.');
                } else if (xhr.status === 500) {
                    alert('Error interno del servidor. Revisa los logs.');
                } else {
                    alert('Error: ' + error);
                }
            }
        });
    };

    // Función para modificar (volver al formulario)
    window.modificarSolicitud = function () {
        console.log("=== MODIFICAR SOLICITUD ===");
        $('#modalConfirmacion').modal('hide');
        // El usuario puede editar los campos en el formulario original
    };

    console.log("✅ JavaScript sin JSON cargado");
});

// Event listeners
$(document).on('click', '#btnEnviar', function (e) {
    e.preventDefault();
    console.log("Botón Enviar clickeado");
    abrirModalConfirmacion();
});

$(document).on('click', '#btnGuardar', function (e) {
    e.preventDefault();
    console.log("Botón Guardar clickeado");
    guardarSolicitudSinJson();
});

$(document).on('click', '#btnModificar', function (e) {
    e.preventDefault();
    console.log("Botón Modificar clickeado");
    modificarSolicitud();
});