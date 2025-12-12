using MediatR;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterCadastroAcessoABAEPorCpf
{
    public class ObterCadastroAcessoABAEPorCpfQuery : IRequest<CadastroAcessoABAEDto>
    {
        public ObterCadastroAcessoABAEPorCpfQuery(string cpf)
        {
            Cpf = cpf;
        }
        public string Cpf { get; set; }
    }
}
