using MediatR;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUsuariosPerfis;

public class ObterUsuariosPerfisQueryHandler : IRequestHandler<ObterUsuariosPerfisQuery, IEnumerable<AbrangenciaUsuarioPerfilDto>>
{
    private readonly IRepositorioUsuario repositorioUsuario;

    public ObterUsuariosPerfisQueryHandler(IRepositorioUsuario repositorioUsuario)
    {
        this.repositorioUsuario = repositorioUsuario;
    }

    public async Task<IEnumerable<AbrangenciaUsuarioPerfilDto>> Handle(ObterUsuariosPerfisQuery request, CancellationToken cancellationToken)
    {
        return await repositorioUsuario.ObterUsuariosPerfis();
    }
}
