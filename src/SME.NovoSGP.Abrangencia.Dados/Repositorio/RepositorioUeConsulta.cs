using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio;

public class RepositorioUeConsulta : RepositorioBaseSGP<Ue>, IRepositorioUeConsulta
{
    public RepositorioUeConsulta(ConnectionStringOptions connectionStrings, IContextoAplicacao contextoAplicacao) : base(connectionStrings, contextoAplicacao)
    {
    }

    public async Task<(List<Ue> Ues, string[] CodigosUesNaoEncontradas)> MaterializarCodigosUe(string[] idUes)
    {
        using var conn = ObterConexaoSGPConsulta();
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
}
