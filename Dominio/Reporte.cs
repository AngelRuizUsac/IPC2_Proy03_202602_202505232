namespace proyecto3.Models
{
    public class Fila
    {
        public string Etiqueta { get; set; } = "";
        public string[] Celdas { get; set; } = Array.Empty<string>();
        public decimal Serie1 { get; set; }
        public decimal Serie2 { get; set; }
    }

    public class Reporte
    {
        public string Titulo { get; set; } = "";
        public string Filtro { get; set; } = "";
        public string[] Encabezados { get; set; } = Array.Empty<string>();
        public ListaFilas Filas { get; set; } = new ListaFilas();
        public string Serie1 { get; set; } = "";
        public string Serie2 { get; set; } = "";
        public decimal Total { get; set; }
    }
}
