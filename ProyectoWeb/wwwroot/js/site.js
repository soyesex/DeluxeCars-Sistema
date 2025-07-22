// Espera a que el documento esté listo
document.addEventListener('DOMContentLoaded', function () {

    // Inicializa la librería AOS con configuraciones recomendadas
    AOS.init({
        duration: 800,       // Duración de la animación en milisegundos
        easing: 'ease-in-out-quad', // Curva de aceleración para un efecto suave
        once: true,          // La animación solo ocurre una vez por elemento
        offset: 100,         // La animación se dispara 100px antes de que el elemento sea visible
    });

});