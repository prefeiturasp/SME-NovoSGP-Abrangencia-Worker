using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Text;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio;

public class RepositorioAbrangencia : RepositorioBaseAbrangencia<AbrangenciaSintetica>, IRepositorioAbrangencia
{
    public RepositorioAbrangencia(ConnectionStringOptions connectionStrings, IContextoAplicacao contextoAplicacao) : base(connectionStrings, contextoAplicacao)
    {
    }

    public async Task<IEnumerable<AbrangenciaSintetica>> ObterAbrangenciaSintetica(string login, Guid perfil, string turmaId = "", bool consideraHistorico = false)
    {
        using var conn = ObterConexaoAbrangencia();
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
