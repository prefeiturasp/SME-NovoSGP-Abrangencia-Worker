using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using SME.NovoSGP.Abrangencia.Infra.Services;

namespace SME.NovoSGP.Abrangencia.IoC;

public static class RegistraDependencias
{
    public static void Registrar(IServiceCollection services)
    {
        //services.AdicionarMediatr();
        //services.AdicionarValidadoresFluentValidation();
        //services.AddPoliticas();

        RegistrarRepositorios(services);
        RegistrarServicos(services);
        RegistrarCasosDeUso(services);
        //RegistrarContextos(services);
        //RegistraMapeamentos.Registrar();
    }

    private static void RegistrarRepositorios(IServiceCollection services)
    {
        //services.TryAddScoped<IRepositorioCache, RepositorioCache>();
        //services.TryAddScoped<IRepositorioQuestionario, RepositorioQuestionario>();
    }

    private static void RegistrarServicos(IServiceCollection services)
    {
        services.TryAddScoped<IServicoTelemetria, ServicoTelemetria>();
        services.TryAddScoped<IServicoLog, ServicoLog>();
        services.TryAddSingleton<IServicoMensageria, ServicoMensageria>();

    }

    private static void RegistrarCasosDeUso(IServiceCollection services)
    {
        //services.TryAddScoped<IObterQuestionarioSondagemUseCase, ObterQuestionarioSondagemUseCase>();
    }
}
