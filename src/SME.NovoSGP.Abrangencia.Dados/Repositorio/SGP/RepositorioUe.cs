using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Data;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP;

public class RepositorioUe : RepositorioBaseSGP<Ue>, IRepositorioUe
{
    private const string QuerySincronizacao = @"SELECT id, ue_id, dre_id, nome, tipo_escola, data_atualizacao FROM public.ue where ue_id in (#ids);";
    private const string Update = "UPDATE public.ue SET nome = @nome, tipo_escola = @tipoEscola, data_atualizacao = @dataAtualizacao WHERE id = @id;";

    public RepositorioUe(ConnectionStringOptions connectionStrings, IContextoAplicacao contextoAplicacao) : base(connectionStrings, contextoAplicacao)
    {
    }

    public async Task<IEnumerable<Ue>> SincronizarAsync(IEnumerable<Ue> entidades, IEnumerable<Dre> dres)
    {
        using var conn = ObterConexaoSGP();
        try
        {
            List<Ue> resultado = new List<Ue>();

            for (int i = 0; i < entidades.Count(); i = i + 900)
            {
                var iteracao = entidades.Skip(i).Take(900);

                var armazenados = await conn.QueryAsync<Ue>(QuerySincronizacao.Replace("#ids", string.Join(",", iteracao.Select(x => $"'{x.CodigoUe}'"))));

                var novos = iteracao.Where(x => !armazenados.Select(y => y.CodigoUe).Contains(x.CodigoUe));

                await PersisteNovosRegistros(dres, resultado, novos);

                var modificados = from c in iteracao
                                  join l in armazenados on c.CodigoUe equals l.CodigoUe
                                  where l.DataAtualizacao != DateTime.Today &&
                                        (c.Nome != l.Nome ||
                                        c.TipoEscola != l.TipoEscola)
                                  select new Ue()
                                  {
                                      CodigoUe = c.CodigoUe,
                                      DataAtualizacao = DateTime.Today,
                                      Dre = l.Dre,
                                      DreId = l.DreId,
                                      Id = l.Id,
                                      Nome = c.Nome,
                                      TipoEscola = c.TipoEscola
                                  };

                foreach (var item in modificados)
                {
                    await conn.ExecuteAsync(Update, new { nome = item.Nome, tipoEscola = item.TipoEscola, dataAtualizacao = item.DataAtualizacao, id = item.Id });

                    resultado.Add(item);
                }

                resultado.AddRange(armazenados.Where(x => !resultado.Select(y => y.CodigoUe).Contains(x.CodigoUe)));
            }

            return resultado;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    private async Task PersisteNovosRegistros(IEnumerable<Dre> dres, List<Ue> resultado, IEnumerable<Ue> novos)
    {
        foreach (var item in novos)
        {
            item.DataAtualizacao = DateTime.Today;
            item.Dre = dres.First(x => x.CodigoDre == item.Dre.CodigoDre);
            item.DreId = item.Dre.Id;
            item.Id = await SalvarAsync(item);
            resultado.Add(item);
        }
    }
}
