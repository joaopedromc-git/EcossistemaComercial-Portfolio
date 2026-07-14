using System.Drawing.Drawing2D;
using System.Globalization;
using NotificadorVendasSoftware.Models;

namespace NotificadorVendasSoftware;

public sealed class PopupVendaForm : Form
{
    private const int LarguraPopup = 620;
    private const int AlturaPopup = 430;
    private const int MargemTela = 24;

    private readonly VendaDto _venda;
    private readonly TemaEquipe _tema;

    private PictureBox? _imagemNegociacao;
    private PictureBox? _imagemLogo;

    public PopupVendaForm(VendaDto venda)
    {
        _venda = venda ??
            throw new ArgumentNullException(nameof(venda));

        _tema = TemaEquipe.Obter(venda.Equipe);

        MontarPopup();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams parametros = base.CreateParams;

            // Adiciona sombra à janela.
            parametros.ClassStyle |= 0x00020000;

            return parametros;
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        PosicionarNoCantoInferiorDireito();

        BringToFront();
        Activate();
    }

    protected override void OnFormClosed(
        FormClosedEventArgs e
    )
    {
        _imagemNegociacao?.Image?.Dispose();
        _imagemLogo?.Image?.Dispose();

        base.OnFormClosed(e);
    }

    private void MontarPopup()
    {
        SuspendLayout();

        Text = "Nova venda";
        Width = LarguraPopup;
        Height = AlturaPopup;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;

        TopMost = true;
        ShowInTaskbar = false;
        BackColor = _tema.Fundo;

        ArredondarFormulario(28);

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _tema.Fundo,
            Padding = Padding.Empty
        };

        Panel topo = CriarCabecalho();
        Panel linhaDivisoria = CriarLinhaDivisoria();
        Panel conteudo = CriarConteudo();

        card.Controls.Add(conteudo);
        card.Controls.Add(linhaDivisoria);
        card.Controls.Add(topo);

        Controls.Add(card);

        ResumeLayout(false);
    }

    private Panel CriarCabecalho()
    {
        var topo = new Panel
        {
            Dock = DockStyle.Top,
            Height = 105,
            BackColor = _tema.Destaque
        };

        Panel painelIcone = CriarPainelIcone();

        var lblTitulo = new Label
        {
            Text = "VENDA REALIZADA",
            Font = new Font(
                "Segoe UI",
                22,
                FontStyle.Bold
            ),
            ForeColor = Color.White,
            AutoSize = false,
            Left = 98,
            Top = 26,
            Width = 310,
            Height = 40
        };

        var lblSubtitulo = new Label
        {
            Text = "Uma nova venda foi registrada.",
            Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Regular
            ),
            ForeColor = _tema.TextoSuave,
            AutoSize = false,
            Left = 100,
            Top = 64,
            Width = 310,
            Height = 26
        };

        _imagemLogo = CriarLogo();

        var btnFecharTopo = new Button
        {
            Text = "×",
            Font = new Font(
                "Segoe UI",
                18,
                FontStyle.Bold
            ),
            ForeColor = Color.White,
            BackColor = _tema.Destaque,
            FlatStyle = FlatStyle.Flat,
            Width = 38,
            Height = 38,
            Left = Width - 46,
            Top = 4,
            Cursor = Cursors.Hand,
            TabStop = false
        };

        btnFecharTopo.FlatAppearance.BorderSize = 0;
        btnFecharTopo.Click += (_, _) => Close();

        topo.Controls.Add(painelIcone);
        topo.Controls.Add(lblTitulo);
        topo.Controls.Add(lblSubtitulo);
        topo.Controls.Add(_imagemLogo);
        topo.Controls.Add(btnFecharTopo);

        btnFecharTopo.BringToFront();

        return topo;
    }

    private Panel CriarPainelIcone()
    {
        var painelIcone = new Panel
        {
            Width = 58,
            Height = 58,
            Left = 24,
            Top = 22,
            BackColor = Color.White
        };

        using var circulo = new GraphicsPath();

        circulo.AddEllipse(
            0,
            0,
            painelIcone.Width,
            painelIcone.Height
        );

        painelIcone.Region = new Region(circulo);

        _imagemNegociacao = new PictureBox
        {
            Width = 50,
            Height = 50,
            Left = 4,
            Top = 4,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        string caminhoImagem = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "deal.png"
        );

        _imagemNegociacao.Image =
            CarregarImagemSegura(caminhoImagem);

        if (_imagemNegociacao.Image is null)
        {
            // O popup continua funcionando mesmo sem a imagem.
            painelIcone.Visible = false;
        }
        else
        {
            painelIcone.Controls.Add(
                _imagemNegociacao
            );
        }

        return painelIcone;
    }

    private PictureBox CriarLogo()
    {
        var logo = new PictureBox
        {
            Width = 125,
            Height = 72,
            Left = Width - 180,
            Top = 17,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        string caminhoLogoEquipe = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            _tema.LogoArquivo
        );

        logo.Image =
            CarregarImagemSegura(
                caminhoLogoEquipe
            );

        if (logo.Image is null)
        {
            string caminhoLogoPadrao = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "logo-padrao.png"
            );

            logo.Image =
                CarregarImagemSegura(
                    caminhoLogoPadrao
                );
        }

        // A ausência da logo não impede o popup de abrir.
        logo.Visible = logo.Image is not null;

        return logo;
    }

    private static Panel CriarLinhaDivisoria()
    {
        return new Panel
        {
            Dock = DockStyle.Top,
            Height = 3,
            BackColor = Color.White
        };
    }

    private Panel CriarConteudo()
    {
        var conteudo = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _tema.Fundo,
            Padding = new Padding(
                34,
                24,
                34,
                24
            )
        };

        string vendedor = TextoOuPadrao(
            _venda.Vendedor,
            "Vendedor não informado"
        );

        string equipe = TextoOuPadrao(
            _venda.Equipe,
            "Equipe não informada"
        );

        string formaPagamento = TextoOuPadrao(
            _venda.FormaPagamento,
            "Não informado"
        );

        string categoria = TextoOuPadrao(
            _venda.TipoPublicacao,
            "Não informada"
        );

        string valorFormatado =
            _venda.Valor.ToString(
                "C",
                CultureInfo.GetCultureInfo(
                    "pt-BR"
                )
            );

        Label lblVendedor = CriarLinha(
            "Vendedor",
            vendedor,
            20
        );

        Label lblEquipe = CriarLinha(
            "Equipe",
            equipe,
            60
        );

        Label lblValor = CriarLinhaDestaque(
            "Valor",
            valorFormatado,
            100
        );

        Label lblPagamento = CriarLinha(
            "Pagamento",
            formaPagamento,
            154
        );

        Label lblCategoria = CriarLinha(
            "Categoria",
            categoria,
            194
        );

        var btnFechar = new Button
        {
            Text = "Fechar",
            Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Bold
            ),
            ForeColor = Color.White,
            BackColor = _tema.Destaque,
            FlatStyle = FlatStyle.Flat,
            Width = 130,
            Height = 42,
            Left = 226,
            Top = 245,
            Cursor = Cursors.Hand
        };

        btnFechar.FlatAppearance.BorderSize = 0;
        btnFechar.Click += (_, _) => Close();

        conteudo.Controls.Add(lblVendedor);
        conteudo.Controls.Add(lblEquipe);
        conteudo.Controls.Add(lblValor);
        conteudo.Controls.Add(lblPagamento);
        conteudo.Controls.Add(lblCategoria);
        conteudo.Controls.Add(btnFechar);

        return conteudo;
    }

    private static Label CriarLinha(
        string titulo,
        string valor,
        int top
    )
    {
        return new Label
        {
            Text = $"{titulo}: {valor}",
            Font = new Font(
                "Segoe UI",
                14,
                FontStyle.Regular
            ),
            ForeColor = Color.White,
            AutoSize = false,
            Left = 34,
            Top = top,
            Width = 520,
            Height = 34,
            AutoEllipsis = true
        };
    }

    private Label CriarLinhaDestaque(
        string titulo,
        string valor,
        int top
    )
    {
        return new Label
        {
            Text = $"{titulo}: {valor}",
            Font = new Font(
                "Segoe UI",
                20,
                FontStyle.Bold
            ),
            ForeColor = _tema.DestaqueClaro,
            AutoSize = false,
            Left = 34,
            Top = top,
            Width = 520,
            Height = 46,
            AutoEllipsis = true
        };
    }

    private static string TextoOuPadrao(
        string? valor,
        string padrao
    )
    {
        return string.IsNullOrWhiteSpace(valor)
            ? padrao
            : valor.Trim();
    }

    private void PosicionarNoCantoInferiorDireito()
    {
        Screen tela = Screen.FromControl(this);

        Rectangle area = tela.WorkingArea;

        Left =
            area.Right -
            Width -
            MargemTela;

        Top =
            area.Bottom -
            Height -
            MargemTela;
    }

    private void ArredondarFormulario(
        int raio
    )
    {
        using var caminho = new GraphicsPath();

        int largura = Width;
        int altura = Height;

        caminho.AddArc(
            0,
            0,
            raio,
            raio,
            180,
            90
        );

        caminho.AddArc(
            largura - raio,
            0,
            raio,
            raio,
            270,
            90
        );

        caminho.AddArc(
            largura - raio,
            altura - raio,
            raio,
            raio,
            0,
            90
        );

        caminho.AddArc(
            0,
            altura - raio,
            raio,
            raio,
            90,
            90
        );

        caminho.CloseFigure();

        Region? regiaoAnterior = Region;

        Region = new Region(caminho);

        regiaoAnterior?.Dispose();
    }

    private static Image? CarregarImagemSegura(
        string caminho
    )
    {
        try
        {
            if (!File.Exists(caminho))
            {
                return null;
            }

            byte[] bytes =
                File.ReadAllBytes(caminho);

            using var memoria =
                new MemoryStream(bytes);

            using Image imagemOriginal =
                Image.FromStream(memoria);

            return new Bitmap(imagemOriginal);
        }
        catch
        {
            // Arquivos ausentes, inválidos ou corrompidos
            // não impedem a exibição da notificação.
            return null;
        }
    }
}