using System.Globalization;
using System.Text.RegularExpressions;

namespace proyecto3.Models
{
    public class Validador
    {
        public static bool NitValido(string nit)
        {
            if (!Regex.IsMatch(nit, @"\A[0-9]{1,20}[0-9K]\z"))
            {
                return false;
            }
            int suma = 0;
            int posicion = 2;
            for (int i = nit.Length - 2; i >= 0; i--)
            {
                suma += (nit[i] - '0') * posicion;
                posicion++;
            }
            int verificador = (11 - suma % 11) % 11;
            if (verificador == 10)
            {
                return nit[nit.Length - 1] == 'K';
            }
            return nit[nit.Length - 1] == (char)('0' + verificador);
        }

        public static bool ExtraerFecha(string tiempo, out DateTime fecha)
        {
            fecha = default;
            Match coincidencia = Regex.Match(tiempo, @"(?<![0-9])[0-9]{2}/[0-9]{2}/[0-9]{4}\s+[0-9]{2}:[0-9]{2}(?![0-9])");
            if (!coincidencia.Success)
            {
                return false;
            }
            string texto = Regex.Replace(coincidencia.Value, @"\s+", " ");
            return DateTime.TryParseExact(texto, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
        }

        public static bool DineroValido(decimal valor)
        {
            return valor >= 0 && valor <= 999999999999999999m && decimal.Round(valor, 2) == valor;
        }
    }
}
