using MediatR;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterUeMaterializarCodigos;

public class ObterUeMaterializarCodigosQueryHandler : IRequestHandler<ObterUeMaterializarCodigosQuery, (List<Ue> Ues, string[] CodigosUesNaoEncontradas)>
{
    private readonly IRepositorioUeConsulta repositorioUe;

    public ObterUeMaterializarCodigosQueryHandler(IRepositorioUeConsulta repositorio)
    {
        this.repositorioUe = repositorio;
    }

    public async Task<(List<Ue> Ues, string[] CodigosUesNaoEncontradas)> Handle(ObterUeMaterializarCodigosQuery request, CancellationToken cancellationToken)
    {
        return await repositorioUe.MaterializarCodigosUe(request.IdUes);
    }

}