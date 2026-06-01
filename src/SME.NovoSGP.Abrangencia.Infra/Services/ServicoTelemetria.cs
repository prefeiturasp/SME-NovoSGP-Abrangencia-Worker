using Elastic.Apm;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Diagnostics;

namespace SME.NovoSGP.Abrangencia.Infra.Services;

public class ServicoTelemetria : IServicoTelemetria
{

    private readonly TelemetriaOptions telemetriaOptions;

    public ServicoTelemetria(TelemetriaOptions telemetriaOptions)
    {
        this.telemetriaOptions = telemetriaOptions ?? throw new ArgumentNullException(nameof(telemetriaOptions));
    }

    public ServicoTelemetriaTransacao IniciarTransacao(string rota)
    {
        var transacao = new ServicoTelemetriaTransacao(rota);

        if (telemetriaOptions.Apm)
            transacao.TransacaoApm = Agent.Tracer.StartTransaction(rota, "WorkerRabbitNovoSGPAbrangencia");

        transacao.InicioOperacao = DateTime.UtcNow;
        transacao.Temporizador = Stopwatch.StartNew();
        return transacao;
    }

    public void FinalizarTransacao(ServicoTelemetriaTransacao servicoTelemetriaTransacao)
    {
        if (telemetriaOptions.Apm)
            servicoTelemetriaTransacao.TransacaoApm?.End();
    }

    public void RegistrarExcecao(ServicoTelemetriaTransacao servicoTelemetriaTransacao, Exception ex)
    {
        if (telemetriaOptions.Apm)
            servicoTelemetriaTransacao.TransacaoApm?.CaptureException(ex);
    }

    public async Task<dynamic> RegistrarComRetornoAsync<T>(Func<Task<object>> acao, string acaoNome,
        string telemetriaNome, string telemetriaValor, string parametros)
    {
        dynamic result = default;
        if (telemetriaOptions.Apm)
        {
            var transactionElk = Agent.Tracer.CurrentTransaction;

            await transactionElk.CaptureSpan(telemetriaNome, acaoNome, async (span) =>
            {
                span.SetLabel(telemetriaNome, telemetriaValor);
                span.SetLabel("Parametros", parametros);
                result = (await acao()) as dynamic;
            });
        }
        else
        {
            result = await acao() as dynamic;
        }
        return result;
    }

    public async Task<dynamic> RegistrarComRetornoAsync<T>(Func<Task<object>> acao, string acaoNome, string telemetriaNome, string telemetriaValor)
    {
        return await RegistrarComRetornoAsync<T>(acao, acaoNome, telemetriaNome, telemetriaValor, null);
    }

    public dynamic RegistrarComRetorno<T>(Func<object> acao, string acaoNome, string telemetriaNome, string telemetriaValor)
    {
        dynamic result = default;
        if (telemetriaOptions.Apm)
        {
            var transactionElk = Agent.Tracer.CurrentTransaction;

            transactionElk.CaptureSpan(telemetriaNome, acaoNome, (span) =>
            {
                span.SetLabel(telemetriaNome, telemetriaValor);
                result = acao();
            });
        }
        else
        {
            result = acao();
        }
        return result;
    }

    public void Registrar(Action acao, string acaoNome, string telemetriaNome, string telemetriaValor)
    {
        if (telemetriaOptions.Apm)
        {
            var transactionElk = Agent.Tracer.CurrentTransaction;

            transactionElk.CaptureSpan(telemetriaNome, acaoNome, (span) =>
            {
                span.SetLabel(telemetriaNome, telemetriaValor);
                acao();
            });
        }
        else
        {
            acao();
        }
    }

    public async Task RegistrarAsync(Func<Task> acao, string acaoNome, string telemetriaNome, string telemetriaValor)
    {
        if (telemetriaOptions.Apm)
        {
            var transactionElk = Agent.Tracer.CurrentTransaction;

            if (transactionElk != null)
            {
                await transactionElk.CaptureSpan(telemetriaNome, acaoNome, async (span) =>
                {
                    span.SetLabel(telemetriaNome, telemetriaValor);
                    await acao();
                });
            }
            else
                await acao();
        }
        else
            await acao();
    }

    public class ServicoTelemetriaTransacao
    {
        public ServicoTelemetriaTransacao(string nome)
        {
            Nome = nome;
            Sucesso = true;
        }

        public string Nome { get; set; }
        public Elastic.Apm.Api.ITransaction TransacaoApm { get; set; }
        public DateTime InicioOperacao { get; set; }
        public Stopwatch Temporizador { get; set; }
        public bool Sucesso { get; set; }
    }
}