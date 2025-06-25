using System;
using System.Data;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace fnPayment;

public class fnPayment
{
    private readonly ILogger<fnPayment> _logger;
    private readonly IConfiguration _configuration;
    private readonly string[] StatusList = { "Aprovado", "Reprovado", "Em análise" };
    private readonly Random random = new Random();


    public fnPayment(ILogger<fnPayment> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration; 
    }



    [Function(nameof(fnPayment))]
    [CosmosDBOutput("%CosmosDB%", "%CosmosContainer%", Connection = "CosmosDBConnection", CreateIfNotExists =true)]
    public async Task<object?> Run(
        [ServiceBusTrigger("payment-queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        PaymentModel payment = null;
        try
        {
            payment = JsonSerializer.Deserialize<PaymentModel>(message.Body.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true

            });
            if (payment == null)
            {
                await messageActions.DeadLetterMessageAsync(message, null, "a mensagem nao pode ser deserializada.");

            }
            int index = random.Next(StatusList.Length);
            string status = StatusList[index];
            payment.Status = status;

            if (status == "Aprovado")
            {
                payment.DataAprovacao = DateTime.Now;
                await SentToNotificationQueue(payment);

            }
            return payment;
        }
        catch (Exception ex)
        {
            await messageActions.DeadLetterMessageAsync(message, null, $"Erro: {ex.Message}");
            throw;
        }
        finally
        {
            await messageActions.CompleteMessageAsync(message);

        }

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }

    private async Task SentToNotificationQueue(PaymentModel payment)
    {
        var connectionString = _configuration.GetSection("ServiceBusConnection").Value.ToString();
        var queueName = _configuration.GetSection("NotificationQueue").Value.ToString();

        var serviceBusClient = new ServiceBusClient(connectionString);
        var sender = serviceBusClient.CreateSender(queueName);
        var message = new ServiceBusMessage(JsonSerializer.Serialize(payment))
        {
            ContentType = "application/json",
            MessageId = payment.IdPayment.ToString()
        };

        message.ApplicationProperties["IdPayment"] = payment.IdPayment;
        message.ApplicationProperties["type"] = "notification";
        message.ApplicationProperties["message"] = "Pagamento Aprovado com sucesso!!";


        try
        {
            await sender.SendMessageAsync(message);
            _logger.LogInformation("Mensagem de notificação enviada para a fila: {id}",payment.IdPayment.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ao enviar a mensagem de notificação na fila");
            throw;
        }
        finally
        {
            await sender.DisposeAsync();
            await serviceBusClient.DisposeAsync();  
        }

    }
}
