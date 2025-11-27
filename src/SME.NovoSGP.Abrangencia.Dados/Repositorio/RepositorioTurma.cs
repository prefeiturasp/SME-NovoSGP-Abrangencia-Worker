using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Excecoes;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Data;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio;

public class RepositorioTurma : RepositorioBaseSGP<Turma>, IRepositorioTurma
{
    public RepositorioTurma(ConnectionStringOptions connectionStrings, IContextoAplicacao contextoAplicacao) : base(connectionStrings, contextoAplicacao)
    {
    }

    public async Task<IEnumerable<Turma>> MaterializarCodigosTurma(string[] idTurmas, string[] codigosNaoEncontrados)
    {
        using var conn = ObterConexaoSGPConsulta();
        try
        {
            List<Turma> resultado = new List<Turma>();
            List<string> naoEncontrados = new List<string>();

            for (int i = 0; i < idTurmas.Count(); i = i + 900)
            {
                var iteracao = idTurmas.Skip(i).Take(900);

                var armazenados = await conn.QueryAsync<Turma>(QuerySincronizacao.Replace("#ids", string.Join(",", idTurmas.Select(x => $"'{x}'"))));

                naoEncontrados.AddRange(iteracao.Where(x => !armazenados.Select(y => y.CodigoTurma).Contains(x)));

                resultado.AddRange(armazenados);
            }
            codigosNaoEncontrados = naoEncontrados.ToArray();

            return resultado;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    public async Task<IEnumerable<Turma>> SincronizarAsync(IEnumerable<Turma> entidades, IEnumerable<Ue> ues)
    {
        using var conn = ObterConexaoSGP();
        try
        {
            List<Turma> resultado = new List<Turma>();

            var anoLetivoConsiderado = (from e in entidades
                                        where !e.Extinta
                                        orderby e.AnoLetivo descending
                                        select e.AnoLetivo).Last();

            await AtualizarRemoverTurmasExtintasAsync(entidades, anoLetivoConsiderado);

            for (int i = 0; i < entidades.Count(); i = i + 900)
            {
                var iteracao = entidades.Skip(i).Take(900);

                var armazenados = (await conn.QueryAsync<Turma>(
                    QuerySincronizacao.Replace("#ids", string.Join(",", iteracao.Select(x => $"'{x.CodigoTurma}'"))))).ToList();

                var idsArmazenados = armazenados.Select(y => y.CodigoTurma);

                var novos = iteracao
                    .Where(x => !x.Extinta && !idsArmazenados.Contains(x.CodigoTurma))
                    .ToList();

                foreach (var item in novos)
                {
                    item.DataAtualizacao = DateTime.Today;
                    item.Ue = ues.First(x => x.CodigoUe == item.Ue.CodigoUe);
                    item.UeId = item.Ue.Id;
                    item.Id = (long)await SalvarAsync(item);
                    resultado.Add(item);
                }

                var modificados = from c in iteracao
                                  join l in armazenados on c.CodigoTurma equals l.CodigoTurma
                                  where c.Nome != l.Nome ||
                                        c.Ano != l.Ano ||
                                        c.TipoTurma != l.TipoTurma ||
                                        c.AnoLetivo != l.AnoLetivo ||
                                        c.ModalidadeCodigo != l.ModalidadeCodigo ||
                                        c.Semestre != l.Semestre ||
                                        c.QuantidadeDuracaoAula != l.QuantidadeDuracaoAula ||
                                        c.TipoTurno != l.TipoTurno ||
                                        c.EnsinoEspecial != l.EnsinoEspecial ||
                                        c.EtapaEJA != l.EtapaEJA ||
                                        c.SerieEnsino != l.SerieEnsino ||
                                        c.DataInicio.HasValue != l.DataInicio.HasValue ||
                                        (c.DataInicio.HasValue && l.DataInicio.HasValue && c.DataInicio.Value.Date != l.DataInicio.Value.Date) ||
                                        c.DataFim.HasValue != l.DataFim.HasValue ||
                                        (c.DataFim.HasValue && l.DataFim.HasValue && c.DataFim.Value.Date != l.DataFim.Value.Date)
                                  select new Turma()
                                  {
                                      Ano = c.Ano,
                                      AnoLetivo = c.AnoLetivo,
                                      CodigoTurma = c.CodigoTurma,
                                      TipoTurma = c.TipoTurma,
                                      DataAtualizacao = DateTime.Today,
                                      Id = l.Id,
                                      ModalidadeCodigo = c.ModalidadeCodigo,
                                      Nome = c.Nome,
                                      QuantidadeDuracaoAula = c.QuantidadeDuracaoAula,
                                      Semestre = c.Semestre,
                                      TipoTurno = c.TipoTurno,
                                      Ue = l.Ue,
                                      UeId = l.UeId,
                                      EnsinoEspecial = c.EnsinoEspecial,
                                      EtapaEJA = c.EtapaEJA,
                                      DataInicio = c.DataInicio,
                                      SerieEnsino = c.SerieEnsino,
                                      DataFim = c.DataFim,
                                      Extinta = c.Extinta,
                                  };

                foreach (var item in modificados)
                {
                    await conn.ExecuteAsync(Update, new
                    {
                        nome = item.Nome,
                        ano = item.Ano,
                        tipoTurma = item.TipoTurma,
                        anoLetivo = item.AnoLetivo,
                        modalidadeCodigo = item.ModalidadeCodigo,
                        semestre = item.Semestre,
                        qtDuracaoAula = item.QuantidadeDuracaoAula,
                        tipoTurno = item.TipoTurno,
                        dataAtualizacao = item.DataAtualizacao,
                        id = item.Id,
                        ensinoEspecial = item.EnsinoEspecial,
                        etapaEja = item.EtapaEJA,
                        dataInicio = item.DataInicio,
                        serieEnsino = item.SerieEnsino,
                        dataFim = item.DataFim
                    });

                    resultado.Add(item);
                }

                resultado.AddRange(armazenados.Where(x => !resultado.Select(y => y.CodigoTurma).Contains(x.CodigoTurma)));
            }

            return resultado;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    private async Task AtualizarRemoverTurmasExtintasAsync(IEnumerable<Turma> entidades, int anoLetivo)
    {
        using var conn = ObterConexaoSGP();
        var codigosTurmas = entidades
           .Where(e => !e.Extinta)
           .OrderBy(e => e.CodigoTurma)
           .Select(e => $"'{e.CodigoTurma}'")?.ToArray();

        var listaTurmas = string.Join(",", codigosTurmas);
        var transacao = conn.BeginTransaction();
        try
        {
            var codigosTurmasParaHistorico = await ObterCodigosTurmasParaQueryAtualizarTurmasComoHistoricas(anoLetivo, true, listaTurmas, transacao);

            if (codigosTurmasParaHistorico.Any())
            {
                var sqlQueryAtualizarTurmasComoHistoricas = QueryDefinirTurmaHistorica
                    .Replace("#turmaId", MapearParaCodigosQuerySql(codigosTurmasParaHistorico));

                await conn.ExecuteAsync(sqlQueryAtualizarTurmasComoHistoricas, transacao);
            }

            var codigosTurmasARemover = await ObterCodigosTurmasParaQueryAtualizarTurmasComoHistoricas(anoLetivo, false, listaTurmas, transacao);

            if (codigosTurmasARemover.Any())
            {
                var sqlExcluirTurmas = Delete.Replace("#queryIdsConselhoClasseTurmasForaListaCodigos", QueryIdsConselhoClasseTurmasForaListaCodigos)
                     .Replace("#queryFechamentoAlunoTurmasForaListaCodigos", QueryFechamentoAlunoTurmasForaListaCodigos)
                     .Replace("#queryIdsFechamentoTurmaTurmasForaListaCodigos", QueryIdsFechamentoTurmaTurmasForaListaCodigos)
                     .Replace("#queryIdsFechamentoTurmaDisciplinaTurmasForaListaCodigos", QueryIdsFechamentoTurmaDisciplinaTurmasForaListaCodigos)
                     .Replace("#queryIdsTurmasForaListaCodigos", QueryIdsTurmasForaListaCodigos)
                     .Replace("#queryIdsAulasTurmasForaListaCodigos", QueryAulasTurmasForaListaCodigos)
                     .Replace("#turmaId", MapearParaCodigosQuerySql(codigosTurmasARemover));
                
                await conn.ExecuteAsync(sqlExcluirTurmas, transacao);
            }

            transacao.Commit();
        }
        catch (Exception ex)
        {
            transacao.Rollback();
            throw new NegocioException("Erro ao atualizar ou excluir turmas extintas", ex);
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    private static string MapearParaCodigosQuerySql(IEnumerable<string> codigos)
    {
        string[] arrCodigos = codigos.Select(x => $"'{x}'").ToArray();
        return string.Join(",", arrCodigos);
    }

    private async Task<IEnumerable<string>> ObterCodigosTurmasParaQueryAtualizarTurmasComoHistoricas(int anoLetivo, bool definirTurmasComoHistorica, string listaTurmas, IDbTransaction transacao)
    {
        using var conn = ObterConexaoSGPConsulta();
        try
        {
            var sqlQuery = GerarQueryCodigosTurmasForaLista(anoLetivo, true).Replace("#idsTurmas", listaTurmas);
            return await conn.QueryAsync<string>(sqlQuery, transacao);
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }       
    }

    private static string GerarQueryCodigosTurmasForaLista(int anoLetivo, bool definirTurmasComoHistorica) =>
           $@"select distinct t.turma_id
                    from turma t
                        inner join tipo_calendario tc
                            on t.ano_letivo = tc.ano_letivo and
                               t.modalidade_codigo = t.modalidade_codigo 
                        inner join periodo_escolar pe
                            on tc.id = pe.tipo_calendario_id             
                        inner join (select id, data_inicio, modalidade_codigo
                                        from turma
                                    where ano_letivo = {anoLetivo} and
                                          turma_id not in (#idsTurmas)) t2
                            on t.id = t2.id and
                               t.modalidade_codigo = t2.modalidade_codigo
                where t.ano_letivo = {anoLetivo} and                      
                      pe.bimestre = 1 and                      
                      t.dt_fim_eol is not null and 
                      t.dt_fim_eol {(definirTurmasComoHistorica ? ">=" : "<")} pe.periodo_inicio";

    private const string Update = @"
                    update
                        public.turma
                    set
                        nome = @nome,
                        ano = @ano,
                        ano_letivo = @anoLetivo,
                        modalidade_codigo = @modalidadeCodigo,
                        semestre = @semestre,
                        qt_duracao_aula = @qtDuracaoAula,
                        tipo_turno = @tipoTurno,
                        data_atualizacao = @dataAtualizacao,
                        ensino_especial = @ensinoEspecial,
                        etapa_eja = @etapaEja,
                        data_inicio = @dataInicio,
                        serie_ensino = @serieEnsino,
                        dt_fim_eol = @dataFim,
                        tipo_turma = @tipoTurma
                    where
                        id = @id;";

    private const string Delete = @"
                    delete from public.compensacao_ausencia_aluno
                    where compensacao_ausencia_id in (select id
                                                      from public.compensacao_ausencia
                                                      where turma_id = #turmaId);

                    delete from public.compensacao_ausencia
                    where turma_id = #turmaId;

                    delete from public.pendencia_fechamento
                    where fechamento_turma_disciplina_id in (#queryIdsFechamentoTurmaDisciplinaTurmasForaListaCodigos);

                    delete from public.wf_aprovacao_nota_fechamento
                    where fechamento_nota_id in (select id
                                                 from public.fechamento_nota
                                                 where fechamento_aluno_id in (#queryFechamentoAlunoTurmasForaListaCodigos));

                    delete from public.fechamento_nota
                    where fechamento_aluno_id in (#queryFechamentoAlunoTurmasForaListaCodigos);

                    delete from public.fechamento_aluno
                    where fechamento_turma_disciplina_id in (#queryIdsFechamentoTurmaDisciplinaTurmasForaListaCodigos);

                    delete from public.fechamento_turma_disciplina
                    where fechamento_turma_id in (#queryIdsFechamentoTurmaTurmasForaListaCodigos);

                    delete from public.conselho_classe_nota
                    where conselho_classe_aluno_id in (select id
                                                       from public.conselho_classe_aluno
                                                       where conselho_classe_id in (#queryIdsConselhoClasseTurmasForaListaCodigos));

                    delete from public.conselho_classe_aluno
                    where conselho_classe_id in (#queryIdsConselhoClasseTurmasForaListaCodigos);        

                    delete from public.conselho_classe
                    where fechamento_turma_id in (select id
                                                  from public.fechamento_turma
                                                  where turma_id = #turmaId);

                    delete from public.fechamento_turma
                    where turma_id in (#queryIdsTurmasForaListaCodigos);
                  
                    delete from public.frequencia_aluno
                    where turma_id = #turmaId;

                    delete from public.diario_bordo
                    where aula_id in (#queryIdsAulasTurmasForaListaCodigos);         
                    
                    delete from public.notificacao_frequencia
                    where aula_id in (#queryIdsAulasTurmasForaListaCodigos);

                    delete from public.registro_frequencia
                    where aula_id in (#queryIdsAulasTurmasForaListaCodigos);

                    delete from public.aula
                    where turma_id = #turmaId;
                    
                    delete from public.turma
                    where turma_id = #turmaId;";

    private const string QuerySincronizacao = @"
                    select
                        id,
                        turma_id,
                        ue_id,
                        nome,
                        ano,
                        ano_letivo,
                        modalidade_codigo,
                        semestre,
                        qt_duracao_aula,
                        tipo_turno,
                        data_atualizacao,
                        ensino_especial,
                        etapa_eja,
                        data_inicio,
                        dt_fim_eol,
                        tipo_turma
                    from
                        public.turma
                    where turma_id in (#ids);";

    private const string QueryIdsTurmasForaListaCodigos = "select id from public.turma where turma_id in (#turmaId)";

    private const string QueryIdsFechamentoTurmaDisciplinaTurmasForaListaCodigos = @"select id
                                                                                         from public.fechamento_turma_disciplina
                                                                                         where fechamento_turma_id in (select id
                                                                                                                       from public.fechamento_turma
                                                                                                                       where turma_id in (#turmaId))";

    private const string QueryIdsFechamentoTurmaTurmasForaListaCodigos = @"select id
                                                                               from public.fechamento_turma
                                                                               where turma_id in (#turmaId)";

    private const string QueryIdsConselhoClasseTurmasForaListaCodigos = @"select id
                                                                              from public.conselho_classe
                                                                              where fechamento_turma_id in (#queryIdsFechamentoTurmaTurmasForaListaCodigos)";

    private const string QueryFechamentoAlunoTurmasForaListaCodigos = @"select id
                                                                            from public.fechamento_aluno
                                                                            where fechamento_turma_disciplina_id in (#queryIdsFechamentoTurmaDisciplinaTurmasForaListaCodigos)";

    private const string QueryAulasTurmasForaListaCodigos = @"select id from public.aula where turma_id in (#turmaId)";


    private const string QueryDefinirTurmaHistorica = "update public.turma set historica = true where turma_id in (#turmaId);";
}
