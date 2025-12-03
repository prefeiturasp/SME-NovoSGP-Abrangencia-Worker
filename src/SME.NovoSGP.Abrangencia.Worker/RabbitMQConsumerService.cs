using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SME.NovoSGP.Abrangencia.Aplicacao.Interfaces;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Extensions;
using SME.NovoSGP.Abrangencia.Infra.Fila;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;

namespace SME.NovoSGP.Abrangencia.Worker;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IServicoLog _servicoLog;
    private readonly IRabbitMQSetupService _rabbitMQSetupService;
    private readonly IRabbitMQMessageProcessor _rabbitMQMessageProcessor;

    private readonly Dictionary<string, ComandoRabbit> _comandos;

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        IServicoLog servicoLog,
        IServicoMensageria servicoMensageria,
        IRabbitMQSetupService rabbitMQSetupService,
        IRabbitMQMessageProcessor rabbitMQMessageProcessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _servicoLog = servicoLog ?? throw new ArgumentNullException(nameof(servicoLog));
    
        _rabbitMQSetupService = rabbitMQSetupService ?? throw new ArgumentNullException(nameof(rabbitMQSetupService));
        _rabbitMQMessageProcessor = rabbitMQMessageProcessor ?? throw new ArgumentNullException(nameof(rabbitMQMessageProcessor));

        _comandos = new Dictionary<string, ComandoRabbit>();
        RegistrarUseCases();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var conexaoRabbit = await _rabbitMQSetupService.CreateConnectionAsync();
        await using var channel = await conexaoRabbit.CreateChannelAsync();

        await _rabbitMQSetupService.SetupExchangesAndQueuesAsync(channel, _comandos);

        await InicializaConsumerAsync(channel, stoppingToken);
    }

    private void RegistrarUseCases()
    {
        _comandos.Add(RotasRabbit.SincronizarAbrangencia, new ComandoRabbit("Sincroniza as abrangencias", typeof(IAbrangenciaUseCase)));
    }

    private async Task InicializaConsumerAsync(IChannel channel, CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                await _rabbitMQMessageProcessor.ProcessMessageAsync(ea, channel, _comandos);
            }
            catch (Exception ex)
            {
                _servicoLog.Registrar($"Erro ao tratar mensagem {ea.DeliveryTag}", ex);
                await channel.BasicRejectAsync(ea.DeliveryTag, false);
            }
        };

        await RegistrarConsumerAsync(consumer, channel);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker ativo em: {Now}", DateTime.Now);
            await Task.Delay(10000, stoppingToken);
        }
    }

    private static async Task RegistrarConsumerAsync(AsyncEventingBasicConsumer consumer, IChannel channel)
    {
        foreach (var fila in typeof(RotasRabbit).ObterConstantesPublicas<string>())
        {
            await channel.BasicConsumeAsync(fila, false, consumer);
        }
    }
}
