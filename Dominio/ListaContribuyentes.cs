namespace proyecto3.Models
{
    public class NodoContribuyente
    {
        public Contribuyente Dato { get; }
        public NodoContribuyente? Siguiente { get; set; }

        public NodoContribuyente(Contribuyente dato)
        {
            Dato = dato;
        }
    }

    public class ListaContribuyentes
    {
        public NodoContribuyente? Primero { get; private set; }
        private NodoContribuyente? ultimo;

        public void Agregar(Contribuyente dato)
        {
            NodoContribuyente nuevo = new NodoContribuyente(dato);
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
