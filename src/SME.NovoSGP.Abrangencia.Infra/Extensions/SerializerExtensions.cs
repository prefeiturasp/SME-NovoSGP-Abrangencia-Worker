using System.Text.Json;

namespace SME.NovoSGP.Abrangencia.Infra.Extensions;

public static class JsonSerializerExtensions
{
    private static JsonSerializerOptions ObterConfigSerializer()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IgnoreNullValues = true
        };
    }

    public static T ConverterObjectStringPraObjeto<T>(this string objectString)
    {
        return string.IsNullOrEmpty(objectString)
            ? default
            : JsonSerializer.Deserialize<T>(objectString, ObterConfigSerializer());
    }

    public static string ConverterObjectParaJson(this object obj)
    {
        return obj == null ? string.Empty : JsonSerializer.Serialize(obj, ObterConfigSerializer());
    }
}