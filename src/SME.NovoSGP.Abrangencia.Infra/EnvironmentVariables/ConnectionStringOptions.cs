namespace SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;

public class ConnectionStringOptions
{
    public static string Secao => "ConnectionStrings";
    public string? SGP_Postgres { get; set; }
    public string? SGP_PostgresConsulta { get; set; }
}