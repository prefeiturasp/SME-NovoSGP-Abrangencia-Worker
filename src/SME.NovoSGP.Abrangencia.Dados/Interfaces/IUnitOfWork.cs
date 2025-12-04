using System.Data;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDbTransaction IniciarTransacao();

    void PersistirTransacao();

    void Rollback();
}
