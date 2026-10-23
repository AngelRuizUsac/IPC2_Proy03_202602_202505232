using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
using proyecto3.Models;
using proyecto3.Backend.Services;

namespace proyecto3.Backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class TributarioController : ControllerBase
    {
        private readonly ServicioTributario servicio;

        public TributarioController(ServicioTributario servicio)
        {
            this.servicio = servicio;
        }

        [HttpGet("salud")]
        public IActionResult Salud()
        {
            return Content("{\"estado\":\"disponible\"}", "application/json");
        }

        [HttpPost("rtu")]
        public IActionResult Rtu([FromBody] JsonElement mensaje)
        {
            if (!EsArreglo(mensaje))
            {
                return BadRequest("Se requiere un arreglo JSON de objetos.");
            }
            return Content(servicio.ActualizarRtu(mensaje), "application/json");
        }

        [HttpPost("autorizaciones")]
        public IActionResult Autorizaciones([FromBody] JsonElement mensaje)
        {
            if (!EsArreglo(mensaje))
            {
                return BadRequest("Se requiere un arreglo JSON de objetos.");
            }
            return Content(servicio.Autorizar(mensaje, ServicioTributario.Ahora()), "application/json");
        }

        [HttpPost("inicializar")]
        public IActionResult Inicializar()
        {
            servicio.Inicializar();
            return Content("{\"estado\":\"inicializado\"}", "application/json");
        }

        [HttpGet("reportes/{tipo}")]
        public IActionResult Reportes(string tipo, string desde = "", string hasta = "", string nit = "", string direccion = "emitidas", bool sinIva = false)
        {
            if (tipo != "contribuyente" && tipo != "facturas" && tipo != "iva" && tipo != "rango" && tipo != "estadisticas")
            {
                return NotFound("Reporte desconocido.");
            }
            if (tipo == "iva")
            {
                hasta = desde;
            }
            if (!DateTime.TryParseExact(desde, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime inicio) || !DateTime.TryParseExact(hasta, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fin) || inicio > fin)
            {
                return BadRequest("Indique fechas válidas en formato yyyy-MM-dd y un rango ordenado.");
            }
            if ((tipo == "contribuyente" || tipo == "facturas") && (!Validador.NitValido(nit) || (direccion != "emitidas" && direccion != "recibidas")))
            {
                return BadRequest("Indique un NIT válido y dirección emitidas o recibidas.");
            }
            return Content(JsonDatos.ReporteJson(servicio.Consultar(tipo, inicio, fin, nit, direccion, sinIva)), "application/json");
        }

        private bool EsArreglo(JsonElement mensaje)
        {
            if (mensaje.ValueKind != JsonValueKind.Array)
            {
                return false;
            }
            foreach (JsonElement elemento in mensaje.EnumerateArray())
            {
                if (elemento.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
