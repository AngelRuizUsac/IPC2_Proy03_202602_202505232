# Datos de demostración

Son datos sintéticos; no corresponden a una conexión con la SAT.

1. Inicializar el sistema si se desea una demostración vacía.
2. Enviar `01-rtu.json` como Actualizar RTU: 2 nuevos, 0 actualizados, 0 inválidos.
3. Enviar `02-autorizaciones.json` como Autorizar facturas: 3 aprobaciones. El prefijo del código será el día de recepción en Guatemala.
4. Consultar del 15 al 16 de enero de 2025: total 392.00; sin IVA 350.00.
5. Consultar IVA del 15 de enero: NIT 12345679 emite 36.00; NIT 76543218 recibe 36.00.
6. Enviar `03-errores.json`: 0 aprobaciones, 1 emisor inexistente y 1 receptor inexistente. En estadísticas del 15 de enero habrá uno de cada uno de los siete errores solicitados.
7. Enviar `04-actualizar-rtu.json`: 0 nuevos, 1 actualizado, 1 inválido.

Repetir archivos acumula solicitudes rechazadas por referencia duplicada; no es una operación idempotente. Los ejemplos originales del enunciado no se corrigen: algunos NIT pueden no superar su algoritmo. Estos archivos son ejemplos nuevos y separados.
