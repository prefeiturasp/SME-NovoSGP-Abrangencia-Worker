using Dommel;
using Npgsql;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using System.Data;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;

public abstract class RepositorioBase<T> where T : EntidadeBase
{
    private readonly string connectionStrings;
    protected RepositorioBase(string connectionStrings)
    {
        this.connectionStrings = connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings));
    }

    protected virtual IDbConnection ObterConexao()
    {
        try
        {
            var conexao = new NpgsqlConnection(connectionStrings);
            conexao.Open();
            return conexao;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERRO CRÍTICO: Falha ao abrir a conexão com o banco de dados. Mensagem: {ex.Message}");
            throw new InvalidOperationException("Falha ao inicializar ObterConexao: Não foi possível abrir a conexão com o banco de dados.", ex);
        }
    }

    protected virtual IDbConnection ObterConexaoLeitura()
    {
        try
        {
            var conexao = new NpgsqlConnection(connectionStrings);
            conexao.Open();
            return conexao;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERRO CRÍTICO: Falha ao abrir a conexão com o banco de dados. Mensagem: {ex.Message}");
            throw new InvalidOperationException("Falha ao inicializar ObterConexaoLeitura: Não foi possível abrir a conexão com o banco de dados.", ex);
        }
    }

    protected virtual async Task<long> SalvarAsync(T entidade)
    {
        var conexao = ObterConexao();
        try
        {
            if (entidade.Id > 0)
            {
                conexao.Update(entidade);
            }
            else
            {
                entidade.Id = (long)await conexao.InsertAsync(entidade);
            }
            return entidade.Id;
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }
    }
}
