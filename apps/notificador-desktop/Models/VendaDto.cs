namespace NotificadorVendasSoftware.Models;

public sealed class VendaDto
{
    public string IdVenda { get; set; } =
        string.Empty;

    public string Vendedor { get; set; } =
        string.Empty;

    public string Equipe { get; set; } =
        string.Empty;

    public decimal Valor { get; set; }

    public string FormaPagamento { get; set; } =
        string.Empty;

    public string StatusMatricula { get; set; } =
        string.Empty;

    public string TipoPublicacao { get; set; } =
        string.Empty;

    public DateTimeOffset? DataVenda { get; set; }

    public string? Mensagem { get; set; }

    public DateTimeOffset? RecebidoEm { get; set; }

    public bool BateuMeta { get; set; }

    public string? TipoMeta { get; set; }

    public decimal? MetaValor { get; set; }

    public decimal? ValorAtualMeta { get; set; }
}