# Documentação do Projeto

Esta pasta reúne os documentos que explicam a arquitetura, as decisões técnicas, o fluxo de autenticação, as medidas de segurança e a configuração demonstrativa do projeto.

O conteúdo foi organizado para permitir a análise do sistema sem exigir a execução completa do ambiente.

---

## Documentos disponíveis

### Arquitetura

[Arquitetura do ecossistema](architecture/README.md)

Apresenta:

- componentes;
- fluxo de dados;
- responsabilidades;
- integrações;
- sincronização;
- dashboards;
- notificações.

### Fluxo de autenticação

[Autenticação e acesso aos dashboards](architecture/fluxo-autenticacao.md)

Explica:

- validação do código de login;
- comparação da senha;
- classificação do usuário;
- consulta das permissões;
- envio dos identificadores ao Metabase.

### Decisões técnicas

[Decisões técnicas](architecture/decisoes-tecnicas.md)

Apresenta os motivos para utilizar:

- n8n;
- PostgreSQL;
- Metabase;
- ASP.NET;
- Windows Forms;
- API separada;
- sincronização incremental.

### Segurança

[Segurança e sanitização](security/README.md)

Documenta:

- dados removidos;
- dados fictícios;
- gerenciamento de credenciais;
- proteção das senhas;
- arquivos que não fazem parte da publicação.

### Configuração demonstrativa

[Configuração do ambiente](setup/README.md)

Apresenta uma referência para configurar:

- PostgreSQL;
- Metabase;
- Portal;
- API;
- aplicativo desktop;
- workflows do n8n.

---

## Componentes relacionados

- [Portal Web](../apps/portal-web/README.md)
- [API de notificações](../apps/notificador-api/README.md)
- [Aplicativo desktop](../apps/notificador-desktop/README.md)
- [Automação n8n](../automation/n8n/README.md)
- [PostgreSQL](../database/postgresql/README.md)
- [Metabase](../analytics/metabase/README.md)
- [Infraestrutura](../infrastructure/README.md)

---

## Observação

Este repositório apresenta uma versão sanitizada de uma solução aplicada em um ambiente real.

A documentação descreve o funcionamento e as decisões adotadas, mas não inclui credenciais, dados empresariais ou configurações privadas.