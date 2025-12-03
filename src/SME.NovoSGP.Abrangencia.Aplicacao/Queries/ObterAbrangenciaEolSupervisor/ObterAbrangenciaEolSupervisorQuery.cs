using MediatR;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterAbrangenciaEolSupervisor;

public class ObterAbrangenciaEolSupervisorQuery : IRequest<IEnumerable<ResponsavelEscolasDto>>
{
    public ObterAbrangenciaEolSupervisorQuery(string dreId, string supervisorId)
    {
        DreId = dreId;
        SupervisorId = supervisorId;
    }

    public string DreId { get; set; }
    public string SupervisorId { get; set; }
}
