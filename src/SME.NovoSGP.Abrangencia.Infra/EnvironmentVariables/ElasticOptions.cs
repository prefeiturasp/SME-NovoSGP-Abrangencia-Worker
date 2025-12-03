namespace SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;

public class ElasticOptions
{
    public static string Secao => "ElasticSearch";
    public string? Urls { get; set; }
    public string? CertificateFingerprint { get; set; }
    public string? IndicePadrao { get; set; }
    public string? Prefixo { get; set; }
    public string? Usuario { get; set; }
    public string? Senha { get; set; }
}
