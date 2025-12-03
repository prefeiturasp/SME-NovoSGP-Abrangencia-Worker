using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SME.NovoSGP.Abrangencia.Aplicacao.Pipelines;

namespace SME.NovoSGP.Abrangencia.IoC.Extensions;

internal static class RegistrarMediatr
{
    internal static void AdicionarMediatr(this IServiceCollection services)
    {
        var assembly = AppDomain.CurrentDomain.Load("SME.NovoSGP.Abrangencia.Aplicacao");

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidacoesPipeline<,>));
    }
}
