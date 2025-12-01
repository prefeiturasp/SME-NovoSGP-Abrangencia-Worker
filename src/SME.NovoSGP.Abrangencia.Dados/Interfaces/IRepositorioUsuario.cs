using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioUsuario
{
    Task<IEnumerable<AbrangenciaUsuarioPerfilDto>> ObterUsuariosPerfis();
}
