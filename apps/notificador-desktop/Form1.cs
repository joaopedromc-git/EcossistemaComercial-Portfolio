using System.Globalization;
using System.Media;
using System.Net;
using System.Text.Json;
using NotificadorVendasSoftware.Models;

namespace NotificadorVendasSoftware;

public partial class Form1 : Form
{
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _ultimaVendaPath;
    private readonly int _intervaloSegundos;

    private readonly System.Windows.Forms.Timer _timerVerificacao = new();

    private string? _ultimoIdVendaExibido;
    private bool _verificacaoEmAndamento;
    private bool _sairDefinitivo;
    private bool _recursosLiberados;

    private readonly string _pastaEstado;
    private readonly string _arquivoEstado;
    private readonly bool _iniciarMinimizado;

    private Label lblTitulo = null!;
    private Label lblStatus = null!;
    private Label lblApi = null!;
    private Label lblUltimaVenda = null!;
    private Label lblUltimaVerificacao = null!;

    private Button btnTestarConexao = null!;
    private Button btnIniciar = null!;
    private Button btnParar = null!;

    private NotifyIcon notifyIcon = null!;
    private ContextMenuStrip menuBandeja = null!;

    public Form1(bool iniciarMinimizado = false)
    {
        _iniciarMinimizado = iniciarMinimizado;

        InitializeComponent();

        var configuracao =
            ConfiguracaoAplicativo.Carregar();

        _apiBaseUrl =
            configuracao.Api.BaseUrl.TrimEnd('/');

        _ultimaVendaPath =
            configuracao.Api.UltimaVendaPath.StartsWith('/')
                ? configuracao.Api.UltimaVendaPath
                : $"/{configuracao.Api.UltimaVendaPath}";

        _intervaloSegundos =
            Math.Clamp(
                configuracao.Api.IntervaloSegundos,
                1,
                3600
            );

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"{_apiBaseUrl}/"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        bool cabecalhoAdicionado =
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "X-Client-Token",
                configuracao.Api.ClientToken
            );

        if (!cabecalhoAdicionado)
        {
            throw new InvalidOperationException(
                "Não foi possível configurar o token de acesso da API."
            );
        }

        _pastaEstado = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData
            ),
            "NotificadorComercial"
        );

        _arquivoEstado = Path.Combine(
            _pastaEstado,
            "estado-notificador.json"
        );

        CarregarEstado();
        MontarTela();
        ConfigurarTimer();
        ConfigurarBandeja();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (!_iniciarMinimizado)
        {
            return;
        }

        await IniciarVerificacao();

        Hide();
        ShowInTaskbar = false;

        notifyIcon.ShowBalloonTip(
            2000,
            "Notificador Comercial",
            "O notificador foi iniciado e está ativo.",
            ToolTipIcon.Info
        );
    }

    private void MontarTela()
    {
        SuspendLayout();

        Text = "Notificador Comercial";
        Width = 560;
        Height = 370;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        lblTitulo = new Label
        {
            Text = "🔔 Notificador Comercial",
            Font = new Font(
                "Segoe UI",
                18,
                FontStyle.Bold
            ),
            AutoSize = true,
            Left = 30,
            Top = 25
        };

        lblStatus = new Label
        {
            Text = "Status: Parado",
            Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Regular
            ),
            AutoSize = true,
            Left = 30,
            Top = 85
        };

        lblApi = new Label
        {
            Text = $"API: {_apiBaseUrl}",
            Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Regular
            ),
            AutoSize = true,
            Left = 30,
            Top = 120
        };

        lblUltimaVenda = new Label
        {
            Text = string.IsNullOrWhiteSpace(
                _ultimoIdVendaExibido
            )
                ? "Última notificação: Nenhuma"
                : $"Último ID já exibido: {_ultimoIdVendaExibido}",
            Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Regular
            ),
            AutoSize = true,
            Left = 30,
            Top = 155
        };

        lblUltimaVerificacao = new Label
        {
            Text = "Última verificação: -",
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Regular
            ),
            AutoSize = true,
            Left = 30,
            Top = 190
        };

        btnTestarConexao = new Button
        {
            Text = "Testar conexão",
            Width = 140,
            Height = 40,
            Left = 30,
            Top = 250
        };

        btnTestarConexao.Click +=
            BtnTestarConexao_Click;

        btnIniciar = new Button
        {
            Text = "Iniciar",
            Width = 100,
            Height = 40,
            Left = 190,
            Top = 250
        };

        btnIniciar.Click += BtnIniciar_Click;

        btnParar = new Button
        {
            Text = "Parar",
            Width = 100,
            Height = 40,
            Left = 310,
            Top = 250,
            Enabled = false
        };

        btnParar.Click += BtnParar_Click;

        Controls.Add(lblTitulo);
        Controls.Add(lblStatus);
        Controls.Add(lblApi);
        Controls.Add(lblUltimaVenda);
        Controls.Add(lblUltimaVerificacao);
        Controls.Add(btnTestarConexao);
        Controls.Add(btnIniciar);
        Controls.Add(btnParar);

        ResumeLayout(false);
        PerformLayout();
    }

    private void ConfigurarTimer()
    {
        _timerVerificacao.Interval =
            _intervaloSegundos * 1000;

        _timerVerificacao.Tick += async (_, _) =>
        {
            await VerificarVendaAutomaticamente();
        };
    }

    private void ConfigurarBandeja()
    {
        menuBandeja = new ContextMenuStrip();

        var itemAbrir =
            new ToolStripMenuItem("Abrir");

        itemAbrir.Click += (_, _) =>
            MostrarJanela();

        var itemIniciar =
            new ToolStripMenuItem("Iniciar");

        itemIniciar.Click += async (_, _) =>
            await IniciarVerificacao();

        var itemParar =
            new ToolStripMenuItem("Parar");

        itemParar.Click += (_, _) =>
            PararVerificacao();

        var itemSair =
            new ToolStripMenuItem("Sair");

        itemSair.Click += (_, _) =>
            SairDoPrograma();

        menuBandeja.Items.Add(itemAbrir);
        menuBandeja.Items.Add(
            new ToolStripSeparator()
        );
        menuBandeja.Items.Add(itemIniciar);
        menuBandeja.Items.Add(itemParar);
        menuBandeja.Items.Add(
            new ToolStripSeparator()
        );
        menuBandeja.Items.Add(itemSair);

        notifyIcon = new NotifyIcon
        {
            Text = "Notificador Comercial",
            Icon = SystemIcons.Application,
            ContextMenuStrip = menuBandeja,
            Visible = true
        };

        notifyIcon.DoubleClick += (_, _) =>
            MostrarJanela();
    }

    private async void BtnTestarConexao_Click(
        object? sender,
        EventArgs e
    )
    {
        await TestarConexaoComApi();
    }

    private async void BtnIniciar_Click(
        object? sender,
        EventArgs e
    )
    {
        await IniciarVerificacao();
    }

    private void BtnParar_Click(
        object? sender,
        EventArgs e
    )
    {
        PararVerificacao();
    }

    private async Task IniciarVerificacao()
    {
        lblStatus.Text = "Status: Ativo";
        btnIniciar.Enabled = false;
        btnParar.Enabled = true;

        if (!_timerVerificacao.Enabled)
        {
            _timerVerificacao.Start();
        }

        await VerificarVendaAutomaticamente();
    }

    private void PararVerificacao()
    {
        _timerVerificacao.Stop();

        lblStatus.Text = "Status: Parado";
        btnIniciar.Enabled = true;
        btnParar.Enabled = false;
    }

    private void MostrarJanela()
    {
        Show();
        WindowState = FormWindowState.Normal;
        ShowInTaskbar = true;
        Activate();
    }

    private void EsconderNaBandeja()
    {
        Hide();
        ShowInTaskbar = false;

        notifyIcon.ShowBalloonTip(
            2000,
            "Notificador Comercial",
            "O programa continua ativo na área de notificação.",
            ToolTipIcon.Info
        );
    }

    private void SairDoPrograma()
    {
        _sairDefinitivo = true;

        Application.Exit();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (
            WindowState ==
            FormWindowState.Minimized
        )
        {
            EsconderNaBandeja();
        }
    }

    protected override void OnFormClosing(
        FormClosingEventArgs e
    )
    {
        if (!_sairDefinitivo)
        {
            e.Cancel = true;
            EsconderNaBandeja();
            return;
        }

        LiberarRecursos();

        base.OnFormClosing(e);
    }

    private async Task TestarConexaoComApi()
    {
        try
        {
            btnTestarConexao.Enabled = false;
            btnTestarConexao.Text = "Testando...";

            VendaDto? venda =
                await BuscarUltimaVenda();

            if (venda is null)
            {
                MessageBox.Show(
                    "A API respondeu corretamente, mas nenhuma notificação foi recebida ainda.",
                    "Conexão funcionando",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                lblUltimaVenda.Text =
                    "Última notificação: Nenhuma";

                return;
            }

            string valorFormatado =
                FormatarValor(venda.Valor);

            lblUltimaVenda.Text =
                $"Última notificação: {venda.Vendedor} - {valorFormatado}";

            MessageBox.Show(
                $"Conexão funcionando!\n\n" +
                $"Vendedor: {venda.Vendedor}\n" +
                $"Equipe: {venda.Equipe}\n" +
                $"Valor: {valorFormatado}\n" +
                $"Pagamento: {venda.FormaPagamento}\n" +
                $"Categoria: {venda.TipoPublicacao}",
                "Notificação encontrada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (TaskCanceledException)
        {
            MessageBox.Show(
                "A API demorou muito para responder.",
                "Tempo limite excedido",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        catch (HttpRequestException)
        {
            MessageBox.Show(
                $"Não foi possível conectar à API.\n\n" +
                $"Verifique se ela está rodando em {_apiBaseUrl}.",
                "Erro de conexão",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        catch (InvalidOperationException erro)
        {
            MessageBox.Show(
                erro.Message,
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        catch (JsonException)
        {
            MessageBox.Show(
                "A API respondeu, mas o conteúdo recebido não está no formato esperado.",
                "Resposta inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        catch (Exception erro)
        {
            MessageBox.Show(
                $"Ocorreu um erro inesperado:\n{erro.Message}",
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            btnTestarConexao.Enabled = true;
            btnTestarConexao.Text = "Testar conexão";
        }
    }

    private async Task VerificarVendaAutomaticamente()
{
    if (_verificacaoEmAndamento)
    {
        return;
    }

    try
    {
        _verificacaoEmAndamento = true;

        lblUltimaVerificacao.Text =
            $"Última verificação: {DateTime.Now:HH:mm:ss}";

        VendaDto? venda =
            await BuscarUltimaVenda();

        lblStatus.Text = "Status: Ativo";

        if (venda is null)
        {
            return;
        }

        string valorFormatado =
            FormatarValor(venda.Valor);

        lblUltimaVenda.Text =
            $"Última notificação: {venda.Vendedor} - {valorFormatado}";

        if (
            string.Equals(
                venda.IdVenda,
                _ultimoIdVendaExibido,
                StringComparison.Ordinal
            )
        )
        {
            return;
        }

        bool popupExibido =
            MostrarAvisoVenda(venda);

        if (!popupExibido)
        {
            lblStatus.Text =
                "Status: Venda recebida, mas o popup não foi exibido";

            return;
        }

        _ultimoIdVendaExibido =
            venda.IdVenda;

        SalvarEstado();
    }
    catch (Exception erro)
    {
        lblStatus.Text =
            $"Status: Erro — {erro.Message}";

        RegistrarErroGeral(erro);
    }
    finally
    {
        _verificacaoEmAndamento = false;
    }
}

    private async Task<VendaDto?> BuscarUltimaVenda()
    {
        using HttpResponseMessage resposta =
            await _httpClient.GetAsync(
                _ultimaVendaPath
            );

        if (
            resposta.StatusCode ==
            HttpStatusCode.NotFound
        )
        {
            return null;
        }

        if (
            resposta.StatusCode ==
            HttpStatusCode.Unauthorized
        )
        {
            throw new InvalidOperationException(
                "A API recusou o acesso. Verifique o ClientToken configurado."
            );
        }

        resposta.EnsureSuccessStatusCode();

        await using Stream conteudo =
            await resposta.Content.ReadAsStreamAsync();

        UltimaVendaResponse? dados =
            await JsonSerializer.DeserializeAsync<UltimaVendaResponse>(
                conteudo,
                OpcoesJson
            );

        return dados?.Venda;
    }

    private bool MostrarAvisoVenda(
    VendaDto venda
)
{
    try
    {
        TocarSomAlerta();

        Form popup;

        if (venda.BateuMeta)
        {
            popup = new MetaBatidaForm(venda);
        }
        else
        {
            popup = new PopupVendaForm(venda);
        }

        popup.Show();
        popup.BringToFront();
        popup.Activate();

        return true;
    }
    catch (Exception erro)
    {
        RegistrarErroPopup(
            venda,
            erro
        );

        MessageBox.Show(
            $"A notificação foi recebida, mas o popup não abriu.\n\n" +
            $"Tipo: {(venda.BateuMeta ? "Meta" : "Venda")}\n" +
            $"Erro: {erro.Message}",
            "Erro ao abrir popup",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );

        return false;
    }
}

    private static void TocarSomAlerta()
    {
        try
        {
            string caminhoSom = Path.Combine(
                AppContext.BaseDirectory,
                "Sounds",
                "alerta-venda.wav"
            );

            if (!File.Exists(caminhoSom))
            {
                SystemSounds.Asterisk.Play();
                return;
            }

            using var player =
                new SoundPlayer(caminhoSom);

            player.Load();
            player.Play();
        }
        catch
        {
            SystemSounds.Asterisk.Play();
        }
    }

    private static void RegistrarErroPopup(
    VendaDto venda,
    Exception erro
)
{
    try
    {
        string pastaLogs = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "NotificadorComercial",
            "Logs"
        );

        Directory.CreateDirectory(
            pastaLogs
        );

        string arquivoLog = Path.Combine(
            pastaLogs,
            "erros-popup.log"
        );

        string conteudo =
            $"[{DateTimeOffset.Now:O}]" +
            Environment.NewLine +
            $"ID: {venda.IdVenda}" +
            Environment.NewLine +
            $"Vendedor: {venda.Vendedor}" +
            Environment.NewLine +
            $"Equipe: {venda.Equipe}" +
            Environment.NewLine +
            $"BateuMeta: {venda.BateuMeta}" +
            Environment.NewLine +
            $"Erro: {erro}" +
            Environment.NewLine +
            new string('-', 70) +
            Environment.NewLine;

        File.AppendAllText(
            arquivoLog,
            conteudo
        );
    }
    catch
    {
        // Falhas no log não interrompem o aplicativo.
    }
}

private static void RegistrarErroGeral(
    Exception erro
)
{
    try
    {
        string pastaLogs = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "NotificadorComercial",
            "Logs"
        );

        Directory.CreateDirectory(
            pastaLogs
        );

        string arquivoLog = Path.Combine(
            pastaLogs,
            "erros-gerais.log"
        );

        File.AppendAllText(
            arquivoLog,
            $"[{DateTimeOffset.Now:O}]{Environment.NewLine}" +
            $"{erro}{Environment.NewLine}" +
            $"{new string('-', 70)}{Environment.NewLine}"
        );
    }
    catch
    {
        // Falhas no log não interrompem o aplicativo.
    }
}

    private void CarregarEstado()
    {
        try
        {
            if (!File.Exists(_arquivoEstado))
            {
                return;
            }

            string json =
                File.ReadAllText(
                    _arquivoEstado
                );

            EstadoNotificador? estado =
                JsonSerializer.Deserialize<EstadoNotificador>(
                    json,
                    OpcoesJson
                );

            if (estado is null)
            {
                return;
            }

            _ultimoIdVendaExibido =
                estado.UltimoIdVendaExibido;
        }
        catch
        {
            _ultimoIdVendaExibido = null;
        }
    }

    private void SalvarEstado()
    {
        try
        {
            Directory.CreateDirectory(
                _pastaEstado
            );

            var estado = new EstadoNotificador
            {
                UltimoIdVendaExibido =
                    _ultimoIdVendaExibido
                    ?? string.Empty,

                UltimaAtualizacao =
                    DateTime.Now
            };

            string json =
                JsonSerializer.Serialize(
                    estado,
                    OpcoesJson
                );

            File.WriteAllText(
                _arquivoEstado,
                json
            );
        }
        catch
        {
            // Uma falha ao salvar o estado não impede
            // o funcionamento principal do aplicativo.
        }
    }

    private static string FormatarValor(decimal valor)
    {
        return valor.ToString(
            "C",
            CultureInfo.GetCultureInfo("pt-BR")
        );
    }

    private void LiberarRecursos()
    {
        if (_recursosLiberados)
        {
            return;
        }

        _recursosLiberados = true;

        _timerVerificacao.Stop();
        _timerVerificacao.Dispose();

        notifyIcon.Visible = false;
        notifyIcon.Dispose();

        menuBandeja.Dispose();
        _httpClient.Dispose();
    }
}