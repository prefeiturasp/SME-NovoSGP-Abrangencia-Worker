using MediatR;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterTurmasPorIds;

public class ObterTurmasPorIdsQueryHandler : IRequestHandler<ObterTurmasPorIdsQuery, IEnumerable<Turma>>
{
    private readonly IRepositorioTurma repositorioTurma;

    public ObterTurmasPorIdsQueryHandler(IRepositorioTurma repositorioTurma)
    {
        this.repositorioTurma = repositorioTurma ?? throw new System.ArgumentNullException(nameof(repositorioTurma));
    }
    public async Task<IEnumerable<Turma>> Handle(ObterTurmasPorIdsQuery request, CancellationToken cancellationToken)
    {
        return await repositorioTurma.ObterTurmasPorIds(request.TurmasIds);
    }
}