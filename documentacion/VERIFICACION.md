# Verificación ejecutada

Preparación local del 18 y 19 de octubre de 2026. Entorno: Windows, SDK .NET 10.0.401, compilación Release.

| Comprobación | Resultado |
|---|---|
| Compilación de la solución | 0 errores, 0 advertencias |
| Dominio, archivos y concurrencia | 20,027 comprobaciones correctas |
| API y frontend por HTTP | 70 comprobaciones correctas |
| Formularios multipart y protección | 12 comprobaciones correctas |
| Publicación y reinicios reales de proceso | 10 comprobaciones correctas |
| Ensayo PDF | 10 páginas: 5 de contenido y 5 anexos; revisión visual de todas |
| PDF de Ayuda | Mismo SHA-256 que documentacion/Ensayo.pdf, también en publicación |
| Navegador | Revisión de formulario, tabla y gráfica de IVA a ancho reducido |

## Qué se comprobó

- Generación y rechazo de verificadores NIT para 9,999 bases; límites de longitud, K, fechas válidas e inválidas y fracciones de centavo.
- RTU nuevo, actualización, registros inválidos y conservación de datos.
- Referencias únicas sobre aprobadas y rechazadas; persistencia de la restricción.
- Siete errores del enunciado y errores de formato; contratos exactos de respuesta JSON.
- Correlativo inicial, cambio de día, regreso a un día previo y continuación después de reiniciar.
- 25 operaciones concurrentes en el servicio y 24 solicitudes concurrentes HTTP, sin repetir autorizaciones.
- Escritura fallida sin publicar cambios en memoria; bloqueo de una segunda instancia sobre la misma carpeta.
- Reportes por NIT, dirección, fechas, montos con y sin IVA, resumen de IVA y estadísticas.
- Lotes JSON vacíos, sintaxis inválida, raíz incorrecta, elementos de tipo incorrecto y filtros inválidos.
- Envío multipart real desde Razor Pages, rechazo de extensión incorrecta y archivo vacío; SHA-256 del archivo original sin cambios.
- Rechazo de POST sin token antifalsificación y ausencia de borrado cuando no se marca la casilla de inicialización.
- PDF de cada petición, tablas, gráficas y documentación servida desde Ayuda.
- Publicación separada de ambos programas; disponibilidad de CSS y PDF; persistencia de datos, duplicados, correlativo e inicialización tras reinicios reales.

## Límites de estos resultados

No son pruebas exhaustivas, de carga masiva, de fallos de hardware ni de despliegue distribuido. El proyecto se ejecutó localmente; no se publicó en Internet. El backend trabaja con una sola instancia por carpeta de datos. El generador de PDF de reportes usa la fuente estándar Helvetica y codificación Latin-1, adecuada para español; caracteres fuera de ese repertorio no tienen cobertura completa en esa exportación. JSON y las vistas conservan Unicode.

Los archivos de prueba se mantuvieron en carpetas temporales separadas. Los resultados de proyecto 2 y práctica 3 no se reutilizan como evidencia de este proyecto.

No se comprobaron GitHub, releases, invitación del auxiliar ni UEDI: el usuario confirmó que aún no existe repositorio. Revisar REVISION_ENTREGA.md.
