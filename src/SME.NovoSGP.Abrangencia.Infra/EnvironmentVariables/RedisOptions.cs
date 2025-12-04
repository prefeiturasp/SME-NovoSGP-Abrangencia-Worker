using StackExchange.Redis;

namespace SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;

public class RedisOptions
{
    public static string Secao => "Redis";
    public string? Endpoint { get; set; }
    //public Proxy Proxy { get; set; }
    public int SyncTimeout { get; set; } = 5000;
    public Proxy Proxy { get; set; }
}