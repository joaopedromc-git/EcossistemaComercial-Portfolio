using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Limita o tamanho do corpo das requisições.
// O payload de uma notificação é pequeno, então 64 KB é suficiente.
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 64 * 1024;
});

// Token usado pelo n8n para enviar notificações.
var tokenWebhook = builder.Configuration["WEBHOOK_TOKEN"];

if (string.IsNullOrWhiteSpace(tokenWebhook))
{
    throw new InvalidOperationException(
        "A variável de ambiente WEBHOOK_TOKEN não foi configurada."
    );
}

// Token separado para os aplicativos desktop consultarem a API.
// Não reutilize o WEBHOOK_TOKEN no aplicativo desktop.
var tokenCliente = builder.Configuration["CLIENT_TOKEN"];

if (string.IsNullOrWhiteSpace(tokenCliente))
{
    throw new InvalidOperationException(
        "A variável de ambiente CLIENT_TOKEN não foi configurada."
    );
}

// Evita crescimento ilimitado da lista em memória.
var limiteVendasEmMemoria = 500;

if (
    int.TryParse(
        builder.Configuration["MAX_VENDAS_EM_MEMORIA"],
        out var limiteConfigurado
    )
    && limiteConfigurado > 0
    && limiteConfigurado <= 10_000
)
{
    limiteVendasEmMemoria = limiteConfigurado;
}

// CORS é necessário apenas quando algum frontend executado no navegador
// precisa acessar a API.
//
// Exemplo:
// CORS_ALLOWED_ORIGINS=http://localhost:5001,https://portal.example.com
var origensPermitidas = (
    builder.Configuration["CORS_ALLOWED_ORIGINS"] ?? string.Empty
)
.Split(
    ',',
    StringSplitOptions.RemoveEmptyEntries |
    StringSplitOptions.TrimEntries
);

if (origensPermitidas.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            "CorsConfigurado",
            policy =>
            {
                policy
                    .WithOrigins(origensPermitidas)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }
        );
    });
}

// Permite que a aplicação reconheça corretamente IP e protocolo
// encaminhados pelo Nginx.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Considera somente o proxy imediatamente anterior.
    options.ForwardLimit = 1;
});

var app = builder.Build();

app.UseForwardedHeaders();

if (origensPermitidas.Length > 0)
{
    app.UseCors("CorsConfigurado");
}

// Armazenamento temporário.
//
// As notificações serão perdidas quando a API for reiniciada.
// Isso é aceitável para a versão atual, na qual a API atua apenas
// como intermediária para o popup.
var vendasRecebidas = new List<VendaDto>();
var vendasLock = new object();

long totalVendasRecebidas = 0;

// Rota inicial e verificação simples de funcionamento.
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        ok = true,
        mensagem = "API de notificações comerciais está funcionando.",
        horarioUtc = DateTimeOffset.UtcNow
    });
});

// Rota própria para health check.
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        ok = true,
        status = "healthy",
        horarioUtc = DateTimeOffset.UtcNow
    });
});

// Receber uma nova venda ou notificação de meta.
app.MapPost(
    "/api/venda",
    IResult (HttpRequest request, VendaDto venda) =>
    {
        var tokenRecebido =
            request.Headers["X-Webhook-Token"].ToString();

        if (!TokenValido(tokenRecebido, tokenWebhook))
        {
            app.Logger.LogWarning(
                "Tentativa de envio bloqueada por token inválido."
            );

            return RespostaNaoAutorizada();
        }

        NormalizarVenda(venda);

        var erros = ValidarVenda(venda);

        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        venda.RecebidoEm = DateTimeOffset.UtcNow;

        int armazenadasEmMemoria;

        lock (vendasLock)
        {
            // Impede notificações duplicadas quando o n8n repete
            // uma requisição devido a timeout ou tentativa automática.
            var vendaExistente = vendasRecebidas.FirstOrDefault(
                item => string.Equals(
                    item.IdVenda,
                    venda.IdVenda,
                    StringComparison.Ordinal
                )
            );

            if (vendaExistente is not null)
            {
                return Results.Ok(new
                {
                    ok = true,
                    duplicada = true,
                    mensagem =
                        "A notificação já havia sido recebida anteriormente.",
                    idVenda = venda.IdVenda,
                    recebidoEm = vendaExistente.RecebidoEm,
                    totalVendasRecebidas,
                    armazenadasEmMemoria = vendasRecebidas.Count
                });
            }

            vendasRecebidas.Add(venda);
            totalVendasRecebidas++;

            // Mantém apenas as notificações mais recentes.
            if (vendasRecebidas.Count > limiteVendasEmMemoria)
            {
                var quantidadeParaRemover =
                    vendasRecebidas.Count - limiteVendasEmMemoria;

                vendasRecebidas.RemoveRange(
                    0,
                    quantidadeParaRemover
                );
            }

            armazenadasEmMemoria = vendasRecebidas.Count;
        }

        // Evita registrar mensagem completa, token ou outros dados
        // potencialmente sensíveis nos logs.
        app.Logger.LogInformation(
            "Notificação recebida. ID: {IdVenda}; Equipe: {Equipe}; Valor: {Valor}",
            venda.IdVenda,
            venda.Equipe,
            venda.Valor
        );

        return Results.Ok(new
        {
            ok = true,
            duplicada = false,
            mensagem = "Notificação recebida com sucesso.",
            idVenda = venda.IdVenda,
            vendedor = venda.Vendedor,
            equipe = venda.Equipe,
            valor = venda.Valor,
            recebidoEm = venda.RecebidoEm,
            totalVendasRecebidas,
            armazenadasEmMemoria
        });
    }
);

// Consultar as notificações armazenadas.
// Exige um token diferente do token utilizado pelo n8n.
app.MapGet(
    "/api/vendas",
    IResult (HttpRequest request) =>
    {
        var tokenRecebido =
            request.Headers["X-Client-Token"].ToString();

        if (!TokenValido(tokenRecebido, tokenCliente))
        {
            return RespostaNaoAutorizada();
        }

        VendaDto[] vendas;
        long totalRecebidas;

        lock (vendasLock)
        {
            vendas = vendasRecebidas
                .OrderByDescending(item => item.RecebidoEm)
                .ToArray();

            totalRecebidas = totalVendasRecebidas;
        }

        return Results.Ok(new
        {
            ok = true,
            totalRecebidas,
            armazenadasEmMemoria = vendas.Length,
            vendas
        });
    }
);

// Consultar somente a última notificação.
app.MapGet(
    "/api/vendas/ultima",
    IResult (HttpRequest request) =>
    {
        var tokenRecebido =
            request.Headers["X-Client-Token"].ToString();

        if (!TokenValido(tokenRecebido, tokenCliente))
        {
            return RespostaNaoAutorizada();
        }

        VendaDto? ultimaVenda;

        lock (vendasLock)
        {
            ultimaVenda = vendasRecebidas.LastOrDefault();
        }

        if (ultimaVenda is null)
        {
            return Results.NotFound(new
            {
                ok = false,
                mensagem = "Nenhuma notificação foi recebida ainda."
            });
        }

        return Results.Ok(new
        {
            ok = true,
            venda = ultimaVenda
        });
    }
);

// Não fixe URL, IP ou porta no código.
// Configure pela variável ASPNETCORE_URLS.
//
// Exemplo:
// ASPNETCORE_URLS=http://127.0.0.1:5000
app.Run();

static bool TokenValido(
    string? tokenRecebido,
    string tokenEsperado
)
{
    if (
        string.IsNullOrWhiteSpace(tokenRecebido) ||
        string.IsNullOrWhiteSpace(tokenEsperado)
    )
    {
        return false;
    }

    var bytesRecebidos =
        Encoding.UTF8.GetBytes(tokenRecebido);

    var bytesEsperados =
        Encoding.UTF8.GetBytes(tokenEsperado);

    if (bytesRecebidos.Length != bytesEsperados.Length)
    {
        return false;
    }

    return CryptographicOperations.FixedTimeEquals(
        bytesRecebidos,
        bytesEsperados
    );
}

static IResult RespostaNaoAutorizada()
{
    return Results.Json(
        new
        {
            ok = false,
            mensagem = "Não autorizado."
        },
        statusCode: StatusCodes.Status401Unauthorized
    );
}

static void NormalizarVenda(VendaDto venda)
{
    venda.IdVenda =
        venda.IdVenda?.Trim() ?? string.Empty;

    venda.Vendedor =
        venda.Vendedor?.Trim() ?? string.Empty;

    venda.Equipe =
        venda.Equipe?.Trim() ?? string.Empty;

    venda.FormaPagamento =
        venda.FormaPagamento?.Trim() ?? string.Empty;

    venda.StatusMatricula =
        venda.StatusMatricula?.Trim() ?? string.Empty;

    venda.TipoPublicacao =
        venda.TipoPublicacao?.Trim() ?? string.Empty;

    venda.Mensagem =
        string.IsNullOrWhiteSpace(venda.Mensagem)
            ? null
            : venda.Mensagem.Trim();

    venda.TipoMeta =
        string.IsNullOrWhiteSpace(venda.TipoMeta)
            ? null
            : venda.TipoMeta.Trim();

    // Quando não for uma notificação de meta, limpa os campos
    // específicos para evitar dados inconsistentes.
    if (!venda.BateuMeta)
    {
        venda.TipoMeta = null;
        venda.MetaValor = null;
        venda.ValorAtualMeta = null;
    }
}

static Dictionary<string, string[]> ValidarVenda(
    VendaDto venda
)
{
    var erros = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(venda.IdVenda))
    {
        erros["idVenda"] =
        [
            "O campo idVenda é obrigatório."
        ];
    }

    if (venda.IdVenda.Length > 150)
    {
        erros["idVenda"] =
        [
            "O campo idVenda deve ter no máximo 150 caracteres."
        ];
    }

    if (string.IsNullOrWhiteSpace(venda.Vendedor))
    {
        erros["vendedor"] =
        [
            "O campo vendedor é obrigatório."
        ];
    }

    if (venda.Vendedor.Length > 150)
    {
        erros["vendedor"] =
        [
            "O campo vendedor deve ter no máximo 150 caracteres."
        ];
    }

    if (string.IsNullOrWhiteSpace(venda.Equipe))
    {
        erros["equipe"] =
        [
            "O campo equipe é obrigatório."
        ];
    }

    if (venda.Equipe.Length > 100)
    {
        erros["equipe"] =
        [
            "O campo equipe deve ter no máximo 100 caracteres."
        ];
    }

    if (venda.Valor < 0)
    {
        erros["valor"] =
        [
            "O campo valor não pode ser negativo."
        ];
    }

    if (string.IsNullOrWhiteSpace(venda.FormaPagamento))
    {
        erros["formaPagamento"] =
        [
            "O campo formaPagamento é obrigatório."
        ];
    }

    if (string.IsNullOrWhiteSpace(venda.StatusMatricula))
    {
        erros["statusMatricula"] =
        [
            "O campo statusMatricula é obrigatório."
        ];
    }

    if (string.IsNullOrWhiteSpace(venda.TipoPublicacao))
    {
        erros["tipoPublicacao"] =
        [
            "O campo tipoPublicacao é obrigatório."
        ];
    }

    if (!venda.DataVenda.HasValue)
    {
        erros["dataVenda"] =
        [
            "O campo dataVenda é obrigatório."
        ];
    }

    if (
        venda.BateuMeta &&
        string.IsNullOrWhiteSpace(venda.TipoMeta)
    )
    {
        erros["tipoMeta"] =
        [
            "O campo tipoMeta é obrigatório quando bateuMeta for verdadeiro."
        ];
    }

    return erros;
}

public sealed class VendaDto
{
    public string IdVenda { get; set; } = string.Empty;

    public string Vendedor { get; set; } = string.Empty;

    public string Equipe { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public string FormaPagamento { get; set; } =
        string.Empty;

    public string StatusMatricula { get; set; } =
        string.Empty;

    public string TipoPublicacao { get; set; } =
        string.Empty;

    // DateTimeOffset preserva o fuso enviado pelo n8n.
    public DateTimeOffset? DataVenda { get; set; }

    public string? Mensagem { get; set; }

    // Sempre armazenado em UTC pela própria API.
    public DateTimeOffset? RecebidoEm { get; set; }

    public bool BateuMeta { get; set; }

    // Pode ser nulo quando BateuMeta for false.
    public string? TipoMeta { get; set; }

    // Podem ser nulos em notificações de vendas comuns.
    public decimal? MetaValor { get; set; }

    public decimal? ValorAtualMeta { get; set; }
}