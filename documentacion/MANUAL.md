# Manual de uso

## Enviar mensaje

Iniciar los dos programas. Abrir **Enviar mensaje**, elegir Actualizar RTU o Autorizar facturas, seleccionar un `.json` y pulsar **Enviar**. La respuesta de la API aparece completa debajo del formulario. Los registros del RTU deben cargarse antes de autorizar facturas que los utilicen. El archivo seleccionado permanece intacto.

## Consultas

En **Peticiones**, elegir:

- **Consultar contribuyente:** NIT, dirección emitidas/recibidas, fecha inicial y final. Muestra cantidad autorizada y monto por día, más el total del rango.
- **Listado de facturas:** mismos filtros, con detalle de cada documento autorizado.
- **Resumen de IVA por fecha y NIT:** usar Desde como fecha; muestra IVA emitido y recibido y su gráfica.
- **Resumen por rango de fechas:** seleccionar Desde/Hasta y marcar Sin IVA si se desea el valor base. Incluye gráfica.
- **Estadísticas y errores:** indicar el rango. Se muestran solicitudes, siete errores del enunciado, errores de formato, aprobadas y cantidad de participantes distintos.

El NIT y la dirección se usan únicamente en las consultas de contribuyente y listado. Hasta no modifica el resumen de IVA de una sola fecha. **Descargar PDF** conserva los filtros y descarga datos y gráficas del reporte actual. En los resúmenes el total representa dinero; en estadísticas representa solicitudes recibidas.

## Ayuda

Muestra nombre, carné y sección, y el enlace al ensayo. El PDF incluye los diagramas obligatorios. La carpeta documentacion conserva la fuente editable en Markdown y los diagramas DOT, SVG y PNG.

## Inicializar

La página pide marcar una casilla antes de enviar la orden de borrado. Al confirmar se vacían RTU, historial y correlativos. El cambio permanece después de reiniciar. Los archivos de ejemplo no se borran.

## Solución de problemas

- Si aparece un error de conexión, iniciar Backend y comprobar `Api:Url` en Frontend/appsettings.json.
- Si un puerto está ocupado, detener la instancia anterior de este proyecto o configurar otros puertos en ambos programas.
- Si el backend no inicia por bloqueo, comprobar que no haya otra instancia usando la misma carpeta de datos.
- Si estado.json es ilegible, conservar una copia antes de repararlo. El programa no lo borra ni lo reinicia automáticamente.
- Después de reiniciar el frontend, recargar los formularios: su protección antifalsificación usa claves de la sesión del proceso.
- Si una solicitud tarda demasiado, consultar las estadísticas antes de reenviarla. Puede haberse guardado aunque el cliente no haya recibido la respuesta.

## Publicación local

Desde la carpeta de la solución:

```powershell
dotnet publish Backend/Backend.csproj -c Release -o publicacion/backend
dotnet publish Frontend/Frontend.csproj -c Release -o publicacion/frontend
```

Ejecutar `dotnet Backend.dll` dentro de publicacion/backend y `dotnet Frontend.dll` dentro de publicacion/frontend. Mantener ambas carpetas separadas. La documentación y Bootstrap viajan con el frontend. Es una publicación de archivos para ejecutar localmente, no una publicación en Internet ni en GitHub.
