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
        } else {
            $panel.slideUp(200);
        }
    }

    // Ejecutar al cargar y al cambiar
    toggleCamposTecnico();
    $rol.on("change", toggleCamposTecnico);
});
