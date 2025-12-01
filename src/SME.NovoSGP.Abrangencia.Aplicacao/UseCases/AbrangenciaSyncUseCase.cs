using MediatR;
using SME.NovoSGP.Abrangencia.Aplicacao.Interfaces;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUsuariosPerfis;
using SME.NovoSGP.Abrangencia.Infra.Fila;

namespace SME.NovoSGP.Abrangencia.Aplicacao.UseCases;

public class AbrangenciaSyncUseCase : AbstractUseCase, IAbrangenciaSyncUseCase
{
    public AbrangenciaSyncUseCase(IMediator mediator) : base(mediator)
    {
    }

    public async Task<bool> Executar(MensagemRabbit param)
    {
        var usuarios = await mediator.Send(new ObterUsuariosPerfisQuery());
        foreach (var usuario in usuarios)
        {
            //await mediator.Send(new SincronizarAbrangenciaUsuarioCommand(usuario.Login, usuario.Perfil));
        }

        return true;
    }
}
