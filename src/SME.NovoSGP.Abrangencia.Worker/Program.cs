using Elastic.Apm.Api;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nest;
using RabbitMQ.Client;
using SME.NovoSGP.Abrangencia.Dados.Interceptors;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using SME.NovoSGP.Abrangencia.Infra.Services;
using SME.NovoSGP.Abrangencia.IoC;
using SME.NovoSGP.Abrangencia.Worker;
using System.Configuration;
using System.Reflection;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

RegistraDependencias.Registrar(builder.Services);
ConfigureServices(builder.Services, builder.Configuration);
ConfigurarRabbitmq(builder.Services, builder.Configuration);
ConfigurarRabbitmqLog(builder.Services, builder.Configuration);
ConfigurarConexoes(builder.Services, builder.Configuration);

builder.Services.AddHostedService<WorkerRabbit>();

IHost host = builder.Build();
await host.RunAsync();


static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    RegistraDependencias.Registrar(services);

    services.Configure<ConnectionStringOptions>(configuration.GetSection(ConnectionStringOptions.Secao));
    services.Configure<RabbitOptions>(configuration.GetSection(RabbitOptions.Secao));
    services.Configure<RabbitLogOptions>(configuration.GetSection(RabbitLogOptions.Secao));
    services.Configure<TelemetriaOptions>(configuration.GetSection(TelemetriaOptions.Secao));
    services.Configure<ElasticOptions>(configuration.GetSection(ElasticOptions.Secao));

    // Registra o cliente do Elasticsearch.
    services.AddSingleton<IElasticClient>(provider =>
    {
        var elasticOptions = provider.GetRequiredService<IOptions<ElasticOptions>>().Value;
        var nodes = elasticOptions.Urls.Split(',').Select(url => new Uri(url)).ToList();

        var connectionPool = new StaticConnectionPool(nodes);
        var connectionSettings = new ConnectionSettings(connectionPool)
            .DefaultIndex(elasticOptions.IndicePadrao);

        if (!string.IsNullOrEmpty(elasticOptions.CertificateFingerprint))
            connectionSettings.CertificateFingerprint(elasticOptions.CertificateFingerprint);

        // CORREÇÃO DE SINTAXE APLICADA AQUI: !string.IsNullOrEmpty(...)
        if (!string.IsNullOrEmpty(elasticOptions.Usuario) && !string.IsNullOrEmpty(elasticOptions.Senha))
        {
            connectionSettings.BasicAuthentication(elasticOptions.Usuario, elasticOptions.Senha);
        }

        return new ElasticClient(connectionSettings);
    });

    // Configura a telemetria e o Dapper.
    services.AddSingleton<IServicoTelemetria>(provider =>
    {
        var telemetriaOptions = provider.GetRequiredService<IOptions<TelemetriaOptions>>().Value;
        var servicoTelemetria = new ServicoTelemetria(telemetriaOptions);
        DapperExtensionMethods.Init(servicoTelemetria);
        return servicoTelemetria;
    });
}

static void ConfigurarRabbitmq(IServiceCollection services, IConfiguration configuration)
{
    var rabbitOptions = new RabbitOptions();
    configuration.GetSection(RabbitLogOptions.Secao).Bind(rabbitOptions, c => c.BindNonPublicProperties = true);
    services.AddSingleton(rabbitOptions);

    var factory = new ConnectionFactory
    {
        HostName = rabbitOptions.HostName,
        UserName = rabbitOptions.UserName,
        Password = rabbitOptions.Password,
        VirtualHost = rabbitOptions.VirtualHost
    };

    services.AddSingleton(factory);

    services.AddSingleton<RabbitMQ.Client.IConnection>(provider =>
    {
        var factory = provider.GetRequiredService<ConnectionFactory>();
        return factory.CreateConnectionAsync().Result;
    });

    services.AddSingleton<IChannel>(provider =>
    {
        var connection = provider.GetRequiredService<RabbitMQ.Client.IConnection>();
        return connection.CreateChannelAsync().Result;
    });
}

static void ConfigurarRabbitmqLog(IServiceCollection services, IConfiguration configuration)
{
    var rabbitLogOptions = new RabbitLogOptions();
    configuration.GetSection(RabbitLogOptions.Secao).Bind(rabbitLogOptions, c => c.BindNonPublicProperties = true);
    services.AddSingleton(rabbitLogOptions);

    var factoryLog = new ConnectionFactory
    {
        HostName = rabbitLogOptions.HostName,
        UserName = rabbitLogOptions.UserName,
        Password = rabbitLogOptions.Password,
        VirtualHost = rabbitLogOptions.VirtualHost
    };

    var conexaoRabbitLog = factoryLog.CreateConnectionAsync().Result;
    IChannel channelLog = conexaoRabbitLog.CreateChannelAsync().Result;
}

static void ConfigurarConexoes(IServiceCollection services, IConfiguration configuration)
{
    var connectionStringOptions = new ConnectionStringOptions();
    configuration.GetSection(ConnectionStringOptions.Secao).Bind(connectionStringOptions, c => c.BindNonPublicProperties = true);
    services.AddSingleton(connectionStringOptions);
}