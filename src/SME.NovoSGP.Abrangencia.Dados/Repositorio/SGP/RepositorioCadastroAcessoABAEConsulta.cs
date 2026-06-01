using SME.NovoSGP.Abrangencia.Dados.Interceptors;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Extensoes;
using SME.NovoSGP.Abrangencia.Infra.Dtos;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using System.Text;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP
{
    public class RepositorioCadastroAcessoABAEConsulta : RepositorioBase<CadastroAcessoABAE>, IRepositorioCadastroAcessoABAEConsulta
    {
        public RepositorioCadastroAcessoABAEConsulta(ConnectionStringOptions connectionStrings) : base(connectionStrings.SGP_PostgresConsultas)
        {
        }

        public async Task<CadastroAcessoABAEDto> ObterCadastroABAEPorCpf(string cpf)
        {
            using var conn = ObterConexao();
            try
            {
                cpf = cpf.FormatarCPF();

                const string query = @"select
	                                    a.id,
	                                    a.nome,
	                                    a.ue_id as ueId,
	                                    a.cpf,
	                                    a.email,
	                                    a.telefone,
	                                    a.telefone,
	                                    a.situacao,
	                                    a.cep,
	                                    a.endereco,
	                                    a.numero,
	                                    a.complemento,
	                                    a.bairro,
	                                    a.cidade,
	                                    a.estado,
	                                    a.excluido
                                    from
	                                    cadastro_acesso_abae a
                                    where
	                                    not a.excluido
	                                    and a.cpf = @cpf";

                return await conn.QueryFirstOrDefaultAsync<CadastroAcessoABAEDto>(query, new
                {
                    cpf
                });
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
