using System.Text;
using System.Text.Json;

namespace proyecto3.Models
{
    public class JsonDatos
    {
        public static string Texto(JsonElement objeto, string nombre)
        {
            if (objeto.ValueKind == JsonValueKind.Object && objeto.TryGetProperty(nombre, out JsonElement valor) && valor.ValueKind == JsonValueKind.String)
            {
                string? texto = valor.GetString();
                if (texto != null)
                {
                    return texto;
                }
            }
            return "";
        }

        public static bool Numero(JsonElement objeto, string nombre, out decimal numero)
        {
            numero = 0;
            return objeto.ValueKind == JsonValueKind.Object && objeto.TryGetProperty(nombre, out JsonElement valor) && valor.ValueKind == JsonValueKind.Number && valor.TryGetDecimal(out numero);
        }

        public static string Serializar(object dato)
        {
            return JsonSerializer.Serialize(dato, dato.GetType());
        }

        public static string ReporteJson(Reporte reporte)
        {
            using MemoryStream memoria = new MemoryStream();
            using (Utf8JsonWriter escritor = new Utf8JsonWriter(memoria))
            {
                escritor.WriteStartObject();
                escritor.WriteString("titulo", reporte.Titulo);
                escritor.WriteString("filtro", reporte.Filtro);
                escritor.WriteString("serie1", reporte.Serie1);
                escritor.WriteString("serie2", reporte.Serie2);
                escritor.WriteNumber("total", reporte.Total);
                escritor.WriteStartArray("encabezados");
                foreach (string encabezado in reporte.Encabezados)
                {
                    escritor.WriteStringValue(encabezado);
                }
                escritor.WriteEndArray();
                escritor.WriteStartArray("filas");
                NodoFila? actual = reporte.Filas.Primero;
                while (actual != null)
                {
                    escritor.WriteStartObject();
                    escritor.WriteString("etiqueta", actual.Dato.Etiqueta);
                    escritor.WriteNumber("serie1", actual.Dato.Serie1);
                    escritor.WriteNumber("serie2", actual.Dato.Serie2);
                    escritor.WriteStartArray("celdas");
                    foreach (string celda in actual.Dato.Celdas)
                    {
                        escritor.WriteStringValue(celda);
                    }
                    escritor.WriteEndArray();
                    escritor.WriteEndObject();
                    actual = actual.Siguiente;
                }
                escritor.WriteEndArray();
                escritor.WriteEndObject();
            }
            return Encoding.UTF8.GetString(memoria.ToArray());
        }

        public static Reporte LeerReporte(string json)
        {
            using JsonDocument documento = JsonDocument.Parse(json);
            JsonElement raiz = documento.RootElement;
            Reporte reporte = new Reporte();
            reporte.Titulo = Texto(raiz, "titulo");
            reporte.Filtro = Texto(raiz, "filtro");
            reporte.Serie1 = Texto(raiz, "serie1");
            reporte.Serie2 = Texto(raiz, "serie2");
            reporte.Total = raiz.GetProperty("total").GetDecimal();
            reporte.Encabezados = LeerTextos(raiz.GetProperty("encabezados"));
            foreach (JsonElement elemento in raiz.GetProperty("filas").EnumerateArray())
            {
                reporte.Filas.Agregar(new Fila { Etiqueta = Texto(elemento, "etiqueta"), Celdas = LeerTextos(elemento.GetProperty("celdas")), Serie1 = elemento.GetProperty("serie1").GetDecimal(), Serie2 = elemento.GetProperty("serie2").GetDecimal() });
            }
            return reporte;
        }

        private static string[] LeerTextos(JsonElement arreglo)
        {
            string[] resultado = new string[arreglo.GetArrayLength()];
            int i = 0;
            foreach (JsonElement elemento in arreglo.EnumerateArray())
            {
                resultado[i] = elemento.ToString();
                i++;
            }
            return resultado;
        }
    }
}
