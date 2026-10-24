using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using proyecto3.Models;
using proyecto3.Frontend.Services;

namespace proyecto3.Frontend.Pages
{
    public class DatosModel : PageModel
    {
        private readonly ClienteApi cliente;
        public ListaContribuyentes Contribuyentes { get; } = new ListaContribuyentes();
        public ListaFacturas Facturas { get; } = new ListaFacturas();
        public int CantidadContribuyentes { get; private set; }
        public int Aprobadas { get; private set; }
        public int Rechazadas { get; private set; }
        public string Error { get; private set; } = "";

        public DatosModel(ClienteApi cliente)
        {
            this.cliente = cliente;
        }

        public async Task OnGetAsync()
        {
            try
            {
                string json = await cliente.Consultar("api/datos");
                using JsonDocument documento = JsonDocument.Parse(json);
                foreach (JsonElement elemento in documento.RootElement.GetProperty("contribuyentes").EnumerateArray())
                {
                    Contribuyente? dato = (Contribuyente?)JsonSerializer.Deserialize(elemento.GetRawText(), typeof(Contribuyente));
                    if (dato != null)
                    {
                        Contribuyentes.Agregar(dato);
                        CantidadContribuyentes++;
                    }
                }
                foreach (JsonElement elemento in documento.RootElement.GetProperty("facturas").EnumerateArray())
                {
                    Factura? dato = (Factura?)JsonSerializer.Deserialize(elemento.GetRawText(), typeof(Factura));
                    if (dato != null)
                    {
                        Facturas.Agregar(dato);
                        if (dato.TieneErrores())
                        {
                            Rechazadas++;
                        }
                        else
                        {
                            Aprobadas++;
                        }
                    }
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
        }

        public string Motivos(Factura factura)
        {
            string motivos = "";
            if (factura.EmisorInvalido)
            {
                motivos += "NIT emisor inválido. ";
            }
            if (factura.ReceptorInvalido)
            {
                motivos += "NIT receptor inválido. ";
            }
            if (factura.EmisorInexistente)
            {
                motivos += "Emisor inexistente. ";
            }
            if (factura.ReceptorInexistente)
            {
                motivos += "Receptor inexistente. ";
            }
            if (factura.IvaIncorrecto)
            {
                motivos += "IVA incorrecto. ";
            }
            if (factura.TotalIncorrecto)
            {
                motivos += "Total incorrecto. ";
            }
            if (factura.ReferenciaDuplicada)
            {
                motivos += "Referencia duplicada. ";
            }
            if (factura.FormatoIncorrecto)
            {
                motivos += "Formato incorrecto. ";
            }
            return motivos;
        }
    }
}
