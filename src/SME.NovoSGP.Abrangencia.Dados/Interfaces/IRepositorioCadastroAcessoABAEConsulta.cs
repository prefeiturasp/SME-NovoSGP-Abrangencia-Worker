using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces
{
    public interface IRepositorioCadastroAcessoABAEConsulta
    {
        Task<CadastroAcessoABAEDto> ObterCadastroABAEPorCpf(string cpf);
    }
}
