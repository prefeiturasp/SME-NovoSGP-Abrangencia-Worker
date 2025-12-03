using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Extensions;
using SME.NovoSGP.Abrangencia.Infra.Fila;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;

namespace SME.NovoSGP.Abrangencia.Worker;

public class RabbitMQSetupService : IRabbitMQSetupService
{
    private readonly RabbitOptions _rabbitOptions;
    private readonly ILogger<RabbitMQSetupService> _logger;

    public RabbitMQSetupService(RabbitOptions rabbitOptions, ILogger<RabbitMQSetupService> logger)
    {
        _rabbitOptions = rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitOptions.HostName,
            UserName = _rabbitOptions.UserName,
            Password = _rabbitOptions.Password,
            VirtualHost = _rabbitOptions.VirtualHost
        };

        return await factory.CreateConnectionAsync();
    }

    public async Task SetupExchangesAndQueuesAsync(IChannel channel, Dictionary<string, ComandoRabbit> comandos)
    {
        await channel.BasicQosAsync(0, _rabbitOptions.LimiteDeMensagensPorExecucao, false);

        await channel.ExchangeDeclareAsync(ExchangeRabbit.WorkerAbrangencia, ExchangeType.Direct, true);
        await channel.ExchangeDeclareAsync(ExchangeRabbit.WorkerAbrangenciaDeadLetter, ExchangeType.Direct, true);

        await DeclararFilasAsync(channel, comandos);
    }

    private async Task DeclararFilasAsync(IChannel channel, Dictionary<string, ComandoRabbit> comandos)
    {
        foreach (var fila in typeof(RotasRabbit).ObterConstantesPublicas<string>())
        {
            var filaDeadLetter = $"{fila}.deadletter";
            var filaDeadLetterFinal = $"{fila}.deadletter.final";

            if (_rabbitOptions.ForcarRecriarFilas)
            {
                await channel.QueueDeleteAsync(fila, ifEmpty: true);
                await channel.QueueDeleteAsync(filaDeadLetter, ifEmpty: true);
                await channel.QueueDeleteAsync(filaDeadLetterFinal, ifEmpty: true);
            }

            var args = ObterArgumentoDaFila(fila, comandos);
            await channel.QueueDeclareAsync(fila, true, false, false, args);
            await channel.QueueBindAsync(fila, ExchangeRabbit.WorkerAbrangencia, fila, null);

            var argsDlq = ObterArgumentoDaFilaDeadLetter(fila, comandos);
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

    private static Dictionary<string, object> ObterArgumentoDaFila(string fila, Dictionary<string, ComandoRabbit> comandos)
    {
        var args = new Dictionary<string, object>
            { { "x-dead-letter-exchange", ExchangeRabbit.WorkerAbrangenciaDeadLetter } };

        if (comandos.ContainsKey(fila) && comandos[fila].ModeLazy)
            args.Add("x-queue-mode", "lazy");

        return args;
    }

    private static Dictionary<string, object> ObterArgumentoDaFilaDeadLetter(string fila, Dictionary<string, ComandoRabbit> comandos)
    {
        var argsDlq = new Dictionary<string, object>();
        var ttl = comandos.ContainsKey(fila) ? comandos[fila].Ttl : ExchangeRabbit.WorkerAbrangenciaDeadLetterTtl;

        argsDlq.Add("x-dead-letter-exchange", ExchangeRabbit.WorkerAbrangencia);
        argsDlq.Add("x-message-ttl", ttl);
        argsDlq.Add("x-queue-mode", "lazy");

        return argsDlq;
    }
}



