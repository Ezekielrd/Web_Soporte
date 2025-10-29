
    // Debe ejecutarse DESPUÉS de cargar jquery.validate + unobtrusive
    $(function () {
      $.validator.setDefaults({
        // jQuery Validate pondrá/quitara estas clases en cada control
        highlight: function (element) {
          $(element).addClass('input-validation-error').removeClass('valid');
        },
        unhighlight: function (element) {
          $(element).removeClass('input-validation-error').addClass('valid');
        }
      });

      // (Opcional) Si la vista estaba renderizada antes, re-parsea por si acaso:
      $('form').each(function() {
        var $f = $(this);
        $f.removeData('validator').removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse($f);
      });
    });
