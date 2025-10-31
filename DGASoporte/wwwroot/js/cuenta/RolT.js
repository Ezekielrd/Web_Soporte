// Script para mostrar/ocultar campos de Técnico según el rol seleccionado
$(function () {
    const $rol = $("#IdRol");
    const $panel = $("#panelTecnico");
    const idRolTecnico = 2; // ← ID del rol que representa 'Técnico'

    function toggleCamposTecnico() {
        const seleccionado = parseInt($rol.val());
        const esTecnico = seleccionado === idRolTecnico;

        if (esTecnico) {
            $panel.slideDown(200);
            // Activar reglas de validación solo si es técnico
            $("[name='CodigoEmpleado']").rules("add", { required: true, maxlength: 50 });
            $("[name='Especialidad']").rules("add", { required: true, maxlength: 100 });
        } else {
            $panel.slideUp(200);
            // Remover validación si ya no es técnico
            $("[name='CodigoEmpleado']").rules("remove");
            $("[name='Especialidad']").rules("remove");
        }
    }

    // Ejecutar al cargar y al cambiar
    toggleCamposTecnico();
    $rol.on("change", toggleCamposTecnico);
});
