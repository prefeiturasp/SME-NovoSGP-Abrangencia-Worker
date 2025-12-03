using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioBase<T> where T : EntidadeBase
{
    Task<long> SalvarAsync(T entidade);
}

