# Ecossistema Comercial de Automação e BI

Projeto de portfólio baseado em uma solução desenvolvida para integrar CRM, automações, banco de dados, dashboards, controle de acesso e notificações comerciais.

A solução centraliza dados de vendas e leads, automatiza atualizações, calcula indicadores e apresenta diferentes níveis de visualização conforme o perfil do usuário.

Este repositório contém uma versão sanitizada do projeto. Nomes, identificadores, credenciais, endereços e dados do ambiente original foram removidos ou substituídos por informações fictícias.

---

## Visão geral

O ecossistema foi construído para conectar diferentes etapas de uma operação comercial:

1. consulta de dados no CRM;
2. sincronização e tratamento das informações;
3. armazenamento no PostgreSQL;
4. cálculo de indicadores;
5. apresentação dos dashboards;
6. autenticação e controle de acesso;
7. envio de notificações de vendas e metas;
8. exibição dos avisos em computadores Windows.

O projeto foi dividido em componentes independentes para facilitar manutenção, segurança e evolução.

---

## Problema tratado

O acompanhamento comercial dependia de consultas manuais, exportações e controles distribuídos entre diferentes ferramentas.

Isso dificultava:

- acompanhar vendas e leads em um único local;
- manter os indicadores atualizados;
- calcular conversão e atingimento de metas;
- separar a visualização por perfil;
- identificar novas vendas rapidamente;
- avisar a equipe quando uma meta fosse atingida;
- controlar duplicidades e falhas de sincronização.

A solução foi criada para concentrar esses processos em um fluxo integrado.

---

## Arquitetura

```mermaid
flowchart LR
    CRM[CRM / HubSpot]

    subgraph Automacao[Automação]
        N8N[n8n]
    end

    subgraph Dados[Camada de dados]
        PG[(PostgreSQL)]
        VIEWS[Views e funções SQL]
    end

    subgraph Analytics[Analytics]
        MB[Metabase]
    end

    subgraph Aplicacoes[Aplicações]
        PORTAL[Portal ASP.NET]
        API[API de notificações]
        DESKTOP[Aplicativo Windows]
    end

    CRM --> N8N
    N8N --> PG
    PG --> VIEWS
    VIEWS --> MB

    PORTAL --> PG
    PORTAL --> MB

    N8N --> API
    API --> DESKTOP
```

### Fluxo principal

1. O n8n consulta vendas e leads no CRM.
2. Os registros são paginados, validados e normalizados.
3. As informações são inseridas ou atualizadas no PostgreSQL.
4. Views e funções SQL transformam os dados em indicadores.
5. O Metabase apresenta os dashboards.
6. O portal autentica o usuário e identifica os dados permitidos.
7. O n8n envia eventos de venda e metas para a API.
8. O aplicativo desktop consulta a API e exibe as notificações.

A documentação detalhada da arquitetura está em:

[Documentação de arquitetura](docs/architecture/README.md)

---

## Tecnologias utilizadas

| Área | Tecnologia |
|---|---|
| Integração com CRM | API REST e HubSpot |
| Automação | n8n |
| Banco de dados | PostgreSQL |
| Business Intelligence | Metabase |
| Portal web | ASP.NET |
| API | ASP.NET Minimal API |
| Aplicativo desktop | Windows Forms |
| Linguagens | C#, SQL e JavaScript |
| Infraestrutura | Linux, Docker e Nginx |
| Autenticação | PostgreSQL, `pgcrypto` e sessões ASP.NET |

---

## Componentes do projeto

### Portal web

Aplicação responsável pela autenticação, identificação do perfil e acesso aos dashboards.

Principais responsabilidades:

- login por código;
- validação de senha;
- primeiro acesso;
- controle de sessão;
- classificação entre vendedor, supervisor e superior;
- consulta dos IDs permitidos;
- geração do acesso ao Metabase.

[Código e documentação do Portal](apps/portal-web/README.md)

---

### Automação com n8n

Os workflows executam a integração entre CRM, PostgreSQL e API.

Principais responsabilidades:

- paginação da API;
- sincronização incremental;
- tratamento de datas;
- normalização de campos;
- inserção e atualização de registros;
- controle da última execução;
- prevenção de notificações duplicadas;
- identificação de metas atingidas.

[Workflows e documentação do n8n](automation/n8n/README.md)

---

### PostgreSQL

O banco concentra os registros operacionais e as regras utilizadas pelas aplicações.

Principais estruturas:

- vendas;
- leads;
- vendedores;
- metas;
- permissões;
- notificações;
- calendário;
- dias úteis;
- controle de sincronização;
- views de indicadores;
- funções de autenticação.

[Schema, seed e documentação do PostgreSQL](database/postgresql/README.md)

---

### Metabase

O Metabase apresenta os indicadores comerciais calculados a partir das tabelas e views do PostgreSQL.

Os dashboards foram organizados em duas visões principais:

- visão superior;
- visão do vendedor.

[Queries, screenshots e documentação do Metabase](analytics/metabase/README.md)

---

### API de notificações

A API recebe eventos enviados pelo n8n e disponibiliza a última notificação para os clientes autorizados.

Os eventos podem representar:

- nova venda;
- meta de vendas atingida;
- meta de arrecadação atingida.

[Código e documentação da API](apps/notificador-api/README.md)

---

### Aplicativo desktop

Aplicação Windows Forms responsável por consultar a API e apresentar os avisos nos computadores dos usuários.

Os pop-ups apresentam:

- vendedor;
- equipe;
- valor;
- forma de pagamento;
- tipo de publicação;
- status da matrícula;
- informações da meta, quando aplicável.

[Código e documentação do aplicativo](apps/notificador-desktop/README.md)

---

## Dashboards

### Visão superior

A visão superior apresenta os resultados consolidados da operação.

Ela inclui:

- vendas e arrecadação;
- quantidade de leads;
- conversão;
- projeção mensal;
- desempenho por equipe;
- desempenho por vendedor;
- ranking por pontuação;
- evolução das vendas por dia;
- vendas realizadas no dia atual.

![Indicadores gerais da visão superior](analytics/metabase/screenshots/superior/05_kpi_geral.jpeg)

![Indicadores por equipe](analytics/metabase/screenshots/superior/02_kpi_equipes.jpeg)

![Ranking por pontos](analytics/metabase/screenshots/superior/07_rank_pontos.jpeg)

---

### Visão do vendedor

A visão do vendedor apresenta somente os indicadores associados ao usuário autenticado.

Ela inclui:

- vendas próprias;
- arrecadação individual;
- leads recebidos;
- conversão;
- metas;
- vendas por dia;
- vendas realizadas no dia;
- posição no ranking.

![Indicadores individuais do vendedor](analytics/metabase/screenshots/vendedor/01_02_kpi_consultor_e_vendas_consultor.jpeg)

![Conversão e vendas por dia](analytics/metabase/screenshots/vendedor/03_04_conversao_e_vendas_dia.jpeg)

![Vendas do dia e ranking](analytics/metabase/screenshots/vendedor/05_06_vendas_hoje_e_ranking.jpeg)

---

## Portal e controle de acesso

O portal utiliza o PostgreSQL para validar o código de login e a senha.

A senha não é armazenada em texto simples. O banco mantém apenas seu hash e realiza a validação por meio de uma função de autenticação.

Após o login, o sistema identifica o tipo de acesso:

- `VENDEDOR`;
- `SUPERVISOR`;
- `SUPERIOR`.

O banco também devolve os identificadores de vendas e leads permitidos para aquele usuário.

Esses identificadores são utilizados como parâmetros dos dashboards.

### Vendedor

Recebe somente os próprios IDs e visualiza apenas seus resultados.

### Supervisor

Recebe seus IDs e os IDs dos vendedores associados a ele.

### Superior

Possui acesso ao painel consolidado e às informações gerais das equipes.

![Tela de login do Portal](apps/portal-web/screenshots/01_login.jpeg)

![Seleção de dashboard para acesso superior](apps/portal-web/screenshots/02-escolha_superior.jpeg)

Mais detalhes estão disponíveis em:

[Fluxo de autenticação](docs/architecture/fluxo-autenticacao.md)

---

## Sincronização incremental

Os workflows não precisam consultar toda a base em cada execução.

A tabela `sync_control` registra o horário da última sincronização concluída. A próxima execução utiliza esse valor para consultar somente registros novos ou modificados.

O processo considera:

- horário da última execução;
- horário atual;
- paginação;
- intervalo mensal;
- data de modificação;
- tratamento de fuso horário;
- conclusão da carga antes de atualizar o controle.

Essa estratégia reduz o volume processado e evita cargas completas desnecessárias.

---

## Indicadores desenvolvidos

Entre os indicadores presentes no projeto estão:

- quantidade de vendas;
- arrecadação;
- quantidade de leads;
- conversão geral;
- conversão por equipe;
- conversão por vendedor;
- metas de vendas;
- metas de arrecadação;
- percentual de atingimento;
- projeção mensal;
- vendas por dia;
- vendas realizadas no dia;
- ranking por pontuação;
- quantidade de vendas por categoria;
- quantidade de vendas por forma de pagamento;
- acompanhamento de dias úteis.

---

## Destaques técnicos

O projeto demonstra experiência prática com:

- integração de APIs REST;
- autenticação em APIs;
- paginação de grandes volumes;
- sincronização incremental;
- tratamento de datas e fusos horários;
- prevenção de registros duplicados;
- transformação de dados com JavaScript;
- consultas SQL com CTEs;
- agregações e filtros condicionais;
- funções e views no PostgreSQL;
- autenticação com senha protegida por hash;
- controle de acesso baseado em perfil;
- dashboards parametrizados;
- integração entre ASP.NET e Metabase;
- APIs em C#;
- aplicações Windows Forms;
- notificações em tempo próximo do real;
- configuração de serviços Linux;
- Docker e proxy reverso com Nginx;
- organização de logs e tratamento de falhas.

---

## Estrutura do repositório

```text
EcossistemaComercial-Portfolio/
├── analytics/
│   └── metabase/
│       ├── docs/
│       ├── queries/
│       │   ├── superior/
│       │   └── vendedor/
│       ├── screenshots/
│       │   ├── superior/
│       │   └── vendedor/
│       └── README.md
│
├── apps/
│   ├── notificador-api/
│   ├── notificador-desktop/
│   └── portal-web/
│
├── automation/
│   └── n8n/
│       ├── workflows/
│       ├── screenshots/
│       └── README.md
│
├── database/
│   └── postgresql/
│       ├── docs/
│       ├── schema/
│       ├── seeds/
│       ├── tests/
│       └── README.md
│
├── docs/
│   ├── architecture/
│   ├── security/
│   ├── setup/
│   └── README.md
│
├── infrastructure/
│   ├── docker/
│   ├── nginx/
│   ├── systemd/
│   └── README.md
│
├── .env.example
├── .gitignore
├── NOTICE.md
└── README.md
```

---

## Dados demonstrativos

O repositório inclui um banco demonstrativo com:

- três equipes fictícias;
- vendedores fictícios;
- supervisores fictícios;
- leads fictícios;
- vendas fictícias;
- metas fictícias;
- relacionamentos compatíveis com as views e queries.

Os arquivos estão disponíveis em:

```text
database/postgresql/schema/01_schema_portfolio.sql
database/postgresql/seeds/02_seed_demo.sql
database/postgresql/tests/03_validacao_demo.sql
```

Nenhum dado do ambiente original está incluído.

---

## Como avaliar o projeto

Para uma leitura rápida, a ordem recomendada é:

1. observar o diagrama de arquitetura deste README;
2. consultar os screenshots dos dashboards;
3. abrir os workflows do n8n;
4. revisar o schema e as views do PostgreSQL;
5. consultar as queries utilizadas no Metabase;
6. observar o fluxo de autenticação do portal;
7. revisar a API e o aplicativo de notificações;
8. consultar os exemplos de infraestrutura.

### Links principais

- [Arquitetura](docs/architecture/README.md)
- [Portal web](apps/portal-web/README.md)
- [Automação n8n](automation/n8n/README.md)
- [PostgreSQL](database/postgresql/README.md)
- [Metabase](analytics/metabase/README.md)
- [API de notificações](apps/notificador-api/README.md)
- [Aplicativo desktop](apps/notificador-desktop/README.md)
- [Infraestrutura](infrastructure/README.md)
- [Segurança e sanitização](docs/security/README.md)
- [Configuração demonstrativa](docs/setup/README.md)

---

## Ambiente demonstrativo

O repositório apresenta uma versão sanitizada do ecossistema.

Os códigos, workflows, queries, schema, seed e exemplos de configuração foram organizados para demonstrar a implementação técnica.

Credenciais e configurações reais não estão incluídas. Alguns serviços externos precisam ser configurados para uma execução completa.

Os dashboards são apresentados por screenshots gerados com dados fictícios.

---

## Segurança e sanitização

Antes da publicação, foram removidos ou substituídos:

- nomes da organização;
- nomes de clientes;
- nomes de vendedores;
- telefones;
- e-mails;
- identificadores reais do CRM;
- tokens;
- senhas;
- chaves de API;
- endereços de servidores;
- domínios;
- credenciais do n8n;
- chave de incorporação do Metabase;
- backups;
- bancos internos;
- históricos de execução.

As configurações de exemplo utilizam valores como:

```text
CHANGE_ME
DEMO_TOKEN
127.0.0.1
api.example.com
```

A documentação completa está em:

[Segurança e sanitização](docs/security/README.md)

---

## Objetivo deste repositório

Este projeto foi organizado como portfólio profissional para apresentar experiência com:

- automação de processos;
- engenharia e tratamento de dados;
- desenvolvimento em C#;
- integração de sistemas;
- banco de dados;
- business intelligence;
- controle de acesso;
- aplicações desktop;
- infraestrutura.

O material não representa uma cópia do ambiente original e não contém informações operacionais da organização em que a solução foi utilizada.

---

## Autor

**João Pedro Martins da Costa**

Desenvolvimento com foco em automação, integração de sistemas, dados, dashboards e aplicações em C#.

---

## Aviso de uso

Este repositório foi publicado para apresentação profissional.

O conteúdo não é distribuído como software de código aberto. Consulte o arquivo [NOTICE.md](NOTICE.md).