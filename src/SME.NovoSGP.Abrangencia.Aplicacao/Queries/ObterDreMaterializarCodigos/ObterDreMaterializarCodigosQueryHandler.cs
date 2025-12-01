using MediatR;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterDreMaterializarCodigos;

public class ObterDreMaterializarCodigosQueryHandler : IRequestHandler<ObterDreMaterializarCodigosQuery, (IEnumerable<Dre> Dres, string[] CodigosDresNaoEncontrados)>
{
    private readonly IRepositorioDreConsulta repositorioDre;

    public ObterDreMaterializarCodigosQueryHandler(IRepositorioDreConsulta repositorio)
    {
        this.repositorioDre = repositorio;
    }

    public async Task<(IEnumerable<Dre> Dres, string[] CodigosDresNaoEncontrados)> Handle(ObterDreMaterializarCodigosQuery request, CancellationToken cancellationToken)
    {
        return await repositorioDre.MaterializarCodigosDre(request.IdDres);
    }

}
