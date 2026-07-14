using System.Drawing.Drawing2D;
using System.Globalization;
using NotificadorVendasSoftware.Models;

namespace NotificadorVendasSoftware;

public sealed class MetaBatidaForm : Form
{
    private static readonly CultureInfo CulturaBrasil =
        CultureInfo.GetCultureInfo("pt-BR");

    private readonly VendaDto _venda;
    private readonly TemaEquipe _tema;

    private readonly System.Windows.Forms.Timer _timer = new();

    private readonly List<Confete> _confetes = [];
    private readonly Random _random = new();

    private PictureBox? _alvoEsquerdo;
    private PictureBox? _alvoDireito;

    private int _contadorTicks;
    private bool _recursosLiberados;

    public MetaBatidaForm(VendaDto venda)
    {
        _venda = venda ??
            throw new ArgumentNullException(nameof(venda));

        _tema = TemaEquipe.Obter(venda.Equipe);

        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = _tema.Fundo;

        Width = 760;
        Height = 430;

        ArredondarFormulario(32);
        MontarConteudo();
        ConfigurarTimer();

        CriarExplosaoConfetes(
            Width / 2,
            Height / 2 - 20,
            55
        );
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        PosicionarNoCentro();

        BringToFront();
        Activate();
    }

    private void MontarConteudo()
    {
        SuspendLayout();

        string vendedor = TextoOuPadrao(
            _venda.Vendedor,
            "Vendedor não informado"
        );

        string equipe = TextoOuPadrao(
            _venda.Equipe,
            "Equipe não informada"
        );

        string tipoMeta = TextoOuPadrao(
            _venda.TipoMeta,
            "Meta"
        );

        _alvoEsquerdo = CriarAlvo(
            130,
            63
        );

        _alvoDireito = CriarAlvo(
            590,
            63
        );

        var lblTitulo = new Label
        {
            Text = "META ATINGIDA!",
            Font = new Font(
                "Segoe UI",
                32,
                FontStyle.Bold
            ),
            ForeColor = _tema.Texto,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Width = Width,
            Height = 70,
            Left = 0,
            Top = 50,
            BackColor = Color.Transparent
        };

        var lblSubtitulo = new Label
        {
            Text =
                $"{vendedor} atingiu a meta de {tipoMeta}.",
            Font = new Font(
                "Segoe UI",
                17,
                FontStyle.Regular
            ),
            ForeColor = _tema.TextoSuave,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Width = Width,
            Height = 42,
            Left = 0,
            Top = 125,
            BackColor = Color.Transparent,
            AutoEllipsis = true
        };

        string detalheMeta =
            CriarTextoDetalheMeta(
                tipoMeta,
                _venda.ValorAtualMeta,
                _venda.MetaValor
            );

        var lblDetalhe = new Label
        {
            Text = detalheMeta,
            Font = new Font(
                "Segoe UI",
                24,
                FontStyle.Bold
            ),
            ForeColor = _tema.DestaqueClaro,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Width = Width,
            Height = 55,
            Left = 0,
            Top = 195,
            BackColor = Color.Transparent,
            AutoEllipsis = true
        };

        var lblEquipe = new Label
        {
            Text = $"Equipe: {equipe}",
            Font = new Font(
                "Segoe UI",
                15,
                FontStyle.Regular
            ),
            ForeColor = _tema.TextoSuave,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Width = Width,
            Height = 35,
            Left = 0,
            Top = 260,
            BackColor = Color.Transparent,
            AutoEllipsis = true
        };

        var btnFechar = new Button
        {
            Text = "Fechar",
            Font = new Font(
                "Segoe UI",
                12,
                FontStyle.Bold
            ),
            ForeColor = Color.White,
            BackColor = _tema.Destaque,
            FlatStyle = FlatStyle.Flat,
            Width = 140,
            Height = 45,
            Left = (Width - 140) / 2,
            Top = 325,
            Cursor = Cursors.Hand
        };

        btnFechar.FlatAppearance.BorderSize = 0;
        btnFechar.Click += (_, _) => Close();

        Controls.Add(_alvoEsquerdo);
        Controls.Add(_alvoDireito);
        Controls.Add(lblTitulo);
        Controls.Add(lblSubtitulo);
        Controls.Add(lblDetalhe);
        Controls.Add(lblEquipe);
        Controls.Add(btnFechar);

        lblTitulo.BringToFront();
        lblSubtitulo.BringToFront();
        lblDetalhe.BringToFront();
        lblEquipe.BringToFront();
        btnFechar.BringToFront();

        _alvoEsquerdo.BringToFront();
        _alvoDireito.BringToFront();

        ResumeLayout(false);
    }

    private void ConfigurarTimer()
    {
        _timer.Interval = 30;
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private PictureBox CriarAlvo(
        int left,
        int top
    )
    {
        var alvo = new PictureBox
        {
            Width = 46,
            Height = 46,
            Left = left,
            Top = top,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        string caminhoAlvo = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "target.png"
        );

        alvo.Image =
            CarregarImagemSegura(caminhoAlvo);

        // A ausência da imagem não impede a abertura do popup.
        alvo.Visible = alvo.Image is not null;

        return alvo;
    }

    private static string CriarTextoDetalheMeta(
        string tipoMeta,
        decimal? valorAtual,
        decimal? metaValor
    )
    {
        if (
            !valorAtual.HasValue &&
            !metaValor.HasValue
        )
        {
            return $"{tipoMeta}: meta alcançada";
        }

        string valorAtualFormatado =
            FormatarValorMeta(
                valorAtual,
                tipoMeta
            );

        string metaValorFormatado =
            FormatarValorMeta(
                metaValor,
                tipoMeta
            );

        return
            $"{tipoMeta}: " +
            $"{valorAtualFormatado} / " +
            $"{metaValorFormatado}";
    }

    private static string FormatarValorMeta(
        decimal? valor,
        string? tipoMeta
    )
    {
        if (!valor.HasValue)
        {
            return "—";
        }

        bool metaFinanceira =
            Contem(
                tipoMeta,
                "receita"
            ) ||
            Contem(
                tipoMeta,
                "valor"
            ) ||
            Contem(
                tipoMeta,
                "arrecad"
            );

        if (metaFinanceira)
        {
            return valor.Value.ToString(
                "C",
                CulturaBrasil
            );
        }

        return valor.Value.ToString(
            "N0",
            CulturaBrasil
        );
    }

    private static bool Contem(
        string? texto,
        string valor
    )
    {
        return texto?.Contains(
            valor,
            StringComparison.OrdinalIgnoreCase
        ) == true;
    }

    private static string TextoOuPadrao(
        string? texto,
        string padrao
    )
    {
        return string.IsNullOrWhiteSpace(texto)
            ? padrao
            : texto.Trim();
    }

    private void Timer_Tick(
        object? sender,
        EventArgs e
    )
    {
        _contadorTicks++;

        if (_contadorTicks % 35 == 0)
        {
            CriarExplosaoConfetes(
                Width / 2,
                Height / 2 - 20,
                18
            );
        }

        if (_contadorTicks % 70 == 0)
        {
            CriarExplosaoConfetes(
                90,
                Height - 90,
                10
            );

            CriarExplosaoConfetes(
                Width - 90,
                Height - 90,
                10
            );
        }

        for (
            int indice = _confetes.Count - 1;
            indice >= 0;
            indice--
        )
        {
            Confete confete =
                _confetes[indice];

            confete.X +=
                confete.VelocidadeX;

            confete.Y +=
                confete.VelocidadeY;

            confete.VelocidadeY += 0.16f;
            confete.VelocidadeX *= 0.992f;

            confete.Angulo +=
                confete.VelocidadeRotacao;

            confete.Vida--;

            bool saiuDaTela =
                confete.X < -100 ||
                confete.X > Width + 100 ||
                confete.Y > Height + 100;

            if (
                confete.Vida <= 0 ||
                saiuDaTela
            )
            {
                _confetes.RemoveAt(indice);
            }
        }

        Invalidate();
    }

    private void CriarExplosaoConfetes(
        int origemX,
        int origemY,
        int quantidade
    )
    {
        const int limiteConfetes = 250;

        if (_confetes.Count >= limiteConfetes)
        {
            return;
        }

        int quantidadePermitida = Math.Min(
            quantidade,
            limiteConfetes - _confetes.Count
        );

        for (
            int indice = 0;
            indice < quantidadePermitida;
            indice++
        )
        {
            double angulo =
                _random.NextDouble() *
                Math.PI *
                2;

            float velocidade =
                _random.Next(3, 10);

            int tipoForma =
                _random.Next(0, 4);

            _confetes.Add(
                new Confete
                {
                    X = origemX,
                    Y = origemY,

                    VelocidadeX =
                        (float)(
                            Math.Cos(angulo) *
                            velocidade
                        ),

                    VelocidadeY =
                        (float)(
                            Math.Sin(angulo) *
                            velocidade
                        ) -
                        _random.Next(1, 5),

                    Angulo =
                        _random.Next(0, 360),

                    VelocidadeRotacao =
                        _random.Next(-8, 9),

                    Largura =
                        tipoForma == 2
                            ? _random.Next(18, 30)
                            : _random.Next(6, 13),

                    Altura =
                        tipoForma == 2
                            ? _random.Next(4, 7)
                            : _random.Next(6, 15),

                    Vida =
                        _random.Next(55, 105),

                    Cor =
                        ObterCorAleatoria(),

                    TipoForma =
                        tipoForma
                }
            );
        }
    }

    private Color ObterCorAleatoria()
    {
        if (_tema.CoresConfete.Length == 0)
        {
            return Color.White;
        }

        int indice =
            _random.Next(
                _tema.CoresConfete.Length
            );

        return _tema.CoresConfete[indice];
    }

    protected override void OnPaint(
        PaintEventArgs e
    )
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        e.Graphics.CompositingQuality =
            CompositingQuality.HighQuality;

        e.Graphics.InterpolationMode =
            InterpolationMode.HighQualityBicubic;

        e.Graphics.PixelOffsetMode =
            PixelOffsetMode.HighQuality;

        foreach (Confete confete in _confetes)
        {
            GraphicsState estado =
                e.Graphics.Save();

            try
            {
                e.Graphics.TranslateTransform(
                    confete.X,
                    confete.Y
                );

                e.Graphics.RotateTransform(
                    confete.Angulo
                );

                DesenharConfete(
                    e.Graphics,
                    confete
                );
            }
            finally
            {
                e.Graphics.Restore(estado);
            }
        }
    }

    private static void DesenharConfete(
        Graphics graphics,
        Confete confete
    )
    {
        using var pincel =
            new SolidBrush(confete.Cor);

        switch (confete.TipoForma)
        {
            case 0:
                graphics.FillEllipse(
                    pincel,
                    -confete.Largura / 2f,
                    -confete.Altura / 2f,
                    confete.Largura,
                    confete.Altura
                );
                break;

            case 1:
                graphics.FillEllipse(
                    pincel,
                    -confete.Largura / 2f,
                    -confete.Altura / 2f,
                    confete.Largura + 4,
                    confete.Altura
                );
                break;

            case 2:
                using (var caneta =
                       new Pen(confete.Cor, 3))
                {
                    caneta.StartCap =
                        LineCap.Round;

                    caneta.EndCap =
                        LineCap.Round;

                    caneta.LineJoin =
                        LineJoin.Round;

                    graphics.DrawBezier(
                        caneta,
                        new PointF(
                            -confete.Largura / 2f,
                            0
                        ),
                        new PointF(
                            -confete.Largura / 4f,
                            -confete.Altura * 2
                        ),
                        new PointF(
                            confete.Largura / 4f,
                            confete.Altura * 2
                        ),
                        new PointF(
                            confete.Largura / 2f,
                            0
                        )
                    );
                }

                break;

            default:
                using (
                    GraphicsPath caminho =
                        CriarCapsulaPath(
                            confete.Largura,
                            confete.Altura
                        )
                )
                {
                    graphics.FillPath(
                        pincel,
                        caminho
                    );
                }

                break;
        }
    }

    private static GraphicsPath CriarCapsulaPath(
        float largura,
        float altura
    )
    {
        var caminho = new GraphicsPath();

        float x = -largura / 2;
        float y = -altura / 2;

        float diametro =
            Math.Min(
                largura,
                altura
            );

        if (largura >= altura)
        {
            caminho.AddArc(
                x,
                y,
                diametro,
                diametro,
                90,
                180
            );

            caminho.AddArc(
                x + largura - diametro,
                y,
                diametro,
                diametro,
                270,
                180
            );
        }
        else
        {
            caminho.AddArc(
                x,
                y,
                diametro,
                diametro,
                180,
                180
            );

            caminho.AddArc(
                x,
                y + altura - diametro,
                diametro,
                diametro,
                0,
                180
            );
        }

        caminho.CloseFigure();

        return caminho;
    }

    private void PosicionarNoCentro()
    {
        Screen tela =
            Screen.FromControl(this);

        Rectangle area =
            tela.WorkingArea;

        Left =
            area.Left +
            (area.Width - Width) / 2;

        Top =
            area.Top +
            (area.Height - Height) / 2;
    }

    private void ArredondarFormulario(
        int raio
    )
    {
        using var caminho =
            new GraphicsPath();

        caminho.AddArc(
            0,
            0,
            raio,
            raio,
            180,
            90
        );

        caminho.AddArc(
            Width - raio,
            0,
            raio,
            raio,
            270,
            90
        );

        caminho.AddArc(
            Width - raio,
            Height - raio,
            raio,
            raio,
            0,
            90
        );

        caminho.AddArc(
            0,
            Height - raio,
            raio,
            raio,
            90,
            90
        );

        caminho.CloseFigure();

        Region? regiaoAnterior =
            Region;

        Region =
            new Region(caminho);

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
            return null;
        }
    }

    protected override void OnFormClosed(
        FormClosedEventArgs e
    )
    {
        LiberarRecursos();

        base.OnFormClosed(e);
    }

    private void LiberarRecursos()
    {
        if (_recursosLiberados)
        {
            return;
        }

        _recursosLiberados = true;

        _timer.Stop();
        _timer.Dispose();

        _alvoEsquerdo?.Image?.Dispose();
        _alvoDireito?.Image?.Dispose();

        _confetes.Clear();
    }

    private sealed class Confete
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float VelocidadeX { get; set; }

        public float VelocidadeY { get; set; }

        public float Angulo { get; set; }

        public float VelocidadeRotacao { get; set; }

        public int Largura { get; set; }

        public int Altura { get; set; }

        public int Vida { get; set; }

        public Color Cor { get; set; }

        public int TipoForma { get; set; }
    }
}