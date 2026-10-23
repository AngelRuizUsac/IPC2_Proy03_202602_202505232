using System.Globalization;
using System.Text;
using proyecto3.Models;

namespace proyecto3.Frontend.Services
{
    public class PaginaPdf
    {
        public StringBuilder Contenido { get; } = new StringBuilder();
        public PaginaPdf? Siguiente { get; set; }
    }

    public class GeneradorPdf
    {
        private PaginaPdf primera = new PaginaPdf();
        private PaginaPdf actual;
        private int cantidad = 1;
        private int y = 750;

        public GeneradorPdf()
        {
            actual = primera;
        }

        private void Espacio(int alto)
        {
            if (y - alto < 45)
            {
                actual.Siguiente = new PaginaPdf();
                actual = actual.Siguiente;
                cantidad++;
                y = 750;
            }
        }

        private string Escapar(string texto)
        {
            return texto.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("\r", " ").Replace("\n", " ");
        }

        private void Linea(string texto, int tamano = 10)
        {
            int limite = 50;
            if (tamano > 10)
            {
                limite = 38;
            }
            while (texto.Length > limite)
            {
                int corte = texto.LastIndexOf(' ', limite);
                if (corte < 1)
                {
                    corte = limite;
                }
                Linea(texto.Substring(0, corte), tamano);
                texto = texto.Substring(corte).TrimStart();
            }
            Espacio(17);
            actual.Contenido.Append("0 0 0 rg BT /F1 ").Append(tamano).Append(" Tf 42 ").Append(y).Append(" Td (").Append(Escapar(texto)).Append(") Tj ET\n");
            y -= 17;
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
                    Barra(fila.Dato.Serie1, maximo, "0.05 0.35 0.85");
                    if (reporte.Serie2 != "")
                    {
                        Barra(fila.Dato.Serie2, maximo, "0.4 0.4 0.4");
                    }
                    y -= 10;
                    fila = fila.Siguiente;
                }
            }
            return Documento();
        }

        private void Barra(decimal valor, decimal maximo, string color)
        {
            string ancho = (valor / maximo * 330).ToString("0.##", CultureInfo.InvariantCulture);
            actual.Contenido.Append(color).Append(" rg 42 ").Append(y - 3).Append(' ').Append(ancho).Append(" 12 re f\n");
            actual.Contenido.Append("0 0 0 rg BT /F1 10 Tf 385 ").Append(y).Append(" Td (").Append(valor.ToString("F2", CultureInfo.InvariantCulture)).Append(") Tj ET\n");
            y -= 21;
        }

        private byte[] Documento()
        {
            using MemoryStream salida = new MemoryStream();
            int objetos = 3 + cantidad * 2;
            long[] posiciones = new long[objetos + 1];
            Escribir(salida, "%PDF-1.4\n");
            Objeto(salida, posiciones, 1, "<< /Type /Catalog /Pages 2 0 R >>");
            StringBuilder hijos = new StringBuilder();
            for (int i = 0; i < cantidad; i++)
            {
                hijos.Append(4 + i * 2).Append(" 0 R ");
            }
            Objeto(salida, posiciones, 2, "<< /Type /Pages /Count " + cantidad + " /Kids [" + hijos + "] >>");
            Objeto(salida, posiciones, 3, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
            PaginaPdf? pagina = primera;
            int numero = 0;
            while (pagina != null)
            {
                int id = 4 + numero * 2;
                Objeto(salida, posiciones, id, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents " + (id + 1) + " 0 R >>");
                string contenido = pagina.Contenido + "0 0 0 rg BT /F1 9 Tf 290 25 Td (" + (numero + 1) + " / " + cantidad + ") Tj ET\n";
                Objeto(salida, posiciones, id + 1, "<< /Length " + Encoding.Latin1.GetByteCount(contenido) + " >>\nstream\n" + contenido + "endstream");
                numero++;
                pagina = pagina.Siguiente;
            }
            long xref = salida.Position;
            Escribir(salida, "xref\n0 " + (objetos + 1) + "\n0000000000 65535 f \n");
            for (int i = 1; i <= objetos; i++)
            {
                Escribir(salida, posiciones[i].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
            }
            Escribir(salida, "trailer\n<< /Size " + (objetos + 1) + " /Root 1 0 R >>\nstartxref\n" + xref + "\n%%EOF\n");
            return salida.ToArray();
        }

        private void Objeto(MemoryStream salida, long[] posiciones, int numero, string contenido)
        {
            posiciones[numero] = salida.Position;
            Escribir(salida, numero + " 0 obj\n" + contenido + "\nendobj\n");
        }

        private void Escribir(MemoryStream salida, string texto)
        {
            salida.Write(Encoding.Latin1.GetBytes(texto));
        }
    }
}
