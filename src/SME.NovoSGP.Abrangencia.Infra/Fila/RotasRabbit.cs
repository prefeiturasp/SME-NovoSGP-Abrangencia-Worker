namespace SME.NovoSGP.Abrangencia.Infra.Fila; 

public class RotasRabbit
{
    public static string RotaLogs => "ApplicationLog";
    public static string Log => "ApplicationLog";

    public const string IniciarSync = "sgp.worker.abrangencia.iniciar.sync-teste";
    public const string SincronizarAbrangencia = "sgp.worker.abrangencia.tratar";
    //public const string WorkerAbrangencia = "sgp.worker.abrangencia.usuario.perfil-teste";
}