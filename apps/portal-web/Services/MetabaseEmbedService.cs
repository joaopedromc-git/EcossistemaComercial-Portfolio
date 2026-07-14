using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using NpgsqlTypes;

namespace PortalVendas.Services;

public sealed class MetabaseEmbedService
{
    private readonly IConfiguration _configuration;
    private readonly NpgsqlDataSource _dataSource;

    public MetabaseEmbedService(
        IConfiguration configuration,
        NpgsqlDataSource dataSource)
    {
        _configuration = configuration;
        _dataSource = dataSource;
    }

    public async Task<string> GerarUrlDashboardAsync(
        ClaimsPrincipal user,
        string modo)
    {
        modo = string.IsNullOrWhiteSpace(modo)
            ? "COMUM"
            : modo.ToUpperInvariant();

        var tipoAcesso =
            user.FindFirst("tipo_acesso")?.Value;

        var usuarioId =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(tipoAcesso)
            || string.IsNullOrWhiteSpace(usuarioId))
        {
            throw new InvalidOperationException(
                "Usuário não autenticado corretamente."
            );
        }

        var siteUrl =
            _configuration["Metabase:SiteUrl"]
            ?? throw new InvalidOperationException(
                "Metabase:SiteUrl não configurado."
            );

        var secretKey =
            _configuration["Metabase:SecretKey"]
            ?? throw new InvalidOperationException(
                "Metabase:SecretKey não configurado."
            );

        var dashboardComumId = int.Parse(
            _configuration["Metabase:DashboardComumId"]
            ?? throw new InvalidOperationException("Metabase:DashboardComumId não configurado.")
        );

        var dashboardEquipeEspecialId = int.Parse(
            _configuration["Metabase:DashboardEquipeEspecialId"]
            ?? throw new InvalidOperationException("Metabase:DashboardEquipeEspecialId não configurado.")
        );

        var dashboardSuperiorId = int.Parse(
            _configuration["Metabase:DashboardSuperiorId"]
            ?? throw new InvalidOperationException("Metabase:DashboardSuperiorId não configurado.")
        );

        Dictionary<string, object> resource;
        Dictionary<string, object> parameters;

        if (modo == "SUPERIOR")
        {
            resource = new Dictionary<string, object>
            {
                ["dashboard"] = dashboardSuperiorId
            };

            parameters = new Dictionary<string, object>();
        }
        else
        {
            var permissoes = await ObterPermissoesAsync(usuarioId, modo);

            var equipeUsuario = user.FindFirst("equipe")?.Value?.Trim();

            var dashboardId = dashboardComumId;

            if (equipeUsuario == "EQUIPE 4")
            {
                dashboardId = dashboardEquipeEspecialId;
            }

            resource = new Dictionary<string, object>
            {
                ["dashboard"] = dashboardId
            };

            parameters = new Dictionary<string, object>
            {
                ["ids_vendas"] = permissoes.IdsVendas,
                ["ids_leads"] = permissoes.IdsLeads
            };
        }

        var payload = new JwtPayload
        {
            { "resource", resource },
            { "params", parameters },
            { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var header = new JwtHeader(credentials);

        var token = new JwtSecurityToken(
            header,
            payload
        );

        var jwt =
            new JwtSecurityTokenHandler().WriteToken(token);

        return
            $"{siteUrl.TrimEnd('/')}/embed/dashboard/{jwt}#theme=night&bordered=false&titled=false";
    }

    private async Task<PermissoesMetabase> ObterPermissoesAsync(
        string usuarioId,
        string modo)
    {
        const string sql = """
            SELECT
                ids_vendas,
                ids_leads
            FROM public.obter_permissoes_metabase(
                @usuario_id,
                @modo
            );
            """;

        await using var command =
            _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "usuario_id",
            NpgsqlDbType.Text,
            usuarioId
        );

        command.Parameters.AddWithValue(
            "modo",
            NpgsqlDbType.Text,
            modo
        );

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return new PermissoesMetabase(
                Array.Empty<string>(),
                Array.Empty<string>()
            );
        }

        var idsVendas =
            reader.IsDBNull(0)
                ? Array.Empty<string>()
                : reader.GetFieldValue<string[]>(0);

        var idsLeads =
            reader.IsDBNull(1)
                ? Array.Empty<string>()
                : reader.GetFieldValue<string[]>(1);

        return new PermissoesMetabase(
            idsVendas,
            idsLeads
        );
    }

    private sealed record PermissoesMetabase(
        string[] IdsVendas,
        string[] IdsLeads
    );
}