using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioBase<T> where T : EntidadeBase
{
    Task<IEnumerable<T>> ListarAsync();
    IEnumerable<T> Listar();

    T ObterPorId(long id);

    Task<T> ObterPorIdAsync(long id);

    void Remover(long id);

    void Remover(T entidade);

    Task RemoverAsync(T entidade);

    long Salvar(T entidade);

    Task<long> SalvarAsync(T entidade);

    Task<bool> Exists(long id, string coluna = null);

    Task<long> RemoverLogico(long id, string coluna = null);
    Task<bool> RemoverLogico(long[] id, string coluna = null);
}

