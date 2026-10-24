using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using proyecto3.Models;

namespace proyecto3.Frontend.Services
{
    public class GeneradorPdf
    {
        private readonly PdfDocument documento = new PdfDocument();
        private XGraphics grafico;
        private double y = 42;

        static GeneradorPdf()
        {
            GlobalFontSettings.FontResolver = new FuentePdf();
        }

        public GeneradorPdf()
        {
            documento.Version = 14;
            grafico = NuevaPagina();
        }

        private XGraphics NuevaPagina()
        {
            PdfPage pagina = documento.AddPage();
            pagina.Width = XUnit.FromPoint(612);
            pagina.Height = XUnit.FromPoint(792);
            y = 42;
            return XGraphics.FromPdfPage(pagina);
        }

        private XFont Fuente(int tamano)
        {
            return new XFont("Proyecto3Unicode", tamano, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
        }

        private void Espacio(int alto)
        {
            if (y + alto > 747)
            {
                grafico.Dispose();
                grafico = NuevaPagina();
            }
        }

        private void Linea(string texto, int tamano = 10)
        {
            texto = texto.Replace("\r", " ").Replace("\n", " ");
            XFont fuente = Fuente(tamano);
            while (grafico.MeasureString(texto, fuente).Width > 528)
            {
                int corte = texto.Length - 1;
                while (corte > 1 && grafico.MeasureString(texto.Substring(0, corte), fuente).Width > 528)
                {
                    corte--;
                }
                if (char.IsHighSurrogate(texto[corte - 1]))
                {
                    corte--;
                }
                int espacio = texto.LastIndexOf(' ', corte - 1, corte);
                if (espacio > 0)
                {
                    corte = espacio;
                }
                Linea(texto.Substring(0, corte), tamano);
                texto = texto.Substring(corte).TrimStart();
            }
            Espacio(17);
            grafico.DrawString(texto, fuente, XBrushes.Black, new XPoint(42, y + tamano));
            y += 17;
        }

        public byte[] Crear(Reporte reporte)
        {
            Linea(reporte.Titulo, 13);
            Linea(reporte.Filtro);
            Linea("Proyecto 3 - Angel Daniel Ruiz Ramos - 202505232");
            Linea("");
            NodoFila? fila = reporte.Filas.Primero;
            if (fila == null)
            {
                Linea("No hay datos para esta consulta.");
            }
            while (fila != null)
            {
                Espacio(Math.Min(200, (fila.Dato.Celdas.Length + 2) * 17));
                Linea(fila.Dato.Etiqueta, 12);
                for (int i = 0; i < fila.Dato.Celdas.Length; i++)
                {
                    Linea(reporte.Encabezados[i] + ": " + fila.Dato.Celdas[i]);
                }
                Linea("");
                fila = fila.Siguiente;
            }
            Linea("Total: " + reporte.Total.ToString("F2", CultureInfo.InvariantCulture), 12);
            if (reporte.Serie1 != "")
            {
                Linea("");
                Linea("Gráfica: " + reporte.Serie1, 12);
                if (reporte.Serie2 != "")
                {
                    Linea("Azul: " + reporte.Serie1 + " / Gris: " + reporte.Serie2);
                }
                decimal maximo = 1;
                fila = reporte.Filas.Primero;
                while (fila != null)
                {
                    maximo = Math.Max(maximo, Math.Max(fila.Dato.Serie1, fila.Dato.Serie2));
                    fila = fila.Siguiente;
                }
                fila = reporte.Filas.Primero;
                while (fila != null)
                {
                    Espacio(90);
                    Linea(fila.Dato.Etiqueta);
                    Barra(fila.Dato.Serie1, maximo, XColor.FromArgb(13, 89, 217));
                    if (reporte.Serie2 != "")
                    {
                        Barra(fila.Dato.Serie2, maximo, XColor.FromArgb(102, 102, 102));
                    }
                    y += 10;
                    fila = fila.Siguiente;
                }
            }
            return Documento();
        }

        private void Barra(decimal valor, decimal maximo, XColor color)
        {
            double ancho = (double)(valor / maximo * 330);
            grafico.DrawRectangle(new XSolidBrush(color), 42, y, ancho, 12);
            grafico.DrawString(valor.ToString("F2", CultureInfo.InvariantCulture), Fuente(10), XBrushes.Black, new XPoint(385, y + 10));
            y += 21;
        }

        private byte[] Documento()
        {
            grafico.Dispose();
            for (int i = 0; i < documento.PageCount; i++)
            {
                using XGraphics pie = XGraphics.FromPdfPage(documento.Pages[i], XGraphicsPdfPageOptions.Append);
                pie.DrawString((i + 1) + " / " + documento.PageCount, Fuente(9), XBrushes.Black, new XPoint(290, 767));
            }
            using MemoryStream salida = new MemoryStream();
            documento.Save(salida, false);
            documento.Dispose();
            return salida.ToArray();
        }
    }
}
