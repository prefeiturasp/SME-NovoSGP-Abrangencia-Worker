using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SME.NovoSGP.Abrangencia.Dominio.Constantes;

namespace SME.NovoSGP.Abrangencia.IoC.Extensions;

internal static class RegistrarHttpClients
{
    internal static void AdicionarHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient(name: ServicosEolConstants.SERVICO, c =>
        {
            c.BaseAddress = new Uri(configuration.GetSection("UrlApiEOL").Value);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("x-api-eol-key", configuration.GetSection("ApiKeyEolApi").Value);

            if (configuration.GetSection("HttpClientTimeoutSecond").Value != null)
                c.Timeout = TimeSpan.FromSeconds(double.Parse(configuration.GetSection("HttpClientTimeoutSecond").Value));
        });

    }
}