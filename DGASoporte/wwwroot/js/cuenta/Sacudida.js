
document.addEventListener('DOMContentLoaded', function () {
    var $form = $('#loginForm');
    if (!$form.length) return;

    // Validar SOLO en submit (no en focus/keyup)
    $.validator.unobtrusive.parse($form);
    var v = $form.data('validator');
    if (v) { v.settings.onkeyup = false; v.settings.onfocusout = false; v.settings.onclick = false; }

    function box(el) { return el.closest('.inp') || el; }
    function shakeInvalid() {
        var invalid = $form.find(':input').filter(function () { return !$(this).valid(); }).toArray();
        invalid.forEach(function (el) { box(el).classList.add('shake'); });
        setTimeout(function () { invalid.forEach(function (el) { box(el).classList.remove('shake'); }); }, 700);
        if (invalid.length) invalid[0].focus();
    }

    $form.on('submit', function (e) {
        if (!$form.valid()) {           // si hay errores, no envía
            e.preventDefault();
            shakeInvalid();
        }
    });
});
