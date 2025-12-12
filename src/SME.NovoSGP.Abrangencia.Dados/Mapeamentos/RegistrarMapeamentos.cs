using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;

namespace SME.NovoSGP.Abrangencia.Dados.Mapeamentos;

public static class RegistrarMapeamentos
{
    public static void Registrar()
    {
        FluentMapper.Initialize(config =>
        {
            config.AddMap(new DreMap());
            config.AddMap(new UeMap());
            config.AddMap(new TurmaMap());
            config.AddMap(new AbrangenciaMap());
            config.AddMap(new CadastroAcessoABAEMap());
            config.AddMap(new UsuarioMap());
            config.ForDommel();
        });
    }
}
