namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class EstruturaInstitucionalRetornoEolDTO
{
    public EstruturaInstitucionalRetornoEolDTO()
    {
        Dres = new List<AbrangenciaDreRetornoEolDto>();
    }

    public List<AbrangenciaDreRetornoEolDto> Dres { get; set; }
}