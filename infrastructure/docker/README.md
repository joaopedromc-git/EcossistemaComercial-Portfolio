# Docker

Esta pasta apresenta uma topologia demonstrativa para executar PostgreSQL, n8n e Metabase.

O arquivo foi preparado para mostrar a organização dos serviços sem incluir credenciais ou configurações do ambiente original.

---

## Serviços

### PostgreSQL

Responsável por armazenar:

- dados comerciais;
- banco interno do Metabase;
- banco interno do n8n.

### Metabase

Responsável pela visualização dos indicadores.

A porta é vinculada somente ao endereço local:

```text
127.0.0.1:3000
```

### n8n

Responsável pelas automações.

A porta é vinculada somente ao endereço local:

```text
127.0.0.1:5678
```

---

## Arquivo

```text
docker-compose.example.yml
```

O sufixo `.example` indica que o arquivo precisa ser copiado e configurado antes do uso.

Exemplo:

```bash
cp docker-compose.example.yml docker-compose.yml
```

---

## Arquivo de ambiente

O Compose pode utilizar um arquivo `.env` baseado no modelo da raiz:

```text
../../.env.example
```

O arquivo real não deve ser incluído no repositório.

Exemplo:

```bash
cp ../../.env.example ../../.env
```

Depois, substitua todos os valores:

```text
CHANGE_ME
```

---

## Preparação dos bancos internos

O PostgreSQL cria inicialmente o banco definido por:

```text
POSTGRES_DATABASE
```

Antes de iniciar o Metabase e o n8n, devem ser criados seus bancos e usuários internos.

Exemplo conceitual:

```sql
CREATE ROLE metabase_portfolio_app
LOGIN PASSWORD 'SUBSTITUA_A_SENHA';

CREATE DATABASE metabase_portfolio_app
OWNER metabase_portfolio_app;

CREATE ROLE n8n_portfolio_app
LOGIN PASSWORD 'SUBSTITUA_A_SENHA';

CREATE DATABASE n8n_portfolio_app
OWNER n8n_portfolio_app;
```

Essas senhas devem corresponder às variáveis do arquivo `.env`.

---

## Ordem de inicialização

### 1. Subir somente o PostgreSQL

```bash
docker compose \
  --env-file ../../.env \
  -f docker-compose.yml \
  up -d postgres
```

### 2. Criar bancos e usuários internos

Acesse o PostgreSQL:

```bash
docker exec -it portfolio_postgres \
  psql -U postgres -d postgres
```

Crie os bancos internos e encerre com:

```text
\q
```

### 3. Executar o schema demonstrativo

O schema e a seed estão em:

```text
../../database/postgresql/
```

### 4. Subir os demais serviços

```bash
docker compose \
  --env-file ../../.env \
  -f docker-compose.yml \
  up -d
```

---

## Verificação

Listar os containers:

```bash
docker compose \
  --env-file ../../.env \
  -f docker-compose.yml \
  ps
```

Acompanhar logs do n8n:

```bash
docker logs -f portfolio_n8n
```

Acompanhar logs do Metabase:

```bash
docker logs -f portfolio_metabase
```

---

## Portas

| Serviço | Porta interna | Exposição |
|---|---:|---|
| PostgreSQL | 5432 | `127.0.0.1:5432` |
| Metabase | 3000 | `127.0.0.1:3000` |
| n8n | 5678 | `127.0.0.1:5678` |

A vinculação ao endereço local impede acesso direto pela internet.

O Nginx encaminha as solicitações públicas.

---

## Volumes

| Volume | Conteúdo |
|---|---|
| `portfolio_postgres_data` | Bancos PostgreSQL |
| `portfolio_n8n_data` | Arquivos persistentes do n8n |

Os volumes não fazem parte do repositório.

---

## Rede

Os containers utilizam:

```text
portfolio_network
```

Dentro dessa rede, os serviços acessam o PostgreSQL pelo hostname:

```text
postgres
```

---

## Observações

- As imagens usam valores configuráveis.
- Em produção, prefira versões fixas já testadas.
- Não utilize senhas presentes em documentação.
- Não exponha PostgreSQL diretamente à internet.
- Faça backup dos volumes.
- Revise as configurações antes de ativar workflows.