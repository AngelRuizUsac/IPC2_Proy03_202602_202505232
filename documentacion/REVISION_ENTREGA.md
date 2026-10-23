# Revisión de requisitos y penalizaciones

Fuente: `[IPC2]Proyecto_3_2S2026-v1 (1).pdf`, 9 páginas. Fecha de esta preparación: 19 de octubre de 2026. El PDF original de Downloads permanece intacto.

## Implementación local

| Requisito | Evidencia |
|---|---|
| POO en C# | Clases concretas en Dominio, Backend y Frontend |
| Frontend Razor Pages | Pages, PageModel y MapRazorPages; no se sustituyó por MVC |
| API .NET consumible por HTTP | Controlador independiente; colección Postman incluida |
| Dos programas comunicados por HTTP | ClienteApi; puertos predeterminados 5304 y 5303 |
| Persistencia en archivos | Repositorio, estado.json, carga al iniciar y guardado antes de responder |
| Expresiones regulares | Validador.ExtraerFecha y NitValido |
| TDA propios | Listas concretas de contribuyentes, facturas y filas |
| Actualizar RTU | Alta, actualización y omisión de NIT inválidos |
| Autorizaciones | NIT, RTU, valor, IVA, total, referencia y correlativo diario |
| Entrada sin modificar | Frontend reenvía bytes; no se edita el archivo de entrada |
| Salida con estructura obligatoria | Contratos exactos de RTU y resultadoAutorizaciones |
| Estadísticas del enunciado | Todos los contadores y participantes distintos por fecha |
| Listado por NIT/rango | Reportes contribuyente y facturas; emitidas y recibidas |
| Dos resúmenes y gráficas | IVA y rango; tablas y barras SVG |
| PDF de todas las peticiones | Exportación con filtros, datos, total y gráficas cuando corresponden |
| Ayuda e inicialización | Páginas dedicadas y orden HTTP de borrado |
| Ensayo de 4 a 7 páginas | 5 páginas de contenido; 5 anexos adicionales |
| Clases, actividades y despliegue | Diagramas incluidos en PDF y en fuentes DOT/SVG/PNG |
| Formato de ensayo proporcionado | Título en mayúsculas; resumen de 164 palabras; abstract paralelo; 5 palabras clave; introducción de 117 palabras; conclusiones y referencias |
| Documentación en carpeta separada | documentacion |

Los resultados ejecutados están en VERIFICACION.md. Esta matriz no certifica por sí misma una prueba exhaustiva ni una calificación.

## Pendientes externos que pueden impedir o penalizar la calificación

1. **No existe repositorio GitHub**, confirmado por el usuario. Crear uno llamado exactamente `IPC2_Proy03_202602_202505232` y subir los archivos de producto y documentación.
2. **No existen cuatro releases semanales verificados.** El enunciado exige cuatro, uno por semana, y establece penalización por faltantes. Cuatro ZIP o etiquetas creadas ahora no prueban ese avance semanal. No se fabricó historial ni se retrocedieron fechas.
3. **Colaborador pendiente:** invitar a `DanielVelAv` por corresponder a la sección C. No se ha enviado invitación.
4. **Release final pendiente:** la calificación se realiza sobre el cuarto release de GitHub. Revisar que incluya la solución, los archivos y el PDF definitivos antes de entregar. Los cambios posteriores no cuentan según el enunciado.
5. **UEDI pendiente:** entregar un archivo de texto con el enlace real del repositorio. No se generó un enlace ficticio ni se realizó un envío.
6. **Fecha límite del enunciado:** 22 de octubre a las 23:59; 2026 corresponde a la edición del documento. No se certifica entrega en plazo.
7. **IDE:** el PDF exige usar los IDEs/frameworks indicados, pero el texto no enumera un IDE específico. La solución está preparada para Visual Studio; no se afirma haber confirmado una autorización adicional del laboratorio.

## Alcance de esta preparación

Se preparó código nuevo orientado al enunciado y al estilo de los trabajos anteriores, en una carpeta independiente. No se reemplazó el proyecto 2, la práctica 3 ni los proyectos personales de Visual Studio. No se publicaron contenidos ni se alteró una entrega externa.

La documentación se generó directamente en PDF y se revisó renderizada. Se aplicó la estructura de la plantilla aportada anteriormente; no se afirma que sea una exportación idéntica del DOCX original. Las decisiones de interpretación están en API.md y pueden contrastarse con las aclaraciones del curso.
