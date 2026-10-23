using System.Globalization;
using proyecto3.Models;

namespace proyecto3.Backend.Services
{
    public class GeneradorReportes
    {
        private readonly Estado estado;

        public GeneradorReportes(Estado estado)
        {
            this.estado = estado;
        }

        private static string Moneda(decimal valor)
        {
            return valor.ToString("F2", CultureInfo.InvariantCulture);
        }

        public Reporte Generar(string tipo, DateTime desde, DateTime hasta, string nit, string direccion, bool sinIva)
        {
            Reporte reporte = new Reporte();
            reporte.Filtro = desde.ToString("dd/MM/yyyy") + " a " + hasta.ToString("dd/MM/yyyy");
            if (tipo == "iva")
            {
                reporte.Titulo = "Resumen de IVA por fecha y NIT";
                reporte.Filtro = desde.ToString("dd/MM/yyyy");
                reporte.Encabezados = new[] { "NIT", "Nombre", "IVA emitido", "IVA recibido" };
                reporte.Serie1 = "IVA emitido";
                reporte.Serie2 = "IVA recibido";
                NodoContribuyente? contribuyente = estado.Contribuyentes.Primero;
                while (contribuyente != null)
                {
                    decimal emitido = 0;
                    decimal recibido = 0;
                    NodoFactura? actual = estado.Facturas.Primero;
                    while (actual != null)
                    {
                        Factura f = actual.Dato;
                        if (f.Codigo > 0 && f.Fecha.Date == desde)
                        {
                            if (f.Emisor == contribuyente.Dato.Nit)
                            {
                                emitido += f.Iva;
                            }
                            if (f.Receptor == contribuyente.Dato.Nit)
                            {
                                recibido += f.Iva;
                            }
                        }
                        actual = actual.Siguiente;
                    }
                    reporte.Filas.Agregar(new Fila { Etiqueta = contribuyente.Dato.Nit, Celdas = new[] { contribuyente.Dato.Nit, contribuyente.Dato.Nombre, Moneda(emitido), Moneda(recibido) }, Serie1 = emitido, Serie2 = recibido });
                    reporte.Total += emitido;
                    contribuyente = contribuyente.Siguiente;
                }
                return reporte;
            }
            if (tipo == "estadisticas")
            {
                reporte.Titulo = "Estadísticas de solicitudes";
                reporte.Encabezados = new[] { "Fecha", "Recibidas", "Emisor inválido", "Receptor inválido", "Emisor inexistente", "Receptor inexistente", "IVA incorrecto", "TOTAL incorrecto", "Referencia duplicada", "Formato incorrecto", "Sin errores", "Emisores distintos", "Receptores distintos" };
            }
            else if (tipo == "contribuyente")
            {
                reporte.Titulo = "Facturas " + direccion + " del contribuyente " + nit;
                reporte.Encabezados = new[] { "Fecha", "Cantidad autorizada", "Total facturado" };
            }
            else if (tipo == "facturas")
            {
                reporte.Titulo = "Listado de facturas " + direccion + " del contribuyente " + nit;
                reporte.Encabezados = new[] { "Fecha", "Referencia", "Emisor", "Receptor", "Valor", "IVA", "Total", "Autorización" };
                NodoFactura? actual = estado.Facturas.Primero;
                while (actual != null)
                {
                    Factura f = actual.Dato;
                    if (f.Codigo > 0 && f.Fecha.Date >= desde && f.Fecha.Date <= hasta && Coincide(f, nit, direccion))
                    {
                        reporte.Filas.Agregar(new Fila { Etiqueta = f.Referencia, Celdas = new[] { f.Fecha.ToString("dd/MM/yyyy HH:mm"), f.Referencia, f.Emisor, f.Receptor, Moneda(f.Valor), Moneda(f.Iva), Moneda(f.Total), f.Codigo.ToString(CultureInfo.InvariantCulture) } });
                        reporte.Total += f.Total;
                    }
                    actual = actual.Siguiente;
                }
                return reporte;
            }
            else
            {
                reporte.Titulo = "Resumen por rango de fechas";
                reporte.Encabezados = new[] { "Fecha", "Valor autorizado" };
                reporte.Serie1 = "Total con IVA";
                if (sinIva)
                {
                    reporte.Serie1 = "Valor sin IVA";
                }
                reporte.Filtro += " | " + reporte.Serie1;
            }
            DateTime dia = desde;
            while (dia <= hasta)
            {
                int[] cantidades = new int[12];
                decimal monto = 0;
                NodoFactura? actual = estado.Facturas.Primero;
                while (actual != null)
                {
                    Factura f = actual.Dato;
                    if (f.Fecha.Date == dia)
                    {
                        cantidades[0]++;
                        if (f.EmisorInvalido)
                        {
                            cantidades[1]++;
                        }
                        if (f.ReceptorInvalido)
                        {
                            cantidades[2]++;
                        }
                        if (f.EmisorInexistente)
                        {
                            cantidades[3]++;
                        }
                        if (f.ReceptorInexistente)
                        {
                            cantidades[4]++;
                        }
                        if (f.IvaIncorrecto)
                        {
                            cantidades[5]++;
                        }
                        if (f.TotalIncorrecto)
                        {
                            cantidades[6]++;
                        }
                        if (f.ReferenciaDuplicada)
                        {
                            cantidades[7]++;
                        }
                        if (f.FormatoIncorrecto)
                        {
                            cantidades[8]++;
                        }
                        if (!string.IsNullOrEmpty(f.Emisor) && EsPrimero(actual, true))
                        {
                            cantidades[10]++;
                        }
                        if (!string.IsNullOrEmpty(f.Receptor) && EsPrimero(actual, false))
                        {
                            cantidades[11]++;
                        }
                        if (f.Codigo > 0 && (tipo != "contribuyente" || Coincide(f, nit, direccion)))
                        {
                            cantidades[9]++;
                            if (sinIva && tipo == "rango")
                            {
                                monto += f.Valor;
                            }
                            else
                            {
                                monto += f.Total;
                            }
                        }
                    }
                    actual = actual.Siguiente;
                }
                string fecha = dia.ToString("dd/MM/yyyy");
                string[] celdas;
                if (tipo == "estadisticas")
                {
                    celdas = new string[13];
                    celdas[0] = fecha;
                    for (int i = 0; i < cantidades.Length; i++)
                    {
                        celdas[i + 1] = cantidades[i].ToString(CultureInfo.InvariantCulture);
                    }
                    reporte.Total += cantidades[0];
                }
                else
                {
                    if (tipo == "contribuyente")
                    {
                        celdas = new[] { fecha, cantidades[9].ToString(CultureInfo.InvariantCulture), Moneda(monto) };
                    }
                    else
                    {
                        celdas = new[] { fecha, Moneda(monto) };
                    }
                    reporte.Total += monto;
                }
                reporte.Filas.Agregar(new Fila { Etiqueta = fecha, Celdas = celdas, Serie1 = monto });
                if (dia == hasta)
                {
                    break;
                }
                dia = dia.AddDays(1);
            }
            return reporte;
        }

        private bool EsPrimero(NodoFactura nodo, bool emisor)
        {
            NodoFactura? actual = estado.Facturas.Primero;
            while (actual != null && actual != nodo)
            {
                if (actual.Dato.Fecha.Date == nodo.Dato.Fecha.Date)
                {
                    if (emisor && actual.Dato.Emisor == nodo.Dato.Emisor)
                    {
                        return false;
                    }
                    if (!emisor && actual.Dato.Receptor == nodo.Dato.Receptor)
                    {
                        return false;
                    }
                }
                actual = actual.Siguiente;
            }
            return true;
        }

        private bool Coincide(Factura factura, string nit, string direccion)
        {
            if (direccion == "recibidas")
            {
                return factura.Receptor == nit;
            }
            return factura.Emisor == nit;
        }
    }
}
