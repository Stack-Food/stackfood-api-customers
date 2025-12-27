# 👤 StackFood Customers API

Microserviço responsável pelo **gerenciamento completo de clientes** do sistema StackFood, incluindo cadastro, autenticação via AWS Cognito e integração com os demais serviços.

---

## 📋 Descrição do Projeto

O **Customers Service** gerencia todo o ciclo de vida dos clientes, desde o cadastro até a autenticação. Faz parte da arquitetura de microserviços da **Fase 5** do Tech Challenge, unificando a gestão de clientes que antes estava dividida entre o monolito (stackfood-api) e a Lambda de autenticação.

**Repositório**: `https://github.com/Stack-Food/stackfood-api-customers`

---

## 🎯 Funcionalidades

### Core
- ✅ Criar customer (cadastro duplo: PostgreSQL + AWS Cognito)
- ✅ Autenticar customer via CPF (retorna JWT do Cognito)
- ✅ Autenticar como convidado (guest)
- ✅ Consultar customer por ID
- ✅ Consultar customer por CPF
- ✅ Atualizar dados do customer
- ✅ Desativar/Ativar customer

### Integrações
- 🔐 **AWS Cognito**: Gestão de identidade e autenticação
- 📤 **SNS**: Publicar eventos CustomerCreated, CustomerUpdated (opcional)
- 🗄️ **PostgreSQL**: Persistência de dados do customer

---

## 🗂️ Estrutura do Projeto

```
stackfood-api-customers/
├── src/
│   ├── StackFood.Customers.API/          # API REST
│   │   ├── Controllers/
│   │   │   └── CustomersController.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   │
│   ├── StackFood.Customers.Domain/       # Entidades e eventos
│   │   ├── Entities/
│   │   │   └── Customer.cs
│   │   └── Events/
│   │       ├── CustomerCreatedEvent.cs
│   │       └── CustomerUpdatedEvent.cs
│   │
│   ├── StackFood.Customers.Application/  # Casos de uso
│   │   ├── UseCases/
│   │   │   ├── CreateCustomerUseCase.cs
│   │   │   ├── GetCustomerByIdUseCase.cs
│   │   │   ├── GetCustomerByCpfUseCase.cs
│   │   │   ├── UpdateCustomerUseCase.cs
│   │   │   └── AuthenticateCustomerUseCase.cs
│   │   ├── Interfaces/
│   │   │   ├── ICustomerRepository.cs
│   │   │   ├── ICognitoService.cs
│   │   │   └── IEventPublisher.cs
│   │   └── DTOs/
│   │       ├── CustomerDTO.cs
│   │       ├── CreateCustomerRequest.cs
│   │       ├── AuthenticateRequest.cs
│   │       └── AuthenticateResponse.cs
│   │
│   └── StackFood.Customers.Infrastructure/ # Infraestrutura
│       ├── Persistence/
│       │   ├── CustomersDbContext.cs
│       │   ├── Repositories/
│       │   │   └── CustomerRepository.cs
│       │   └── Configurations/
│       │       └── CustomerConfiguration.cs
│       └── ExternalServices/
│           ├── CognitoService.cs         # Integração com AWS Cognito
│           └── SnsEventPublisher.cs      # Publicação de eventos
│
├── tests/
│   └── StackFood.Customers.Tests/
│
├── Dockerfile
├── StackFood.Customers.sln
└── README.md
```

---

## 🗄️ Banco de Dados

### Tipo: **PostgreSQL**

### Tabela: `customers`

```sql
CREATE TABLE customers (
    id uuid PRIMARY KEY,
    name varchar(200) NOT NULL,
    email varchar(200) NOT NULL,
    cpf varchar(14) NOT NULL,
    cognito_user_id varchar(100),
    is_active boolean NOT NULL,
    created_at timestamp NOT NULL,
    updated_at timestamp NOT NULL
);

-- Índices
CREATE UNIQUE INDEX idx_customers_cpf ON customers(cpf);
CREATE UNIQUE INDEX idx_customers_email ON customers(email);
CREATE INDEX idx_customers_cognito_user_id ON customers(cognito_user_id);
```

### Customer Entity

```csharp
public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Cpf { get; private set; }
    public string? CognitoUserId { get; private set; }  // Link com Cognito
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
}
```

---

## 🌐 APIs/Endpoints

### **Base URL**: `/api/customers`

| Método | Endpoint | Descrição | Request | Response |
|--------|----------|-----------|---------|----------|
| POST | `/api/customers` | Criar customer | `CreateCustomerRequest` | `CustomerDTO` |
| GET | `/api/customers/{id}` | Consultar por ID | - | `CustomerDTO` |
| GET | `/api/customers/cpf/{cpf}` | Consultar por CPF | - | `CustomerDTO` |
| PUT | `/api/customers/{id}` | Atualizar customer | `UpdateCustomerRequest` | `CustomerDTO` |
| POST | `/api/customers/auth` | Autenticar customer | `AuthenticateRequest` | `AuthenticateResponse` |

### DTOs:

#### CreateCustomerRequest
```json
{
  "name": "João Silva",
  "email": "joao@example.com",
  "cpf": "12345678900"
}
```

#### AuthenticateRequest
```json
{
  "cpf": "12345678900"
}
```

#### AuthenticateResponse
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "customer": {
    "id": "uuid",
    "name": "João Silva",
    "email": "joao@example.com",
    "cpf": "12345678900",
    "cognitoUserId": "cpf-do-usuario",
    "isActive": true,
    "createdAt": "2025-12-20T10:00:00Z",
    "updatedAt": "2025-12-20T10:00:00Z"
  },
  "message": "Customer authenticated successfully"
}
```

---

## 🔐 Integração com AWS Cognito

### Fluxo de Cadastro

```
1. Cliente envia POST /api/customers
   ↓
2. Customers API cria usuário no Cognito
   - Username: CPF
   - Attributes: name, email, email_verified
   - Password: Stackfood#123 (padrão)
   ↓
3. Customers API salva no PostgreSQL
   - Armazena CognitoUserId para referência
   ↓
4. Retorna CustomerDTO
```

### Fluxo de Autenticação

```
1. Cliente envia POST /api/customers/auth { "cpf": "123..." }
   ↓
2. Customers API valida no PostgreSQL
   - Verifica se customer existe
   - Verifica se está ativo
   ↓
3. Customers API autentica no Cognito
   - Username: CPF
   - Password: Stackfood#123
   ↓
4. Cognito retorna JWT Token (IdToken)
   ↓
5. Customers API retorna token + dados do customer
```

### Autenticação como Convidado

```json
POST /api/customers/auth
{
  "cpf": ""  // CPF vazio = convidado
}

Response:
{
  "token": "guest-jwt-token",
  "customer": null,
  "message": "Authenticated as guest"
}
```

---

## 📡 Mensageria SNS (Opcional)

### Publishers (Envia para SNS)

#### Tópico: `sns-customer-events`

**Eventos**:

1. **CustomerCreatedEvent**
```json
{
  "customerId": "uuid",
  "name": "João Silva",
  "email": "joao@example.com",
  "cpf": "12345678900",
  "cognitoUserId": "cpf",
  "createdAt": "2025-12-20T10:00:00Z",
  "timestamp": "2025-12-20T10:00:00Z"
}
```

2. **CustomerUpdatedEvent**
```json
{
  "customerId": "uuid",
  "name": "João Silva Updated",
  "email": "joao.new@example.com",
  "updatedAt": "2025-12-20T11:00:00Z",
  "timestamp": "2025-12-20T11:00:00Z"
}
```

---

## 🛠️ Tecnologias Utilizadas

- **Linguagem:** C# (.NET 8)
- **Banco de Dados:** PostgreSQL (EF Core 8.0)
- **Autenticação:** AWS Cognito (SDK)
- **Mensageria:** SNS (AWS SDK - opcional)
- **Arquitetura:** Clean Architecture
- **Documentação:** Swagger/OpenAPI
- **Containerização:** Docker
- **Testes:** xUnit

---

## 🚀 Como Executar Localmente

### Pré-requisitos

- [Docker](https://www.docker.com/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- AWS Cognito configurado (ou usar o User Pool existente)

### Passos

#### 1. **Com Docker Compose (Recomendado)**

```bash
cd stack-food
docker-compose up -d customers-api
```

#### 2. **Localmente (Desenvolvimento)**

```bash
cd stackfood-api-customers

# Restaurar dependências
dotnet restore

# Configurar appsettings.json
# Editar ConnectionStrings e Cognito settings

# Executar API
dotnet run --project src/StackFood.Customers.API

# API rodando em http://localhost:8084
# Swagger em http://localhost:8084
```

---

## ⚙️ Variáveis de Ambiente

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente ASP.NET Core | `Development` |
| `ConnectionStrings__DefaultConnection` | String de conexão PostgreSQL | `Host=localhost;Port=5432;Database=stackfood_customers...` |
| `Cognito__Region` | Região AWS Cognito | `us-east-1` |
| `Cognito__UserPoolId` | ID do User Pool | `us-east-1_fIpUH0TPW` |
| `Cognito__ClientId` | ID do Client App | `79surdr98kupss8u8n8h73barl` |
| `Cognito__DefaultPassword` | Senha padrão | `Stackfood#123` |
| `Cognito__GuestUsername` | Username do convidado | `convidado` |
| `Cognito__GuestPassword` | Senha do convidado | `Convidado123!` |
| `AWS__Region` | Região AWS para SNS | `us-east-1` |
| `AWS__ServiceURL` | URL do LocalStack (dev) | `http://localhost:4566` |
| `AWS__SNS__CustomerEventsTopicArn` | ARN do tópico SNS | `arn:aws:sns:...` |

---

## 🧪 Testes

### Executar Testes

```bash
dotnet test
```

### Cobertura de Código

```bash
dotnet test /p:CollectCoverage=true
```

---

## 🐳 Docker

### Build da Imagem

```bash
docker build -t stackfood-customers-api:latest -f Dockerfile .
```

### Executar com Docker Compose

```bash
docker-compose up customers-api
```

---

## 📦 Pacotes NuGet Utilizados

- `Microsoft.EntityFrameworkCore` (8.0.0)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.0)
- `Microsoft.EntityFrameworkCore.Design` (8.0.0)
- `AWSSDK.CognitoIdentityProvider` (4.0.x)
- `AWSSDK.SimpleNotificationService` (4.0.x)
- `AWSSDK.Extensions.NETCore.Setup` (4.0.x)
- `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (8.0.0)

---

## 🔗 Integração com Outros Serviços

### Serviços que dependem de Customers:

- **Orders Service**: Referencia CustomerId e CustomerName nos pedidos
- **API Gateway/Lambda**: Pode usar para autenticação centralizada

### Migração do Monolito:

Este microserviço substitui:
- `stackfood-api/CustomerController` (PostgreSQL)
- `stackfood-lambda/CustomerHandler` (Cognito)

Unificando as funcionalidades em um único serviço.

---

## 👥 Participantes

| Nome | RM | E-mail | Discord |
|------|-----|--------|---------|
| Leonardo Duarte | RM364564 | leo.duarte.dev@gmail.com | _leonardoduarte |
| Luiz Felipe Maia | RM361928 | luiz.felipeam@hotmail.com | luiz_08 |
| Leonardo Luiz Lemos | RM364201 | leoo_lemos@outlook.com | leoo_lemos |
| Rodrigo Rodriguez Figueiredo de Oliveira Silva | RM362272 | rodrigorfig1@gmail.com | lilroz |
| Vinicius Targa Gonçalves | RM364425 | viniciustarga@gmail.com | targa1765 |

---

## 💡 Observações Finais

- ✅ **Arquitetura Unificada**: Combina PostgreSQL (dados) + Cognito (autenticação)
- ✅ **Clean Architecture**: Separação clara de responsabilidades
- ✅ **Fonte Única de Verdade**: Customer centralizado em um único serviço
- ✅ **Compatível**: Mantém compatibilidade com Lambda e monolito durante migração
- ⚠️ **Próximos passos**: Migrar Orders/Payments para usar Customers API

---

**Status**: ✅ Pronto para uso
**Última atualização**: 2025-12-20
