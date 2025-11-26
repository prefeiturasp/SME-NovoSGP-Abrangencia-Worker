namespace SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;

public class ConnectionStringOptions
{
    public static string Secao => "ConnectionStrings";
    public string? AbrangenciaExterna { get; set; }
    public string? AbrangenciaPostgres { get; set; }
    public string? AbrangenciaPostgresConsulta { get; set; }
}