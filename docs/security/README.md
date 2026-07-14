# Segurança e Sanitização

Este repositório contém uma versão preparada para apresentação profissional.

O processo de sanitização foi realizado para demonstrar a implementação técnica sem expor informações da organização original.

---

## Informações removidas

Foram removidos ou substituídos:

- nome da empresa;
- nomes de clientes;
- nomes de vendedores;
- telefones;
- e-mails;
- identificadores reais do CRM;
- endereços internos;
- IPs;
- domínios;
- tokens;
- senhas;
- chaves de API;
- chaves de incorporação;
- credenciais do PostgreSQL;
- credenciais do n8n;
- histórico de execuções;
- dados fixados em nodes;
- backups;
- arquivos compilados;
- logs.

---

## Dados fictícios

O ambiente demonstrativo utiliza:

```text
Equipe Atlas
Equipe Aurora
Equipe Horizonte
```

Também foram criados:

- vendedores fictícios;
- supervisores fictícios;
- leads fictícios;
- vendas fictícias;
- metas fictícias;
- identificadores demonstrativos.

Os relacionamentos foram mantidos para que as views e queries continuem coerentes.

---

## Arquivos de configuração

O repositório publica somente modelos:

```text
.env.example
appsettings.example.json
```

Os modelos utilizam valores como:

```text
CHANGE_ME
127.0.0.1
api.example.com
```

Não devem ser incluídos:

```text
.env
appsettings.json
appsettings.Development.json
appsettings.Production.json
config.local.ps1
credencial-banco.txt
```

---

## Senhas do Portal

As senhas não são armazenadas em texto simples.

O PostgreSQL utiliza:

```text
pgcrypto
```

para gerar e validar o hash.

O fluxo evita devolver o campo `senha_hash` ao Portal ou ao navegador.

---

## Tokens da API

A API utiliza dois tokens:

```text
X-Webhook-Token
X-Client-Token
```

O primeiro permite o envio de eventos.

O segundo permite a consulta das notificações.

Os valores reais devem ser fornecidos por configuração externa.

---

## Credenciais do n8n

Os workflows publicados não devem conter blocos de credenciais.

Após a importação, é necessário cadastrar novamente:

- credencial do CRM;
- credencial PostgreSQL;
- token da API;
- URLs do ambiente.

---

## Metabase

Não fazem parte da publicação:

- banco interno;
- usuário administrador;
- chave de embedding;
- credenciais;
- URL original;
- histórico;
- coleções privadas.

Os dashboards são demonstrados por:

- queries SQL;
- screenshots;
- documentação;
- banco fictício.

---

## PostgreSQL

A publicação não contém:

- dump de produção;
- arquivos do diretório de dados;
- senha administrativa;
- conexão de produção;
- backups.

O Metabase pode utilizar um usuário somente de leitura.

As aplicações devem utilizar usuários específicos, evitando o uso do usuário administrativo.

---

## Aplicações .NET

Não são publicados:

```text
bin/
obj/
.vs/
*.dll
*.exe
*.pdb
*.csproj.user
```

Esses arquivos são gerados durante a compilação e não são necessários para avaliar o código.

---

## Screenshots

As imagens devem ser revisadas antes da publicação.

Não podem exibir:

- barra do navegador com domínio real;
- nome do ambiente;
- usuário logado;
- e-mail;
- nomes reais;
- dados de clientes;
- IDs reais;
- URLs internas.

---

## Scripts

Scripts de instalação e atualização devem utilizar caminhos fictícios.

Exemplo:

```bat
set "ORIGEM=D:\Atualizacao\Notificador"
set "DESTINO=C:\NotificadorPortfolio"
```

Não devem conter:

- compartilhamento de rede real;
- IP local;
- nome de servidor;
- caminho de usuário;
- credencial.

---

## Arquivos protegidos pelo `.gitignore`

O `.gitignore` foi preparado para bloquear:

- configurações locais;
- segredos;
- arquivos compilados;
- bancos internos;
- logs;
- backups;
- certificados;
- chaves privadas;
- dados do n8n;
- dados do Metabase.

Como o projeto será transferido por ZIP antes da publicação, os arquivos também serão verificados fisicamente antes da compactação.

---

## Limitações

A sanitização reduz a possibilidade de executar o ambiente imediatamente sem configuração adicional.

Essa escolha é intencional.

O objetivo do repositório é demonstrar:

- arquitetura;
- organização;
- código;
- regras de negócio;
- SQL;
- integrações;
- decisões técnicas.

---

## Checklist de publicação

Antes de criar o ZIP:

- [ ] remover `bin` e `obj`;
- [ ] remover `appsettings.json`;
- [ ] remover arquivos `.env`;
- [ ] revisar workflows;
- [ ] revisar screenshots;
- [ ] revisar scripts;
- [ ] procurar IPs e domínios;
- [ ] procurar tokens;
- [ ] procurar e-mails;
- [ ] procurar nomes reais;
- [ ] remover backups;
- [ ] gerar uma árvore final;
- [ ] abrir o ZIP e conferir seu conteúdo.