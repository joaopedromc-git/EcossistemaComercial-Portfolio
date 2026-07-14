# Configuração Demonstrativa

Este documento apresenta uma referência para preparar o ambiente demonstrativo.

A execução completa não é necessária para avaliar o projeto. Os códigos, queries, workflows e screenshots permitem compreender o funcionamento da solução.

---

## Pré-requisitos

Para executar todos os componentes seriam necessários:

- Windows para o aplicativo desktop;
- .NET SDK compatível com os projetos;
- PostgreSQL;
- Java para executar o Metabase por JAR;
- n8n;
- navegador;
- acesso a uma API compatível com o CRM ou dados simulados.

---

# 1. PostgreSQL

## Criar o banco

Exemplo:

```sql
CREATE DATABASE ecossistema_portfolio;
```

## Executar o schema

Na pasta:

```text
database/postgresql/
```

execute:

```powershell
psql `
    -h 127.0.0.1 `
    -p 5432 `
    -U postgres `
    -d ecossistema_portfolio `
    -v ON_ERROR_STOP=1 `
    -f ".\schema\01_schema_portfolio.sql"
```

## Executar a seed

```powershell
psql `
    -h 127.0.0.1 `
    -p 5432 `
    -U postgres `
    -d ecossistema_portfolio `
    -v ON_ERROR_STOP=1 `
    -f ".\seeds\02_seed_demo.sql"
```

## Executar a validação

```powershell
psql `
    -h 127.0.0.1 `
    -p 5432 `
    -U postgres `
    -d ecossistema_portfolio `
    -v ON_ERROR_STOP=1 `
    -f ".\tests\03_validacao_demo.sql"
```

Resultado esperado:

| Estrutura | Quantidade |
|---|---:|
| Vendedores | 9 |
| Leads | 120 |
| Vendas | 36 |

---

# 2. Metabase

O Metabase pode ser executado localmente por JAR ou container.

Para o ambiente demonstrativo, ele deve se conectar ao banco:

```text
ecossistema_portfolio
```

Configuração de exemplo:

```text
Host: 127.0.0.1
Porta: 5432
Banco: ecossistema_portfolio
Usuário: metabase_portfolio_reader
Senha: definida localmente
```

As queries estão em:

```text
analytics/metabase/queries/
```

Os dashboards não são distribuídos com o banco interno do Metabase.

Eles podem ser reconstruídos utilizando as queries e os screenshots como referência.

---

# 3. Portal Web

Acesse:

```text
apps/portal-web/
```

Copie:

```text
appsettings.example.json
```

como:

```text
appsettings.json
```

Preencha:

- conexão PostgreSQL;
- URL do Metabase;
- chave de incorporação;
- IDs dos dashboards.

Depois execute:

```powershell
dotnet restore
dotnet run
```

---

# 4. API de notificações

Acesse:

```text
apps/notificador-api/
```

Copie:

```text
appsettings.example.json
```

como:

```text
appsettings.json
```

Preencha:

- token do webhook;
- token do cliente;
- origens permitidas.

Execute:

```powershell
dotnet restore
dotnet run
```

---

# 5. Aplicativo desktop

Acesse:

```text
apps/notificador-desktop/
```

Copie:

```text
appsettings.example.json
```

como:

```text
appsettings.json
```

Preencha:

- URL da API;
- token do cliente;
- intervalo de consulta.

Execute no Windows:

```powershell
dotnet restore
dotnet run
```

A API precisa estar disponível para que o aplicativo receba eventos.

---

# 6. n8n

Os workflows estão em:

```text
automation/n8n/workflows/
```

Após a importação, configure:

1. credencial da API do CRM;
2. credencial PostgreSQL;
3. URL da API de notificações;
4. token do webhook;
5. agendamento;
6. variáveis do ambiente.

Mantenha os workflows desativados durante a configuração.

---

# 7. Arquivo de ambiente

A raiz contém:

```text
.env.example
```

Esse arquivo apresenta uma visão geral das variáveis necessárias.

Ele não contém valores reais.

Não utilize `CHANGE_ME` em um ambiente disponível publicamente.

---

# 8. Ordem recomendada

Para uma execução completa:

```text
1. PostgreSQL
2. Schema e seed
3. Metabase
4. API
5. Portal
6. Aplicativo desktop
7. n8n
```

---

# 9. Avaliação sem execução

Para analisar o projeto sem preparar o ambiente:

1. leia o README principal;
2. observe os diagramas;
3. consulte os screenshots;
4. abra as queries SQL;
5. revise o schema;
6. visualize os workflows;
7. consulte os serviços em C#;
8. leia as decisões técnicas.

---

# Documentação relacionada

- [README principal](../../README.md)
- [Arquitetura](../architecture/README.md)
- [PostgreSQL](../../database/postgresql/README.md)
- [Metabase](../../analytics/metabase/README.md)
- [Portal](../../apps/portal-web/README.md)
- [API](../../apps/notificador-api/README.md)
- [Aplicativo desktop](../../apps/notificador-desktop/README.md)
- [Automação n8n](../../automation/n8n/README.md)
- [Infraestrutura](../../infrastructure/README.md)