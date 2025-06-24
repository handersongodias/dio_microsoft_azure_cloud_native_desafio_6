# Desafio DIO com Microsoft Azure Cloud Native
# Criação de um Serviço de aluguel de veiculo utilizando  totalmente Cloud-Native
Este projeto foi desenvolvido no portal Azure

---

## 🚀 Funcionalidades

-  Criar uma aplicação que simula a reserva e aluguel de carro
-  Criar e configurar o ServiceBus para receber e enviar as requisições
-  Criar e codificar azures functions
-  criar API com nodejs, codificação com C# para as functions

 
---

## 🧰 Tecnologias

- portal.azure
- application insights
- serviceBus
- azure functions
- logicapp
- azure sql
- cosmosdb
- nodejs
- key vault
- docker
- visual studio


---

### 1. Acessar o portal azure
    Acessar o portal azure, selecionar     
    criar o rg,
    criar serviceBus e fila
    criar container registry ACR,
    criar container app,
    criar as functions app,
    criar banco de dados azure sql,
    criar cosmosdb       
    criar logic app
    criar key vault
    
    
### 3. Codificar 
    codificar com javascript , node js criar API, e C# para as functions
    
### 4. Arquivos
    Os arquivos com os codigo estao na pasta arquivos
    
### 5. Detalhe

Detalhe importante, caso você esteja utilizando a conta "free do Azure", ela apresentará esse erro ao tentar criar um 
novo containerapp env: 
             
     "" MaxNumberOfGlobalEnvironmentsInSubExceeded) The subscription 'xxx-xxxx-xxxxx-xxxx' cannot have more than 1 Container App Environments.""
---
a solução é utilizar a mesmo managedEnvironment criado anteriormente , e configurar o gerenciador de identidade nesse managedEnvironment
para que ele tenha acesso ao Azure container registry (ACR), essa configuração pode ser realizada via portal ou CLI, coloquei as etapas para realizar no portal:

Para conceder **acesso seguro de pull ao Azure Container Registry (ACR)** usando uma **identidade gerenciada**, siga este passo a passo:

---

### ✅ 1. Habilite a identidade gerenciada no recurso
Se for um **Azure Container App**, **Web App**, **Function App** ou outro serviço compatível:

- Vá até o recurso no portal do Azure.
- Acesse **Identidade** no menu lateral.
- Ative a **Identidade atribuída pelo sistema** (ou crie uma **identidade atribuída pelo usuário**, se preferir).

---

### ✅ 2. Conceda permissão de pull no ACR
Agora, você precisa dar à identidade permissão para acessar o ACR:

- Vá até o **Azure Container Registry** no portal.
- Clique em **Controle de acesso (IAM)**.
- Selecione **Adicionar > Adicionar atribuição de função**.
- Escolha a função **AcrPull**.
- No campo **Membro**, selecione a identidade gerenciada do recurso que você ativou no passo anterior.

---

### ✅ 3. Verifique se a aplicação está configurada para usar essa identidade
Se estiver implantando um Container App, por exemplo:

- Durante a criação ou edição do app, selecione **Autenticação com ACR via identidade gerenciada**.
- Escolha a identidade correta (sistema ou usuário) e o ACR desejado.

---

Esse processo garante que **nenhuma senha ou token fixo** seja usado, e o acesso ao ACR será feito com base em **permissões controladas e auditáveis** via Azure AD.

espero que essa informação ajude!!
     
      

