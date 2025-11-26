using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using RabbitMQ.Client;
using SME.NovoSGP.Abrangencia.Dados.Interceptors;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using SME.NovoSGP.Abrangencia.Infra.Services;
using SME.NovoSGP.Abrangencia.IoC;

namespace SME.NovoSGP.Abrangencia.Worker.Infra;

internal class HostingExtention
{
    private readonly IConfiguration Configuration;

    public HostingExtention(IConfiguration configuration)
    {
        this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void ConfigureServices(IServiceCollection services)
    {
        ConfigEnvoiromentVariables(services);
        RegistraDependencias.Registrar(services);

        services.AddHostedService<WorkerRabbit>();
    }

    private void ConfigEnvoiromentVariables(IServiceCollection services)
    {
        ConfigurarConexoes(services);
        ConfigurarRabbitmq(services);
        ConfigurarRabbitmqLog(services);
        ConfigurarTelemetria(services);
        ConfigurarElasticSearch(services);
        //ConfigurarCoresso(services);
        //ConfigurarEol(services);
    }

    //private void ConfigurarEol(IServiceCollection services)
    //{
    //    var eolOptions = new EolOptions();
    //    Configuration.GetSection(EolOptions.Secao).Bind(eolOptions, c => c.BindNonPublicProperties = true);
    //    services.AddSingleton(eolOptions);
    //}

    //private void ConfigurarCoresso(IServiceCollection services)
    //{
    //    var coressoOptions = new CoressoOptions();
    //    Configuration.GetSection(CoressoOptions.Secao).Bind(coressoOptions, c => c.BindNonPublicProperties = true);
    //    services.AddSingleton(coressoOptions);
    //}

    private void ConfigurarConexoes(IServiceCollection services)
    {
        var connectionStringOptions = new ConnectionStringOptions();
        Configuration.GetSection(ConnectionStringOptions.Secao).Bind(connectionStringOptions, c => c.BindNonPublicProperties = true);
        services.AddSingleton(connectionStringOptions);
    }

    private void ConfigurarTelemetria(IServiceCollection services)
    {
        var telemetriaOptions = new TelemetriaOptions();
        Configuration.GetSection(TelemetriaOptions.Secao).Bind(telemetriaOptions, c => c.BindNonPublicProperties = true);
        services.AddSingleton(telemetriaOptions);

        var servicoTelemetria = new ServicoTelemetria(telemetriaOptions);
        services.AddSingleton<IServicoTelemetria>(servicoTelemetria);
        DapperExtensionMethods.Init(servicoTelemetria);
    }

    private void ConfigurarRabbitmqLog(IServiceCollection services)
    {
        var rabbitLogOptions = new RabbitLogOptions();
        Configuration.GetSection(RabbitLogOptions.Secao).Bind(rabbitLogOptions, c => c.BindNonPublicProperties = true);
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

    private void ConfigurarElasticSearch(IServiceCollection services)
    {
        var elasticOptions = new ElasticOptions();
        Configuration.GetSection(ElasticOptions.Secao).Bind(elasticOptions, c => c.BindNonPublicProperties = true);
        services.AddSingleton(elasticOptions);

        var nodes = new List<Uri>();
        if (elasticOptions.Urls.Contains(','))
        {
            string[] urls = elasticOptions.Urls.Split(',');
            foreach (string url in urls)
                nodes.Add(new Uri(url));
        }
        else
        {
            nodes.Add(new Uri(elasticOptions.Urls));
        }

        var connectionPool = new StaticConnectionPool(nodes);
        var connectionSettings = new ConnectionSettings(connectionPool);
        connectionSettings.DefaultIndex(elasticOptions.IndicePadrao);

        if (!string.IsNullOrEmpty(elasticOptions.CertificateFingerprint))
            connectionSettings.CertificateFingerprint(elasticOptions.CertificateFingerprint);

        if (!string.IsNullOrEmpty(elasticOptions.Usuario) && !string.IsNullOrEmpty(elasticOptions.Senha))
            connectionSettings.BasicAuthentication(elasticOptions.Usuario, elasticOptions.Senha);

        var elasticClient = new ElasticClient(connectionSettings);
        services.AddSingleton<IElasticClient>(elasticClient);
    }

    private void ConfigurarRabbitmq(IServiceCollection services)
    {
        var rabbitOptions = new RabbitOptions();
        Configuration.GetSection(RabbitOptions.Secao).Bind(rabbitOptions, c => c.BindNonPublicProperties = true);
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

    //public void Configure(IApplicationBuilder app, IHostEnvironment env)
    //{
    //    if (env.IsDevelopment())
    //    {
    //        app.UseDeveloperExceptionPage();
    //    }

    //    app.Run(async (context) =>
    //    {
    //        await context.Response.WriteAsync("workerrabbitmq!");
    //    });
    //}
}
