using Dommel;
using Npgsql;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Extensoes;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Data;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;

public abstract class RepositorioBaseSGP<T> : RepositorioBase, IRepositorioBase<T> where T : EntidadeBase
{
    private readonly IContextoAplicacao contextoAplicacao;
    private readonly ConnectionStringOptions connectionStrings;

    protected RepositorioBaseSGP(ConnectionStringOptions connectionStrings, IContextoAplicacao contextoAplicacao) : base(contextoAplicacao)
    {
        this.connectionStrings = connectionStrings;
    }

    protected virtual IDbConnection ObterConexaoSGP()
    {
        try
        {
            var conexao = new NpgsqlConnection(connectionStrings.SGP_Postgres);
            conexao.Open();
            return conexao;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERRO CRÍTICO: Falha ao abrir a conexão com o banco de dados. Mensagem: {ex.Message}");
            throw new InvalidOperationException("Falha ao inicializar SGP_Postgres: Não foi possível abrir a conexão com o banco de dados.", ex);
        }
    }

    protected virtual IDbConnection ObterConexaoSGPConsulta()
    {
        try
        {
            var conexao = new NpgsqlConnection(connectionStrings.SGP_PostgresConsulta);
            conexao.Open();
            return conexao;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERRO CRÍTICO: Falha ao abrir a conexão com o banco de dados. Mensagem: {ex.Message}");
            throw new InvalidOperationException("Falha ao inicializar SGP_PostgresConsulta: Não foi possível abrir a conexão com o banco de dados.", ex);
        }
    }

    public async Task<long> SalvarAsync(T entidade)
    {
        var conexao = ObterConexaoSGP();
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
                entidade.Id = (long)await conexao.InsertAsync(entidade);
            }
        }
        finally
        {
            conexao.Close();
            conexao.Dispose();
        }

        return entidade.Id;
    }
}
