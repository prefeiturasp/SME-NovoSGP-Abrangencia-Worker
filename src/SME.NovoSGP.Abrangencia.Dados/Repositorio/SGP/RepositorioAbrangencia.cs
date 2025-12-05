using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Extensoes;
using SME.NovoSGP.Abrangencia.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using System.Text;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP;

public class RepositorioAbrangencia : RepositorioBase<AbrangenciaSintetica>, IRepositorioAbrangencia
{
    public RepositorioAbrangencia(ConnectionStringOptions connectionStrings) : base(connectionStrings.SGP_Postgres)
    {
    }

    public async Task AtualizaAbrangenciaHistorica(IEnumerable<long> ids)
    {
        using var conn = ObterConexao();
        try
        {
            var dtFimVinculo = DateTimeExtension.HorarioBrasilia().Date;

            string comando = $@" update abrangencia as a
                                set historico = true, dt_fim_vinculo = '{dtFimVinculo.Year}-{dtFimVinculo.Month}-{dtFimVinculo.Day}'
                                from abrangencia ab
                                left join turma t on t.id = ab.turma_id
                                where a.id = ab.id
                                and (ab.turma_id is null Or (t.id = ab.turma_id and t.ano_letivo = {dtFimVinculo.Year}))                                    
                                and a.id in (#ids) ";

            for (int i = 0; i < ids.Count(); i = i + 900)
            {
                var iteracao = ids.Skip(i).Take(900);
                await conn.ExecuteAsync(comando.Replace("#ids", string.Join(",", iteracao.Concat(new long[] { 0 }))));
            }
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    public async Task ExcluirAbrangencias(IEnumerable<long> ids)
    {
        using var conn = ObterConexao();
        try
        {
            const string comando = @"delete from public.abrangencia where id in (#ids) and historico = false";

            for (int i = 0; i < ids.Count(); i = i + 900)
            {
                var iteracao = ids.Skip(i).Take(900);

                await conn.ExecuteAsync(comando.Replace("#ids", string.Join(",", iteracao.Concat(new long[] { 0 }))));
            }
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    public async Task InserirAbrangencias(IEnumerable<Dominio.Entidades.Abrangencia> abrangencias, string login)
    {
        using var conn = ObterConexao();
        try
        {
            foreach (var item in abrangencias)
            {
                const string comando = @"insert into public.abrangencia (usuario_id, dre_id, ue_id, turma_id, perfil, historico)
                                        values ((select id from usuario where login = @login), @dreId, @ueId, @turmaId, @perfil, @historico)
                                        RETURNING id"
                ;

                await conn.ExecuteAsync(comando,
                    new
                    {
                        login,
                        dreId = item.DreId,
                        ueId = item.UeId,
                        turmaId = item.TurmaId,
                        perfil = item.Perfil,
                        historico = item.Historico
                    });
            }
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    public async Task<IEnumerable<AbrangenciaSintetica>> ObterAbrangenciaSintetica(string login, Guid perfil, string turmaId = "", bool consideraHistorico = false)
    {
        using var conn = ObterConexao();
        try
        {
            var query = new StringBuilder();

            query.AppendLine("select");
            query.AppendLine("id,");
            query.AppendLine("usuario_id UsuarioId,");
            query.AppendLine("login,");
            query.AppendLine("dre_id DreId,");
            query.AppendLine("codigo_dre CodigoDre,");
            query.AppendLine("ue_id UeId,");
            query.AppendLine("codigo_ue CodigoUe,");
            query.AppendLine("turma_id TurmaId,");
            query.AppendLine("codigo_turma CodigoTurma,");
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
