using MediatR;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUePorId
{
    public class ObterUePorIdQueryHandler : IRequestHandler<ObterUePorIdQuery, Ue>
    {
        private readonly IRepositorioUeConsulta repositorioUe;
        public ObterUePorIdQueryHandler(IRepositorioUeConsulta repositorioUe)
        {
            this.repositorioUe = repositorioUe;
        }

        public Task<Ue> Handle(ObterUePorIdQuery request, CancellationToken cancellationToken)
        {
            return repositorioUe.ObterUeComDrePorId(request.Id);
        }
    }
}
