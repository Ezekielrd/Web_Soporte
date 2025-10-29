let currentForm = null; // guardará el form de la fila que abrió el modal

function openResetModal(btn) {
    currentForm = btn.closest('form'); // form de la fila
    // limpiar el modal cada vez que se abre
    document.getElementById('pwdInput').value = '';
    document.getElementById('pwdError').classList.add('d-none');
}

function submitPasswordReset() {
    if (!currentForm) return;

    const passwordInput = document.getElementById('pwdInput');
    const errorDisplay = document.getElementById('pwdError');
    const pwd = passwordInput.value.trim();

    if (pwd.length < 8) {
        errorDisplay.classList.remove('d-none');
        passwordInput.focus();
        return;
    }
    errorDisplay.classList.add('d-none');

    // Asignar al hidden de ESTE form (sin usar id duplicados)
    const hidden = currentForm.querySelector('input[name="newPassword"]');
    hidden.value = pwd;

    // Cerrar modal y enviar
    const modalEl = document.getElementById('passwordResetModal');
    const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
    modal.hide();

    currentForm.submit();
    currentForm = null; // limpiar referencia
}

// DataTable (ok)
document.addEventListener('DOMContentLoaded', () => {
    new DataTable('#example', {
        searching: false,
        language: { url: 'https://cdn.datatables.net/plug-ins/2.1.4/i18n/es-ES.json' }
    });
});