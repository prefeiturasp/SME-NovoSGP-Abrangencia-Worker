using MediatR;
using Newtonsoft.Json;
using SME.NovoSGP.Abrangencia.Dominio.Constantes;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterPerfisPorLogin
{
    public class ObterPerfisPorLoginQueryHandler : IRequestHandler<ObterPerfisPorLoginQuery, IEnumerable<Guid>>
    {
        private readonly IHttpClientFactory httpClientFactory;
        public ObterPerfisPorLoginQueryHandler(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<Guid>> Handle(ObterPerfisPorLoginQuery request, CancellationToken cancellationToken)
        {
            using var httpClient = httpClientFactory.CreateClient(ServicosEolConstants.SERVICO);
            var resposta = await httpClient.GetAsync(string.Format(ServicosEolConstants.URL_AUTENTICACAO_SGP_CARREGAR_PERFIS_POR_LOGIN, request.Login), cancellationToken);
            if (!resposta.IsSuccessStatusCode)
                return [];

            var json = await resposta.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<PerfisApiEolDto>(json);
            return resultado?.Perfis?.Select(p => p) ?? [];
        }
    }
}
