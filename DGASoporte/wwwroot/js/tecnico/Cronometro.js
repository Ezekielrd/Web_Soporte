// wwwroot/js/cronometro.js
(function ($) {
    // Lee datos que inyecta Razor
    const estado = $('#cronometroData').data('estado');
    const fechaInicio = $('#cronometroData').data('inicio'); // ISO string
    const segundosBase = parseInt($('#cronometroData').data('base'), 10) || 0;

    // Solo arranca si está "EnProceso" y hay fecha válida
    if (!fechaInicio || estado !== 'EnProceso') return;

    const inicio = new Date(fechaInicio); // ISO → Date válido
    let segundosTranscurridos = 0;

    function pintaReloj(totalSeg) {
        const h = String(Math.floor(totalSeg / 3600)).padStart(2, '0');
        const m = String(Math.floor((totalSeg % 3600) / 60)).padStart(2, '0');
        const s = String(totalSeg % 60).padStart(2, '0');
        $('#tiempoLabel').text(`${h}:${m}:${s}`);
    }

    // Primer pintado
    segundosTranscurridos = Math.floor((new Date() - inicio) / 1000);
    pintaReloj(segundosBase + segundosTranscurridos);

    // Actualización cada segundo
    setInterval(function () {
        segundosTranscurridos = Math.floor((new Date() - inicio) / 1000);
        pintaReloj(segundosBase + segundosTranscurridos);
    }, 1000);
})(jQuery);