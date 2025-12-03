using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SME.NovoSGP.Abrangencia.Infra.Extensions;

public static class EnumExtensao
{
    /// <summary>
    ///     A generic extension method that aids in reflecting
    ///     and retrieving any attribute that is applied to an `Enum`.
    /// </summary>
    public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
    {
        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<TAttribute>();
    }
    public static string Name(this Enum enumValue)
        => enumValue.GetAttribute<DisplayAttribute>().Name;

    public static string ShortName(this Enum enumValue)
        => enumValue.GetAttribute<DisplayAttribute>().ShortName;


    public static string Description(this Enum enumValue)
       => enumValue.GetAttribute<DisplayAttribute>().Description;

    public static string GroupName(this Enum enumValue)
       => enumValue.GetAttribute<DisplayAttribute>().GroupName;


    public static IEnumerable<TEnum> Listar<TEnum>()
        where TEnum : struct
    {
        if (!typeof(TEnum).IsEnum) throw new InvalidOperationException();

        return ((TEnum[])Enum.GetValues(typeof(TEnum)));
    }


    public static Dictionary<Enum, string> ToDictionary<TEnum>()
        where TEnum : struct
    {
        if (!typeof(TEnum).IsEnum) throw new InvalidOperationException();

        return ((TEnum[])Enum.GetValues(typeof(TEnum))).Cast<Enum>().ToDictionary(key => key, value => value.Name());
    }

    public static bool EhMaiorQueZero(this long valor)
    {
        return valor > 0;
    }

    public static bool EhMaiorQueZero(this int valor)
    {
        return valor > 0;
    }

    public static bool EhMenorQueZero(this long valor)
    {
        return valor < 0;
    }

    public static bool EhIgualZero(this long valor)
    {
        return valor == 0;
    }

    public static string ObterCaseWhenSQL<TEnum>(string atributoComparacao)
    {
        StringBuilder sql = new StringBuilder();
        sql.AppendLine("CASE");
        foreach (var value in Enum.GetValues(typeof(TEnum)))
        {
            var enumName = Enum.GetName(typeof(TEnum), value);
            var enumMemberInfo = typeof(TEnum).GetMember(enumName)[0];
            var displayAttribute = (DisplayAttribute)Attribute.GetCustomAttribute(enumMemberInfo, typeof(DisplayAttribute));
            var shortName = displayAttribute.ShortName;

            sql.AppendLine($"    WHEN {atributoComparacao} = {(int)value} THEN '{shortName}'");
        }
        sql.AppendLine("    ELSE ''");
        sql.AppendLine("END");

        return sql.ToString();
    }

    public static bool EhOpcaoTodos(this long? valor)
    {
        return (!valor.HasValue || valor.Equals(-99) || valor.Equals(0));
    }

    public static string ObterDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        var attribute = field?.GetCustomAttribute<DisplayAttribute>();

        return attribute?.Name ?? value.ToString();
    }
}