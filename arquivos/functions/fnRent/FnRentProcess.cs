using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace fbRentProcess;

public class ProcessaLocacao
{
    private readonly ILogger<ProcessaLocacao> _logger;
    private readonly IConfiguration _configuration;

    public ProcessaLocacao(ILogger<ProcessaLocacao> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function(nameof(ProcessaLocacao))]
    public async Task Run(
        [ServiceBusTrigger("fila-locacao-auto", Connection = "ServiceBusConnections")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        var body = message.Body.ToString();
        _logger.LogInformation("Message Body: {body}", body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        RentModel? rentModel = null;
        try
        {
            rentModel = JsonSerializer.Deserialize<RentModel>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,

            });
            if (rentModel is null)
            {
                _logger.LogError("Mensagem mal formatada");
                await messageActions.DeadLetterMessageAsync(message,null,"Mensagem mal formatada");
                return;
            }
            var connectionString = _configuration.GetConnectionString("SqlConnectionString");
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var command = new SqlCommand(@"INSERT INTO Locacao (Nome, Email, Modelo, Ano, TempoAluguel, Data) VALUES (@Nome, @Email,@Modelo, @Ano, @TempoAluguel, @Data)", connection);
            command.Parameters.AddWithValue("@Nome", rentModel.nome);
            command.Parameters.AddWithValue("@Email", rentModel.email);
            command.Parameters.AddWithValue("@Modelo", rentModel.modelo);
            command.Parameters.AddWithValue("@Ano", rentModel.ano);
            command.Parameters.AddWithValue("@TempoAluguel", rentModel.tempoAluguel);
            command.Parameters.AddWithValue("@Data", rentModel.data);

          
           
            var serviceBusConnection = _configuration.GetSection("ServiceBusConnections").Value.ToString();
            var serviceBusQueue = _configuration.GetSection("ServiceBusQueue").Value.ToString();

            sendMessageToPay(serviceBusConnection, serviceBusQueue,rentModel);

            var rowsResult = await command.ExecuteNonQueryAsync();
            connection.Close();
            // Complete the message
            await messageActions.CompleteMessageAsync(message);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar a mensagem: {menssageId}", message.MessageId);
            await messageActions.DeadLetterMessageAsync(message, null, $"Erro ao processar a mensagem:{ex.Message}");
            
        }

 
    }

    private void sendMessageToPay(string serviceBusConnection, string serviceBusQueue, RentModel rentModel)
    {
        ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusConnection);
        ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusQueue);
        ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(rentModel));
        message.ContentType = "application/json";
        message.ApplicationProperties.Add("Tipo", "Pagamento");
        message.ApplicationProperties.Add("Nome", rentModel.nome);
        message.ApplicationProperties.Add("Email", rentModel.email);
        message.ApplicationProperties.Add("Modelo", rentModel.modelo);
        message.ApplicationProperties.Add("Ano", rentModel.ano);
        message.ApplicationProperties.Add("TempoAluguel", rentModel.tempoAluguel);
        message.ApplicationProperties.Add("Data", rentModel.data.ToString("yyyy-MM-ddTHH:mm:ss.fffffffk"));
        
        serviceBusSender.SendMessageAsync(message).Wait();
        serviceBusSender.DisposeAsync();



    }
}