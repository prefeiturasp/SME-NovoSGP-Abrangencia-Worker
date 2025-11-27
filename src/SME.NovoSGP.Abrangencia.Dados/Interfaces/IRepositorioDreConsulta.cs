using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioDreConsulta
{
    Task<(IEnumerable<Dre> Dres, string[] CodigosDresNaoEncontrados)> MaterializarCodigosDre(string[] idDres);
}
