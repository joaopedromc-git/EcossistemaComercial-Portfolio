namespace NotificadorVendasSoftware.Models;

public class UltimaVendaResponse
{
    public bool Ok { get; set; }
    public string Mensagem { get; set; } = "";
    public VendaDto? Venda { get; set; }
}