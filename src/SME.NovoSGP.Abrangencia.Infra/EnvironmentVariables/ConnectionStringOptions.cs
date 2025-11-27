namespace SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;

public class ConnectionStringOptions
{
    public static string Secao => "ConnectionStrings";
    public string? AbrangenciaExterna { get; set; }
    public string? Abrangencia_Postgres { get; set; }
    public string? Abrangencia_PostgresConsulta { get; set; }
    public string? SGP_Postgres { get; set; }
    public string? SGP_PostgresConsulta { get; set; }
}