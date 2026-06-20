using UnityEngine;

// Carica le immagini (sprite) del gioco dai file PNG che stanno in
// Assets/Resources. Ogni oggetto usa un'immagine vera, presa dai pacchetti
// gratuiti CC0 (vedi la cartella "oggetti gioco" e il suo LEGGIMI).
//
// PER CAMBIARE un'immagine: metti un altro file PNG in Assets/Resources e
// scrivi qui sotto il suo nome (SENZA l'estensione .png). Tutto qui.
public static class FabbricaImmagini
{
    // ---- Nomi dei file in Assets/Resources (pagina parametri centralizzata) ----
    private const string PERSONAGGIO = "personaggio"; // la navicella del giocatore (ex Astro)
    private const string STELLA      = "stella";      // la stellina da raccogliere (ex caramella)
    private const string PIANETA     = "pianeta";     // il pianeta amico di fine missione
    private const string ASTEROIDE   = "asteroide";   // la roccia delle barriere
    private const string BOMBA       = "bomba";       // l'ostacolo bomba
    private const string ESPLOSIONE  = "esplosione";  // lo scoppio della bomba
    private const string PORTA       = "porta";       // il portale di fine livello
    private const string CORIANDOLO  = "coriandolo";  // i coriandoli della festa

    // =========================================================
    // I METODI che il resto del gioco chiama
    // =========================================================
    public static Sprite CreaAstro()         { return Carica(PERSONAGGIO); }
    public static Sprite CreaCaramella()      { return Carica(STELLA); }
    public static Sprite CreaPianetaAmico()   { return Carica(PIANETA); }
    public static Sprite CreaBomba()          { return Carica(BOMBA); }
    public static Sprite CreaEsplosione()     { return Carica(ESPLOSIONE); }
    public static Sprite CreaPorta()          { return Carica(PORTA); }
    public static Sprite CreaCoriandolo()     { return Carica(CORIANDOLO); }

    // L'asteroide e' una sola immagine di roccia INTERA: chi la usa (Livelli.cs)
    // la disegna tutta, scalandola in modo uniforme (cosi' resta tonda, non si
    // taglia), e ne sceglie il colore con lo SpriteRenderer.
    public static Sprite CreaAsteroide() { return Carica(ASTEROIDE); }

    // Quadrato pieno di un colore. NON e' un "disegno": e' solo un rettangolo
    // colorato, lo uso per l'alone rosso che pulsa attorno alla bomba.
    public static Sprite CreaQuadratoPieno(Color colore)
    {
        const int LATO = 16;
        Color[] pixel = new Color[LATO * LATO];
        for (int i = 0; i < pixel.Length; i++)
        {
            pixel[i] = colore;
        }

        Texture2D tex = new Texture2D(LATO, LATO, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixel);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, LATO, LATO),
            new Vector2(0.5f, 0.5f),
            LATO);
    }

    // =========================================================
    // Funzione di supporto: carica un PNG da Assets/Resources e ne fa uno sprite
    // =========================================================
    // Resources.Load vuole il nome SENZA estensione.
    // Normalizzo la dimensione: il lato piu' lungo diventa 1 unita' di Unity,
    // cosi' immagini di misure diverse appaiono grandi all'incirca uguali e chi
    // le usa puo' poi ridimensionarle con localScale.
    // Se l'immagine manca NON blocco il gioco: mostro un quadrato magenta
    // (il classico segnale "immagine non trovata") e scrivo un avviso.
    static Sprite Carica(string nome)
    {
        Texture2D tex = Resources.Load<Texture2D>(nome);
        if (tex == null)
        {
            Debug.LogWarning("FabbricaImmagini: non trovo l'immagine '" + nome +
                             "' in Assets/Resources. Uso un quadrato di ripiego.");
            return CreaQuadratoPieno(new Color(1f, 0f, 1f)); // magenta = manca il file
        }

        float latoLungo = Mathf.Max(tex.width, tex.height);
        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            latoLungo,              // PPU = lato piu' lungo -> immagine alta circa 1 unita'
            0,
            SpriteMeshType.FullRect); // sprite a rettangolo pieno (semplice e prevedibile)
    }
}
