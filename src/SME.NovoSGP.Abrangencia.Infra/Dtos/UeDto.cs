using SME.NovoSGP.Abrangencia.Dominio.Enumerados;

namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class UeDto
{
    public string Ue_id { get; set; }
    public long Dre_Id { get; set; }
    public long Id { get; set; }
    public string Nome { get; set; }
    public TipoEscola TipoEscola { get; set; }
}
