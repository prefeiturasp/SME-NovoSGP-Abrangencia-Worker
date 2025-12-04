using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using System.Text;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP
{
    public class RepositorioSupervisorEscolaDre : RepositorioBase<SupervisorEscolaDre>, IRepositorioSupervisorEscolaDre
    {
        public RepositorioSupervisorEscolaDre(ConnectionStringOptions connectionStrings) : base(connectionStrings.SGP_Postgres)
        {
        }

        public async Task<IEnumerable<SupervisorEscolaDre>> ObtemPorDreESupervisor(string dreId, string supervisorId, bool excluidos = false)
        {
            using var conn = ObterConexaoLeitura();
            try
            {
                var query = new StringBuilder();

                query.AppendLine("select id as AtribuicaoSupervisorId, dre_id DreId, escola_id EscolaId, supervisor_id SupervisorId, criado_em, criado_por, alterado_em, alterado_por, criado_rf, alterado_rf, excluido as AtribuicaoExcluida, tipo as TipoAtribuicao ");
                query.AppendLine("from supervisor_escola_dre sed");
                query.AppendLine("where 1 = 1");

                if (!excluidos)
                    query.AppendLine("and excluido = false");

                if (!string.IsNullOrEmpty(supervisorId))
                    query.AppendLine("and sed.supervisor_id = @supervisorId");

                if (!string.IsNullOrEmpty(dreId))
                    query.AppendLine("and sed.dre_id = @dreId");

                return await conn.QueryAsync<SupervisorEscolaDre>(query.ToString(), new { supervisorId, dreId });
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
