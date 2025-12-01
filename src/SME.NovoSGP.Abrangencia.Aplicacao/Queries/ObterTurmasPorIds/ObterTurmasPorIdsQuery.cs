using MediatR;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterTurmasPorIds;

public class ObterTurmasPorIdsQuery : IRequest<IEnumerable<Turma>>
{
    public ObterTurmasPorIdsQuery(long[] turmasIds)
    {
        TurmasIds = turmasIds;
    }

    public long[] TurmasIds { get; set; }
}