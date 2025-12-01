using MediatR;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterFuncionariosPorLogins;
using SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterFuncionariosPorRFs;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.Dtos;
using SME.NovoSGP.Abrangencia.Infra.Enumerados;
using SME.NovoSGP.Abrangencia.Infra.Extensions;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterAbrangenciaEolSupervisor
{
    public class ObterAbrangenciaEolSupervisorQueryHandler : IRequestHandler<ObterAbrangenciaEolSupervisorQuery, IEnumerable<ResponsavelEscolasDto>>
    {
        private readonly IRepositorioSupervisorEscolaDre repositorioSupervisorEscolaDre;
        private readonly IMediator mediator;

        public ObterAbrangenciaEolSupervisorQueryHandler(IRepositorioSupervisorEscolaDre repositorioSupervisorEscolaDre, IMediator mediator)
        {
            this.repositorioSupervisorEscolaDre = repositorioSupervisorEscolaDre;
            this.mediator = mediator;
        }

        public async Task<IEnumerable<ResponsavelEscolasDto>> Handle(ObterAbrangenciaEolSupervisorQuery request, CancellationToken cancellationToken)
        {
            var responsaveisEscolasDres = await repositorioSupervisorEscolaDre.ObtemPorDreESupervisor(request.DreId, request.SupervisorId);

            IEnumerable<ResponsavelEscolasDto> lista = new List<ResponsavelEscolasDto>();

            if (responsaveisEscolasDres.Any())
                lista = await MapearResponsavelEscolaDre(responsaveisEscolasDres);

            return lista;
        }

        private async Task<IEnumerable<ResponsavelEscolasDto>> MapearResponsavelEscolaDre(IEnumerable<SupervisorEscolaDre> supervisoresEscolasDres)
        {
            ResponsavelRetornoDto listaResponsaveis = null;
            var listaRetorno = new List<ResponsavelEscolasDto>();

            var supervisor = supervisoresEscolasDres.ToList();
            var totalRegistros = supervisor.Count;
            for (int i = 0; i < totalRegistros; i++)
            {
                if (supervisor[i].SupervisorId != null)
                {
                    switch (supervisor[i].Tipo)
                    {
                        case (int)TipoResponsavelAtribuicao.PsicologoEscolar:
                        case (int)TipoResponsavelAtribuicao.Psicopedagogo:
                        case (int)TipoResponsavelAtribuicao.AssistenteSocial:
                            {
                                var nomesFuncionariosAtribuidos = await mediator.Send(new ObterFuncionariosPorLoginsQuery(new List<string> { supervisor[i].SupervisorId }));
                                if (nomesFuncionariosAtribuidos.Any())
                                    listaResponsaveis = new ResponsavelRetornoDto() { CodigoRfOuLogin = nomesFuncionariosAtribuidos.FirstOrDefault().Login, NomeServidor = nomesFuncionariosAtribuidos.FirstOrDefault().NomeServidor };
                                break;
                            }
                        default:
                            {
                                var nomesServidoresAtribuidos = await mediator.Send(new ObterFuncionariosPorRFsQuery(new List<string> { supervisor[i].SupervisorId }));
                                if (nomesServidoresAtribuidos.Any())
                                    listaResponsaveis = new ResponsavelRetornoDto() { CodigoRfOuLogin = nomesServidoresAtribuidos.FirstOrDefault().CodigoRF, NomeServidor = nomesServidoresAtribuidos.FirstOrDefault().Nome };
                                break;
                            }
                    }
                }

                string nomeResponsavel = listaResponsaveis != null ? listaResponsaveis?.NomeServidor + " - " + listaResponsaveis?.CodigoRfOuLogin
                                         : string.Empty;

                var itemRetorno = new ResponsavelEscolasDto()
                {
                    Id = supervisor[i].Id,
                    Responsavel = supervisor[i].Excluido ? null : nomeResponsavel,
                    ResponsavelId = supervisor[i].Excluido ? null : supervisor[i].SupervisorId,
                    TipoResponsavel = ObterTipoResponsavelDescricao(supervisor[i].Tipo),
                    TipoResponsavelId = supervisor[i].Tipo,
                    UeId = supervisor[i].EscolaId,
                    DreId = supervisor[i].DreId
                };

                listaRetorno.Add(itemRetorno);
            }
            return listaRetorno;
        }

        private static string? ObterTipoResponsavelDescricao(int tipo)
        {
            var tipoDescricao = Enum.GetValues(typeof(TipoResponsavelAtribuicao))
                .Cast<TipoResponsavelAtribuicao>()
                .Where(w => (int)w == tipo)
                .Select(d => new { descricao = d.Name() })
                .FirstOrDefault()?.descricao;

            return tipoDescricao;
        }
    }
}
