namespace proyecto3.Models
{
    public class NodoFila
    {
        public Fila Dato { get; }
        public NodoFila? Siguiente { get; set; }

        public NodoFila(Fila dato)
        {
            Dato = dato;
        }
    }

    public class ListaFilas
    {
        public NodoFila? Primero { get; private set; }
        private NodoFila? ultimo;

        public void Agregar(Fila dato)
        {
            NodoFila nuevo = new NodoFila(dato);
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
