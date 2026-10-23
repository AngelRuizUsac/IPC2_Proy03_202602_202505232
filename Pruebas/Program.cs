using System.Text.Json;
using proyecto3.Models;
using proyecto3.Backend.Services;
using proyecto3.Frontend.Services;

int comprobaciones = 0;
void Verificar(bool condicion, string detalle)
{
    comprobaciones++;
    if (!condicion)
    {
        throw new Exception(detalle);
    }
}
string Nit(string baseNit)
{
    int suma = 0;
    for (int i = 0; i < baseNit.Length; i++)
    {
        suma += (baseNit[i] - '0') * (baseNit.Length - i + 1);
    }
    int v = (11 - suma % 11) % 11;
    if (v == 10)
    {
        return baseNit + "K";
    }
    return baseNit + v;
}
JsonElement Json(string texto)
{
    using JsonDocument documento = JsonDocument.Parse(texto);
    return documento.RootElement.Clone();
}
string FacturaJson(string referencia, string emisor, string receptor, string fecha = "Guatemala, 15/01/2025 15:25 hrs.", decimal valor = 100, decimal iva = 12, decimal total = 112)
{
    return JsonDatos.Serializar(new { TIEMPO = fecha, REFERENCIA = referencia, NIT_EMISOR = emisor, NIT_RECEPTOR = receptor, VALOR = valor, IVA = iva, TOTAL = total });
}
long Codigo(string respuesta)
{
    return Json(respuesta).GetProperty("resultadoAutorizaciones").GetProperty("detalleAprobaciones")[0].GetProperty("codigoAprobacion").GetInt64();
}
int Aprobadas(string respuesta)
{
    return Json(respuesta).GetProperty("resultadoAutorizaciones").GetProperty("cantidadAutorizacionesAprobadas").GetInt32();
}

for (int i = 1; i < 10000; i++)
{
    string correcto = Nit(i.ToString());
    Verificar(Validador.NitValido(correcto), "NIT válido " + correcto);
    char incorrecto = '0';
    if (correcto[correcto.Length - 1] == '0')
    {
        incorrecto = '1';
    }
    Verificar(!Validador.NitValido(correcto.Substring(0, correcto.Length - 1) + incorrecto), "Verificador incorrecto");
}

Verificar(Validador.NitValido(Nit("12345678901234567890")), "20 dígitos");
Verificar(!Validador.NitValido(Nit("123456789012345678901")), "21 dígitos rechazados");
Verificar(!Validador.NitValido("CF") && !Validador.NitValido("12-3") && !Validador.NitValido(""), "Formato NIT");
Verificar(Validador.ExtraerFecha("Texto arbitrario 29/02/2024 23:59 hrs.", out DateTime bisiesto) && bisiesto.Day == 29, "Fecha y texto extra");
Verificar(!Validador.ExtraerFecha("29/02/2025 23:59", out _) && !Validador.ExtraerFecha("01/01/2025 24:00", out _), "Fechas inválidas");
Verificar(!Validador.DineroValido(1.001m) && Validador.DineroValido(1.00m), "Decimales");
string carpeta = Path.Combine(Path.GetTempPath(), "proyecto3-prueba-" + Guid.NewGuid().ToString("N"));
string emisor = Nit("1234567");
string receptor = Nit("7654321");
DateTime dia = new DateTime(2026, 10, 18, 23, 59, 0);
DateTime fechaFactura = new DateTime(2025, 1, 15);
try
{
    using (Repositorio repositorio = new Repositorio(carpeta))
    {
        ServicioTributario servicio = new ServicioTributario(repositorio);
        string rtu = "[{\"NIT\":\"" + emisor + "\",\"nombre\":\"Empresa Águila\"},{\"NIT\":\"" + receptor + "\",\"nombre\":\"Cliente\"},{\"NIT\":\"INVALIDO\",\"nombre\":\"Error\"}]";
        JsonElement respuesta = Json(servicio.ActualizarRtu(Json(rtu)));
        Verificar(respuesta.GetProperty("CONTRIBUYENTES_NUEVOS").GetInt32() == 2 && respuesta.GetProperty("NITS_INVALIDOS").GetInt32() == 1, "RTU alta");
        Verificar(Json(servicio.ActualizarRtu(Json(rtu))).GetProperty("CONTRIBUYENTES_ACTUALIZADOS").GetInt32() == 2, "RTU actualizar");
        string aprobado = servicio.Autorizar(Json("[" + FacturaJson("A1", emisor, receptor) + "]"), dia);
        Verificar(Codigo(aprobado) == 2026101800000001, "Primer correlativo por recepción");
        Verificar(Aprobadas(servicio.Autorizar(Json("[" + FacturaJson("A1", emisor, receptor) + "]"), dia)) == 0, "Referencia duplicada");
        Verificar(Codigo(servicio.Autorizar(Json("[" + FacturaJson("A2", emisor, receptor) + "]"), dia.AddMinutes(1))) == 2026101900000001, "Nuevo día");
        Verificar(Codigo(servicio.Autorizar(Json("[" + FacturaJson("A3", emisor, receptor) + "]"), dia)) == 2026101800000002, "Regreso a día anterior");
        string lote = "[" + FacturaJson("E1", "x", receptor) + "," + FacturaJson("E2", emisor, "x") + "," + FacturaJson("E3", Nit("999"), receptor) + "," + FacturaJson("E4", emisor, Nit("998")) + "," + FacturaJson("E5", emisor, receptor, iva: 11, total: 111) + "," + FacturaJson("E6", emisor, receptor, total: 100) + "," + FacturaJson("E7", emisor, receptor, fecha: "inválida") + "]";
        Verificar(Aprobadas(servicio.Autorizar(Json(lote), dia)) == 0, "Rechazos");
        Verificar(Aprobadas(servicio.Autorizar(Json("[" + FacturaJson("E1", emisor, receptor, fecha: "16/01/2025 12:00") + "]"), dia)) == 0, "Referencia de solicitud rechazada tampoco se reutiliza");
        Reporte estadisticas = servicio.Consultar("estadisticas", fechaFactura, fechaFactura, "", "", false);
        string[] c = estadisticas.Filas.Primero!.Dato.Celdas;
        Verificar(c[1] == "10" && c[2] == "1" && c[3] == "1" && c[4] == "1" && c[5] == "1" && c[6] == "1" && c[7] == "1" && c[8] == "1" && c[10] == "3", "Todas las estadísticas");
        Verificar(servicio.Consultar("rango", fechaFactura, fechaFactura, "", "", false).Total == 336, "Rango total");
        Verificar(servicio.Consultar("rango", fechaFactura, fechaFactura, "", "", true).Total == 300, "Rango sin IVA");
        Verificar(servicio.Consultar("contribuyente", fechaFactura, fechaFactura, receptor, "recibidas", false).Total == 336, "Recibidas");
        Verificar(servicio.Consultar("contribuyente", fechaFactura, fechaFactura, receptor, "emitidas", false).Total == 0, "Emitidas vacías");
        Reporte iva = servicio.Consultar("iva", fechaFactura, fechaFactura, "", "", false);
        Verificar(iva.Total == 36 && iva.Filas.Primero!.Dato.Serie1 == 36 && iva.Filas.Primero.Siguiente!.Dato.Serie2 == 36, "IVA por NIT");
        byte[] pdf = new GeneradorPdf().Crear(iva);
        Verificar(System.Text.Encoding.ASCII.GetString(pdf, 0, 8) == "%PDF-1.4", "PDF");
        bool bloqueado = false;
        try
        {
            using Repositorio otro = new Repositorio(carpeta);
        }
        catch (IOException)
        {
            bloqueado = true;
        }
        Verificar(bloqueado, "Bloqueo entre procesos");
        using FileStream ocupado = new FileStream(Path.Combine(carpeta, "estado.json.tmp"), FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        bool falloGuardado = false;
        try
        {
            servicio.Autorizar(Json("[" + FacturaJson("NO-GUARDADA", emisor, receptor) + "]"), dia);
        }
        catch (IOException)
        {
            falloGuardado = true;
        }
        Verificar(falloGuardado && servicio.Consultar("rango", fechaFactura, fechaFactura, "", "", false).Total == 336, "Sin cambios en memoria tras error de guardado");
    }
    using (Repositorio repositorio = new Repositorio(carpeta))
    {
        ServicioTributario servicio = new ServicioTributario(repositorio);
        Verificar(Codigo(servicio.Autorizar(Json("[" + FacturaJson("A4", emisor, receptor) + "]"), dia)) == 2026101800000003, "Persistencia de correlativo y RTU");
        Verificar(Aprobadas(servicio.Autorizar(Json("[" + FacturaJson("A1", emisor, receptor) + "]"), dia)) == 0, "Persistencia referencia");
        Parallel.For(0, 25, i => servicio.Autorizar(Json("[" + FacturaJson("P" + i, emisor, receptor) + "]"), dia));
        Verificar(servicio.Consultar("contribuyente", fechaFactura, fechaFactura, emisor, "emitidas", false).Total == 29 * 112, "Concurrencia");
        Verificar(Codigo(servicio.Autorizar(Json("[" + FacturaJson("FINAL", emisor, receptor) + "]"), dia)) == 2026101800000029, "Correlativos concurrentes");
        servicio.Inicializar();
        Verificar(servicio.Consultar("iva", fechaFactura, fechaFactura, "", "", false).Filas.Primero == null, "Inicialización RTU");
    }
    using (Repositorio repositorio = new Repositorio(carpeta))
    {
        ServicioTributario servicio = new ServicioTributario(repositorio);
        Verificar(servicio.Consultar("rango", fechaFactura, fechaFactura, "", "", false).Total == 0, "Inicialización persistente");
    }
}
finally
{
    Directory.Delete(carpeta, true);
}
Console.WriteLine("Correctas: " + comprobaciones + " comprobaciones.");
