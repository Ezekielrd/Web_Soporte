// Se ejecuta cuando el documento está completamente cargado
$(document).ready(function () {
    var $switch = $('#switchActivo');
    var $label = $('.labelEstado');
    // Función para actualizar el texto del label
    function actualizarEstado() {
        if ($switch.prop('checked')) {
            // Si el checkbox está marcado
            $label.text('Activo');
        } else {
            // Si el checkbox NO está marcado
            $label.text('Desactivo');
        }
    }
    // 1. Establecer el estado inicial del label al cargar la página
    actualizarEstado();
    // 2. Escuchar el evento 'change' del switch para actualizar el label dinámicamente
    $switch.on('change', function () {
        actualizarEstado();
    });

});