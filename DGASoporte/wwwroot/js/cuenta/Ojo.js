
window.addEventListener('DOMContentLoaded', () => {
    const inputs = document.querySelectorAll('.secure-password');
    const icons = document.querySelectorAll('.togglePassword');

    inputs.forEach((input, index) => {
        const icon = icons[index];
        if (!icon) return;

        // 👁 Ojo oculto por defecto
        icon.style.display = "none";

        // Función para mostrar/ocultar ojo según contenido
        function toggleEye() {
            if (input.value.length > 0) {
                icon.style.display = "inline";
            } else {
                icon.style.display = "none";
                // Siempre fuerza oculto si no hay texto
                input.dataset.allowPlain = '0';
                input.type = 'password';
                icon.textContent = '👁';
            }
        }

        // Mostrar mientras está presionado
        icon.addEventListener('mousedown', () => {
            input.dataset.allowPlain = '1';
            input.type = 'text';
            icon.textContent = '👀';
        });

        // Ocultar al soltar o salir
        const hide = () => {
            input.dataset.allowPlain = '0';
            input.type = 'password';
            icon.textContent = '👁';
        };

        icon.addEventListener('mouseup', hide);
        icon.addEventListener('mouseleave', hide);
        icon.addEventListener('blur', hide);

        // Al escribir → mostrar/ocultar ojo
        input.addEventListener('input', () => {
            toggleEye();
            // si se borra el texto, se fuerza oculto
            if (input.value.length === 0) hide();
        });

        // Estado inicial
        toggleEye();
    });
});

