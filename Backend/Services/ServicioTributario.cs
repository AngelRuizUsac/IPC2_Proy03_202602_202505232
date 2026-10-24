using System.Globalization;
using System.Text;
using System.Text.Json;
using proyecto3.Models;

namespace proyecto3.Backend.Services
{
    public class ServicioTributario
    {
        private Estado estado;
        private readonly Repositorio repositorio;
        private readonly object bloqueo = new object();

        public ServicioTributario(Repositorio repositorio)
        {
            this.repositorio = repositorio;
            estado = repositorio.Cargar();
        }

        public static DateTime Ahora()
        {
            return DateTime.UtcNow.AddHours(-6);
        }

        public string ActualizarRtu(JsonElement mensaje)
        {
            lock (bloqueo)
            {
                Estado siguiente = Estado.Importar(estado.Exportar());
                int nuevos = 0;
                int actualizados = 0;
                int invalidos = 0;
                foreach (JsonElement elemento in mensaje.EnumerateArray())
                {
                    string nit = JsonDatos.Texto(elemento, "NIT");
                    string nombre = JsonDatos.Texto(elemento, "nombre");
                    if (!Validador.NitValido(nit))
                    {
                        invalidos++;
                        continue;
                    }
                    Contribuyente? existente = siguiente.Buscar(nit);
                    if (existente == null)
                    {
                        siguiente.Contribuyentes.Agregar(new Contribuyente { Nit = nit, Nombre = nombre });
                        nuevos++;
                    }
                    else
                    {
                        existente.Nombre = nombre;
                        actualizados++;
                    }
                }
                repositorio.Guardar(siguiente);
                estado = siguiente;
                return "{\"CONTRIBUYENTES_NUEVOS\":" + nuevos + ",\"CONTRIBUYENTES_ACTUALIZADOS\":" + actualizados + ",\"NITS_INVALIDOS\":" + invalidos + "}";
            }
        }

        public string Autorizar(JsonElement mensaje, DateTime recepcion)
        {
            lock (bloqueo)
            {
                Estado siguiente = Estado.Importar(estado.Exportar());
                long prefijo = long.Parse(recepcion.ToString("yyyyMMdd", CultureInfo.InvariantCulture)) * 100000000;
                long correlativo = 0;
                NodoFactura? anterior = siguiente.Facturas.Primero;
                while (anterior != null)
                {
                    if (anterior.Dato.Codigo / 100000000 == prefijo / 100000000)
                    {
                        correlativo = Math.Max(correlativo, anterior.Dato.Codigo % 100000000);
                    }
                    anterior = anterior.Siguiente;
                }
                int aprobadas = 0;
                int emisorInexistente = 0;
                int receptorInexistente = 0;
                ListaFacturas aprobaciones = new ListaFacturas();
                foreach (JsonElement elemento in mensaje.EnumerateArray())
                {
                    Factura factura = LeerFactura(elemento, recepcion);
                    factura.EmisorInvalido = !Validador.NitValido(factura.Emisor);
                    factura.ReceptorInvalido = !Validador.NitValido(factura.Receptor);
                    factura.EmisorInexistente = !factura.EmisorInvalido && siguiente.Buscar(factura.Emisor) == null;
                    factura.ReceptorInexistente = !factura.ReceptorInvalido && siguiente.Buscar(factura.Receptor) == null;
                    factura.IvaIncorrecto = factura.Iva != decimal.Round(factura.Valor * 0.12m, 2, MidpointRounding.AwayFromZero);
                    factura.TotalIncorrecto = factura.Total != factura.Valor + factura.Iva;
                    anterior = siguiente.Facturas.Primero;
                    while (anterior != null)
                    {
                        if (factura.Referencia != "" && anterior.Dato.Referencia == factura.Referencia)
                        {
                            factura.ReferenciaDuplicada = true;
                            break;
                        }
                        anterior = anterior.Siguiente;
                    }
                    if (factura.EmisorInexistente)
                    {
                        emisorInexistente++;
                    }
                    if (factura.ReceptorInexistente)
                    {
                        receptorInexistente++;
                    }
                    if (!factura.TieneErrores())
                    {
                        if (correlativo == 99999999)
                        {
                            throw new InvalidOperationException("Se agotó el correlativo diario.");
                        }
                        correlativo++;
                        factura.Codigo = prefijo + correlativo;
                        aprobadas++;
                        aprobaciones.Agregar(factura);
                    }
                    siguiente.Facturas.Agregar(factura);
                }
                string respuesta = Respuesta(aprobaciones, aprobadas, emisorInexistente, receptorInexistente);
                repositorio.Guardar(siguiente);
                estado = siguiente;
                return respuesta;
            }
        }

        private Factura LeerFactura(JsonElement elemento, DateTime recepcion)
        {
            Factura factura = new Factura();
            factura.Recepcion = recepcion;
            if (!Validador.ExtraerFecha(JsonDatos.Texto(elemento, "TIEMPO"), out DateTime fecha))
            {
                factura.FormatoIncorrecto = true;
                fecha = recepcion;
            }
            factura.Fecha = fecha;
            factura.Referencia = JsonDatos.Texto(elemento, "REFERENCIA");
            factura.Emisor = JsonDatos.Texto(elemento, "NIT_EMISOR");
            factura.Receptor = JsonDatos.Texto(elemento, "NIT_RECEPTOR");
            bool valorCorrecto = JsonDatos.Numero(elemento, "VALOR", out decimal valor);
            bool ivaCorrecto = JsonDatos.Numero(elemento, "IVA", out decimal iva);
            bool totalCorrecto = JsonDatos.Numero(elemento, "TOTAL", out decimal total);
            if (!valorCorrecto || !ivaCorrecto || !totalCorrecto || !Validador.DineroValido(valor) || valor <= 0 || !Validador.DineroValido(iva) || !Validador.DineroValido(total) || string.IsNullOrWhiteSpace(factura.Referencia) || factura.Referencia.Length > 40)
            {
                factura.FormatoIncorrecto = true;
            }
            if (Validador.DineroValido(valor))
            {
                factura.Valor = valor;
            }
            if (Validador.DineroValido(iva))
            {
                factura.Iva = iva;
            }
            if (Validador.DineroValido(total))
            {
                factura.Total = total;
            }
            return factura;
        }

        private string Respuesta(ListaFacturas aprobaciones, int aprobadas, int emisor, int receptor)
        {
            using MemoryStream memoria = new MemoryStream();
            using (Utf8JsonWriter escritor = new Utf8JsonWriter(memoria))
            {
                escritor.WriteStartObject();
                escritor.WriteStartObject("resultadoAutorizaciones");
                escritor.WriteNumber("cantidadAutorizacionesAprobadas", aprobadas);
                escritor.WriteNumber("cantidadAutorizacionesRechazadasPorNitEmisorInexistente", emisor);
                escritor.WriteNumber("cantidadAutorizacionesRechazadasPorNitReceptorInexistente", receptor);
                escritor.WriteStartArray("detalleAprobaciones");
                NodoFactura? actual = aprobaciones.Primero;
                while (actual != null)
                {
                    escritor.WriteStartObject();
                    escritor.WriteString("referencia", actual.Dato.Referencia);
                    escritor.WriteNumber("codigoAprobacion", actual.Dato.Codigo);
                    escritor.WriteEndObject();
                    actual = actual.Siguiente;
                }
                escritor.WriteEndArray();
                escritor.WriteEndObject();
                escritor.WriteEndObject();
            }
            return Encoding.UTF8.GetString(memoria.ToArray());
        }

        public string ConsultarDatosActuales()
        {
            lock (bloqueo)
            {
                return estado.Exportar();
            }
        }

        public void Inicializar()
        {
            lock (bloqueo)
            {
                Estado vacio = new Estado();
                repositorio.Guardar(vacio);
                estado = vacio;
            }
        }

        public Reporte Consultar(string tipo, DateTime desde, DateTime hasta, string nit, string direccion, bool sinIva)
        {
            lock (bloqueo)
            {
                return new GeneradorReportes(estado).Generar(tipo, desde, hasta, nit, direccion, sinIva);
            }
        }
    }
}
