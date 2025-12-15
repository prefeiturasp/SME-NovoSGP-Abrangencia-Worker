using MediatR;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterCadastroAcessoABAEPorCpf
{
    public class ObterCadastroAcessoABAEPorCpfUsuarioQueryHandler : IRequestHandler<ObterCadastroAcessoABAEPorCpfQuery, CadastroAcessoABAEDto>
    {
        private readonly IRepositorioCadastroAcessoABAEConsulta repositorioCadastroAcessoABAEConsulta;
        public ObterCadastroAcessoABAEPorCpfUsuarioQueryHandler(IRepositorioCadastroAcessoABAEConsulta repositorioCadastroAcessoABAEConsulta)
        {
            this.repositorioCadastroAcessoABAEConsulta = repositorioCadastroAcessoABAEConsulta;
        }

        public Task<CadastroAcessoABAEDto> Handle(ObterCadastroAcessoABAEPorCpfQuery request, CancellationToken cancellationToken)
        {
            return repositorioCadastroAcessoABAEConsulta.ObterCadastroABAEPorCpf(request.Cpf);
        }
    }
}
