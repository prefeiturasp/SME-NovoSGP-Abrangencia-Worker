using Dapper;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Infra.Dtos;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.SGP;

public class RepositorioUsuario : RepositorioBase<AbrangenciaUsuarioPerfilDto>, IRepositorioUsuario
{
    public RepositorioUsuario(ConnectionStringOptions connectionStrings) : base(connectionStrings.SGP_Postgres)
    {
    }

    public async Task<IEnumerable<AbrangenciaUsuarioPerfilDto>> ObterUsuariosPerfis()
    {
        using var conn = ObterConexao();
        try
        {
            var query = @"select  u.rf_codigo Login,
                                       a.perfil Perfil 
                                from usuario u
                                inner join (select distinct(a.usuario_id ),
	                                   a.perfil 
                                from abrangencia a ) a  on u.id = a.usuario_id ";
            var resultado = await conn.QueryAsync<AbrangenciaUsuarioPerfilDto>(query);
            return resultado;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }
}
