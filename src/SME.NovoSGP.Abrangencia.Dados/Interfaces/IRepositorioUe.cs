using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioUe
{
    Task<IEnumerable<Ue>> SincronizarAsync(IEnumerable<Ue> entidades, IEnumerable<Dre> dres);
    Task<IEnumerable<Ue>> ObterUePorCodigoUe(string codigoUe);
    Task<Ue> ObterUePorUeId(int ueId);
    Task<IEnumerable<UeDto>> ListarUesDuplicadas();
}
