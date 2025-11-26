using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;

namespace SME.NovoSGP.Abrangencia.Dados.Mapeamentos;

public static class RegistrarMapeamentos
{
    public static void Registrar()
    {
        FluentMapper.Initialize(config =>
        {

            config.ForDommel();
        });
    }
}
