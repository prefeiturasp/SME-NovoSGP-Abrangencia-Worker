namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class AbrangenciaDreRetornoEolDto
{
    public AbrangenciaDreRetornoEolDto()
    {
        Ues = new List<AbrangenciaUeRetornoEolDto>();
    }

    public string Abreviacao { get; set; }
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public IList<AbrangenciaUeRetornoEolDto> Ues { get; set; }
}
