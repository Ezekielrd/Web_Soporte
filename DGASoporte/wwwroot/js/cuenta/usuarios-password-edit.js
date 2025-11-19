// archivo: wwwroot/js/usuarios-password-edit.js
// Requiere: jQuery + jquery.validate + jquery.validate.unobtrusive
(function (window, $) {
    if (!$) return;

    // Sólo inicializamos helpers una vez
    let helpersInicializados = false;

    function initHelpers() {
        if (helpersInicializados) return;
        helpersInicializados = true;

        if (!$.validator || !$.validator.methods) return;

        // Polyfill pattern (por si falta)
        if (!$.validator.methods.pattern) {
            $.validator.addMethod('pattern', function (value, element, param) {
                if (this.optional(element)) return true;
                var regex = param instanceof RegExp ? param : new RegExp(param);
                return regex.test(value);
            }, 'Formato inválido.');
        }

        // Clases de estado
        $.validator.setDefaults({
            highlight: function (element) {
                $(element).addClass('input-validation-error')
                    .removeClass('valid');
            },
            unhighlight: function (element) {
                $(element).removeClass('input-validation-error')
                    .addClass('valid');
            }
        });

        // Métodos condicionales (validan solo si hay texto)
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
    }

    // ⚙ Función principal: aplicar lógica de "Password opcional" en forms de edición
    function initPasswordEdicion(root) {
        initHelpers();

        if (!$.validator || !$.validator.unobtrusive) return;

        // root puede ser document o el contenido del modal
        const $root = root ? $(root) : $(document);

        // Formularios de edición: deben tener class="form-edicion"
        const $forms = $root.find('form.form-edicion');
        if (!$forms.length) return;

        $forms.each(function () {
            const $form = $(this);

            // Re-parse unobtrusive sólo en este form
            $form.removeData('validator').removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse($form);

            const $pwd = $form.find('#Password, [name="Password"]').first();
            if (!$pwd.length) return;

            // Quitar "required" (permite dejarlo vacío)
            $pwd.removeAttr('data-val-required');
            try { $pwd.rules('remove', 'required'); } catch (e) { }

            // Leer parámetros existentes de DataAnnotations
            const min = +($pwd.attr('data-val-length-min') || $pwd.attr('minlength') || 0);
            const max = +($pwd.attr('data-val-length-max') || $pwd.attr('maxlength') || 0);
            const pattern = $pwd.attr('data-val-regex-pattern') || $pwd.attr('pattern') || null;

            const msgLen = $pwd.attr('data-val-length') || 'Longitud inválida.';
            const msgRegex = $pwd.attr('data-val-regex') || 'Formato inválido.';

            // Limpiar reglas previas
            try { $pwd.rules('remove'); } catch (e) { }

            // Agregar reglas condicionales
            const rules = {};
            const msgs = {};

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
            if (Object.keys(msgs).length) {
                rules.messages = msgs;
            }

            if (Object.keys(rules).length) {
                $pwd.rules('add', rules);
            }

            // Limpiar estado visual previo
            $pwd.removeClass('input-validation-error valid');
            const $span = $form.find('[data-valmsg-for="Password"]');
            $span.removeClass('field-validation-error')
                .addClass('field-validation-valid')
                .text('');
        });
    }

    // Ejecutar para la vista normal cuando se carga la página
    $(function () {
        initPasswordEdicion(document);
    });

    // Exponer para usarlo cuando cargamos el formulario en el modal
    window.Usuarios = window.Usuarios || {};
    window.Usuarios.initPasswordEdicion = initPasswordEdicion;

})(window, window.jQuery);
