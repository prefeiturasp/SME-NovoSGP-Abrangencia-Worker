namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class AbrangenciaUsuarioPerfilDto : Dominio.Entidades.EntidadeBase
{
    public string Login { get; set; }
    public List<Guid> Perfil { get; set; }
}
