$(document).ready(function () {
    // 1. Toggle para mostrar/ocultar contraseña
    $('#togglePassword').click(function () {
        var passwordField = $(this).closest('.input-group').find('input[type="password"], input[type="text"]');
        var icon = $(this).find('i');

        if (passwordField.attr('type') === 'password') {
            passwordField.attr('type', 'text');
            icon.removeClass('bi-eye').addClass('bi-eye-slash');
        } else {
            passwordField.attr('type', 'password');
            icon.removeClass('bi-eye-slash').addClass('bi-eye');
        }
    });

    // 2. Validación de fortaleza de contraseña
    var passwordInput = $('input[type="password"][id$="Password"]:not([id*="Confirm"])');

    passwordInput.on('keyup', function () {
        var password = $(this).val();
        var strengthScore = 0;

        // Criterios de fortaleza
        if (password.length >= 8) strengthScore++;
        if (/[A-Z]/.test(password)) strengthScore++;
        if (/[a-z]/.test(password)) strengthScore++;
        if (/[0-9]/.test(password)) strengthScore++;
        if (/[^A-Za-z0-9]/.test(password)) strengthScore++;

        // Limpiar indicadores anteriores
        $(this).removeClass('is-invalid is-valid border-warning border-success');
        $('#passwordStrengthFeedback').remove();

        // Si hay contenido, mostrar nivel de fortaleza
        if (password.length > 0) {
            if (strengthScore < 3) {
                // Débil
                $(this).addClass('is-invalid');
                $(this).after('<div id="passwordStrengthFeedback" class="invalid-feedback">Contraseña débil. Use al menos 8 caracteres incluyendo mayúsculas, minúsculas, números y símbolos.</div>');
            } else if (strengthScore < 5) {
                // Media
                $(this).addClass('border-warning');
                $(this).after('<div id="passwordStrengthFeedback" class="valid-feedback text-warning">Contraseña media. Añada más variedad de caracteres para mayor seguridad.</div>');
            } else {
                // Fuerte
                $(this).addClass('border-success');
                $(this).after('<div id="passwordStrengthFeedback" class="valid-feedback text-success">Contraseña fuerte.</div>');
            }
        }
    });
});