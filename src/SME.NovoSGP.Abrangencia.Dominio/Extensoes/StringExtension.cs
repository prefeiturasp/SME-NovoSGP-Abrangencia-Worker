using System.Text.RegularExpressions;

namespace SME.NovoSGP.Abrangencia.Dominio.Extensoes
{
    public static class StringExtension
    {
        public static string SomenteNumeros(this string valor)
        {
            return Regex.Replace(valor, "[^0-9]", "");
        }

        public static string FormatarCPF(this string cpf)
        => Regex.Replace(cpf.SomenteNumeros(), @"(\d{3})(\d{3})(\d{3})(\d{2})", "$1.$2.$3-$4");
    }
}
