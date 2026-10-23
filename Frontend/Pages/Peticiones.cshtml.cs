using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using proyecto3.Models;
using proyecto3.Frontend.Services;

namespace proyecto3.Frontend.Pages
{
    public class PeticionesModel : PageModel
    {
        private readonly ClienteApi cliente;
        [BindProperty(SupportsGet = true)] public string Tipo { get; set; } = "contribuyente";
        [BindProperty(SupportsGet = true)] public string Desde { get; set; } = DateTime.UtcNow.AddHours(-6).ToString("yyyy-MM-dd");
        [BindProperty(SupportsGet = true)] public string Hasta { get; set; } = DateTime.UtcNow.AddHours(-6).ToString("yyyy-MM-dd");
        [BindProperty(SupportsGet = true)] public string Nit { get; set; } = "";
        [BindProperty(SupportsGet = true)] public string Direccion { get; set; } = "emitidas";
        [BindProperty(SupportsGet = true)] public bool SinIva { get; set; }
        public Reporte? Resultado { get; private set; }
        public string Error { get; private set; } = "";
        public decimal Maximo { get; private set; } = 1;
        public string EnlacePdf { get; private set; } = "";

        public PeticionesModel(ClienteApi cliente)
        {
            this.cliente = cliente;
        }

        public async Task<IActionResult> OnGetAsync(bool consultar = false, bool pdf = false)
        {
            if (!consultar && !pdf)
            {
                return Page();
            }
            if (Nit == null)
            {
                Nit = "";
            }
            if (Desde == null || Hasta == null || Tipo == null || Direccion == null)
            {
                Error = "Complete el tipo de consulta, las fechas y la dirección.";
                return Page();
            }
            string parametros = "desde=" + Uri.EscapeDataString(Desde) + "&hasta=" + Uri.EscapeDataString(Hasta) + "&nit=" + Uri.EscapeDataString(Nit) + "&direccion=" + Uri.EscapeDataString(Direccion) + "&sinIva=" + SinIva.ToString().ToLowerInvariant();
            try
            {
                string json = await cliente.Consultar("api/reportes/" + Uri.EscapeDataString(Tipo) + "?" + parametros);
                Resultado = JsonDatos.LeerReporte(json);
                EnlacePdf = "/Peticiones?pdf=true&tipo=" + Uri.EscapeDataString(Tipo) + "&" + parametros;
                if (pdf)
                {
                    return File(new GeneradorPdf().Crear(Resultado), "application/pdf", "reporte-" + Tipo + ".pdf");
                }
                NodoFila? actual = Resultado.Filas.Primero;
                while (actual != null)
                {
                    Maximo = Math.Max(Maximo, Math.Max(actual.Dato.Serie1, actual.Dato.Serie2));
                    actual = actual.Siguiente;
                }
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
            }
            catch (TaskCanceledException)
            {
                Error = "La API no respondió a tiempo.";
            }
            return Page();
        }

        public string Ancho(decimal valor)
        {
            return (valor / Maximo * 360).ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
