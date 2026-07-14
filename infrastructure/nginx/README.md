# Nginx

O Nginx funciona como proxy reverso dos serviços do ecossistema.

Ele recebe as solicitações externas e encaminha cada subdomínio para a porta interna correspondente.

---

## Roteamento demonstrativo

| Subdomínio | Serviço interno |
|---|---|
| `portal.example.com` | `127.0.0.1:5001` |
| `api.example.com` | `127.0.0.1:5000` |
| `n8n.example.com` | `127.0.0.1:5678` |
| `dashboard.example.com` | `127.0.0.1:3000` |

Os endereços são fictícios.

---

## Arquivo de exemplo

```text
ecossistema.conf.example
```

Antes do uso, copie para o diretório de configuração do servidor.

Exemplo conceitual:

```bash
sudo cp ecossistema.conf.example \
  /etc/nginx/sites-available/ecossistema.conf
```

Crie o link:

```bash
sudo ln -s \
  /etc/nginx/sites-available/ecossistema.conf \
  /etc/nginx/sites-enabled/ecossistema.conf
```

---

## Validação

Antes de recarregar o serviço:

```bash
sudo nginx -t
```

Somente após uma validação bem-sucedida:

```bash
sudo systemctl reload nginx
```

---

## HTTPS

O arquivo de exemplo contém somente a estrutura HTTP inicial.

Em uma implantação pública, devem ser adicionados:

- certificados TLS;
- redirecionamento de HTTP para HTTPS;
- renovação automática;
- cabeçalhos de segurança adequados.

As chaves privadas e certificados não devem ser publicados.

---

## WebSocket e n8n

O bloco do n8n inclui headers de atualização de conexão para permitir funcionalidades que dependem de comunicação persistente.

Também utiliza tempos de leitura maiores para execuções demoradas.

---

## Tamanho de requisição

O n8n pode processar arquivos e payloads maiores.

O exemplo utiliza:

```nginx
client_max_body_size 50m;
```

Esse valor deve ser ajustado conforme a necessidade do ambiente.

---

## Logs

O Nginx normalmente registra:

- acessos;
- erros;
- falhas de proxy;
- solicitações inválidas.

Os logs reais não fazem parte deste repositório.

---

## Segurança

Antes de utilizar a configuração:

- substitua os domínios;
- ative HTTPS;
- configure firewall;
- não exponha portas internas;
- restrinja o acesso administrativo ao n8n e Metabase;
- revise os limites de requisição;
- proteja endpoints sensíveis;
- mantenha os serviços atualizados.