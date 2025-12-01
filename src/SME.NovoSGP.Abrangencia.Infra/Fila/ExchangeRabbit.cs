namespace SME.NovoSGP.Abrangencia.Infra.Fila;

public static class ExchangeRabbit
{
    public static string WorkerAbrangencia => "sgp.abrangencia.workers";
    public static string WorkerAbrangenciaDeadLetter => "sgp.abrangencia.workers.deadletter";
    public static string Logs => "EnterpriseApplicationLog";
    public static int WorkerAbrangenciaDeadLetterTtl => 10 * 60 * 1000; /*10 Min * 60 Seg * 1000 milisegundos = 10 minutos em milisegundos*/
    public static int WorkerAbrangenciaDeadLetterDeadLetterTtl_3 => 3 * 60 * 1000; /*10 Min * 60 Seg * 1000 milisegundos = 10 minutos em milisegundos*/
}