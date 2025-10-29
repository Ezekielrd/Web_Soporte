// Cambia el comportamiento global de jQuery Validate para poner/quitar clases
$.validator.setDefaults({
    highlight: function (element) {
        $(element).addClass('input-validation-error').removeClass('valid');
    },
    unhighlight: function (element) {
        $(element).removeClass('input-validation-error').addClass('valid');
    }
});