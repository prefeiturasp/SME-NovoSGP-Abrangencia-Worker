namespace SME.NovoSGP.Abrangencia.Dominio.Entidades;

public class Abrangencia : EntidadeBase
{
    public long? DreId { get; set; }
    public Guid Perfil { get; set; }
    public long? TurmaId { get; set; }
    public long? UeId { get; set; }
    public long UsuarioId { get; set; }
    public bool Historico { get; set; }
}
