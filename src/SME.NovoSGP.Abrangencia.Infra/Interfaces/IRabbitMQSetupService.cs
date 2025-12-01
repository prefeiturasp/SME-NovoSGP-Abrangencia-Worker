using RabbitMQ.Client;
using SME.NovoSGP.Abrangencia.Infra.Fila;

namespace SME.NovoSGP.Abrangencia.Infra.Interfaces;

public interface IRabbitMQSetupService
{
    Task<IConnection> CreateConnectionAsync();
    Task SetupExchangesAndQueuesAsync(IChannel channel, Dictionary<string, ComandoRabbit> comandos);
}