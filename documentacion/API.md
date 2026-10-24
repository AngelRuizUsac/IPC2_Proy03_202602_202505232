# Contrato de la API

Base local: `http://localhost:5303`. Content-Type de solicitudes y respuestas de negocio: `application/json`.

| Método | Ruta | Función |
|---|---|---|
| GET | /api/salud | Comprobar disponibilidad |
| POST | /api/rtu | Actualizar RTU mediante arreglo JSON |
| POST | /api/autorizaciones | Procesar arreglo de solicitudes de autorización |
| POST | /api/inicializar | Borrar de forma persistente RTU e historial |
| GET | /api/reportes/contribuyente | Cantidad y total por fecha para NIT y dirección |
| GET | /api/reportes/facturas | Documentos autorizados individuales |
| GET | /api/reportes/iva | IVA emitido y recibido por NIT en una fecha |
| GET | /api/reportes/rango | Total diario con o sin IVA |
| GET | /api/reportes/estadisticas | Solicitudes, errores, aprobadas y NIT distintos |

## RTU

Enviar el contenido de `ejemplos/01-rtu.json` sin cambiar los nombres `NIT` y `nombre`.

```json
{"CONTRIBUYENTES_NUEVOS":2,"CONTRIBUYENTES_ACTUALIZADOS":0,"NITS_INVALIDOS":0}
```

Los NIT válidos existentes actualizan el nombre; los inválidos no modifican el RTU. Dentro de un lote se aplican los registros en orden.

## Autorizaciones

Entrada: arreglo con `TIEMPO`, `REFERENCIA`, `NIT_EMISOR`, `NIT_RECEPTOR`, `VALOR`, `IVA`, `TOTAL`. NIT y referencia son cadenas; los importes son números JSON. El frontend envía los bytes originales y no reescribe el archivo.

```json
{
  "resultadoAutorizaciones": {
    "cantidadAutorizacionesAprobadas": 1,
    "cantidadAutorizacionesRechazadasPorNitEmisorInexistente": 0,
    "cantidadAutorizacionesRechazadasPorNitReceptorInexistente": 0,
    "detalleAprobaciones": [
      {"referencia": "A1990", "codigoAprobacion": 2026101900000001}
    ]
  }
}
```

El código anterior es ilustrativo: su fecha real es la de recepción en Guatemala. Es un entero de 16 dígitos; al consumirlo desde JavaScript debe evitarse redondearlo si supera su entero seguro. La aplicación C# usa `long` y el frontend muestra el JSON como texto sin convertirlo a Number.

No se agregan claves al contrato obligatorio. Los demás rechazos se consultan en `/api/reportes/estadisticas`. Una factura puede acumular varios errores; por ello, los contadores de causas no deben sumarse para deducir el número de rechazos.

## Filtros de reportes

`desde` y `hasta`: `yyyy-MM-dd`, inclusivos. En IVA solo se utiliza `desde`. `nit`: requerido para contribuyente y facturas. `direccion`: `emitidas` o `recibidas`. `sinIva=true`: aplica al resumen por rango.

Ejemplo: `/api/reportes/contribuyente?desde=2025-01-15&hasta=2025-01-16&nit=12345679&direccion=emitidas`.

Los reportes devuelven `titulo`, `filtro`, `serie1`, `serie2`, `total`, `encabezados` y `filas`. Cada fila contiene `etiqueta`, `celdas`, `serie1` y `serie2`. Los encabezados definen el significado y el orden de las celdas. Los reportes monetarios excluyen solicitudes rechazadas. Las estadísticas incluyen todos los intentos.

## Errores de transporte

- 400: JSON ilegible, raíz distinta de arreglo, elemento distinto de objeto o filtros inválidos. No se guarda un lote estructuralmente ilegible.
- 404: ruta o tipo de reporte desconocido.
- 500: fallo de almacenamiento u otra operación no completada. El servicio conserva el estado anterior cuando falla el guardado.
- 200: lote procesado, incluso si algunas o todas sus facturas fueron rechazadas por negocio.

## Decisiones documentadas

- Regex extrae la primera fecha con formato `dd/MM/yyyy HH:mm`; se admite texto adicional antes y después.
- NIT sin guiones, 1 a 20 dígitos más verificador, `K` mayúscula. No se trata `CF` como excepción porque no la indica el enunciado. Se conservan ceros iniciales.
- Posiciones del NIT contadas desde la derecha: verificador en posición 1, primer multiplicador en posición 2.
- Referencia única sobre todo el historial persistente, incluidas solicitudes rechazadas; comparación exacta. Para corregir un rechazo se envía una referencia nueva.
- Importes `decimal`, positivos para VALOR y no negativos para IVA/TOTAL; máximo técnico 999999999999999999 por importe. Se aceptan 100 y 100.00 como el mismo importe; se rechaza una fracción distinta de centavos. No se modifica el JSON recibido.
- IVA = Round(VALOR * 0.12, 2, AwayFromZero). TOTAL = VALOR + IVA recibido; las dos reglas se evalúan separadamente.
- Fecha de autorización: recepción en Guatemala (UTC-6). Fecha de reporte: fecha extraída de TIEMPO. Si es ilegible, se registra error de formato bajo la recepción.
- NIT inválido y NIT válido inexistente se cuentan como categorías excluyentes por participante.
- Emisores/receptores distintos del reporte estadístico: identificadores no vacíos de todos los intentos. Los movimientos monetarios solo usan aprobaciones.
- El resumen de IVA muestra todos los contribuyentes del RTU, con cero si no tuvieron movimientos.
- La inicialización elimina también el historial de correlativos, como parte de volver al estado inicial. Por diseño puede reutilizar códigos que existían antes del borrado explícito.
- Una sola instancia escribe en cada carpeta de datos. No se promete coordinación distribuida ni recuperación automática de un archivo corrupto.

La API de demostración no incorpora autenticación, porque el enunciado no define usuarios. El acceso público a Internet requiere configurar alojamiento, HTTPS y control de acceso antes de exponer operaciones como inicializar; la entrega local no constituye un despliegue público.

## Consulta de datos actuales

`GET /api/datos` devuelve una instantánea completa bajo el bloqueo del servicio, con `version`, `contribuyentes` y `facturas`. Incluye solicitudes aprobadas y rechazadas, con todas las propiedades guardadas de Contribuyente y Factura. No aplica filtros ni modifica el almacenamiento. El frontend la presenta en `/Datos` con listas enlazadas propias. No hay un historial persistido de consultas de reportes ni de los lotes HTTP originales; se muestran los registros de autorización conservados. La fecha registrada es el valor almacenado: cuando la fecha original fue inválida, el procesamiento existente usó la recepción como alternativa y marcó FormatoIncorrecto.
