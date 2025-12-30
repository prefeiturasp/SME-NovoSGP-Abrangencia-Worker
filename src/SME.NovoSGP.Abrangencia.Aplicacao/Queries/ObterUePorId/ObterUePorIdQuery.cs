using MediatR;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUePorId
{
    public class ObterUePorIdQuery : IRequest<Ue>
    {
        public ObterUePorIdQuery(long id)
        {
            Id = id;
        }

        public long Id { get; set; }
    }
}
