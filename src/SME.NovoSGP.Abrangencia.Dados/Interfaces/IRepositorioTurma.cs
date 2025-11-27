using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioTurma
{
    Task<IEnumerable<Turma>> MaterializarCodigosTurma(string[] idTurmas, string[] codigosNaoEncontrados);
    Task<IEnumerable<Turma>> SincronizarAsync(IEnumerable<Turma> entidades, IEnumerable<Ue> ues);
}
