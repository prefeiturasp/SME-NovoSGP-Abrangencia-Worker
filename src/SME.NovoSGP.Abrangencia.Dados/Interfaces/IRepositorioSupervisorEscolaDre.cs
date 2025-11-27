using SME.NovoSGP.Abrangencia.Dominio.Entidades;

namespace SME.NovoSGP.Abrangencia.Dados.Interfaces
{
    public interface IRepositorioSupervisorEscolaDre : IRepositorioBase<SupervisorEscolaDre>
    {
        Task<IEnumerable<SupervisorEscolaDre>> ObtemPorDreESupervisor(string dreId, string supervisorId, bool excluidos = false);
    }
}
