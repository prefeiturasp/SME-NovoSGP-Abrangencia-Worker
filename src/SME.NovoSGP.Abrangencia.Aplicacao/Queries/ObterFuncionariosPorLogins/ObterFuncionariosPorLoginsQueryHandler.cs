using MediatR;
using Newtonsoft.Json;
using SME.NovoSGP.Abrangencia.Dominio.Constantes;
using SME.NovoSGP.Abrangencia.Infra.Dtos;
using SME.NovoSGP.Abrangencia.Infra.Exceptions;
using System.Text;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterFuncionariosPorLogins;

public class ObterFuncionariosPorLoginsQueryHandler : IRequestHandler<ObterFuncionariosPorLoginsQuery, IEnumerable<FuncionarioUnidadeDto>>
{
    private readonly IHttpClientFactory httpClientFactory;

    public ObterFuncionariosPorLoginsQueryHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<FuncionarioUnidadeDto>> Handle(ObterFuncionariosPorLoginsQuery request, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient(ServicosEolConstants.SERVICO);

        var resposta = await httpClient.PostAsync(ServicosEolConstants.URL_FUNCIONARIOS_BUSCAR_LISTA_LOGIN, new StringContent(JsonConvert.SerializeObject(request.Logins),
            Encoding.UTF8, "application/json-patch+json"), cancellationToken);

        if (resposta.IsSuccessStatusCode)
        {
            var json = await resposta.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<IEnumerable<FuncionarioUnidadeDto>>(json);
        }

        throw new NegocioException($"Não foi possível localizar os logins: {string.Join(",", request.Logins)}.");
    }
}
