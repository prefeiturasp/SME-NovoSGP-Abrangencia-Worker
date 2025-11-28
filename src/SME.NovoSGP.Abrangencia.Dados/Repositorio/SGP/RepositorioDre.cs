using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP;

public class RepositorioDre : RepositorioBaseSGP<Dre>, IRepositorioDre
{
    private const string QuerySincronizacao = @"SELECT id, dre_id, abreviacao, nome, data_atualizacao FROM public.dre where dre_id in (#ids);";
    private const string Update = "UPDATE public.dre SET abreviacao = @abreviacao, nome = @nome, data_atualizacao = @dataAtualizacao WHERE id = @id;";

    public RepositorioDre(ConnectionStringOptions connectionStrings, IContextoAplicacao contextoAplicacao) : base(connectionStrings, contextoAplicacao)
    {
    }

    public async Task<IEnumerable<Dre>> SincronizarAsync(IEnumerable<Dre> entidades)
    {
        using var conn = ObterConexaoSGP();
        try
        {
            List<Dre> resultado = new List<Dre>();

            var armazenados = await conn.QueryAsync<Dre>(QuerySincronizacao.Replace("#ids", string.Join(",", entidades.Select(x => $"'{x.CodigoDre}'"))));

            var novos = entidades.Where(x => !armazenados.Select(y => y.CodigoDre).Contains(x.CodigoDre));

            foreach (var item in novos)
            {
                item.DataAtualizacao = DateTime.Today;
                item.Id = await SalvarAsync(item);

                resultado.Add(item);
            }

            var modificados = from c in entidades
                              join l in armazenados on c.CodigoDre equals l.CodigoDre
                              where l.DataAtualizacao != DateTime.Today &&
                                    (c.Abreviacao != l.Abreviacao ||
                                    c.Nome != l.Nome)
                              select new Dre()
                              {
                                  Id = l.Id,
                                  Nome = c.Nome,
                                  Abreviacao = c.Abreviacao,
                                  CodigoDre = c.CodigoDre,
                                  DataAtualizacao = DateTime.Today
                              };

            foreach (var item in modificados)
            {
                await conn.ExecuteAsync(Update, new { abreviacao = item.Abreviacao, nome = item.Nome, dataAtualizacao = item.DataAtualizacao, id = item.Id });
                resultado.Add(item);
            }

            resultado.AddRange(armazenados.Where(x => !resultado.Select(y => y.CodigoDre).Contains(x.CodigoDre)));

            return resultado;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }
}
