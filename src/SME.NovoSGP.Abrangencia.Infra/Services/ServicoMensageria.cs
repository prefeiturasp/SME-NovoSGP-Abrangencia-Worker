using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Extensions;
using SME.NovoSGP.Abrangencia.Infra.Fila;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Text;

namespace SME.NovoSGP.Abrangencia.Infra.Services;

public class ServicoMensageria : IServicoMensageria
{
    private readonly RabbitOptions rabbitOptions;
    private readonly IServicoTelemetria servicoTelemetria;
    private readonly ILogger<ServicoLog> logger;

    public ServicoMensageria(RabbitOptions rabbitOptions,
        IServicoTelemetria servicoTelemetria,
        ILogger<ServicoLog> logger)
    {
        this.rabbitOptions = rabbitOptions ?? throw new ArgumentNullException(nameof(rabbitOptions));
        this.servicoTelemetria = servicoTelemetria ?? throw new ArgumentNullException(nameof(servicoTelemetria));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Publicar(MensagemRabbit mensagemRabbit, string rota, string exchange, string nomeAcao)
    {
        var body = Encoding.UTF8.GetBytes(mensagemRabbit.ConverterObjectParaJson());

        await servicoTelemetria.RegistrarAsync(
            async () => await PublicarMensagem(rota, body, exchange), nomeAcao, rota, string.Empty);

        return true;
    }

    private async Task PublicarMensagem(string rota, byte[] body, string exchange = null)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbitOptions.HostName,
                UserName = rabbitOptions.UserName,
                Password = rabbitOptions.Password,
                VirtualHost = rabbitOptions.VirtualHost
            };

            using var conexaoRabbit = await factory.CreateConnectionAsync();
            using var channel = await conexaoRabbit.CreateChannelAsync();
            var props = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                ExchangeRabbit.Logs,
                RotasRabbit.RotaLogs,
                true,
                props,
                body
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao publicar mensagem no RabbitMQ");
        }
    }
}