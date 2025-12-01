using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.NovoSGP.Abrangencia.Aplicacao.Interfaces;
using SME.NovoSGP.Abrangencia.Aplicacao.UseCases;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using SME.NovoSGP.Abrangencia.Infra.Services;
using SME.NovoSGP.Abrangencia.IoC.Extensions;

namespace SME.NovoSGP.Abrangencia.IoC;

public static class RegistraDependencias
{
    public static void Registrar(IServiceCollection services, IConfiguration configuration)
    {
        services.AdicionarMediatr();
        services.AdicionarValidadoresFluentValidation();
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        //services.AddPoliticas();

        RegistrarRepositorios(services);
        RegistrarServicos(services, configuration);
        RegistrarCasosDeUso(services);
        //RegistrarContextos(services);
        //RegistraMapeamentos.Registrar();
    }

    private static void RegistrarRepositorios(IServiceCollection services)
    {
        //services.TryAddScoped<IRepositorioCache, RepositorioCache>();
        services.TryAddScoped<IRepositorioAbrangencia, RepositorioAbrangencia>();
        services.TryAddScoped<IRepositorioDre, RepositorioDre>();
        services.TryAddScoped<IRepositorioDreConsulta, RepositorioDreConsulta>();
        services.TryAddScoped<IRepositorioSupervisorEscolaDre, RepositorioSupervisorEscolaDre>();
        services.TryAddScoped<IRepositorioTurma, RepositorioTurma>();
        services.TryAddScoped<IRepositorioUe, RepositorioUe>();
        services.TryAddScoped<IRepositorioUeConsulta, RepositorioUeConsulta>();
        services.TryAddScoped<IRepositorioUsuario, RepositorioUsuario>();

    }

    private static void RegistrarServicos(IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IServicoTelemetria, ServicoTelemetria>();
        services.TryAddScoped<IServicoLog, ServicoLog>();
        services.TryAddSingleton<IServicoMensageria, ServicoMensageria>();
        services.AddHttpClient();
        services.AdicionarHttpClients(configuration);
    }

    private static void RegistrarCasosDeUso(IServiceCollection services)
    {
        services.TryAddScoped<IAbrangenciaSyncUseCase, AbrangenciaSyncUseCase>();
        services.TryAddScoped<IAbrangenciaUseCase, AbrangenciaUseCase>();
    }
}
