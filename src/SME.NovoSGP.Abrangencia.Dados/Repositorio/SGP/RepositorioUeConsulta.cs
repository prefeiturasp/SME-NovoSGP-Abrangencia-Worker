using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP;

public class RepositorioUeConsulta : RepositorioBase<Ue>, IRepositorioUeConsulta
{
    public RepositorioUeConsulta(ConnectionStringOptions connectionStrings) : base(connectionStrings.SGP_Postgres)
    {
    }

    public async Task<(List<Ue> Ues, string[] CodigosUesNaoEncontradas)> MaterializarCodigosUe(string[] idUes)
    {
        using var conn = ObterConexaoLeitura();
        try
        {
            string query = @"SELECT id, ue_id, dre_id, nome, tipo_escola, data_atualizacao FROM public.ue where ue_id in (#ids);";

            List<Ue> resultado = new List<Ue>();
            List<string> naoEncontrados = new List<string>();
            string[] codigosNaoEncontrados;

            for (int i = 0; i < idUes.Count(); i = i + 900)
            {
                var iteracao = idUes.Skip(i).Take(900);

                var armazenados = await conn.QueryAsync<Ue>(query.Replace("#ids", string.Join(",", idUes.Select(x => $"'{x}'"))));

                naoEncontrados.AddRange(iteracao.Where(x => !armazenados.Select(y => y.CodigoUe).Contains(x)));

                resultado.AddRange(armazenados);
            }
            codigosNaoEncontrados = naoEncontrados.ToArray();

            return (resultado, codigosNaoEncontrados);
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    public async Task<Ue> ObterUeComDrePorId(long ueId)
    {
        using var conn = ObterConexaoLeitura();
        try
        {
            var query = @"select
	                        u.id,
	                        u.ue_id as codigoUe,
	                        u.data_atualizacao::timestamp as dataAtualizacao,
	                        u.dre_id as dreId,
	                        u.nome,
	                        u.tipo_escola as tipoEscola,
	                        d.id,
	                        d.abreviacao,
	                        d.dre_id as codigoDre,
	                        d.data_atualizacao::timestamp as dataAtualizacao,
	                        d.nome
                        from
	                        ue u
                        inner join dre d on
	                        d.id = u.dre_id
                        where
	                        u.id = @ueId";

            var resultado = await conn.QueryAsync<Ue, Dre, Ue>(query, (ue, dre) =>
            {
                ue.AdicionarDre(dre);
                return ue;
            },
            new { ueId },
            splitOn: "id");

            return resultado.FirstOrDefault()!;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }
}
