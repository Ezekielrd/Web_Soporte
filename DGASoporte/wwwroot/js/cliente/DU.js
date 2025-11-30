document.addEventListener('DOMContentLoaded', function () {
    const root = document;

    const ddlDivision = root.querySelector('#DivisionId');
    const ddlUnidad = root.querySelector('#UnidadId');
    const grupoUnidad = root.querySelector('#grupoUnidad');

    if (!ddlDivision || !ddlUnidad || !grupoUnidad) {
        return; // esta vista no tiene esos controles
    }

    const opcionDefault = ddlUnidad.querySelector('option[value=""]');
    const todasOpcionesUnidad = Array.from(
        ddlUnidad.querySelectorAll('option[data-division]')
    );

    const unidadInicial = ddlUnidad.value; // ej. "5" cuando viene de la solicitud

    function actualizarUnidades(keepSelected) {
        const divisionId = ddlDivision.value;

        // Limpia el select y deja solo la opción por defecto
        ddlUnidad.innerHTML = '';
        if (opcionDefault) {
            ddlUnidad.appendChild(opcionDefault.cloneNode(true));
        }

        let opcionesFiltradas;

        if (!divisionId) {
            // SIN división seleccionada: solo unidades SIN división
            opcionesFiltradas = todasOpcionesUnidad.filter(
                opt => !opt.dataset.division
            );
        } else {
            // Con división: solo unidades de esa división
            opcionesFiltradas = todasOpcionesUnidad.filter(
                opt => opt.dataset.division === divisionId
            );
        }

        opcionesFiltradas.forEach(opt => {
            ddlUnidad.appendChild(opt.cloneNode(true));
        });

        ddlUnidad.disabled = opcionesFiltradas.length === 0;

        if (keepSelected && unidadInicial) {
            ddlUnidad.value = unidadInicial;
        } else {
            ddlUnidad.value = "";
        }
    }

    // Al cargar la vista
    actualizarUnidades(true);

    // Al cambiar la división
    ddlDivision.addEventListener('change', () => actualizarUnidades(false));
});
