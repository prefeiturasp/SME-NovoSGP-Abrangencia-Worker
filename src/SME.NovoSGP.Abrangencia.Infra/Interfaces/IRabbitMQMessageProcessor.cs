using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SME.NovoSGP.Abrangencia.Infra.Fila;

namespace SME.NovoSGP.Abrangencia.Infra.Interfaces;

public interface IRabbitMQMessageProcessor
{
    Task ProcessMessageAsync(BasicDeliverEventArgs ea, IChannel channel, Dictionary<string, ComandoRabbit> comandos);
}