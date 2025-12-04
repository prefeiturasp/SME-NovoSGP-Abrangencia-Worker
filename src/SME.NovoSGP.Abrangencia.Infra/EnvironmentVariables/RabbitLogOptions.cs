namespace SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;

public class RabbitLogOptions
{
    public const string Secao = "ConfiguracaoRabbitOptions";
    public string? HostName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? VirtualHost { get; set; }
}