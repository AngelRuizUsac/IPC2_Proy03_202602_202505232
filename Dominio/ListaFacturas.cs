namespace proyecto3.Models
{
    public class NodoFactura
    {
        public Factura Dato { get; }
        public NodoFactura? Siguiente { get; set; }

        public NodoFactura(Factura dato)
        {
            Dato = dato;
        }
    }

    public class ListaFacturas
    {
        public NodoFactura? Primero { get; private set; }
        private NodoFactura? ultimo;

        public void Agregar(Factura dato)
        {
            NodoFactura nuevo = new NodoFactura(dato);
            if (Primero == null)
            {
                Primero = nuevo;
            }
            else
            {
                ultimo!.Siguiente = nuevo;
            }
            ultimo = nuevo;
        }
    }
}
