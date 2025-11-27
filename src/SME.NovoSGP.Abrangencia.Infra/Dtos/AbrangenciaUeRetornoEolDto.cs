using SME.NovoSGP.Abrangencia.Dominio.Enumerados;

namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class AbrangenciaUeRetornoEolDto
{
    public AbrangenciaUeRetornoEolDto()
    {
        Turmas = new List<AbrangenciaTurmaRetornoEolDto>();
    }

    public string Codigo { get; set; }
    public TipoEscola CodTipoEscola { get; set; }
    public string Nome { get; set; }
    public IList<AbrangenciaTurmaRetornoEolDto> Turmas { get; set; }
}
