using MediatR;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterFuncionariosPorLogins;

public class ObterFuncionariosPorLoginsQuery : IRequest<IEnumerable<FuncionarioUnidadeDto>>
{
    public ObterFuncionariosPorLoginsQuery(IEnumerable<string> logins)
    {
        Logins = logins;
    }

    public IEnumerable<string> Logins { get; set; }
}
