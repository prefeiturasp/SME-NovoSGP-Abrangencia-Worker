using MediatR;
using RabbitMQ.Client;
using SME.NovoSGP.Abrangencia.Aplicacao.Interfaces;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterAbrangenciaCompactaVigenteEolPorLoginEPerfil;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterAbrangenciaEolSupervisor;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterAbrangenciaParaSupervisor;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterDreMaterializarCodigos;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterEstruturaInstuticionalVigentePorTurma;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUeMaterializarCodigos;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Constantes;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Enumerados;
using SME.NovoSGP.Abrangencia.Infra.Dtos;
using SME.NovoSGP.Abrangencia.Infra.Fila;

namespace SME.NovoSGP.Abrangencia.Aplicacao.UseCases;

public class AbrangenciaUseCase : AbstractUseCase, IAbrangenciaUseCase
{
    private readonly IRepositorioAbrangencia repositorioAbrangencia;
    private readonly IRepositorioTurma repositorioTurma;
    private readonly IRepositorioUe repositorioUe;
    private readonly IRepositorioDre repositorioDre;
    public AbrangenciaUseCase(IMediator mediator, IChannel channel, IRepositorioAbrangencia repositorioAbrangencia, IRepositorioTurma repositorioTurma, IRepositorioUe repositorioUe, IRepositorioDre repositorioDre) : base(mediator, channel)
    {
        this.repositorioAbrangencia = repositorioAbrangencia;
        this.repositorioTurma = repositorioTurma;
        this.repositorioUe = repositorioUe;
        this.repositorioDre = repositorioDre;
    }

    public async Task<bool> Executar(MensagemRabbit param)
    {
        string login = "";
        Guid perfil = new Guid();

        AbrangenciaCompactaVigenteRetornoEOLDTO consultaEol = null;
        AbrangenciaCompactaVigenteRetornoEOLDTO abrangenciaEol = null;

        var ehSupervisor = perfil == Perfis.PERFIL_SUPERVISOR;
        var ehProfessorCJ = perfil == Perfis.PERFIL_CJ || perfil == Perfis.PERFIL_CJ_INFANTIL;

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
        else
            consultaEol = await mediator.Send(new ObterAbrangenciaCompactaVigenteEolPorLoginEPerfilQuery(login, perfil));

        if (consultaEol != null || abrangenciaEol != null)
        {
            // Enquanto o EOl consulta, tentamos ganhar tempo obtendo a consulta sintetica
            var consultaAbrangenciaSintetica = repositorioAbrangencia.ObterAbrangenciaSintetica(login, perfil, string.Empty);

            if (abrangenciaEol != null)
                abrangenciaEol = consultaEol;
            var abrangenciaSintetica = await consultaAbrangenciaSintetica;

            if (abrangenciaEol != null)
            {
                // sincronizamos as dres, ues e turmas
                var estrutura = await MaterializarEstruturaInstitucional(abrangenciaEol);

                //// sincronizamos a abrangencia do login + perfil
                //await SincronizarAbrangencia(abrangenciaSintetica, abrangenciaEol?.Abrangencia?.Abrangencia, ehSupervisor, estrutura, login, perfil);
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
        IEnumerable<Turma> turmas = Enumerable.Empty<Turma>();
        string[] codigosNaoEncontrados;

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
            //turmas = await repositorioTurma.MaterializarCodigosTurma(abrangenciaEol.IdTurmas, codigosNaoEncontrados)
            //    .Union(await ImportarTurmasNaoEncontradas(codigosNaoEncontrados));
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

    private async Task SincronizarEstruturaInstitucional(EstruturaInstitucionalRetornoEolDTO estrutura)
    {
        //var dres = estrutura.Dres.Select(x => new Dre() { Abreviacao = x.Abreviacao, CodigoDre = x.Codigo, Nome = x.Nome });
        //var ues = estrutura.Dres.SelectMany(x => x.Ues.Select(y => new Ue { CodigoUe = y.Codigo, TipoEscola = y.CodTipoEscola, Nome = y.Nome, Dre = new Dre() { CodigoDre = x.Codigo } }));
        //var turmas = estrutura.Dres.SelectMany(x => x.Ues.SelectMany(y => y.Turmas.Select(z =>
        //    new Turma
        //    {
        //        Ano = z.Ano,
        //        AnoLetivo = z.AnoLetivo,
        //        CodigoTurma = z.Codigo,
        //        //Para turma do tipo 7 (Itinerarios 2A Ano) a modalidade é definida como Médio
        //        ModalidadeCodigo = z.TipoTurma == Dominio.Enumerados.TipoTurma.Itinerarios2AAno ? Modalidade.Medio : (Modalidade)Convert.ToInt32(z.CodigoModalidade),
        //        QuantidadeDuracaoAula = z.DuracaoTurno,
        //        Nome = z.NomeTurma,
        //        Semestre = z.Semestre,
        //        TipoTurno = z.TipoTurno,
        //        Ue = new Ue() { CodigoUe = y.Codigo },
        //        EnsinoEspecial = z.EnsinoEspecial,
        //        EtapaEJA = z.EtapaEJA,
        //        DataInicio = z.DataInicioTurma,
        //        SerieEnsino = z.SerieEnsino,
        //        DataFim = z.DataFim,
        //        Extinta = z.Extinta,
        //        TipoTurma = z.TipoTurma
        //    })));

        //dres = await repositorioDre.SincronizarAsync(dres);
        //ues = await repositorioUe.SincronizarAsync(ues, dres);
        //await repositorioTurma.SincronizarAsync(turmas, ues);
    }
}