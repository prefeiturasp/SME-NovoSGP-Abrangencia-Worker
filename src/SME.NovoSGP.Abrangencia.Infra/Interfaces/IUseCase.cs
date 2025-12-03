namespace SME.NovoSGP.Abrangencia.Infra.Interfaces;

public interface IUseCase<in TParameter, TResponse>
{
    Task<TResponse> Executar(TParameter param);
}
