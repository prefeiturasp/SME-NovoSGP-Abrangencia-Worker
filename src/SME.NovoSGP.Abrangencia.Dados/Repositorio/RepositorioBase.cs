using Dapper;
using Dommel;
using Npgsql;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Extensoes;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Data;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio;

public abstract class RepositorioBase<T> : IRepositorioBase<T> where T : EntidadeBase
{
    private readonly IContextoAplicacao contextoAplicacao;
    private readonly ConnectionStringOptions connectionStrings;

    protected RepositorioBase(ConnectionStringOptions connectionStrings, IContextoAplicacao contextoAplicacao)
    {
        this.connectionStrings = connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings));
        this.contextoAplicacao = contextoAplicacao ?? throw new ArgumentNullException(nameof(contextoAplicacao));
    }

    protected virtual IDbConnection ObterConexaoAbrangencia()
    {
        try
        {
            var conexao = new NpgsqlConnection(connectionStrings.AbrangenciaPostgres);
            conexao.Open();
            return conexao;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERRO CRÍTICO: Falha ao abrir a conexão com o banco de dados. Mensagem: {ex.Message}");
            throw new InvalidOperationException("Falha ao inicializar SondagemContext: Não foi possível abrir a conexão com o banco de dados.", ex);
        }
    }

    protected virtual IDbConnection ObterConexaoAbrangenciaConsulta()
    {
        try
        {
            var conexao = new NpgsqlConnection(connectionStrings.AbrangenciaPostgresConsulta);
            conexao.Open();
            return conexao;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERRO CRÍTICO: Falha ao abrir a conexão com o banco de dados. Mensagem: {ex.Message}");
            throw new InvalidOperationException("Falha ao inicializar SondagemContext: Não foi possível abrir a conexão com o banco de dados.", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> ListarAsync()
    {
        var conexao = ObterConexaoAbrangenciaConsulta();
        try
        {
            return await conexao.GetAllAsync<T>();
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }
    }

    public virtual async Task<T> ObterPorIdAsync(long id)
    {
        var conexao = ObterConexaoAbrangencia();
        try
        {
            return await conexao.GetAsync<T>(id: id);
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }
    }

    public virtual async Task RemoverAsync(long id)
    {
        var conexao = ObterConexaoAbrangencia();
        try
        {
            var entidade = await conexao.GetAsync<T>(id);
            await conexao.DeleteAsync(entidade);
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }
    }

    public virtual async Task RemoverAsync(T entidade)
    {
        var conexao = ObterConexaoAbrangencia();
        try
        {
            await conexao.DeleteAsync(entidade);
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }
    }

    public virtual async Task<long> SalvarAsync(T entidade)
    {
        var conexao = ObterConexaoAbrangencia();
        try
        {
            if (entidade.Id > 0)
            {
                entidade.AlteradoEm = DateTimeExtension.HorarioBrasilia();
                entidade.AlteradoPor = UsuarioLogadoNomeCompleto;
                entidade.AlteradoRF = UsuarioLogadoRF;
                await conexao.UpdateAsync(entidade);
            }
            else
            {
                entidade.CriadoPor = UsuarioLogadoNomeCompleto;
                entidade.CriadoRF = UsuarioLogadoRF;
                entidade.Id = (long)(await conexao.InsertAsync(entidade));
            }
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }

        return entidade.Id;
    }

    public virtual async Task<long> RemoverLogico(long id, string coluna = null)
    {
        var conexao = ObterConexaoAbrangencia();
        try
        {
            var tableName = Dommel.Resolvers.Table(typeof(T), conexao);
            var columName = coluna ?? "id";

            var query = $@"update {tableName} 
                            set excluido = true
                              , alterado_por = @alteradoPor
                              , alterado_rf = @alteradoRF 
                              , alterado_em = @alteradoEm
                        where {columName}=@id RETURNING id";

            return await conexao.ExecuteScalarAsync<long>(query
                , new
                {
                    id,
                    alteradoPor = UsuarioLogadoNomeCompleto,
                    alteradoRF = UsuarioLogadoRF,
                    alteradoEm = DateTimeExtension.HorarioBrasilia()
                });
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }
    }

    public virtual async Task<bool> RemoverLogico(long[] ids, string coluna = null)
    {
        var conexao = ObterConexaoAbrangencia();
        try
        {
            var tableName = Resolvers.Table(typeof(T), conexao);
            var columName = coluna ?? "id";

            var query = $@"update {tableName} 
                            set excluido = true
                              , alterado_por = @alteradoPor
                              , alterado_rf = @alteradoRF 
                              , alterado_em = @alteradoEm
                        where {columName}= ANY(@id)";

            return await conexao.ExecuteScalarAsync<bool>(query
                , new
                {
                    id = ids,
                    alteradoPor = UsuarioLogadoNomeCompleto,
                    alteradoRF = UsuarioLogadoRF,
                    alteradoEm = DateTimeExtension.HorarioBrasilia()
                });
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }


    }

    protected static CommandDefinition CriarComando(string sql, object parameters, CancellationToken cancellationToken)
    {
        return new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            cancellationToken: cancellationToken
        );
    }

    public IEnumerable<T> Listar()
    {
        throw new NotImplementedException();
    }

    public T ObterPorId(long id)
    {
        throw new NotImplementedException();
    }

    public void Remover(long id)
    {
        throw new NotImplementedException();
    }

    public void Remover(T entidade)
    {
        throw new NotImplementedException();
    }

    public long Salvar(T entidade)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Exists(long id, string coluna = null)
    {
        throw new NotImplementedException();
    }

    public string UsuarioLogado => contextoAplicacao.UsuarioLogado;

    public string UsuarioLogadoNomeCompleto => contextoAplicacao.NomeUsuario;

    public string PerfilUsuario => contextoAplicacao.PerfilUsuario;

    public string UsuarioLogadoRF => contextoAplicacao.ObterVariavel<string>("RF") ?? "0";

    public string Administrador => contextoAplicacao.Administrador;
}
