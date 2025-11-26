(function () {
    const spanReloj = document.getElementById('reloj-header');
    if (!spanReloj) return;

    function dosDigitos(n) {
        return n.toString().padStart(2, '0');
    }

    function actualizarReloj() {
        const ahora = new Date();

        const dia = dosDigitos(ahora.getDate());
        const mes = dosDigitos(ahora.getMonth() + 1); // 0-11
        const anio = ahora.getFullYear();

        const horas = dosDigitos(ahora.getHours());
        const minutos = dosDigitos(ahora.getMinutes());
        const segundos = dosDigitos(ahora.getSeconds());

        // Formato: 19/11/2025 14:33:16
        spanReloj.textContent = `${dia}/${mes}/${anio} ${horas}:${minutos}:${segundos}`;
    }

    // Primera vez
    actualizarReloj();
    // Actualizar cada segundo
    setInterval(actualizarReloj, 1000);
})();