using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio;

public class RepositorioDreConsulta : RepositorioBaseSGP<Dre>, IRepositorioDreConsulta
{
    public RepositorioDreConsulta(ConnectionStringOptions connectionStrings, IContextoAplicacao contextoAplicacao) : base(connectionStrings, contextoAplicacao)
    {
    }

    public async Task<(IEnumerable<Dre> Dres, string[] CodigosDresNaoEncontrados)> MaterializarCodigosDre(string[] idDres)
    {
        using var conn = ObterConexaoSGPConsulta();
        try
        {
            string query = @"SELECT id, dre_id, abreviacao, nome, data_atualizacao FROM public.dre where dre_id in (#ids);";
            string[] naoEncontradas;

            var armazenados = await conn.QueryAsync<Dre>(query.Replace("#ids", string.Join(",", idDres.Select(x => $"'{x}'"))));

            naoEncontradas = idDres.Where(x => !armazenados.Select(y => y.CodigoDre).Contains(x)).ToArray();

            return (armazenados, naoEncontradas);
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }
}
