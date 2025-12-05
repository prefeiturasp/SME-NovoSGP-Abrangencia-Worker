using Elastic.Apm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Enums;
using SME.NovoSGP.Abrangencia.Infra.Exceptions;
using SME.NovoSGP.Abrangencia.Infra.Extensions;
using SME.NovoSGP.Abrangencia.Infra.Fila;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Text;

namespace SME.NovoSGP.Abrangencia.Worker;

public class RabbitMQMessageProcessor : IRabbitMQMessageProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IServicoTelemetria _servicoTelemetria;
    private readonly IServicoLog _servicoLog;
    private readonly IServicoMensageria _servicoMensageria;
    private readonly ILogger<RabbitMQMessageProcessor> _logger;
    private readonly string apmTransactionType = "WorkerRabbitAbrangencia";

    public RabbitMQMessageProcessor(
        IServiceScopeFactory serviceScopeFactory,
        IServicoTelemetria servicoTelemetria,
        IServicoLog servicoLog,
        IServicoMensageria servicoMensageria,
        ILogger<RabbitMQMessageProcessor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _servicoTelemetria = servicoTelemetria ?? throw new ArgumentNullException(nameof(servicoTelemetria));
        _servicoLog = servicoLog ?? throw new ArgumentNullException(nameof(servicoLog));
        _servicoMensageria = servicoMensageria ?? throw new ArgumentNullException(nameof(servicoMensageria));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessMessageAsync(BasicDeliverEventArgs ea, IChannel channel, Dictionary<string, ComandoRabbit> comandos)
    {
        var mensagem = Encoding.UTF8.GetString(ea.Body.ToArray());
        _logger.LogInformation("Mensagem recebida: {mensagem}", mensagem);
        var rota = ea.RoutingKey;

        if (!comandos.ContainsKey(rota))
        {
            await channel.BasicRejectAsync(ea.DeliveryTag, false);
            return;
        }

        var transacao =  Agent.Tracer.StartTransaction(rota, apmTransactionType);
        //var transacao = _servicoTelemetria.IniciarTransacao(rota);
        var mensagemRabbit = mensagem.ConverterObjectStringPraObjeto<MensagemRabbit>();
        var comandoRabbit = comandos[rota];

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var casoDeUso = scope.ServiceProvider.GetService(comandoRabbit.TipoCasoUso);

            if (casoDeUso == null)
                throw new ArgumentNullException(comandoRabbit.TipoCasoUso.Name);

            await _servicoTelemetria.RegistrarAsync(() =>
                comandoRabbit.TipoCasoUso.ObterMetodo("Executar").InvokeAsync(casoDeUso, mensagemRabbit),
                "RabbitMQ",
                rota,
                rota);

            await channel.BasicAckAsync(ea.DeliveryTag, false);
            //await servicoMensageriaMetricas.Concluido(rota);
        }
        catch (NegocioException nex)
        {
            transacao?.CaptureException(nex);

            await channel.BasicAckAsync(ea.DeliveryTag, false);
            //await servicoMensageriaMetricas.Concluido(rota);

            await RegistrarErroTratamentoMensagem(ea, mensagemRabbit, nex, LogNivel.Negocio, $"Erros: {nex.Message}");
        }
        catch (ValidacaoException vex)
        {
            transacao?.CaptureException(vex);

                 channel.BasicAckAsync(ea.DeliveryTag, false);
            //await servicoMensageriaMetricas.Concluido(rota);

            await RegistrarErroTratamentoMensagem(ea, mensagemRabbit, vex, LogNivel.Negocio, $"Erros: {JsonConvert.SerializeObject(vex.Mensagens())}");
        }
        catch (Exception ex)
        {
            transacao?.CaptureException(ex);

            var rejeicoes = GetRetryCount(ea.BasicProperties);
            if (++rejeicoes >= comandoRabbit.QuantidadeReprocessamentoDeadLetter)
            {
                await channel.BasicAckAsync(ea.DeliveryTag, false);

                var filaLimbo = $"{ea.RoutingKey}.limbo";
                await _servicoMensageria.Publicar(mensagemRabbit, filaLimbo, ExchangeRabbit.WorkerAbrangenciaDeadLetter, "PublicarDeadLetter");
            }
            else await channel.BasicRejectAsync(ea.DeliveryTag, false);

            //await servicoMensageriaMetricas.Erro(rota);
            await RegistrarErroTratamentoMensagem(ea, mensagemRabbit, ex, LogNivel.Critico, $"Erros: {ex.Message}");
        }
        finally
        {
            transacao?.End();
        }
    }

    protected virtual Task RegistrarErroTratamentoMensagem(BasicDeliverEventArgs ea, MensagemRabbit mensagemRabbit, Exception ex, LogNivel logNivel, string observacao)
    {
        return Task.CompletedTask;
    }

    private ulong GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers == null || !properties.Headers.ContainsKey("x-death"))
            return 0;

        var deathProperties = (List<object>)properties.Headers["x-death"];
        if (deathProperties.Count == 0)
            return 0;

        var lastRetry = (Dictionary<string, object>)deathProperties[0];

        if (!lastRetry.ContainsKey("count"))
            return 0;

        var count = lastRetry["count"];

        return (ulong)Convert.ToInt64(count);
    }

    private void RegistrarLog(BasicDeliverEventArgs ea, MensagemRabbit mensagemRabbit, Exception ex, LogNivel logNivel, string observacao)
    {
        var mensagem = $"Worker Abrangencia: Rota -> {ea.RoutingKey}  Cod Correl -> {mensagemRabbit.CodigoCorrelacao.ToString()[..3]}";

        // Cria a LogMensagem, mas não a passa diretamente para o servicoLog.Registrar
        var logMensagem = new LogMensagem(mensagem, logNivel, observacao, ex?.StackTrace, ex?.InnerException?.Message);

        // Cria uma exceção que vai ser passada para o serviço de log (se o servicoLog requer uma Exception)
        var exceptionToLog = new Exception(logMensagem.Mensagem, ex);

        _servicoLog.Registrar(exceptionToLog);  // Registra a exceção com os dados da LogMensagem
    }
}
