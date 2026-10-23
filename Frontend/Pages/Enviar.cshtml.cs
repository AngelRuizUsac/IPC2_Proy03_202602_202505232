using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using proyecto3.Frontend.Services;

namespace proyecto3.Frontend.Pages
{
    public class EnviarModel : PageModel
    {
        private readonly ClienteApi cliente;
        [BindProperty] public IFormFile? Archivo { get; set; }
        [BindProperty] public string Tipo { get; set; } = "rtu";
        public string Respuesta { get; private set; } = "";
        public string Error { get; private set; } = "";

        public EnviarModel(ClienteApi cliente)
        {
            this.cliente = cliente;
        }

        public async Task OnPostAsync()
        {
            if (Archivo == null || Archivo.Length == 0 || !string.Equals(Path.GetExtension(Archivo.FileName), ".json", StringComparison.OrdinalIgnoreCase) || (Tipo != "rtu" && Tipo != "autorizaciones"))
            {
                Error = "Seleccione un archivo JSON y el tipo de solicitud.";
                return;
            }
            try
            {
                using MemoryStream memoria = new MemoryStream();
                await Archivo.CopyToAsync(memoria);
                Respuesta = await cliente.Enviar("api/" + Tipo, memoria.ToArray());
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
            }
            catch (TaskCanceledException)
            {
                Error = "La API tardó demasiado. Consulte las estadísticas antes de reenviar el archivo.";
            }
        }
    }
}
