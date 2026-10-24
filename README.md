# Proyecto 3 - Autorización de DTE

Angel Daniel Ruiz Ramos · 202505232 · Sección C

Solución local completa con frontend **Razor Pages**, backend **API ASP.NET Core**, clases de dominio y almacenamiento persistente en archivos. Es un simulador académico; no se conecta con la SAT.

## Abrir en Visual Studio

1. Instalar las herramientas de desarrollo web de ASP.NET y un SDK compatible con **.NET 10**. Este destino coincide con el entorno disponible y con el proyecto personal previamente verificado.
2. Abrir `Proyecto3.sln`.
3. En las propiedades de la solución, seleccionar **varios proyectos de inicio** y establecer **Iniciar** para Backend y Frontend. Dominio no se ejecuta.
4. Ejecutar. Frontend: `http://localhost:5304`. API: `http://localhost:5303/api/salud`.
5. Enviar los archivos de `ejemplos` siguiendo `ejemplos/LEEME.md`.

## Ejecutar desde dos terminales

En la carpeta de esta solución, ejecutar cada comando en una terminal distinta:

```powershell
dotnet run --project Backend/Backend.csproj
```

```powershell
dotnet run --project Frontend/Frontend.csproj
```

La configuración de la URL del backend está en `Frontend/appsettings.json`. Se puede sobrescribir con la variable `Api__Url`. El almacenamiento predeterminado es `Backend/datos`; `Datos__Carpeta` permite elegir otra carpeta. Detener ambos procesos con Ctrl+C en sus terminales. La sesión de Visual Studio del proyecto 2 no es necesaria.

## Organización

| Carpeta | Responsabilidad |
|---|---|
| Dominio | Contribuyente, Factura, listas concretas, validador y contrato interno de reportes |
| Backend | API HTTP, reglas de negocio, persistencia y consultas |
| Frontend | Razor Pages, envío de bytes JSON, tablas, SVG y generación PDF |
| documentacion | Ensayo PDF y Markdown, diagramas, API, manual y revisión de entrega |
| ejemplos | Archivos de entrada independientes del enunciado |
| Pruebas | Verificación reproducible de dominio, persistencia y concurrencia |

Los reportes PDF utilizan PDFsharp 6.2.4 (MIT), restaurado con NuGet, y una fuente Unicode incluida con licencia SIL OFL. La primera compilación requiere acceso a NuGet. Hay JavaScript local para navegación y filtros; no se usa base de datos ni Graphviz al ejecutar. Bootstrap se incluye localmente con su licencia original. Los diagramas ya están generados; sus fuentes DOT son editables.

La carpeta conserva un repositorio Git local con una confirmación real de esta preparación. No tiene remoto ni releases; ese historial no acredita avances semanales anteriores. El ZIP de distribución contiene los archivos de producto y excluye la carpeta interna `.git`.

## Compilar y verificar

```powershell
dotnet build Proyecto3.sln -c Release
dotnet run --project Pruebas/Pruebas.csproj -c Release
```

Las pruebas usan una carpeta temporal propia y la eliminan al finalizar. No utilizan los datos del backend de uso normal. `Pruebas/verificar_http.py` es una comprobación complementaria con Python 3 y los puertos de prueba 5313/5314; sus instrucciones están en `Pruebas/LEEME.md`.

## Documentación y entrega

El ensayo tiene 7 páginas con los cinco diagramas de clases, actividades y despliegue integrados junto a sus explicaciones, sin anexos separados. Ayuda abre una copia idéntica del PDF. El documento se generó en PDF siguiendo la estructura de la plantilla proporcionada; no es una exportación de Word ni se incluye un DOCX.

Consultar `documentacion/REVISION_ENTREGA.md` antes de publicar. El usuario confirmó que todavía no existe repositorio GitHub. La entrega externa NO está realizada: faltan repositorio, colaborador, releases y UEDI. Un ZIP local no satisface esos requisitos ni acredita cuatro semanas de avances.

## Datos actuales

La opción Datos actuales muestra todos los contribuyentes y todas las solicitudes de autorización guardadas, aprobadas y rechazadas, sin filtros de fecha o NIT. Incluye importes, fechas, códigos y motivos de rechazo; Actualizar vuelve a consultar el backend. No almacena historial de consultas de reportes ni recupera datos eliminados mediante Inicializar.
