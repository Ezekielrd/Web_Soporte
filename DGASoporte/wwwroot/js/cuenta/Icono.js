
document.addEventListener('DOMContentLoaded', () => {
    const input = document.getElementById('contrasena') || document.getElementById('contraseña');
    const btn = document.getElementById('togglePassword');
    if (!input || !btn) return;

    function updateBtn() { btn.hidden = input.value.trim().length === 0; }
    input.addEventListener('input', updateBtn);

    btn.addEventListener('click', () => {
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        btn.textContent = show ? '👀' : '👁';
    });

    // estado inicial (por autocompletar)
    updateBtn();
    setTimeout(updateBtn, 0);
});
