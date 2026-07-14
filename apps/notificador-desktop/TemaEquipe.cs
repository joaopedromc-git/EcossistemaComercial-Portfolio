using System.Drawing;

namespace NotificadorVendasSoftware;

public sealed class TemaEquipe
{
    public Color Fundo { get; init; }

    public Color Destaque { get; init; }

    public Color DestaqueClaro { get; init; }

    public Color Texto { get; init; } =
        Color.White;

    public Color TextoSuave { get; init; }

    public string LogoArquivo { get; init; } =
        string.Empty;

    public Color[] CoresConfete { get; init; } =
        Array.Empty<Color>();

    private static readonly TemaEquipe TemaAtlas = new()
    {
        Fundo = Color.FromArgb(6, 32, 63),
        Destaque = Color.FromArgb(30, 136, 229),
        DestaqueClaro = Color.FromArgb(227, 242, 253),
        Texto = Color.White,
        TextoSuave = Color.FromArgb(220, 235, 250),
        LogoArquivo = "logo-equipe-atlas.png",

        CoresConfete =
        [
            Color.FromArgb(30, 136, 229),
            Color.FromArgb(100, 181, 246),
            Color.White,
            Color.FromArgb(227, 242, 253)
        ]
    };

    private static readonly TemaEquipe TemaAurora = new()
    {
        Fundo = Color.FromArgb(75, 62, 0),
        Destaque = Color.FromArgb(255, 193, 7),
        DestaqueClaro = Color.FromArgb(255, 248, 225),
        Texto = Color.White,
        TextoSuave = Color.FromArgb(255, 249, 220),
        LogoArquivo = "logo-equipe-aurora.png",

        CoresConfete =
        [
            Color.FromArgb(255, 193, 7),
            Color.FromArgb(255, 235, 59),
            Color.White,
            Color.FromArgb(255, 248, 225)
        ]
    };

    private static readonly TemaEquipe TemaHorizonte = new()
    {
        Fundo = Color.FromArgb(7, 54, 36),
        Destaque = Color.FromArgb(46, 125, 50),
        DestaqueClaro = Color.FromArgb(232, 245, 233),
        Texto = Color.White,
        TextoSuave = Color.FromArgb(220, 240, 225),
        LogoArquivo = "logo-equipe-horizonte.png",

        CoresConfete =
        [
            Color.FromArgb(46, 125, 50),
            Color.FromArgb(102, 187, 106),
            Color.White,
            Color.FromArgb(232, 245, 233)
        ]
    };

    private static readonly TemaEquipe TemaPadrao = new()
    {
        Fundo = Color.FromArgb(18, 25, 32),
        Destaque = Color.FromArgb(103, 58, 183),
        DestaqueClaro = Color.FromArgb(237, 231, 246),
        Texto = Color.White,
        TextoSuave = Color.FromArgb(215, 215, 225),
        LogoArquivo = "logo-padrao.png",

        CoresConfete =
        [
            Color.FromArgb(103, 58, 183),
            Color.FromArgb(179, 157, 219),
            Color.White,
            Color.FromArgb(255, 215, 0)
        ]
    };

    public static TemaEquipe Obter(
        string? equipe
    )
    {
        if (string.IsNullOrWhiteSpace(equipe))
        {
            return TemaPadrao;
        }

        string nomeEquipe =
            equipe.Trim();

        if (
            Contem(
                nomeEquipe,
                "Equipe Atlas"
            ) ||
            Contem(
                nomeEquipe,
                "Atlas"
            )
        )
        {
            return TemaAtlas;
        }

        if (
            Contem(
                nomeEquipe,
                "Equipe Aurora"
            ) ||
            Contem(
                nomeEquipe,
                "Aurora"
            )
        )
        {
            return TemaAurora;
        }

        if (
            Contem(
                nomeEquipe,
                "Equipe Horizonte"
            ) ||
            Contem(
                nomeEquipe,
                "Horizonte"
            )
        )
        {
            return TemaHorizonte;
        }

        return TemaPadrao;
    }

    private static bool Contem(
        string texto,
        string valor
    )
    {
        return texto.Contains(
            valor,
            StringComparison.OrdinalIgnoreCase
        );
    }
}