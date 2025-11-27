using SME.NovoSGP.Abrangencia.Infra.Enumerados;

namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class AbrangenciaCargoRetornoEolDTO
{
    public AbrangenciaEnum Abrangencia { get; set; }
    public List<int> CargosId { get; set; }
    public int CdTipoFuncaoAtividade { get; set; }
    public GruposSGP Grupo { get; set; }
    public Guid GrupoID { get; set; }
    public int? TipoFuncaoAtividade { get; set; }
}
