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
            transacao.TransacaoApm = Agent.Tracer.StartTransaction(rota, "WorkerRabbitNovoSGP.Abrangencia");

        if (!telemetriaOptions.ApplicationInsights)
            return transacao;

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
        dynamic result;

        DateTime inicioOperacao = default;
        Stopwatch temporizador = default;

        if (telemetriaOptions.ApplicationInsights)
        {
            inicioOperacao = DateTime.UtcNow;
            temporizador = Stopwatch.StartNew();
        }

        if (telemetriaOptions.Apm)
        {
            var temporizadorApm = Stopwatch.StartNew();
            result = await acao();
            temporizadorApm.Stop();

            Agent.Tracer.CurrentTransaction.CaptureSpan(telemetriaNome, acaoNome, span =>
            {
                span.SetLabel(telemetriaNome, telemetriaValor);

                if (!string.IsNullOrEmpty(parametros))
                    span.SetLabel("Parametros", parametros);

                span.Duration = temporizadorApm.Elapsed.TotalMilliseconds;
            });
        }
        else
            result = await acao();

        if (!telemetriaOptions.ApplicationInsights || temporizador == null)
            return result;

        temporizador.Stop();

        return result;
    }

    public async Task<dynamic> RegistrarComRetornoAsync<T>(Func<Task<object>> acao, string acaoNome, string telemetriaNome, string telemetriaValor)
    {
        return await RegistrarComRetornoAsync<T>(acao, acaoNome, telemetriaNome, telemetriaValor, null);
    }

    public dynamic RegistrarComRetorno<T>(Func<object> acao, string acaoNome, string telemetriaNome, string telemetriaValor)
    {
        dynamic result;

        DateTime inicioOperacao = default;
        Stopwatch temporizador = default;

        if (telemetriaOptions.ApplicationInsights)
        {
            inicioOperacao = DateTime.UtcNow;
            temporizador = Stopwatch.StartNew();
        }

        if (telemetriaOptions.Apm)
        {
            var temporizadorApm = Stopwatch.StartNew();
            result = acao();
            temporizadorApm.Stop();

            Agent.Tracer.CurrentTransaction.CaptureSpan(telemetriaNome, acaoNome, (span) =>
            {
                span.SetLabel(telemetriaNome, telemetriaValor);
                span.Duration = temporizadorApm.Elapsed.TotalMilliseconds;
            });
        }
        else
            result = acao();

        if (!telemetriaOptions.ApplicationInsights || temporizador == null)
            return result;

        temporizador.Stop();

        return result;
    }

    public void Registrar(Action acao, string acaoNome, string telemetriaNome, string telemetriaValor)
    {
        DateTime inicioOperacao = default;
        Stopwatch temporizador = default;

        if (telemetriaOptions.ApplicationInsights)
        {
            inicioOperacao = DateTime.UtcNow;
            temporizador = Stopwatch.StartNew();
        }

        if (telemetriaOptions.Apm)
        {
            var temporizadorApm = Stopwatch.StartNew();
            acao();
            temporizadorApm.Stop();

            Agent.Tracer.CurrentTransaction.CaptureSpan(telemetriaNome, acaoNome, (span) =>
            {
                span.SetLabel(telemetriaNome, telemetriaValor);
                span.Duration = temporizadorApm.Elapsed.TotalMilliseconds;
            });
        }
        else
            acao();

        if (!telemetriaOptions.ApplicationInsights || temporizador == null)
            return;

        temporizador.Stop();
    }

    public async Task RegistrarAsync(Func<Task> acao, string acaoNome, string telemetriaNome, string telemetriaValor)
    {
        DateTime inicioOperacao = default;
        Stopwatch temporizador = default;

        if (telemetriaOptions.ApplicationInsights)
        {
            inicioOperacao = DateTime.UtcNow;
            temporizador = Stopwatch.StartNew();
        }

        if (telemetriaOptions.Apm)
        {
            var temporizadorApm = Stopwatch.StartNew();
            await acao();
            temporizadorApm.Stop();

            Agent.Tracer.CurrentTransaction.CaptureSpan(telemetriaNome, acaoNome, (span) =>
            {
                span.SetLabel(telemetriaNome, telemetriaValor);
                span.Duration = temporizadorApm.Elapsed.TotalMilliseconds;
            });
        }
        else
            await acao();


        if (telemetriaOptions.ApplicationInsights && temporizador != null)
        {
            temporizador.Stop();
        }
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