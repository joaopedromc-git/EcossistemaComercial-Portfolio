using System.Text.Json;

public sealed class ConfiguracaoAplicativo
{
    public ConfiguracaoApi Api { get; set; } = new();

    public static ConfiguracaoAplicativo Carregar()
    {
        var caminhoArquivo = Path.Combine(
            AppContext.BaseDirectory,
            "appsettings.json"
        );

        if (!File.Exists(caminhoArquivo))
        {
            throw new InvalidOperationException(
                $"O arquivo de configuração não foi encontrado: {caminhoArquivo}"
            );
        }

        var json = File.ReadAllText(caminhoArquivo);

        var configuracao =
            JsonSerializer.Deserialize<ConfiguracaoAplicativo>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

        if (configuracao is null)
        {
            throw new InvalidOperationException(
                "Não foi possível carregar o appsettings.json."
            );
        }

        configuracao.Validar();

        return configuracao;
    }

    private void Validar()
    {
        if (string.IsNullOrWhiteSpace(Api.BaseUrl))
        {
            throw new InvalidOperationException(
                "Api:BaseUrl não foi configurada."
            );
        }

        if (!Uri.TryCreate(
                Api.BaseUrl,
                UriKind.Absolute,
                out _))
        {
            throw new InvalidOperationException(
                "Api:BaseUrl não contém uma URL válida."
            );
        }

        if (string.IsNullOrWhiteSpace(Api.UltimaVendaPath))
        {
            throw new InvalidOperationException(
                "Api:UltimaVendaPath não foi configurado."
            );
        }

        if (string.IsNullOrWhiteSpace(Api.ClientToken))
        {
            throw new InvalidOperationException(
                "Api:ClientToken não foi configurado."
            );
        }

        if (Api.IntervaloSegundos < 1)
        {
            throw new InvalidOperationException(
                "Api:IntervaloSegundos deve ser maior que zero."
            );
        }
    }
}

public sealed class ConfiguracaoApi
{
    public string BaseUrl { get; set; } = string.Empty;

    public string UltimaVendaPath { get; set; } =
        "/api/vendas/ultima";

    public string ClientToken { get; set; } =
        string.Empty;

    public int IntervaloSegundos { get; set; } = 5;
}