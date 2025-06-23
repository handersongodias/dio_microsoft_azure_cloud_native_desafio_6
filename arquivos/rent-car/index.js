const express = require('express');
const cors = require('cors');
const {DefaultAzureCredentials} = require('@azure/identity');
const {ServiceBusClient} = require('@azure/service-bus');
require('dotenv').config();

const app = express();
app.use(express.json());
app.use(cors());

app.post('/api/locacao', async (req, res) => {
  if (!req.body) {
    return res.status(400).json({ error: 'Corpo da requisição ausente ou inválido.' });
  }
  const { nome, email, modelo, ano, tempoAluguel } = req.body;
  const connectionString = [informar connectionstring servicebus];
  const mensagem = {
    nome,
    email,
    modelo,
    ano,
    tempoAluguel,
    data: new Date().toISOString()
  };

  try {
    const queueName = 'fila-locacao-auto';
    const sbClient = new ServiceBusClient(connectionString);
    const sender = sbClient.createSender(queueName);
    const message = {
      body: mensagem,
      contentType: 'application/json',
      subject: 'Locação'
    };
    await sender.sendMessages(message);
    await sender.close();
    await sbClient.close();
    res.status(200).json({ message: 'Locação de veiculo enviada com sucesso para o Service Bus.' });
  } catch (error) {
    console.error('Erro ao enviar mensagem para o Service Bus:', error);
    res.status(500).json({ error: 'Erro ao processar a locação.' });
  }
});
app.listen(3001, () => {
  console.log('Servidor rodando na porta 3001');
});