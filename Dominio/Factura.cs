namespace proyecto3.Models
{
    public class Factura
    {
        public DateTime Fecha { get; set; }
        public DateTime Recepcion { get; set; }
        public string Referencia { get; set; } = "";
        public string Emisor { get; set; } = "";
        public string Receptor { get; set; } = "";
        public decimal Valor { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public long Codigo { get; set; }
        public bool EmisorInvalido { get; set; }
        public bool ReceptorInvalido { get; set; }
        public bool EmisorInexistente { get; set; }
        public bool ReceptorInexistente { get; set; }
        public bool IvaIncorrecto { get; set; }
        public bool TotalIncorrecto { get; set; }
        public bool ReferenciaDuplicada { get; set; }
        public bool FormatoIncorrecto { get; set; }

        public bool TieneErrores()
        {
            return EmisorInvalido || ReceptorInvalido || EmisorInexistente || ReceptorInexistente || IvaIncorrecto || TotalIncorrecto || ReferenciaDuplicada || FormatoIncorrecto;
        }
    }
}
