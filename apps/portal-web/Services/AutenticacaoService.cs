using Npgsql;
using NpgsqlTypes;
using PortalVendas.Models;

namespace PortalVendas.Services;

public sealed class AutenticacaoService
{
    private readonly NpgsqlDataSource _dataSource;

    public AutenticacaoService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<UsuarioPortal?> AutenticarAsync(string codigoLogin, string senha)
    {
        const string sql =
            "SELECT " +
            "id, " +
            "id_leads, " +
            "nome_vendedor, " +
            "codigo_login, " +
            "equipe, " +
            "tipo_acesso, " +
            "acesso_superior, " +
            "primeiro_acesso " +
            "FROM public.autenticar_usuario_portal(@codigo_login, @senha);";

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "codigo_login",
            NpgsqlDbType.Text,
            codigoLogin.Trim()
        );

        command.Parameters.AddWithValue(
            "senha",
            NpgsqlDbType.Text,
            senha
        );

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new UsuarioPortal
        {
            Id = reader.GetString(0),

            IdLeads = reader.IsDBNull(1)
                ? null
                : reader.GetString(1),

            NomeVendedor = reader.GetString(2),
            CodigoLogin = reader.GetString(3),

            Equipe = reader.IsDBNull(4)
                ? null
                : reader.GetString(4),

            TipoAcesso = reader.GetString(5),
            AcessoSuperior = reader.GetBoolean(6),
            PrimeiroAcesso = reader.GetBoolean(7)
        };
    }

    public async Task<bool> AlterarSenhaPrimeiroAcessoAsync(
        string idUsuario,
        string novaSenha)
    {
        const string sql =
            "SELECT public.alterar_senha_primeiro_acesso(@id_usuario, @nova_senha);";

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "id_usuario",
            NpgsqlDbType.Text,
            idUsuario
        );

        command.Parameters.AddWithValue(
            "nova_senha",
            NpgsqlDbType.Text,
            novaSenha
        );

        var result = await command.ExecuteScalarAsync();

        return result is bool sucesso && sucesso;
    }
}