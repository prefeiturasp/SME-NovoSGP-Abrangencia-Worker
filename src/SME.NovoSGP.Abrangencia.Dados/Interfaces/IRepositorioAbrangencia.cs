using SME.NovoSGP.Abrangencia.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioAbrangencia
{
    Task<IEnumerable<AbrangenciaSintetica>> ObterAbrangenciaSintetica(string login, Guid perfil, string turmaId = "", bool consideraHistorico = false);
}
