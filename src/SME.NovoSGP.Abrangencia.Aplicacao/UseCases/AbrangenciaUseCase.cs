using MediatR;
using Microsoft.Extensions.Logging;
using SME.NovoSGP.Abrangencia.Aplicacao.Interfaces;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterAbrangenciaCompactaVigenteEolPorLoginEPerfil;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterAbrangenciaEolSupervisor;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterAbrangenciaParaSupervisor;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterCadastroAcessoABAEPorCpf;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterDreMaterializarCodigos;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterEstruturaInstuticionalVigentePorTurma;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterPerfisPorLogin;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterTurmasPorIds;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUeMaterializarCodigos;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUePorId;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Constantes;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Enumerados;
using SME.NovoSGP.Abrangencia.Entidades;
using SME.NovoSGP.Abrangencia.Infra.Dtos;
using SME.NovoSGP.Abrangencia.Infra.Fila;

namespace SME.NovoSGP.Abrangencia.Aplicacao.UseCases;

public class AbrangenciaUseCase : AbstractUseCase, IAbrangenciaUseCase
{
    private readonly IRepositorioAbrangencia repositorioAbrangencia;
    private readonly IRepositorioTurma repositorioTurma;
    private readonly IRepositorioUe repositorioUe;
    private readonly IRepositorioDre repositorioDre;
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<AbrangenciaUseCase> logger;
    public AbrangenciaUseCase(IMediator mediator, IRepositorioAbrangencia repositorioAbrangencia, IRepositorioTurma repositorioTurma, IRepositorioUe repositorioUe, IRepositorioDre repositorioDre,
        IUnitOfWork unitOfWork, IRepositorioUsuario repositorioUsuario, ILogger<AbrangenciaUseCase> logger) : base(mediator)
    {
        this.repositorioAbrangencia = repositorioAbrangencia;
        this.repositorioTurma = repositorioTurma;
        this.repositorioUe = repositorioUe;
        this.repositorioDre = repositorioDre;
        this.unitOfWork = unitOfWork;
        this.repositorioUsuario = repositorioUsuario;
        this.logger = logger;
    }

    public async Task<bool> Executar(MensagemRabbit param)
    {
        var usuario = param.ObterObjetoMensagem<AbrangenciaUsuarioPerfilDto>();

        foreach (var perfil in usuario.Perfil)
        {
            try
            {
                await ProcessarAbrangencia(usuario.Login, perfil);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Erro ao processar abrangência para o usuário {usuario.Login} e perfil {perfil}");
            }
        }

        return true;
    }

    private async Task<bool> ProcessarAbrangencia(string login, Guid perfil)
    {
        if (string.IsNullOrWhiteSpace(login)) return true;

        AbrangenciaCompactaVigenteRetornoEOLDTO consultaEol = null;
        AbrangenciaCompactaVigenteRetornoEOLDTO abrangenciaEol = null;

        var ehSupervisor = perfil == Perfis.PERFIL_SUPERVISOR;
        var ehProfessorCJ = perfil == Perfis.PERFIL_CJ || perfil == Perfis.PERFIL_CJ_INFANTIL;
        var ehABAE = perfil == Perfis.PERFIL_ABAE;

        if (ehSupervisor)
        {
            var uesIds = await ObterAbrangenciaEolSupervisor(login);
            if (!uesIds.Any())
                return true;
            var abrangenciaSupervisor = await mediator.Send(new ObterAbrangenciaParaSupervisorQuery(uesIds.ToArray()));
            abrangenciaEol = new AbrangenciaCompactaVigenteRetornoEOLDTO()
            {
                Abrangencia = abrangenciaSupervisor.Abrangencia,
                IdUes = abrangenciaSupervisor.Dres.SelectMany(x => x.Ues.Select(y => y.Codigo)).ToArray()
            };
        }
        else if (ehProfessorCJ)
            return true;
        else if (ehABAE)
        {
            var usuario = await repositorioUsuario.ObterPorCodigoRfLogin(null!, login);

            if (usuario is not null)
            {
                //se for usuário ABAE, o CPF e o login serão os mesmos
                var cadastroABAE = await mediator.Send(new ObterCadastroAcessoABAEPorCpfQuery(login));

                if (cadastroABAE?.UeId is not null)
                {
                    // Obter informações da UE e DRE baseadas no cadastro ABAE
                    var ue = await mediator.Send(new ObterUePorIdQuery(cadastroABAE.UeId));

                    if (ue?.Dre is not null)
                    {
                        abrangenciaEol = new AbrangenciaCompactaVigenteRetornoEOLDTO()
                        {
                            Abrangencia = new AbrangenciaCargoRetornoEolDTO { Abrangencia = Dominio.Enumerados.Abrangencia.UE },
                            IdDres = new[] { ue.Dre.CodigoDre },
                            IdUes = new[] { ue.CodigoUe }
                        };
                    }
                }
            }
        }
        else
            consultaEol = await mediator.Send(new ObterAbrangenciaCompactaVigenteEolPorLoginEPerfilQuery(login, perfil));

        if (consultaEol != null || abrangenciaEol != null)
        {
            // Enquanto o EOl consulta, tentamos ganhar tempo obtendo a consulta sintetica
            var consultaAbrangenciaSintetica = repositorioAbrangencia.ObterAbrangenciaSintetica(login, perfil, string.Empty);

            if (abrangenciaEol == null)
                abrangenciaEol = consultaEol;
            var abrangenciaSintetica = await consultaAbrangenciaSintetica;

            if (abrangenciaEol != null)
            {
                // sincronizamos as dres, ues e turmas
                var estrutura = await MaterializarEstruturaInstitucional(abrangenciaEol);

                // sincronizamos a abrangencia do login + perfil
                await SincronizarAbrangencia(abrangenciaSintetica, abrangenciaEol?.Abrangencia?.Abrangencia, ehSupervisor, estrutura, login, perfil);
            }
        }

        return true;
    }

    private async Task<string[]> ObterAbrangenciaEolSupervisor(string login)
    {
        var listaEscolasDresSupervior = await mediator.Send(new ObterAbrangenciaEolSupervisorQuery(login, string.Empty));

        if (listaEscolasDresSupervior.Any())
            return listaEscolasDresSupervior.Select(escola => escola.UeId).ToArray();

        return Array.Empty<string>();
    }

    private async Task<(IEnumerable<Dre> Dres, IEnumerable<Ue> Ues, IEnumerable<Turma> Turmas)> MaterializarEstruturaInstitucional(AbrangenciaCompactaVigenteRetornoEOLDTO abrangenciaEol)
    {
        IEnumerable<Dre> dres = Enumerable.Empty<Dre>();
        IEnumerable<Ue> ues = Enumerable.Empty<Ue>();
        List<Turma> turmas = new List<Turma>();
        string[] codigosNaoEncontrados = [];

        if (abrangenciaEol.IdDres != null && abrangenciaEol.IdDres.Length > 0)
        {
            var retorno = await mediator.Send(new ObterDreMaterializarCodigosQuery(abrangenciaEol.IdDres));
            dres = retorno.Dres;
            codigosNaoEncontrados = retorno.CodigosDresNaoEncontrados;
        }

        if (abrangenciaEol.IdUes != null && abrangenciaEol.IdUes.Length > 0)
        {
            var retorno = await mediator.Send(new ObterUeMaterializarCodigosQuery(abrangenciaEol.IdUes));
            ues = retorno.Ues;
            codigosNaoEncontrados = retorno.CodigosUesNaoEncontradas;
        }

        if (abrangenciaEol.IdTurmas != null && abrangenciaEol.IdTurmas.Length > 0)
        {
            turmas.AddRange(await repositorioTurma.MaterializarCodigosTurma(abrangenciaEol.IdTurmas, codigosNaoEncontrados));
            turmas.AddRange(await ImportarTurmasNaoEncontradas(codigosNaoEncontrados));
        }

        return (dres, ues, turmas);
    }

    private async Task<IEnumerable<Turma>> ImportarTurmasNaoEncontradas(string[] codigosNaoEncontrados)
    {
        if (codigosNaoEncontrados != null && codigosNaoEncontrados.Length > 0)
        {
            var estruturaInstitucionalRetornoEolDTO = await mediator.Send(new ObterEstruturaInstuticionalVigentePorTurmaQuery(codigosTurma: codigosNaoEncontrados));
            if (estruturaInstitucionalRetornoEolDTO != null)
                await SincronizarEstruturaInstitucional(estruturaInstitucionalRetornoEolDTO);
        }

        return await repositorioTurma.MaterializarCodigosTurma(codigosNaoEncontrados, codigosNaoEncontrados);

    }

    private async Task SincronizarAbrangencia(IEnumerable<AbrangenciaSintetica> abrangenciaSintetica, Dominio.Enumerados.Abrangencia? abrangencia, bool ehSupervisor, (IEnumerable<Dre> Dres, IEnumerable<Ue> Ues, IEnumerable<Turma> Turmas) estrutura, string login, Guid perfil)
    {
        unitOfWork.IniciarTransacao();
        try
        {
            if (ehSupervisor)
                await SincronizarAbrangenciaPorUes(abrangenciaSintetica, estrutura.Ues, login, perfil);
            else
            {
                switch (abrangencia)
                {
                    case Dominio.Enumerados.Abrangencia.Dre:
                    case Dominio.Enumerados.Abrangencia.SME:
                        await SincronizarAbrangenciaPorDres(abrangenciaSintetica, estrutura.Dres, login, perfil);
                        break;

                    case Dominio.Enumerados.Abrangencia.DreEscolasAtribuidas:
                    case Dominio.Enumerados.Abrangencia.UeTurmasDisciplinas:
                    case Dominio.Enumerados.Abrangencia.UE:
                        if (perfil.EhPerfilPOA())
                            await SincronizarAbragenciaPorTurmas(abrangenciaSintetica, estrutura.Turmas, login, perfil);
                        else
                            await SincronizarAbrangenciaPorUes(abrangenciaSintetica, estrutura.Ues, login, perfil);
                        break;

                    case Dominio.Enumerados.Abrangencia.Professor:
                        await SincronizarAbragenciaPorTurmas(abrangenciaSintetica, estrutura.Turmas, login, perfil);
                        break;
                }
            }
            unitOfWork.PersistirTransacao();
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    private Task SincronizarAbrangenciaPorUes(
                                                IEnumerable<AbrangenciaSintetica> abrangenciaSintetica,
                                                IEnumerable<Ue> ues,
                                                string login,
                                                Guid perfil)
    {
        return SincronizarAbrangenciaGenerico(
            abrangenciaSintetica,
            ues,
            a => a.UeId,
            u => u.Id,
            u => new Dominio.Entidades.Abrangencia
            {
                Perfil = perfil,
                UeId = u.Id
            },
            login);
    }

    private Task SincronizarAbrangenciaPorDres(
                                    IEnumerable<AbrangenciaSintetica> abrangenciaSintetica,
                                    IEnumerable<Dre> dres,
                                    string login,
                                    Guid perfil)
    {
        return SincronizarAbrangenciaGenerico(
            abrangenciaSintetica,
            dres,
            a => a.DreId,
            d => d.Id,
            d => new Dominio.Entidades.Abrangencia
            {
                Perfil = perfil,
                DreId = d.Id
            },
            login);
    }

    private async Task SincronizarAbrangenciaGenerico<T>(
                                                            IEnumerable<AbrangenciaSintetica> abrangenciasAtuais,
                                                            IEnumerable<T> entidades,
                                                            Func<AbrangenciaSintetica, long?> obterIdAbrangencia,
                                                            Func<T, long> obterIdEntidade,
                                                            Func<T, Dominio.Entidades.Abrangencia> criarAbrangencia,
                                                            string login)
    {
        var novas = entidades
            .Where(e => !abrangenciasAtuais
                .Select(a => obterIdAbrangencia(a))
                .Contains(obterIdEntidade(e)));

        await repositorioAbrangencia.InserirAbrangencias(
            novas.Select(criarAbrangencia), login);

        var paraAtualizar = abrangenciasAtuais
            .Where(a => !entidades
                .Select(obterIdEntidade)
                .Contains(obterIdAbrangencia(a) ?? 0));

        var perfisHistorico = paraAtualizar
            .Where(a => a.EhPerfilProfessor())
            .Select(a => a.Id);

        await repositorioAbrangencia.AtualizaAbrangenciaHistorica(perfisHistorico);

        var perfisGestao = paraAtualizar
            .Where(a => !a.EhPerfilProfessor())
            .Select(a => a.Id);

        await repositorioAbrangencia.ExcluirAbrangencias(perfisGestao);
    }

    private async Task SincronizarAbragenciaPorTurmas(IEnumerable<AbrangenciaSintetica> abrangenciaSintetica, IEnumerable<Turma> turmas, string login, Guid perfil)
    {
        bool ehPerfilProfessorInfantil = perfil == Perfis.PERFIL_PROFESSOR_INFANTIL;
        abrangenciaSintetica = RemoverAbrangenciaSinteticaDuplicada(abrangenciaSintetica);
        var abr = abrangenciaSintetica.GroupBy(x => x.CodigoTurma).Select(y => y.OrderBy(a => a.CodigoTurma));
        var idsParaAtualizar = new List<long>();

        if (ehPerfilProfessorInfantil)
            turmas = VerificaSeExisteTurmaNaoInfantilEmPerfilProfessorInfantil(turmas);

        if (!turmas.Any() && abrangenciaSintetica.Any())
        {
            idsParaAtualizar = abrangenciaSintetica.Select(x => x.Id).ToList();

            if (ehPerfilProfessorInfantil && abrangenciaSintetica.Any(a => a.Perfil == perfil))
            {
                var idsTurmas = abrangenciaSintetica.Where(a => a.Perfil == perfil).Select(a => a.TurmaId).ToList();

                var dadosTurmas = idsTurmas.Any() ? await mediator.Send(new ObterTurmasPorIdsQuery(idsTurmas.ToArray())) : null;

                var idsParaExcluir = dadosTurmas?.Where(d => d.ModalidadeTipoCalendario != ModalidadeTipoCalendario.Infantil)?.Select(d => d.Id)?.ToList();

                if (idsParaExcluir != null && idsParaExcluir.Any())
                {
                    await repositorioAbrangencia.ExcluirAbrangencias(idsParaExcluir);

                    idsParaAtualizar = new List<long>();
                }

            }
        }

        var novas = turmas.Where(x => !abrangenciaSintetica.Select(y => y.TurmaId).Contains(x.Id));

        var paraAtualizar = abrangenciaSintetica.GroupBy(x => x.CodigoTurma).SelectMany(y => y.OrderBy(a => a.CodigoTurma).Take(1));

        var listaAbrangenciaSintetica = new List<AbrangenciaSintetica>();
        var listaParaAtualizar = new List<AbrangenciaSintetica>();

        listaAbrangenciaSintetica.AddRange(abrangenciaSintetica.ToList());
        listaParaAtualizar.AddRange(paraAtualizar.ToList());
        var registrosDuplicados = listaAbrangenciaSintetica.Except(listaParaAtualizar);

        if (registrosDuplicados.Any())
            idsParaAtualizar = registrosDuplicados.Select(x => x.Id).ToList();

        if (abrangenciaSintetica.Any() &&
            turmas.Any() &&
            abrangenciaSintetica.Count() != turmas.Count())
            idsParaAtualizar.AddRange(VerificaTurmasAbrangenciaAtualParaHistorica(abrangenciaSintetica, turmas));

        await repositorioAbrangencia.InserirAbrangencias(novas.Select(x => new Dominio.Entidades.Abrangencia() { Perfil = perfil, TurmaId = x.Id }), login);

        await repositorioAbrangencia.AtualizaAbrangenciaHistorica(idsParaAtualizar);
    }

    public IEnumerable<long> VerificaTurmasAbrangenciaAtualParaHistorica(IEnumerable<AbrangenciaSintetica> abrangenciaAtual, IEnumerable<Turma> turmasAbrangenciaEol)
    {
        var turmasNaAbrangenciaAtualExistentesEol = from ta in turmasAbrangenciaEol
                                                    join aa in abrangenciaAtual
                                                    on ta.Id equals aa.TurmaId into turmasIguais
                                                    from tI in turmasIguais.DefaultIfEmpty()
                                                    select tI;

        return abrangenciaAtual.Except(turmasNaAbrangenciaAtualExistentesEol).Select(t => t.Id);
    }

    private IEnumerable<AbrangenciaSintetica> RemoverAbrangenciaSinteticaDuplicada(IEnumerable<AbrangenciaSintetica> abrangenciaSintetica)
    {
        var retorno = new List<AbrangenciaSintetica>();
        var abrangencia = abrangenciaSintetica.GroupBy(x => x.CodigoTurma).Select(y => y.OrderBy(a => a.CodigoTurma));
        foreach (var item in abrangencia)
        {
            retorno.Add(item.FirstOrDefault());
        }

        return retorno;
    }

    private IEnumerable<Turma> VerificaSeExisteTurmaNaoInfantilEmPerfilProfessorInfantil(IEnumerable<Turma> turmasAbrangenciaEol)
          => (turmasAbrangenciaEol != null && turmasAbrangenciaEol.Any())
           ? turmasAbrangenciaEol.Where(t => t.ModalidadeCodigo == Modalidade.EducacaoInfantil)?.ToList()
           : turmasAbrangenciaEol;

    private async Task SincronizarEstruturaInstitucional(EstruturaInstitucionalRetornoEolDTO estrutura)
    {
        var dres = estrutura.Dres.Select(x => new Dre() { Abreviacao = x.Abreviacao, CodigoDre = x.Codigo, Nome = x.Nome });
        var ues = estrutura.Dres.SelectMany(x => x.Ues.Select(y => new Ue { CodigoUe = y.Codigo, TipoEscola = y.CodTipoEscola, Nome = y.Nome, Dre = new Dre() { CodigoDre = x.Codigo } }));
        var turmas = estrutura.Dres.SelectMany(x => x.Ues.SelectMany(y => y.Turmas.Select(z =>
            new Turma
            {
                Ano = z.Ano,
                AnoLetivo = z.AnoLetivo,
                CodigoTurma = z.Codigo,
                //Para turma do tipo 7 (Itinerarios 2A Ano) a modalidade é definida como Médio
                ModalidadeCodigo = z.TipoTurma == Dominio.Enumerados.TipoTurma.Itinerarios2AAno ? Modalidade.Medio : (Modalidade)Convert.ToInt32(z.CodigoModalidade),
                QuantidadeDuracaoAula = z.DuracaoTurno,
                Nome = z.NomeTurma,
                Semestre = z.Semestre,
                TipoTurno = z.TipoTurno,
                Ue = new Ue() { CodigoUe = y.Codigo },
                EnsinoEspecial = z.EnsinoEspecial,
                EtapaEJA = z.EtapaEJA,
                DataInicio = z.DataInicioTurma,
                SerieEnsino = z.SerieEnsino,
                DataFim = z.DataFim,
                Extinta = z.Extinta,
                TipoTurma = z.TipoTurma
            })));

        dres = await repositorioDre.SincronizarAsync(dres);
        ues = await repositorioUe.SincronizarAsync(ues, dres);
        await repositorioTurma.SincronizarAsync(turmas, ues);
    }
}