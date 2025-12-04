using MediatR;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUsuariosPerfis;

public class ObterUsuariosPerfisQuery : IRequest<IEnumerable<AbrangenciaUsuarioPerfilDto>>
{
}
