using Dapper;
using Elastic.Apm.Api;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Infra.Dtos;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using System.Text;

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

    public async Task<Usuario> ObterPorCodigoRfLogin(string codigoRf, string login)
    {
        using var conn = ObterConexao();

        try
        {
            var query = new StringBuilder();
            query.AppendLine("select * from usuario");
            query.AppendLine("where 1=1");

            if (!string.IsNullOrEmpty(codigoRf))
                query.AppendLine("and rf_codigo = @codigoRf");

            if (!string.IsNullOrEmpty(login))
                query.AppendLine("and login = @login");
            else
                query.AppendLine("or login = @codigoRf");

            query.AppendLine("limit 1");

            var usuarios = await conn.QueryAsync<Usuario>(query.ToString(), new { codigoRf, login });

            return usuarios.FirstOrDefault()!;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }
}
