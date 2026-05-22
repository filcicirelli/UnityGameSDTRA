using UnityEngine;

// =====================================================================
//  CaricatoreLivelli
// ---------------------------------------------------------------------
//  A partire da un DatiLivello costruisce in scena tutti gli oggetti
//  del livello: lo sfondo (stelle e pianeti decorativi), Astro (che
//  segue il mouse), gli asteroidi, le bombe e le caramelle.
//
//  La chiave e la porta sono create solo dopo che tutte le caramelle
//  sono state raccolte (lo chiede il GestoreGioco).
//
//  Tutto viene messo come figlio di un GameObject "Missione", cosi'
//  quando passo a un nuovo livello mi basta distruggere quel
//  contenitore per fare pulizia.
// =====================================================================
public static class CaricatoreLivelli
{
    // Riferimenti agli oggetti di scena
    private static GameObject contenitore;
    private static Transform contenitoreCaramelle;
    private static Transform contenitoreAsteroidi;
    private static Transform contenitoreBombe;
    private static DatiLivello livelloCorrente;

    // Palette delle caramelle per il livello 1 (generate casualmente)
    static readonly Color[] COLORI_RANDOM = new Color[]
    {
        new Color(1f,    0.35f, 0.55f),    // fragola
        new Color(0.40f, 0.85f, 1f),       // menta blu
        new Color(0.95f, 0.45f, 0.85f),    // anguria
        new Color(0.55f, 1f,    0.55f),    // mela verde
        new Color(1f,    0.65f, 0.20f),    // arancia
        new Color(0.95f, 0.85f, 0.30f),    // limone
    };

    // -----------------------------------------------------------------
    //  Costruzione e pulizia del livello
    // -----------------------------------------------------------------

    public static int Carica(DatiLivello dati)
    {
        // Pulisco quello che c'era prima
        Pulisci();

        livelloCorrente = dati;
        contenitore = new GameObject("Missione");

        CostruisciStelleDiSfondo();
        CostruisciPianetiDecorativi();
        CostruisciAstro();

        // Creo i sotto-contenitori per tenere la gerarchia ordinata
        contenitoreCaramelle = CreaFiglio("Caramelle");
        contenitoreAsteroidi = CreaFiglio("Asteroidi");
        contenitoreBombe     = CreaFiglio("Bombe");

        CostruisciAsteroidi(dati);
        CostruisciBombe(dati);
        return CostruisciCaramelle(dati);
    }

    public static void Pulisci()
    {
        if (contenitore != null)
        {
            Object.Destroy(contenitore);
        }
        contenitore = null;
        contenitoreCaramelle = null;
        contenitoreAsteroidi = null;
        contenitoreBombe = null;
        livelloCorrente = null;
    }

    static Transform CreaFiglio(string nome)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(contenitore.transform);
        return go.transform;
    }

    // -----------------------------------------------------------------
    //  Sfondo (stelle e pianeti decorativi)
    // -----------------------------------------------------------------

    static void CostruisciStelleDiSfondo()
    {
        Transform radice = CreaFiglio("Stelle");

        // 120 piccoli puntini bianchi in posizione e luminosita' casuali
        for (int i = 0; i < 120; i++)
        {
            float x = Random.Range(-9.5f, 9.5f);
            float y = Random.Range(-5.5f, 5.5f);
            float lum = Random.Range(0.4f, 1.0f);
            Color colore = new Color(lum, lum, lum * 0.9f + 0.1f, 1f);

            GameObject stella = new GameObject("Stella");
            stella.transform.SetParent(radice);
            stella.transform.position = new Vector3(x, y, 5f);

            SpriteRenderer sr = stella.AddComponent<SpriteRenderer>();
            sr.sprite = FabbricaImmagini.CreaStella(colore);
            sr.sortingOrder = -5;

            float scala = Random.Range(0.4f, 1.1f);
            stella.transform.localScale = new Vector3(scala, scala, 1f);
        }
    }

    static void CostruisciPianetiDecorativi()
    {
        Transform radice = CreaFiglio("Pianeti");

        // Quattro pianeti agli angoli, ciascuno con due colori per
        // dare l'effetto "screziato".
        Color[] coloriPrincipali = new Color[]
        {
            new Color(0.95f, 0.45f, 0.45f),
            new Color(0.50f, 0.80f, 1f),
            new Color(0.95f, 0.85f, 0.40f),
            new Color(0.70f, 0.55f, 0.95f),
        };
        Color[] coloriSecondari = new Color[]
        {
            new Color(0.60f, 0.20f, 0.30f),
            new Color(0.20f, 0.40f, 0.70f),
            new Color(0.70f, 0.40f, 0.10f),
            new Color(0.30f, 0.20f, 0.55f),
        };
        Vector2[] posizioni = new Vector2[]
        {
            new Vector2(-7.0f,  3.2f),
            new Vector2( 7.2f,  2.5f),
            new Vector2(-6.2f, -3.0f),
            new Vector2( 6.5f, -3.3f),
        };

        for (int i = 0; i < posizioni.Length; i++)
        {
            GameObject pianeta = new GameObject("Pianeta_" + i);
            pianeta.transform.SetParent(radice);
            pianeta.transform.position = posizioni[i];

            float scala = Random.Range(1.1f, 1.6f);
            pianeta.transform.localScale = new Vector3(scala, scala, 1f);

            SpriteRenderer sr = pianeta.AddComponent<SpriteRenderer>();
            sr.sprite = FabbricaImmagini.CreaPianeta(coloriPrincipali[i], coloriSecondari[i]);
            sr.sortingOrder = -2;
        }
    }

    // -----------------------------------------------------------------
    //  Astro (il protagonista che segue il mouse)
    // -----------------------------------------------------------------

    static void CostruisciAstro()
    {
        GameObject a = new GameObject("Astro");
        a.transform.SetParent(contenitore.transform);
        a.transform.position = new Vector3(0f, 0f, 0f);
        a.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        SpriteRenderer sr = a.AddComponent<SpriteRenderer>();
        sr.sprite = FabbricaImmagini.CreaAstro();
        // sortingOrder alto: Astro deve sempre essere visibile davanti
        // alle caramelle, alla chiave e alla porta (e' il puntatore).
        sr.sortingOrder = 10;

        a.AddComponent<Astro>();
    }

    // -----------------------------------------------------------------
    //  Asteroidi (le barriere rettangolari)
    // -----------------------------------------------------------------

    static void CostruisciAsteroidi(DatiLivello dati)
    {
        for (int i = 0; i < dati.asteroidi.Count; i++)
        {
            DatiAsteroide d = dati.asteroidi[i];

            GameObject go = new GameObject("Asteroide");
            go.transform.SetParent(contenitoreAsteroidi);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = FabbricaImmagini.CreaTesseraAsteroide(d.colore);
            sr.sortingOrder = 0;

            Asteroide ast = go.AddComponent<Asteroide>();
            ast.Inizializza(d.centro, d.dimensione);
        }
    }

    // -----------------------------------------------------------------
    //  Bombe
    // -----------------------------------------------------------------

    static void CostruisciBombe(DatiLivello dati)
    {
        for (int i = 0; i < dati.bombe.Count; i++)
        {
            Vector2 pos = dati.bombe[i];

            GameObject go = new GameObject("Bomba");
            go.transform.SetParent(contenitoreBombe);
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = FabbricaImmagini.CreaBomba();
            sr.sortingOrder = 3;

            Bomba b = go.AddComponent<Bomba>();
            b.Inizializza(pos);
        }
    }

    // -----------------------------------------------------------------
    //  Caramelle (random nel livello 1, fisse negli altri)
    // -----------------------------------------------------------------

    static int CostruisciCaramelle(DatiLivello dati)
    {
        int totale;

        if (dati.caramelleCasuali > 0)
        {
            totale = dati.caramelleCasuali;
            for (int i = 0; i < totale; i++)
            {
                Vector2 pos = PosizioneCaramellaCasuale(dati);
                Color colore = COLORI_RANDOM[Random.Range(0, COLORI_RANDOM.Length)];
                CreaCaramella(pos, colore, i);
            }
        }
        else
        {
            totale = dati.caramelle.Count;
            for (int i = 0; i < totale; i++)
            {
                CreaCaramella(dati.caramelle[i].posizione, dati.caramelle[i].colore, i);
            }
        }

        return totale;
    }

    static void CreaCaramella(Vector2 posizione, Color colore, int numero)
    {
        GameObject c = new GameObject("Caramella_" + numero);
        c.transform.SetParent(contenitoreCaramelle);
        c.transform.localScale = new Vector3(1.0f, 1.0f, 1f);

        SpriteRenderer sr = c.AddComponent<SpriteRenderer>();
        sr.sprite = FabbricaImmagini.CreaCaramella(colore);
        sr.sortingOrder = 2;

        Caramella cr = c.AddComponent<Caramella>();
        cr.Inizializza(new Vector3(posizione.x, posizione.y, 0f));
    }

    static Vector2 PosizioneCaramellaCasuale(DatiLivello dati)
    {
        // Provo fino a 30 volte a trovare una posizione "buona":
        //  - non troppo vicina al centro (dove parte Astro)
        //  - non dentro un asteroide
        for (int tentativi = 0; tentativi < 30; tentativi++)
        {
            float x = Random.Range(-7f, 7f);
            float y = Random.Range(-3.5f, 4f);
            Vector2 p = new Vector2(x, y);

            if (p.magnitude < 1.2f) continue;
            if (DentroUnAsteroide(p, dati)) continue;
            return p;
        }
        // Fallback se proprio non trovo (caso quasi impossibile)
        return new Vector2(3f, 3f);
    }

    static bool DentroUnAsteroide(Vector2 punto, DatiLivello dati)
    {
        for (int i = 0; i < dati.asteroidi.Count; i++)
        {
            DatiAsteroide a = dati.asteroidi[i];
            Rect r = new Rect(
                a.centro.x - a.dimensione.x / 2f,
                a.centro.y - a.dimensione.y / 2f,
                a.dimensione.x, a.dimensione.y);
            if (r.Contains(punto)) return true;
        }
        return false;
    }

    // -----------------------------------------------------------------
    //  Chiave e Porta (compaiono dopo aver raccolto tutte le caramelle)
    // -----------------------------------------------------------------

    public static void GeneraChiave()
    {
        if (contenitore == null) return;

        GameObject go = new GameObject("Chiave");
        go.transform.SetParent(contenitore.transform);
        go.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FabbricaImmagini.CreaChiave();
        sr.sortingOrder = 5;

        Chiave k = go.AddComponent<Chiave>();
        // y = 2.0 cosi' resta ben sotto il muro superiore dei livelli 2 e 3.
        k.Inizializza(new Vector2(0f, 2.0f));
    }

    public static void GeneraPorta()
    {
        if (contenitore == null) return;

        GameObject go = new GameObject("Porta");
        go.transform.SetParent(contenitore.transform);
        go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FabbricaImmagini.CreaPorta();
        sr.sortingOrder = 5;

        Porta p = go.AddComponent<Porta>();
        p.Inizializza(ScegliPosizionePortaCasuale(Vector2.zero));
    }

    // Sceglie una posizione "valida" per la porta:
    // distante da quella attuale, fuori dagli asteroidi, lontana dalle bombe.
    public static Vector2 ScegliPosizionePortaCasuale(Vector2 daEvitare)
    {
        if (livelloCorrente == null) return new Vector2(0f, 2f);

        for (int tentativi = 0; tentativi < 40; tentativi++)
        {
            // Range stretto: lascio margine ai muri laterali e al soffitto.
            float x = Random.Range(-5.5f, 5.5f);
            float y = Random.Range(-2.5f, 2.8f);
            Vector2 p = new Vector2(x, y);

            if (Vector2.Distance(p, daEvitare) < 3f) continue;
            if (DentroUnAsteroide(p, livelloCorrente)) continue;
            if (TroppoVicinoAUnaBomba(p)) continue;
            return p;
        }
        // Fallback
        return new Vector2(4f, 2f);
    }

    static bool TroppoVicinoAUnaBomba(Vector2 punto)
    {
        for (int i = 0; i < Bomba.Tutte.Count; i++)
        {
            Bomba b = Bomba.Tutte[i];
            if (b == null) continue;
            if (Vector2.Distance(punto, b.transform.position) < 1.2f) return true;
        }
        return false;
    }

    // -----------------------------------------------------------------
    //  Effetti speciali (festa, esplosione)
    // -----------------------------------------------------------------

    public static void GeneraPianetaAmico()
    {
        if (contenitore == null) return;

        GameObject p = new GameObject("PianetaAmico");
        p.transform.SetParent(contenitore.transform);
        p.transform.position = new Vector3(0f, 1.5f, -2f);

        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = FabbricaImmagini.CreaPianetaAmico();
        sr.sortingOrder = 6;

        p.AddComponent<PianetaAmico>();
    }

    public static void GeneraCoriandoli()
    {
        if (contenitore == null) return;

        GameObject c = new GameObject("Coriandoli");
        c.transform.SetParent(contenitore.transform);
        c.AddComponent<Coriandoli>();
    }

    public static void GeneraEsplosione(Vector3 posizione)
    {
        if (contenitore == null) return;

        GameObject e = new GameObject("Esplosione");
        e.transform.SetParent(contenitore.transform);
        e.transform.position = new Vector3(posizione.x, posizione.y, -3f);

        SpriteRenderer sr = e.AddComponent<SpriteRenderer>();
        sr.sprite = FabbricaImmagini.CreaEsplosione();
        sr.sortingOrder = 8;

        e.AddComponent<Esplosione>();
    }
}
