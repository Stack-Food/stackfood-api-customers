# StackFood Customers - Kubernetes Manifests

Manifestos Kubernetes para o microserviço Customers seguindo GitOps com ArgoCD.

## 📂 Estrutura

```
k8s/
├── base/                          # Configurações base (reutilizáveis)
│   ├── deployment.yaml           # Deployment com 2 replicas
│   ├── service.yaml              # ClusterIP service (port 8084)
│   ├── hpa.yaml                  # HorizontalPodAutoscaler (2-10 pods)
│   └── kustomization.yaml        # Kustomize base
├── prod/                          # Overlay de produção
│   ├── configmap.yaml            # Variáveis de ambiente
│   ├── secret.yaml               # Credenciais (DB, AWS)
│   └── kustomization.yaml        # Kustomize overlay
└── argocd-application.yaml        # ArgoCD Application (GitOps)

## 🚀 Deploy com ArgoCD

### 1. Aplicar o ArgoCD Application

```bash
kubectl apply -f k8s/argocd-application.yaml
```

### 2. Verificar sincronização

```bash
kubectl get application customers -n argocd
```

### 3. Acessar logs

```bash
kubectl logs -f deployment/stackfood-customers -n customers
```

## 🔧 Deploy Manual (sem ArgoCD)

```bash
# Aplicar manifests do ambiente prod
kubectl apply -k k8s/prod/

# Verificar pods
kubectl get pods -n customers

# Verificar service
kubectl get svc -n customers
```

## 📝 Configuração

### ConfigMap (k8s/prod/configmap.yaml)

Variáveis de ambiente que precisam ser ajustadas:

- `ConnectionStrings__DefaultConnection` - Connection string PostgreSQL
- `Cognito__UserPoolId` - ID do User Pool Cognito
- `Cognito__ClientId` - Client ID do Cognito
- `AWS__SNS__CustomerEventsTopicArn` - ARN do tópico SNS

### Secret (k8s/prod/secret.yaml)

Credenciais sensíveis que precisam ser substituídas:

- `POSTGRES_PASSWORD` - Senha do banco de dados
- `AWS_ACCESS_KEY_ID` - Access Key AWS
- `AWS_SECRET_ACCESS_KEY` - Secret Key AWS

## 🔄 Atualização de Imagem

O ArgoCD detecta automaticamente mudanças no Git. Para atualizar a imagem:

1. Faça push da nova imagem para o registry:
   ```bash
   docker push ghcr.io/stack-food/stackfood-api-customers:latest
   ```

2. O ArgoCD sincroniza automaticamente (ou force sync):
   ```bash
   argocd app sync customers
   ```

## 📊 Monitoramento

### Health Checks

- **Liveness Probe**: `/health` (porta 8084)
- **Readiness Probe**: `/health` (porta 8084)

### Métricas Prometheus

Anotações configuradas para scraping automático:
- `prometheus.io/scrape: "true"`
- `prometheus.io/path: "/metrics"`
- `prometheus.io/port: "8084"`

## 🌐 Comunicação Interna

O serviço pode ser acessado internamente no cluster via:

```
http://stackfood-customers.customers.svc.cluster.local:8084
```

## 🔗 API Gateway

Este microserviço é roteado pelo AWS API Gateway:

- **Rota externa**: `https://api.stackfood.com.br/customers/*`
- **Destino**: VPC Link → NLB → NGINX Ingress → stackfood-customers:8084

## 📌 Observações

- **Namespace**: `customers` (criado automaticamente pelo ArgoCD)
- **Replicas**: Min 2, Max 10 (HPA baseado em CPU/Memory)
- **Resources**: Requests (100m CPU, 256Mi RAM), Limits (500m CPU, 512Mi RAM)
- **Image Pull Secret**: `ghcr-secret` (deve existir no namespace)
