using System.Collections.Generic;
using UnityEngine;

// Disegna da codice tutte le immagini (sprite) del gioco, cosi' non
// servono file immagine esterni.
//
// COME FUNZIONA (semplice!):
// Ogni immagine e' un "disegno" fatto di lettere, una riga di testo per
// ogni riga di pixel. La LEGENDA qui sotto dice quale colore corrisponde
// a ogni lettera. Il punto '.' (e lo spazio) vuol dire "trasparente".
//
// Per cambiare un disegno basta cambiare le lettere: per esempio se metto
// una 'W' (bianco) in piu' aggiungo un pixel bianco. Niente matematica.
public static class FabbricaImmagini
{
    private const int LATO = 16; // i disegni sono griglie 16x16
    private const int PPU = 16;  // 16 pixel = 1 unita' di Unity

    // ---- LEGENDA COLORI: una lettera = un colore ----
    static readonly Dictionary<char, Color> LEGENDA = new Dictionary<char, Color>
    {
        { '.', Color.clear },                          // trasparente
        { ' ', Color.clear },                          // trasparente

        { 'V', new Color(0.40f, 0.85f, 0.40f) },       // verde Astro
        { 'v', new Color(0.20f, 0.55f, 0.25f) },       // verde scuro (bordo)
        { 'b', new Color(0.75f, 1f,    0.75f) },       // pancia chiara
        { 'W', Color.white },                          // bianco
        { 'N', Color.black },                          // nero
        { 'h', new Color(0.85f, 0.85f, 0.95f) },       // grigio chiaro (zaino/riflessi)
        { 'H', new Color(0.45f, 0.50f, 0.65f) },       // grigio scuro

        { 'G', new Color(1f,    0.85f, 0.20f) },       // oro
        { 'g', new Color(0.70f, 0.50f, 0.10f) },       // oro scuro

        { 'P', new Color(0.55f, 0.30f, 0.85f) },       // viola portale
        { 'L', new Color(0.85f, 0.65f, 1f)    },       // viola chiaro (luce)
        { 'M', new Color(0.75f, 0.55f, 0.30f) },       // legno cornice
        { 'm', new Color(0.35f, 0.20f, 0.08f) },       // legno scuro

        { 'R', new Color(0.95f, 0.55f, 0.85f) },       // rosa
        { 'A', new Color(0.55f, 0.80f, 1f)    },       // azzurro
        { 'C', new Color(0.40f, 0.20f, 0.55f) },       // bordo viola scuro
        { 'k', new Color(0.40f, 0.25f, 0.15f) },       // marrone scuro (bocca/miccia)

        { 'S', new Color(0.12f, 0.10f, 0.18f) },       // quasi nero (bomba)
        { 'F', new Color(1f,    0.55f, 0.20f) },       // fiamma arancio

        { 'Y', new Color(1f,    1f,    0.60f) },       // giallo (esplosione)
        { 'O', new Color(1f,    0.55f, 0.20f) },       // arancio (esplosione)
        { 'r', new Color(0.85f, 0.20f, 0.20f) },       // rosso (esplosione)
    };

    // =========================================================
    // I DISEGNI (la prima riga e' quella in alto)
    // =========================================================

    // ASTRO: alieno verde con antenna, occhi, sorriso e zainetto
    static readonly string[] ASTRO =
    {
        ".......GG.......",
        ".......gg.......",
        ".......v........",
        ".....vvvvv......",
        "....vVVVVVv.....",
        "...vVVVVVVVv....",
        "..hvVVVVVVVvh...",
        ".HhVWNVVNWVhH...",
        ".HhVWNVVNWVhH...",
        "..hVVVVVVVVh....",
        "...VVbbbbVV.....",
        "...vVbkkbVv.....",
        "....vVVVVv......",
        ".....vVVv.......",
        "......vv........",
        "................",
    };

    // CARAMELLA: caramella rotonda con incarto ai lati ('#' = colore scelto)
    static readonly string[] CARAMELLA =
    {
        "................",
        "......++++......",
        ".....+####+.....",
        "....+######+....",
        "...+########+...",
        "...+###WW###+...",
        "o.+##########+.o",
        "oo+##########+oo",
        "oo+##########+oo",
        "o.+##########+.o",
        "...+########+...",
        "....+######+....",
        ".....+####+.....",
        "......++++......",
        "................",
        "................",
    };

    // CHIAVE dorata: testa tonda con buco, asta e dentini
    static readonly string[] CHIAVE =
    {
        "................",
        "...ggg..........",
        "..gWGGg.........",
        "..gG.Gg.........",
        "..gGGGgGGGGGGG..",
        "..gGGGGGGGGGGGg.",
        "..gGGGgGGGGGGGg.",
        "...ggg.....G.G.G",
        "...........G.G..",
        "................",
        "................",
        "................",
        "................",
        "................",
        "................",
        "................",
    };

    // PORTA / portale viola con cornice di legno e stella al centro
    static readonly string[] PORTA =
    {
        "................",
        "...mmmmmmmmm....",
        "...mMMMMMMMm....",
        "...mMPPPPPMm....",
        "...mMPPPPPMm....",
        "...mMPPWPPMm....",
        "...mMPWLWPMm....",
        "...mMPPWPPMm....",
        "...mMPPPPPMm....",
        "...mMPPPPPMm....",
        "...mMPPPPPMm....",
        "...mMPPPPPMm....",
        "...mMMMMMMMm....",
        "...mmmmmmmmm....",
        "................",
        "................",
    };

    // BOMBA: sfera scura con riflesso, miccia e fiamma
    static readonly string[] BOMBA =
    {
        "........F.......",
        ".......FFF......",
        "........k.......",
        ".......k........",
        ".....SSSSSS.....",
        "....SSSSSSSS....",
        "...ShhSSSSSSS...",
        "...ShSSSSSSSS...",
        "...SSSSSSSSSS...",
        "...SSSSSSSSSS...",
        "...SSSSSSSSSS...",
        "....SSSSSSSS....",
        ".....SSSSSS.....",
        "................",
        "................",
        "................",
    };

    // PIANETA AMICO: pianeta sorridente (azzurro sopra, rosa sotto)
    static readonly string[] PIANETA_AMICO =
    {
        "................",
        ".....CCCCCC.....",
        "...CCAAAAAACC...",
        "..CAAAAAAAAAAC..",
        ".CAAAAAAAAAAAAC.",
        ".CAAANAAAANAAAC.",
        ".CAAANAAAANAAAC.",
        ".CAAAAAAAAAAAAC.",
        ".CRRRRRRRRRRRRC.",
        ".CRRkRRRRRRkRRC.",
        ".CRRkkkkkkkkRRC.",
        "..CRRRRRRRRRRC..",
        "...CCRRRRRRCC...",
        ".....CCCCCC.....",
        "................",
        "................",
    };

    // ASTEROIDE: blocco roccioso (viene allungato per fare le barriere)
    // '#' = colore scelto, '+' = versione piu' scura (crateri/bordo)
    static readonly string[] ASTEROIDE =
    {
        "++++++++++++++++",
        "+##############+",
        "+###+#######+##+",
        "+##############+",
        "+######+#######+",
        "+##############+",
        "+##+########+##+",
        "+##############+",
        "+#######+######+",
        "+##############+",
        "+####+#####+###+",
        "+##############+",
        "+###+######+###+",
        "+##############+",
        "+#####+####+###+",
        "++++++++++++++++",
    };

    // PIANETA decorativo di sfondo ('#' = colore scelto, '+' = piu' scuro)
    static readonly string[] PIANETA =
    {
        "................",
        ".....######.....",
        "...##########...",
        "..############..",
        ".#####+######+#.",
        ".##############.",
        ".####+#####+###.",
        ".##############.",
        ".###+######+###.",
        ".##############.",
        ".#####+####+###.",
        "..############..",
        "...##########...",
        ".....######.....",
        "................",
        "................",
    };

    // ESPLOSIONE: anello luminoso a tre colori
    static readonly string[] ESPLOSIONE =
    {
        "................",
        ".....rrrrrr.....",
        "...rrOOOOOOrr...",
        "..rOOOOOOOOOOr..",
        ".rOOOYYYYYYOOOr.",
        ".rOOYYYYYYYYOOr.",
        "rOOYYYYYYYYYYOOr",
        "rOOYYYYYYYYYYOOr",
        "rOOYYYYYYYYYYOOr",
        ".rOOYYYYYYYYOOr.",
        ".rOOOYYYYYYOOOr.",
        "..rOOOOOOOOOOr..",
        "...rrOOOOOOrr...",
        ".....rrrrrr.....",
        "................",
        "................",
    };

    // =========================================================
    // I METODI che il resto del gioco chiama
    // =========================================================

    public static Sprite CreaAstro()        { return Disegna(ASTRO); }
    public static Sprite CreaChiave()        { return Disegna(CHIAVE); }
    public static Sprite CreaPorta()         { return Disegna(PORTA); }
    public static Sprite CreaBomba()         { return Disegna(BOMBA); }
    public static Sprite CreaPianetaAmico()  { return Disegna(PIANETA_AMICO); }
    public static Sprite CreaEsplosione()    { return Disegna(ESPLOSIONE); }

    // Questi prendono un colore: il disegno usa '#' (colore), '+' (scuro), 'o' (chiaro)
    public static Sprite CreaCaramella(Color tinta)      { return DisegnaColorato(CARAMELLA, tinta); }
    public static Sprite CreaTesseraAsteroide(Color c)   { return DisegnaColorato(ASTEROIDE, c); }
    public static Sprite CreaPianeta(Color c)            { return DisegnaColorato(PIANETA, c); }

    // Quadrato pieno di un colore (lo uso per stelle, coriandoli, aloni e barre)
    public static Sprite CreaQuadratoPieno(Color colore)
    {
        Color[] pixel = new Color[LATO * LATO];
        for (int i = 0; i < pixel.Length; i++)
        {
            pixel[i] = colore;
        }
        return Cuoci(pixel);
    }

    // =========================================================
    // Funzioni di supporto
    // =========================================================

    // Trasforma un disegno (lettere) in uno sprite usando la LEGENDA
    static Sprite Disegna(string[] disegno)
    {
        Color[] pixel = new Color[LATO * LATO];

        for (int riga = 0; riga < LATO; riga++)
        {
            string testo = riga < disegno.Length ? disegno[riga] : "";
            // la prima riga del disegno e' in alto -> y piu' grande
            int y = LATO - 1 - riga;

            for (int x = 0; x < LATO; x++)
            {
                char c = x < testo.Length ? testo[x] : '.';
                Color colore;
                if (!LEGENDA.TryGetValue(c, out colore))
                {
                    colore = Color.clear; // lettera sconosciuta = trasparente
                }
                pixel[y * LATO + x] = colore;
            }
        }

        return Cuoci(pixel);
    }

    // Come Disegna, ma '#'/'+'/'o' prendono il colore scelto (e le sue
    // versioni piu' scura e piu' chiara). Serve per caramelle, asteroidi
    // e pianeti, che cambiano colore.
    static Sprite DisegnaColorato(string[] disegno, Color tinta)
    {
        Color corpo  = tinta;
        Color scuro  = Scurisci(tinta, 0.5f);
        Color chiaro = Schiarisci(tinta, 0.5f);

        Color[] pixel = new Color[LATO * LATO];

        for (int riga = 0; riga < LATO; riga++)
        {
            string testo = riga < disegno.Length ? disegno[riga] : "";
            int y = LATO - 1 - riga;

            for (int x = 0; x < LATO; x++)
            {
                char c = x < testo.Length ? testo[x] : '.';

                Color colore;
                if (c == '#') colore = corpo;
                else if (c == '+') colore = scuro;
                else if (c == 'o') colore = chiaro;
                else if (!LEGENDA.TryGetValue(c, out colore)) colore = Color.clear;

                pixel[y * LATO + x] = colore;
            }
        }

        return Cuoci(pixel);
    }

    static Color Scurisci(Color c, float quanto)
    {
        // Avvicino i colori al nero
        return new Color(c.r * quanto, c.g * quanto, c.b * quanto, c.a);
    }

    static Color Schiarisci(Color c, float quanto)
    {
        // Avvicino i colori al bianco
        return new Color(
            c.r + (1f - c.r) * quanto,
            c.g + (1f - c.g) * quanto,
            c.b + (1f - c.b) * quanto,
            c.a);
    }

    // Trasforma l'array di colori in una immagine (Texture) e poi in uno Sprite.
    // filterMode = Point cosi' i pixel restano "a quadretti" (pixel art).
    static Sprite Cuoci(Color[] pixel)
    {
        Texture2D tex = new Texture2D(LATO, LATO, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixel);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, LATO, LATO),
            new Vector2(0.5f, 0.5f),
            PPU);
    }
}
