namespace PortalVendas.Models;

public sealed class UsuarioPortal
{
    public required string Id { get; init; }

    public string? IdLeads { get; init; }

    public bool PrimeiroAcesso { get; init; }

    public required string NomeVendedor { get; init; }

    public required string CodigoLogin { get; init; }

    public string? Equipe { get; init; }

    public required string TipoAcesso { get; init; }

    public bool AcessoSuperior { get; init; }
}