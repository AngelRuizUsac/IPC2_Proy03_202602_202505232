const enlaces = document.querySelectorAll('.navegacion a');
let rutaActual = window.location.pathname.toLowerCase();
if (rutaActual === '/index') {
    rutaActual = '/';
}
for (const enlace of enlaces) {
    let rutaEnlace = new URL(enlace.href).pathname.toLowerCase();
    if (rutaEnlace === '/index') {
        rutaEnlace = '/';
    }
    if (rutaActual === rutaEnlace) {
        enlace.classList.add('activo');
        enlace.setAttribute('aria-current', 'page');
    }
}

const formulario = document.getElementById('formulario-peticiones');
if (formulario !== null) {
    const tipo = document.getElementById('Tipo');
    const hasta = document.getElementById('Hasta');
    const nit = document.getElementById('Nit');
    const direccion = document.getElementById('Direccion');
    const sinIva = document.getElementById('SinIva');
    const etiqueta = document.getElementById('etiqueta-fecha');

    function mostrarCampo(campo, visible) {
        campo.parentElement.hidden = !visible;
        campo.disabled = !visible;
    }

    function actualizarFiltros() {
        const porContribuyente = tipo.value === 'contribuyente' || tipo.value === 'facturas';
        mostrarCampo(hasta, tipo.value !== 'iva');
        mostrarCampo(nit, porContribuyente);
        mostrarCampo(direccion, porContribuyente);
        mostrarCampo(sinIva, tipo.value === 'rango');
        etiqueta.textContent = 'Desde';
        if (tipo.value === 'iva') {
            etiqueta.textContent = 'Fecha';
        }
    }

    tipo.addEventListener('change', actualizarFiltros);
    window.addEventListener('pageshow', actualizarFiltros);
    actualizarFiltros();
}
