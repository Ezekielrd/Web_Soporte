// Ejecutar después de jquery.validate + jquery.validate.unobtrusive
$(function () {
    //Polyfill de 'pattern' si no existe (para versiones sin additional-methods) ====
    if (!$.validator || !$.validator.methods) return; // seguridad
    if (!$.validator.methods.pattern) {
        $.validator.addMethod('pattern', function (value, element, param) {
            if (this.optional(element)) return true;
            var regex = param instanceof RegExp ? param : new RegExp(param);
            return regex.test(value);
        }, 'Formato inválido.');
    }

    //Clases de estado
    $.validator.setDefaults({
        highlight: function (element) {
            $(element).addClass('input-validation-error').removeClass('valid');
        },
        unhighlight: function (element) {
            $(element).removeClass('input-validation-error').addClass('valid');
        }
    });

    //Re-parse unobtrusive
    $('form').each(function () {
        var $f = $(this);
        $f.removeData('validator').removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse($f);
    });

    //Metodos condicionales (validan solo si hay texto) ====
    if (!$.validator.methods.minlengthIfSet) {
        $.validator.addMethod('minlengthIfSet', function (value, element, param) {
            if (!value) return true;
            return $.validator.methods.minlength.call(this, value, element, param);
        });
    }
    if (!$.validator.methods.maxlengthIfSet) {
        $.validator.addMethod('maxlengthIfSet', function (value, element, param) {
            if (!value) return true;
            return $.validator.methods.maxlength.call(this, value, element, param);
        });
    }
    if (!$.validator.methods.patternIfSet) {
        $.validator.addMethod('patternIfSet', function (value, element, param) {
            if (!value) return true;
            return $.validator.methods.pattern.call(this, value, element, param);
        });
    }

    //Solo para formulario de EDICIÓN ====
    var $formEdicion = $('form.form-edicion');
    if ($formEdicion.length) {
        var $pwd = $formEdicion.find('#Password, [name="Password"]').first();
        if ($pwd.length) {
            //Quitar "required" (permite dejarlo vacío)
            $pwd.removeAttr('data-val-required');
            try { $pwd.rules('remove', 'required'); } catch (e) { }

            //Leer parámetros existentes de DataAnnotations
            var min = +($pwd.attr('data-val-length-min') || $pwd.attr('minlength') || 0);
            var max = +($pwd.attr('data-val-length-max') || $pwd.attr('maxlength') || 0);
            var pattern = $pwd.attr('data-val-regex-pattern') || $pwd.attr('pattern') || null;

            // Mensajes (si el unobtrusive los genero)
            var msgLen = $pwd.attr('data-val-length') || 'Longitud inválida.';
            var msgRegex = $pwd.attr('data-val-regex') || 'Formato inválido.';

            //Limpiar reglas duplicadas
            try { $pwd.rules('remove'); } catch (e) { }

            //Agregar reglas condicionales (si hay algo escrito)
            var rules = {};
            var msgs = {};

            if (min > 0) {
                rules.minlengthIfSet = min;
                msgs.minlengthIfSet = msgLen;
            }
            if (max > 0) {
                rules.maxlengthIfSet = max;
                msgs.maxlengthIfSet = msgLen;
            }
            if (pattern) {
                rules.patternIfSet = pattern;
                msgs.patternIfSet = msgRegex;
            }
            if (Object.keys(msgs).length) rules.messages = msgs;

            $pwd.rules('add', rules);

            //Limpiar estado visual previo
            $pwd.removeClass('input-validation-error valid');
            var $span = $formEdicion.find('[data-valmsg-for="Password"]');
            $span.removeClass('field-validation-error').addClass('field-validation-valid').text('');
        }
    }
});
