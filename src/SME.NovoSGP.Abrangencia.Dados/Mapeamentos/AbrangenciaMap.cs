using Dapper.FluentMap.Dommel.Mapping;

namespace SME.NovoSGP.Abrangencia.Dados.Mapeamentos;

public class AbrangenciaMap : DommelEntityMap<Dominio.Entidades.Abrangencia>
{
    public AbrangenciaMap()
    {
        ToTable("abrangencia");
        Map(c => c.DreId).ToColumn("dre_id");
        Map(c => c.Id).ToColumn("id").IsIdentity().IsKey();
        Map(c => c.Perfil).ToColumn("perfil");
        Map(c => c.TurmaId).ToColumn("turma_id");
        Map(c => c.UeId).ToColumn("ue_id");
        Map(c => c.UsuarioId).ToColumn("usuario_id");
        Map(c => c.Historico).ToColumn("historico");
    }
}