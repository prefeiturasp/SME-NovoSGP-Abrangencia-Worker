using MediatR;

namespace SME.NovoSGP.Abrangencia.Aplicacao.UseCases;

public abstract class AbstractUseCase
{
    protected readonly IMediator mediator;

    public AbstractUseCase(IMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }
}
