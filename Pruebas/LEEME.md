# Pruebas

## Dominio y persistencia

Desde la solución: `dotnet run --project Pruebas/Pruebas.csproj -c Release`.

Incluye validadores, todos los errores, dinero, filtros, unicidad, cambio de día, persistencia, exclusión de otra instancia, rollback por error de escritura, concurrencia e inicialización. Usa una carpeta temporal exclusiva.

## HTTP complementario

Requiere Python 3, sin paquetes adicionales. **Usa exclusivamente los puertos de prueba 5313 y 5314 y reinicializa sus datos.** No dirigirlo a una instancia que contenga información de uso personal.

En una terminal de PowerShell, desde la raíz de la solución:

```powershell
$env:Datos__Carpeta = Join-Path $env:TEMP 'proyecto3-http-pruebas'
dotnet run --project Backend/Backend.csproj --no-launch-profile -- --urls http://localhost:5313
```

En otra terminal:

```powershell
$env:Api__Url = 'http://localhost:5313/'
dotnet run --project Frontend/Frontend.csproj --no-launch-profile -- --urls http://localhost:5314
```

En una tercera terminal, desde la raíz de la solución:

```powershell
python Pruebas/verificar_http.py
```

Los PDF de prueba se guardan en `tmp/proyecto3`. Detener las instancias con Ctrl+C al terminar. El script no modifica los ejemplos.
