using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using System.Text;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP;

public class RepositorioAbrangencia : RepositorioBase<AbrangenciaSintetica>, IRepositorioAbrangencia
{
    public RepositorioAbrangencia(ConnectionStringOptions connectionStrings) : base(connectionStrings.SGP_Postgres)
    {
    }

    public Task AtualizaAbrangenciaHistorica(IEnumerable<long> paraAtualizar)
    {
        throw new NotImplementedException();
    }

    public Task ExcluirAbrangencias(IEnumerable<long> ids)
    {
        throw new NotImplementedException();
    }

    public Task InserirAbrangencias(IEnumerable<Dominio.Entidades.Abrangencia> abrangencias, string login)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<AbrangenciaSintetica>> ObterAbrangenciaSintetica(string login, Guid perfil, string turmaId = "", bool consideraHistorico = false)
    {
        using var conn = ObterConexao();
        try
        {
            var query = new StringBuilder();

            query.AppendLine("select");
            query.AppendLine("id,");
            query.AppendLine("usuario_id,");
            query.AppendLine("login,");
            query.AppendLine("dre_id,");
            query.AppendLine("codigo_dre,");
            query.AppendLine("ue_id,");
            query.AppendLine("codigo_ue,");
            query.AppendLine("turma_id,");
            query.AppendLine("codigo_turma,");
            query.AppendLine("perfil,");
            query.AppendLine("historico");
            query.AppendLine("from");
            query.AppendLine("public.v_abrangencia_sintetica where login = @login and perfil = @perfil");

            if (consideraHistorico)
                query.AppendLine("and historico = true");
            else query.AppendLine("and historico = false");

            if (!string.IsNullOrEmpty(turmaId))
                query.AppendLine("and codigo_turma = @turmaId");

            return await conn.QueryAsync<AbrangenciaSintetica>(query.ToString(), new { login, perfil, turmaId });
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }
}
