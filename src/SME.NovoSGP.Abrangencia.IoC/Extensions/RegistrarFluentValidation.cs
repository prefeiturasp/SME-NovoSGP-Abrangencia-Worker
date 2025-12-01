using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SME.NovoSGP.Abrangencia.IoC.Extensions;

internal static class RegistrarFluentValidation
{
    internal static void AdicionarValidadoresFluentValidation(this IServiceCollection services)
    {
        var assemblyInfra = AppDomain.CurrentDomain.Load("SME.NovoSGP.Abrangencia.Infra");

        AssemblyScanner
            .FindValidatorsInAssembly(assemblyInfra)
            .ForEach(result => services.AddScoped(result.InterfaceType, result.ValidatorType));

        var assembly = AppDomain.CurrentDomain.Load("SME.NovoSGP.Abrangencia.Aplicacao");

        AssemblyScanner
            .FindValidatorsInAssembly(assembly)
            .ForEach(result => services.AddScoped(result.InterfaceType, result.ValidatorType));
    }
}
