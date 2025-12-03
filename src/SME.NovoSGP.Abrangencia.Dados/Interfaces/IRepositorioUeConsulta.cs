using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioUeConsulta
{
    Task<(List<Ue> Ues, string[] CodigosUesNaoEncontradas)> MaterializarCodigosUe(string[] idUes);
}
