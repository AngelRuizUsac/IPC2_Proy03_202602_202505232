using System.Text;

namespace proyecto3.Backend.Services
{
    public class Repositorio : IDisposable
    {
        private readonly string archivo;
        private readonly FileStream bloqueo;

        public Repositorio(string carpeta)
        {
            Directory.CreateDirectory(carpeta);
            archivo = Path.Combine(carpeta, "estado.json");
            bloqueo = new FileStream(Path.Combine(carpeta, "instancia.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

        public Estado Cargar()
        {
            if (File.Exists(archivo))
            {
                return Estado.Importar(File.ReadAllText(archivo, Encoding.UTF8));
            }
            return new Estado();
        }

        public void Guardar(Estado estado)
        {
            string temporal = archivo + ".tmp";
            byte[] contenido = Encoding.UTF8.GetBytes(estado.Exportar());
            using (FileStream flujo = new FileStream(temporal, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                flujo.Write(contenido);
                flujo.Flush(true);
            }
            File.Move(temporal, archivo, true);
        }

        public void Dispose()
        {
            bloqueo.Dispose();
        }
    }
}
