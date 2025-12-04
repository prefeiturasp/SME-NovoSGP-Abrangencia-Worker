namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class AbrangenciaRetornoEolDto
{
    public AbrangenciaRetornoEolDto()
    {
        Dres = new List<AbrangenciaDreRetornoEolDto>();
    }

    public AbrangenciaCargoRetornoEolDTO Abrangencia { get; set; }
    public IList<AbrangenciaDreRetornoEolDto> Dres { get; set; }
}
