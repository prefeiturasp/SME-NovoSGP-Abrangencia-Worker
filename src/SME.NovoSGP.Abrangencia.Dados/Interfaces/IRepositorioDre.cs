using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioDre
{
    Task<IEnumerable<Dre>> SincronizarAsync(IEnumerable<Dre> entidades);
}
