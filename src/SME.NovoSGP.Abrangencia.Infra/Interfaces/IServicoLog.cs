using SME.NovoSGP.Abrangencia.Dominio.Enums;

namespace SME.NovoSGP.Abrangencia.Infra.Interfaces;

public interface IServicoLog
{
    void Registrar(Exception ex);
    void Registrar(string mensagem, Exception ex);
    void Registrar(LogNivel nivel, string erro, string observacoes, string stackTrace);
}