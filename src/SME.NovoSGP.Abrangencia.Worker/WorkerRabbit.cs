using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SME.NovoSGP.Abrangencia.Aplicacao.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Enums;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Exceptions;
using SME.NovoSGP.Abrangencia.Infra.Extensions;
using SME.NovoSGP.Abrangencia.Infra.Fila;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Text;
using System.Text.Json;

namespace SME.NovoSGP.Abrangencia.Worker;

public class WorkerRabbit : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly RabbitOptions rabbitOptions;
    private readonly ILogger<WorkerRabbit> logger;
    private readonly IServicoTelemetria servicoTelemetria;
    private readonly IServicoLog servicoLog;
    private readonly IServicoMensageria servicoMensageria;

    private readonly Dictionary<string, ComandoRabbit> comandos;

    public WorkerRabbit(
        IServiceScopeFactory serviceScopeFactory,
        RabbitOptions rabbitOptions,
        ILogger<WorkerRabbit> logger,
        IServicoTelemetria servicoTelemetria,
        IServicoLog servicoLog,
        IServicoMensageria servicoMensageria)
    {
        this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        this.rabbitOptions = rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.servicoTelemetria = servicoTelemetria ?? throw new ArgumentNullException(nameof(servicoTelemetria));
        this.servicoLog = servicoLog ?? throw new ArgumentNullException(nameof(servicoLog));
        this.servicoMensageria = servicoMensageria ?? throw new ArgumentNullException(nameof(servicoMensageria));

        comandos = new Dictionary<string, ComandoRabbit>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitOptions.HostName,
            UserName = rabbitOptions.UserName,
            Password = rabbitOptions.Password,
            VirtualHost = rabbitOptions.VirtualHost
        };

        await using var conexaoRabbit = await factory.CreateConnectionAsync();
        await using var channel = await conexaoRabbit.CreateChannelAsync();

        var props = new BasicProperties
        {
            Persistent = true
        };

        await channel.BasicQosAsync(0, rabbitOptions.LimiteDeMensagensPorExecucao, false);

        await channel.ExchangeDeclareAsync(ExchangeRabbit.WorkerAbrangencia, ExchangeType.Direct, true);
        await channel.ExchangeDeclareAsync(ExchangeRabbit.WorkerAbrangenciaDeadLetter, ExchangeType.Direct, true);

        RegistrarUseCases();
        await DeclararFilasAsync(channel);

        await InicializaConsumerAsync(channel, stoppingToken);
    }

    private async Task DeclararFilasAsync(IChannel channel)
    {
        foreach (var fila in typeof(RotasRabbit).ObterConstantesPublicas<string>())
        {
            var filaDeadLetter = $"{fila}.deadletter";
            var filaDeadLetterFinal = $"{fila}.deadletter.final";

            if (rabbitOptions.ForcarRecriarFilas)
            {
                await channel.QueueDeleteAsync(fila, ifEmpty: true);
                await channel.QueueDeleteAsync(filaDeadLetter, ifEmpty: true);
                await channel.QueueDeleteAsync(filaDeadLetterFinal, ifEmpty: true);
            }

            var args = ObterArgumentoDaFila(fila);
            await channel.QueueDeclareAsync(fila, true, false, false, args);
            await channel.QueueBindAsync(fila, ExchangeRabbit.WorkerAbrangencia, fila, null);

            var argsDlq = ObterArgumentoDaFilaDeadLetter(fila);
            await channel.QueueDeclareAsync(filaDeadLetter, true, false, false, argsDlq);
            await channel.QueueBindAsync(filaDeadLetter, ExchangeRabbit.WorkerAbrangenciaDeadLetter, fila, null);

            var argsFinal = new Dictionary<string, object> { { "x-queue-mode", "lazy" } };

            await channel.QueueDeclareAsync(
                queue: filaDeadLetterFinal,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: argsFinal);

            await channel.QueueBindAsync(filaDeadLetterFinal, ExchangeRabbit.WorkerAbrangenciaDeadLetter, filaDeadLetterFinal, null);
        }
    }

    private Dictionary<string, object> ObterArgumentoDaFila(string fila)
    {
        var args = new Dictionary<string, object>
            { { "x-dead-letter-exchange", ExchangeRabbit.WorkerAbrangenciaDeadLetter } };

        if (comandos.ContainsKey(fila) && comandos[fila].ModeLazy)
            args.Add("x-queue-mode", "lazy");

        return args;
    }

    private Dictionary<string, object> ObterArgumentoDaFilaDeadLetter(string fila)
    {
        var argsDlq = new Dictionary<string, object>();
        var ttl = comandos.ContainsKey(fila) ? comandos[fila].Ttl : ExchangeRabbit.WorkerAbrangenciaDeadLetterTtl;

        argsDlq.Add("x-dead-letter-exchange", ExchangeRabbit.WorkerAbrangencia);
        argsDlq.Add("x-message-ttl", ttl);
        argsDlq.Add("x-queue-mode", "lazy");

        return argsDlq;
    }

    private ulong GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers == null || !properties.Headers.ContainsKey("x-death"))
            return 0;

        var deathProperties = (List<object>)properties.Headers["x-death"];
        if (deathProperties.Count == 0)
            return 0;

        var lastRetry = (Dictionary<string, object>)deathProperties[0];

        if (!lastRetry.ContainsKey("count"))
            return 0;

        var count = lastRetry["count"];

        return (ulong)Convert.ToInt64(count);
    }

    private void RegistrarUseCases()
    {
        comandos.Add(RotasRabbit.IniciarSync, new ComandoRabbit("Inicia o processo de sincronização", typeof(IAbrangenciaSyncUseCase)));
        comandos.Add(RotasRabbit.SincronizarAbrangencia, new ComandoRabbit("Sincriniza as abrangencias", typeof(IAbrangenciaUseCase)));
    }

    private async Task InicializaConsumerAsync(IChannel channel, CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                await TratarMensagem(ea, channel);
            }
            catch (Exception ex)
            {
                servicoLog.Registrar($"Erro ao tratar mensagem {ea.DeliveryTag}", ex);
                await channel.BasicRejectAsync(ea.DeliveryTag, false);
            }
        };

        await RegistrarConsumerAsync(consumer, channel);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker ativo em: {Now}", DateTime.Now);
            await Task.Delay(10000, stoppingToken);
        }
    }

    private async Task TratarMensagem(BasicDeliverEventArgs ea, IChannel channel)
    {
        var mensagem = Encoding.UTF8.GetString(ea.Body.ToArray());
        logger.LogInformation("Mensagem recebida: {mensagem}", mensagem);
        var rota = ea.RoutingKey;

        if (comandos.ContainsKey(rota))
        {
            var transacao = servicoTelemetria.IniciarTransacao(rota);

            var mensagemRabbit = mensagem.ConverterObjectStringPraObjeto<MensagemRabbit>();
            var comandoRabbit = comandos[rota];

            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var casoDeUso = scope.ServiceProvider.GetService(comandoRabbit.TipoCasoUso);

                if (casoDeUso == null)
                    throw new ArgumentNullException(comandoRabbit.TipoCasoUso.Name);

                await servicoTelemetria.RegistrarAsync(() =>
                    comandoRabbit.TipoCasoUso.ObterMetodo("Executar").InvokeAsync(casoDeUso, mensagemRabbit),
                    "RabbitMQ",
                    rota,
                    rota);

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (NegocioException nex)
            {
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                RegistrarLog(ea, mensagemRabbit, nex, LogNivel.Negocio, $"Erros: {nex.Message}");
                servicoTelemetria.RegistrarExcecao(transacao, nex);
            }
            catch (ValidacaoException vex)
            {
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                RegistrarLog(ea, mensagemRabbit, vex, LogNivel.Negocio, $"Erros: {JsonSerializer.Serialize(vex.Mensagens())}");
                servicoTelemetria.RegistrarExcecao(transacao, vex);
            }
            catch (Exception ex)
            {
                servicoTelemetria.RegistrarExcecao(transacao, ex);
                var rejeicoes = GetRetryCount(ea.BasicProperties);

                if (++rejeicoes >= comandoRabbit.QuantidadeReprocessamentoDeadLetter)
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);

                    var filaFinal = $"{ea.RoutingKey}.deadletter.final";

                    await servicoMensageria.Publicar(mensagemRabbit, filaFinal,
                        ExchangeRabbit.WorkerAbrangenciaDeadLetter,
                        "PublicarDeadLetter");
                }
                else
                {
                    await channel.BasicRejectAsync(ea.DeliveryTag, false);
                }

                RegistrarLog(ea, mensagemRabbit, ex, LogNivel.Critico, $"Erros: {ex.Message}");
            }
            finally
            {
                servicoTelemetria.FinalizarTransacao(transacao);
            }
        }
        else
        {
            await channel.BasicRejectAsync(ea.DeliveryTag, false);
        }
    }

    private static async Task RegistrarConsumerAsync(AsyncEventingBasicConsumer consumer, IChannel channel)
    {
        foreach (var fila in typeof(RotasRabbit).ObterConstantesPublicas<string>())
        {
            // Usando BasicConsumeAsync e aguardando a execução da tarefa
            await channel.BasicConsumeAsync(fila, false, consumer);
        }
    }

    private void RegistrarLog(BasicDeliverEventArgs ea, MensagemRabbit mensagemRabbit, Exception ex, LogNivel logNivel, string observacao)
    {
        var mensagem = $"Worker Serap: Rota -> {ea.RoutingKey}  Cod Correl -> {mensagemRabbit.CodigoCorrelacao.ToString()[..3]}";

        // Cria a LogMensagem, mas não a passa diretamente para o servicoLog.Registrar
        var logMensagem = new LogMensagem(mensagem, logNivel, observacao, ex?.StackTrace, ex?.InnerException?.Message);

        // Cria uma exceção que vai ser passada para o serviço de log (se o servicoLog requer uma Exception)
        var exceptionToLog = new Exception(logMensagem.Mensagem, ex);

        servicoLog.Registrar(exceptionToLog);  // Registra a exceção com os dados da LogMensagem
    }
}
