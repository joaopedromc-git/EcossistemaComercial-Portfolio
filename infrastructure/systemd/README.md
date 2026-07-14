# Serviços systemd

Esta pasta apresenta exemplos de unidades `systemd` para executar a API e o Portal ASP.NET em um servidor Linux.

Os arquivos demonstram como manter as aplicações em execução sem incluí-las nos containers do exemplo.

---

## Serviços

```text
notificador-api.service.example
portal-web.service.example
```

---

## Estrutura demonstrativa no servidor

```text
/opt/ecossistema-portfolio/
└── apps/
    ├── notificador-api/
    │   └── NotificadorVendasApi.dll
    │
    └── portal-web/
        └── PortalVendas.dll
```

Arquivos de ambiente:

```text
/etc/ecossistema-portfolio/notificador-api.env
/etc/ecossistema-portfolio/portal-web.env
```

Os arquivos `.env` reais não fazem parte do repositório.

---

## Instalação conceitual

Copie a unidade:

```bash
sudo cp notificador-api.service.example \
  /etc/systemd/system/notificador-api.service
```

Recarregue as unidades:

```bash
sudo systemctl daemon-reload
```

Ative e inicie:

```bash
sudo systemctl enable --now notificador-api
```

---

## Verificação

Status:

```bash
sudo systemctl status notificador-api
```

Logs:

```bash
sudo journalctl -u notificador-api -f
```

O mesmo processo pode ser utilizado para o Portal.

---

## Portas

| Aplicação | Porta local |
|---|---:|
| API | 5000 |
| Portal | 5001 |

As aplicações escutam somente em:

```text
127.0.0.1
```

O acesso externo ocorre pelo Nginx.

---

## Publicação das aplicações

Os binários podem ser gerados com:

```bash
dotnet publish -c Release
```

A pasta publicada deve ser transferida para o diretório configurado na unidade.

Os arquivos compilados não são mantidos no repositório de portfólio.

---

## Segurança

Os exemplos utilizam:

- usuário `www-data`;
- configuração externa;
- reinício automático;
- diretório temporário privado;
- bloqueio de novos privilégios;
- portas locais.

Em um ambiente real, o usuário e as permissões devem ser ajustados conforme a política do servidor.