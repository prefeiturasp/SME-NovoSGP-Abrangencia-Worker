using System.Net;

namespace SME.NovoSGP.Abrangencia.Infra.Exceptions;

public class NegocioException : Exception
{
    public NegocioException(string mensagem, int statusCode = 409) : base(mensagem)
    {
        StatusCode = statusCode;
    }

    public NegocioException(string mensagem, HttpStatusCode statusCode) : base(mensagem)
    {
        StatusCode = (int)statusCode;
    }

    public int StatusCode { get; }
}
