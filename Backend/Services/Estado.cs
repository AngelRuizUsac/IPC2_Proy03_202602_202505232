using proyecto3.Models;
using System.Text;
using System.Text.Json;

namespace proyecto3.Backend.Services
{
    public class Estado
    {
        public ListaContribuyentes Contribuyentes { get; } = new ListaContribuyentes();
        public ListaFacturas Facturas { get; } = new ListaFacturas();

        public Contribuyente? Buscar(string nit)
        {
            NodoContribuyente? actual = Contribuyentes.Primero;
            while (actual != null)
            {
                if (actual.Dato.Nit == nit)
                {
                    return actual.Dato;
                }
                actual = actual.Siguiente;
            }
            return null;
        }

        public string Exportar()
        {
            using MemoryStream memoria = new MemoryStream();
            using (Utf8JsonWriter escritor = new Utf8JsonWriter(memoria))
            {
                escritor.WriteStartObject();
                escritor.WriteNumber("version", 1);
                escritor.WriteStartArray("contribuyentes");
                NodoContribuyente? contribuyente = Contribuyentes.Primero;
                while (contribuyente != null)
                {
                    escritor.WriteRawValue(JsonDatos.Serializar(contribuyente.Dato));
                    contribuyente = contribuyente.Siguiente;
                }
                escritor.WriteEndArray();
                escritor.WriteStartArray("facturas");
                NodoFactura? factura = Facturas.Primero;
                while (factura != null)
                {
                    escritor.WriteRawValue(JsonDatos.Serializar(factura.Dato));
                    factura = factura.Siguiente;
                }
                escritor.WriteEndArray();
                escritor.WriteEndObject();
            }
            return Encoding.UTF8.GetString(memoria.ToArray());
        }

        public static Estado Importar(string json)
        {
            using JsonDocument documento = JsonDocument.Parse(json);
            JsonElement raiz = documento.RootElement;
            if (raiz.GetProperty("version").GetInt32() != 1)
            {
                throw new InvalidDataException("Versión de almacenamiento desconocida.");
            }
            Estado estado = new Estado();
            foreach (JsonElement elemento in raiz.GetProperty("contribuyentes").EnumerateArray())
            {
                Contribuyente? dato = (Contribuyente?)JsonSerializer.Deserialize(elemento.GetRawText(), typeof(Contribuyente));
                if (dato == null)
                {
                    throw new InvalidDataException("Contribuyente inválido en almacenamiento.");
                }
                estado.Contribuyentes.Agregar(dato);
            }
            foreach (JsonElement elemento in raiz.GetProperty("facturas").EnumerateArray())
            {
                Factura? dato = (Factura?)JsonSerializer.Deserialize(elemento.GetRawText(), typeof(Factura));
                if (dato == null)
                {
                    throw new InvalidDataException("Factura inválida en almacenamiento.");
                }
                estado.Facturas.Agregar(dato);
            }
            return estado;
        }
    }
}
