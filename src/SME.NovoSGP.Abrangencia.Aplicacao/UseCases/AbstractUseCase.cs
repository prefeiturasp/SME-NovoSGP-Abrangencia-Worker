using MediatR;
using RabbitMQ.Client;

namespace SME.NovoSGP.Abrangencia.Aplicacao.UseCases;

public abstract class AbstractUseCase
{
    protected readonly IMediator mediator;
    protected readonly IChannel channel;

    public AbstractUseCase(IMediator mediator, IChannel channel)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
    }
}
