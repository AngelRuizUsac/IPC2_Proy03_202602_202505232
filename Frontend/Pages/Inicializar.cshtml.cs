using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using proyecto3.Frontend.Services;

namespace proyecto3.Frontend.Pages
{
    public class InicializarModel : PageModel
    {
        private readonly ClienteApi cliente;
        [BindProperty] public bool Confirmar { get; set; }
        public string Aviso { get; private set; } = "";

        public InicializarModel(ClienteApi cliente)
        {
            this.cliente = cliente;
        }

        public async Task OnPostAsync()
        {
            if (!Confirmar)
            {
                Aviso = "Confirme que desea borrar todos los datos.";
                return;
            }
            try
            {
                await cliente.Enviar("api/inicializar", Array.Empty<byte>());
                Aviso = "El sistema está vacío.";
            }
            catch (HttpRequestException ex)
            {
                Aviso = ex.Message;
            }
            catch (TaskCanceledException)
            {
                Aviso = "No se recibió respuesta. Consulte el estado del sistema.";
            }
        }
    }
}
