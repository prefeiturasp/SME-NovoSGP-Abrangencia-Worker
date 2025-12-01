namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class AbrangenciaCompactaVigenteRetornoEOLDTO
{
    public string Login { get; set; }
    public AbrangenciaCargoRetornoEolDTO Abrangencia { get; set; }
    public string[] IdDres { get; set; }
    public string[] IdUes { get; set; }
    public string[] IdTurmas { get; set; }
}
