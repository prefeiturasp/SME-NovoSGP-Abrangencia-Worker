using MediatR;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUesPorIds
{
    public class ObterUesPorIdsQuery : IRequest<IEnumerable<Ue>>
    {
        public ObterUesPorIdsQuery(long[] ids)
        {
            Ids = ids;
        }

        public long[] Ids { get; set; }
    }
}
