# Decisões Técnicas

Este documento registra as principais decisões adotadas durante o desenvolvimento do ecossistema.

As escolhas foram orientadas pelas necessidades de integração, manutenção, custo e velocidade de implementação.

---

## n8n para as automações

O n8n foi utilizado para organizar a integração entre CRM, PostgreSQL e API.

### Motivos

- criação visual dos fluxos;
- facilidade para testar etapas;
- integração com HTTP e PostgreSQL;
- suporte a agendamentos;
- tratamento de ramificações;
- manutenção rápida das regras;
- visualização do histórico de execução.

### Aplicação no projeto

O n8n ficou responsável por:

- consulta ao CRM;
- paginação;
- transformação;
- atualização do banco;
- controle incremental;
- verificação das notificações;
- envio dos eventos.

---

## PostgreSQL como base central

O PostgreSQL substituiu controles que estavam distribuídos em arquivos e consultas manuais.

### Motivos

- integridade dos dados;
- suporte a transações;
- funções;
- views;
- constraints;
- índices;
- capacidade de agregação;
- integração com Metabase, n8n e C#.

### Aplicação no projeto

O banco concentra:

- dados operacionais;
- indicadores;
- permissões;
- autenticação;
- controles de duplicidade;
- calendário;
- sincronização.

---

## Views para os indicadores

As regras principais foram organizadas em views.

### Motivos

- evitar duplicação de SQL;
- manter nomes estáveis para o Metabase;
- centralizar regras de associação;
- facilitar manutenção;
- separar dados operacionais de dados analíticos.

### Exemplos

```text
vendas_mes
leads_mes
vw_kpi_consultor
vw_kpi_geral
kpi_equipes
ranking_mensal
```

---

## Sincronização incremental

Os workflows não fazem uma carga completa a cada execução.

### Motivos

- reduzir chamadas à API;
- reduzir tempo de execução;
- diminuir o volume processado;
- permitir atualizações frequentes;
- facilitar recuperação após falhas.

### Implementação

A tabela:

```text
sync_control
```

registra o horário da última execução concluída.

---

## Metabase para Business Intelligence

O Metabase foi utilizado como camada de visualização.

### Motivos

- conexão direta com PostgreSQL;
- criação rápida de dashboards;
- suporte a SQL;
- filtros;
- incorporação em aplicações;
- boa relação entre recursos e custo.

### Aplicação no projeto

Foram criadas:

- visão superior;
- visão do vendedor;
- queries parametrizadas;
- tabelas;
- indicadores;
- rankings;
- gráficos de evolução.

---

## Portal ASP.NET para controle de acesso

O Portal foi criado para evitar o acesso direto ao ambiente de BI.

### Motivos

- autenticação própria;
- controle de sessão;
- definição de perfil;
- seleção do dashboard;
- consulta de IDs permitidos;
- incorporação do Metabase.

### Benefício

O usuário acessa somente as informações autorizadas para seu perfil.

---

## Validação de senha no PostgreSQL

A comparação da senha ocorre no banco.

### Motivos

- evitar transportar o hash;
- concentrar a regra de autenticação;
- utilizar `pgcrypto`;
- impedir a exposição da senha armazenada.

O Portal recebe somente os dados necessários para a sessão.

---

## API separada para notificações

A API de notificações foi mantida separada do Portal.

### Motivos

- independência entre os componentes;
- comunicação simples com o n8n;
- possibilidade de mais clientes;
- implantação separada;
- menor acoplamento.

---

## Aplicativo Windows Forms

O aplicativo desktop foi utilizado para exibir os avisos.

### Motivos

- funcionamento em computadores Windows;
- aviso sem página aberta;
- controle visual;
- reprodução de som;
- possibilidade de execução minimizada;
- distribuição simples no ambiente utilizado.

---

## Consulta periódica da API

O aplicativo consulta o último evento em intervalos definidos.

### Motivos

- implementação simples;
- compatibilidade com a infraestrutura;
- baixo volume de usuários;
- facilidade de diagnóstico;
- ausência de dependência de WebSocket.

Em cenários com maior escala, essa comunicação poderia ser substituída por tecnologias de entrega em tempo real.

---

## Controle de duplicidade

O projeto utiliza diferentes controles para evitar repetições.

| Situação | Controle |
|---|---|
| Registro de venda | Identificador único |
| Registro de lead | Identificador único |
| Notificação de venda | `vendas_notificadas` |
| Notificação de meta | `metas_notificadas` |
| Período sincronizado | `sync_control` |
| Login | Índice único |

---

## Configuração externa

Tokens, URLs e senhas não ficam fixos no código.

Os valores são fornecidos por:

- variáveis de ambiente;
- arquivos locais não publicados;
- credenciais do n8n;
- User Secrets;
- configurações da infraestrutura.

---

## Separação em componentes

A arquitetura permite alterar um componente sem reescrever toda a solução.

Exemplos:

- o dashboard pode mudar sem alterar o desktop;
- a API pode mudar sem alterar as queries;
- as regras do n8n podem mudar sem alterar o Portal;
- novas views podem ser adicionadas sem mudar a origem;
- novos clientes podem consultar a API.

---

## Possíveis evoluções

Entre as evoluções possíveis estão:

- filas de mensagens;
- WebSocket ou SignalR;
- testes automatizados adicionais;
- observabilidade centralizada;
- cache para dashboards;
- deploy automatizado;
- gestão centralizada de segredos;
- containers para todos os componentes;
- monitoramento de disponibilidade.