using SME.NovoSGP.Abrangencia.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioAbrangencia
{
    Task<IEnumerable<AbrangenciaSintetica>> ObterAbrangenciaSintetica(string login, Guid perfil, string turmaId = "", bool consideraHistorico = false);
    Task InserirAbrangencias(IEnumerable<Dominio.Entidades.Abrangencia> abrangencias, string login);
    Task AtualizaAbrangenciaHistorica(IEnumerable<long> paraAtualizar);
    Task ExcluirAbrangencias(IEnumerable<long> ids);
}
