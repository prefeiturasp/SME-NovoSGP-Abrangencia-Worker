using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces;

public interface IRepositorioTurma
{
    Task<IEnumerable<Turma>> MaterializarCodigosTurma(string[] idTurmas, string[] codigosNaoEncontrados);
    Task<IEnumerable<Turma>> SincronizarAsync(IEnumerable<Turma> entidades, IEnumerable<Ue> ues);
    Task<IEnumerable<Turma>> ObterTurmasPorIds(long[] turmasIds);

    Task<IEnumerable<Turma>> ObterTurmasPorUeId(long ueId);
    Task<IEnumerable<Turma>> ListarTurmaPorAnoLetivo(int anoLetivo);
    Task<bool> AtualizarUeTurma(long turmaId, long ueId);
}
