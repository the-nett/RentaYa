document.addEventListener('DOMContentLoaded', function () {
    const formulario = document.getElementById('miFormulario');
    const terminosCheckbox = document.getElementById('terminosCondiciones');
    const mensajeErrorDiv = document.getElementById('mensajeError');
            
    formulario.addEventListener('submit', function (event) {
        if (!terminosCheckbox.checked) {
            event.preventDefault(); // Evita que el formulario se envíe
            mensajeErrorDiv.classList.remove('d-none'); // Muestra el mensaje de error
        } else {
            mensajeErrorDiv.classList.add('d-none'); // Oculta el mensaje de error si estaba visible
            // Aquí iría la lógica para enviar el formulario si el checkbox está marcado
            //console.log('Formulario listo para ser enviado (simulado).');
            //formulario.submit(); 
        }   
    });     
});         