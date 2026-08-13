using MediatR;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUesPorIds
{
    public class ObterUesPorIdsQueryHandler : IRequestHandler<ObterUesPorIdsQuery, IEnumerable<Ue>>
    {
        private readonly IRepositorioUeConsulta repositorioUe;
        public ObterUesPorIdsQueryHandler(IRepositorioUeConsulta repositorioUe)
        {
            this.repositorioUe = repositorioUe;
        }

        public Task<IEnumerable<Ue>> Handle(ObterUesPorIdsQuery request, CancellationToken cancellationToken)
        {
            return repositorioUe.ObterUesComDrePorIds(request.Ids);
        }
    }
}
