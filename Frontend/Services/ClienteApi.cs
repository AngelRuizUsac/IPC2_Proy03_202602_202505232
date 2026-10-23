using System.Net.Http.Headers;

namespace proyecto3.Frontend.Services
{
    public class ClienteApi : IDisposable
    {
        private readonly HttpClient cliente;

        public ClienteApi(string url)
        {
            cliente = new HttpClient();
            cliente.BaseAddress = new Uri(url);
            cliente.Timeout = TimeSpan.FromMinutes(2);
        }

        public async Task<string> Enviar(string ruta, byte[] contenido)
        {
            using ByteArrayContent cuerpo = new ByteArrayContent(contenido);
            cuerpo.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using HttpResponseMessage respuesta = await cliente.PostAsync(ruta, cuerpo);
            string texto = await respuesta.Content.ReadAsStringAsync();
            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HttpRequestException("La API respondió " + (int)respuesta.StatusCode + ": " + texto);
            }
            return texto;
        }

        public async Task<string> Consultar(string ruta)
        {
            using HttpResponseMessage respuesta = await cliente.GetAsync(ruta);
            string texto = await respuesta.Content.ReadAsStringAsync();
            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HttpRequestException("La API respondió " + (int)respuesta.StatusCode + ": " + texto);
            }
            return texto;
        }

        public void Dispose()
        {
            cliente.Dispose();
        }
    }
}
