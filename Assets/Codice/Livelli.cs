using System.Collections.Generic;
using UnityEngine;

// Questo file si occupa dei livelli del gioco. Ha due parti:
//   1) DefinizioneLivelli: i DATI di ogni livello (caramelle, asteroidi, bombe)
//   2) CaricatoreLivelli:   COSTRUISCE in scena gli oggetti a partire dai dati
// In fondo ci sono le classette che descrivono un livello.


// =============================================================
// DEFINIZIONE LIVELLI
// Per ogni livello dico: il titolo, l'obiettivo, dove vanno
// le caramelle, gli asteroidi e le bombe.
//
// Per AGGIUNGERE un livello: aggiungere un case nello switch sotto e
// scrivere un nuovo metodo CostruisciLivelloN() simile agli altri.
// =============================================================
public static class DefinizioneLivelli
{
    // Numero totale di livelli (cambialo se ne aggiungi o togli)
    public const int Conteggio = 3;

    public static DatiLivello Ottieni(int indice)
    {
        switch (indice)
        {
            case 0: return CostruisciLivello1();
            case 1: return CostruisciLivello2();
            case 2: return CostruisciLivello3();
            default: return null;
        }
    }

    // Colori che vengono dati alle caramelle nei livelli "fissi"
    static readonly Color[] COLORI_CARAMELLE = new Color[]
    {
        new Color(1f,    0.85f, 0.30f),
        new Color(0.40f, 0.85f, 1f),
        new Color(0.95f, 0.45f, 0.85f),
        new Color(0.55f, 1f,    0.55f),
        new Color(1f,    0.55f, 0.30f),
    };

    // Colore degli asteroidi (uguale per tutti)
    static readonly Color COLORE_ASTEROIDE = new Color(0.55f, 0.50f, 0.45f);

    // =========================================================
    // LIVELLO 1 - PRIMO VOLO
    // Spazio aperto, nessun ostacolo, solo caramelle.
    // =========================================================
    static DatiLivello CostruisciLivello1()
    {
        DatiLivello l = new DatiLivello();
        l.titolo = "LIVELLO 1 - PRIMO VOLO";
        l.obiettivo = "RACCOGLI " + Impostazioni.CARAMELLE_LIV1 + " CARAMELLE";
        l.caramelleCasuali = Impostazioni.CARAMELLE_LIV1;
        return l;
    }

    // =========================================================
    // LIVELLO 2 - TRA GLI ASTEROIDI
    // Aggiungo dei rettangoli (asteroidi) che fanno male.
    // =========================================================
    static DatiLivello CostruisciLivello2()
    {
        DatiLivello l = new DatiLivello();
        l.titolo = "LIVELLO 2 - TRA GLI ASTEROIDI";
        l.obiettivo = "EVITA GLI ASTEROIDI";

        // Asteroidi: due muri laterali, un soffitto, due ostacoli al centro.
        // Formato: NuovoAsteroide(centroX, centroY, larghezza, altezza)
        l.asteroidi.Add(NuovoAsteroide(-7.5f,  0.0f, 1.2f, 5.0f));
        l.asteroidi.Add(NuovoAsteroide( 7.5f,  0.0f, 1.2f, 5.0f));
        l.asteroidi.Add(NuovoAsteroide( 0.0f,  4.5f, 9.0f, 1.0f));
        l.asteroidi.Add(NuovoAsteroide(-2.5f,  1.0f, 1.5f, 1.5f));
        l.asteroidi.Add(NuovoAsteroide( 2.5f, -1.5f, 1.8f, 1.0f));

        // Posizioni delle caramelle (scelte a mano nei "vuoti")
        Vector2[] posizioni = new Vector2[]
        {
            new Vector2(-5.5f,  3.0f),
            new Vector2(-3.0f,  3.5f),
            new Vector2( 3.0f,  3.5f),
            new Vector2( 5.5f,  3.0f),
            new Vector2(-5.0f,  0.0f),
            new Vector2( 0.0f,  2.5f),
            new Vector2( 5.0f,  0.5f),
            new Vector2(-3.5f, -2.0f),
            new Vector2( 0.0f, -3.0f),
            new Vector2( 4.5f, -3.0f),
        };
        AggiungiCaramelle(l, posizioni);

        return l;
    }

    // =========================================================
    // LIVELLO 3 - CAMPO MINATO
    // Stessi asteroidi del 2 piu' 4 bombe.
    // =========================================================
    static DatiLivello CostruisciLivello3()
    {
        DatiLivello l = new DatiLivello();
        l.titolo = "LIVELLO 3 - CAMPO MINATO";
        l.obiettivo = "ATTENZIONE ALLE BOMBE";

        // Riuso gli stessi asteroidi del livello 2
        l.asteroidi.Add(NuovoAsteroide(-7.5f,  0.0f, 1.2f, 5.0f));
        l.asteroidi.Add(NuovoAsteroide( 7.5f,  0.0f, 1.2f, 5.0f));
        l.asteroidi.Add(NuovoAsteroide( 0.0f,  4.5f, 9.0f, 1.0f));
        l.asteroidi.Add(NuovoAsteroide(-2.5f,  1.0f, 1.5f, 1.5f));
        l.asteroidi.Add(NuovoAsteroide( 2.5f, -1.5f, 1.8f, 1.0f));

        // Bombe (lontane dal centro dove parte Astro)
        l.bombe.Add(new Vector2(-6.0f,  2.5f));
        l.bombe.Add(new Vector2( 6.0f,  2.5f));
        l.bombe.Add(new Vector2(-4.0f, -3.5f));
        l.bombe.Add(new Vector2( 4.0f, -3.5f));

        // Caramelle (nei "vicoli" tra le bombe)
        Vector2[] posizioni = new Vector2[]
        {
            new Vector2(-4.0f,  3.0f),
            new Vector2(-1.5f,  3.0f),
            new Vector2( 1.5f,  3.0f),
            new Vector2( 4.0f,  3.0f),
            new Vector2(-5.0f,  0.0f),
            new Vector2( 0.0f,  2.5f),
            new Vector2( 5.0f,  0.0f),
            new Vector2(-1.5f, -3.0f),
            new Vector2( 1.5f, -3.0f),
            new Vector2( 0.0f, -3.5f),
        };
        AggiungiCaramelle(l, posizioni);

        return l;
    }

    // ---- Funzioni di aiuto ----

    static DatiAsteroide NuovoAsteroide(float cx, float cy, float larghezza, float altezza)
    {
        DatiAsteroide a = new DatiAsteroide();
        a.centro = new Vector2(cx, cy);
        a.dimensione = new Vector2(larghezza, altezza);
        a.colore = COLORE_ASTEROIDE;
        return a;
    }

    static void AggiungiCaramelle(DatiLivello livello, Vector2[] posizioni)
    {
        for (int i = 0; i < posizioni.Length; i++)
        {
            DatiCaramella c = new DatiCaramella();
            c.posizione = posizioni[i];
            c.colore = COLORI_CARAMELLE[i % COLORI_CARAMELLE.Length];
            livello.caramelle.Add(c);
        }
    }
}


// =============================================================
// CARICATORE LIVELLI
// Da un DatiLivello costruisce in scena tutti gli oggetti:
// sfondo, Astro, asteroidi, bombe e caramelle.
// La chiave e la porta vengono create dopo, quando il GestoreGioco
// segnala che tutte le caramelle sono state raccolte.
// =============================================================
public static class CaricatoreLivelli
{
    // Riferimenti agli oggetti in scena
    private static GameObject contenitore;
    private static Transform contenitoreCaramelle;
    private static Transform contenitoreAsteroidi;
    private static Transform contenitoreBombe;
    private static DatiLivello livelloCorrente;

    // Colori delle caramelle del livello 1 (random)
    static readonly Color[] COLORI_RANDOM = new Color[]
    {
        new Color(1f,    0.35f, 0.55f),
        new Color(0.40f, 0.85f, 1f),
        new Color(0.95f, 0.45f, 0.85f),
        new Color(0.55f, 1f,    0.55f),
        new Color(1f,    0.65f, 0.20f),
        new Color(0.95f, 0.85f, 0.30f),
    };

    // ---- Caricamento e pulizia ----

    public static int Carica(DatiLivello dati)
    {
        Pulisci(); // tolgo quello che c'era prima

        livelloCorrente = dati;
        contenitore = new GameObject("Missione");

        CostruisciSfondo();
        CostruisciAstro();

        // Sotto-contenitori per tenere ordinata la gerarchia in Unity
        contenitoreCaramelle = CreaFiglio("Caramelle");
        contenitoreAsteroidi = CreaFiglio("Asteroidi");
        contenitoreBombe = CreaFiglio("Bombe");

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

    // ---- Sfondo: il wallpaper (immagine della nebulosa) ----

    static void CostruisciSfondo()
    {
        Transform radice = CreaFiglio("Sfondo");

        // Carico il wallpaper dalla cartella Assets/Resources (file "sfondo.jpg").
        // Resources.Load vuole il nome SENZA estensione.
        Texture2D texture = Resources.Load<Texture2D>("sfondo");
        if (texture == null)
        {
            // Se l'immagine manca non blocco il gioco: resta lo sfondo nero della telecamera.
            Debug.LogWarning("Sfondo: non trovo 'sfondo' in Assets/Resources/sfondo.jpg");
            return;
        }

        // Trasformo l'immagine in uno sprite da mettere in scena
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);

        GameObject sfondo = new GameObject("Wallpaper");
        sfondo.transform.SetParent(radice);
        sfondo.transform.position = new Vector3(0f, 0f, 10f); // ben dietro a tutto

        SpriteRenderer sr = sfondo.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -10; // disegnato per primo: sta sotto a ogni altra cosa

        // Ingrandisco il wallpaper finche' copre tutta la visuale della telecamera,
        // qualunque sia la dimensione della finestra.
        Camera cam = Camera.main;
        float altezzaVista = (cam != null ? cam.orthographicSize : 6f) * 2f;
        float aspetto = (cam != null ? cam.aspect : 16f / 9f);
        float larghezzaVista = altezzaVista * aspetto;

        float larghezzaSprite = sprite.bounds.size.x;
        float altezzaSprite = sprite.bounds.size.y;

        // Prendo la scala piu' grande tra larghezza e altezza, cosi' non restano bordi vuoti.
        float scala = Mathf.Max(larghezzaVista / larghezzaSprite, altezzaVista / altezzaSprite) * 1.02f;
        sfondo.transform.localScale = new Vector3(scala, scala, 1f);
    }

    // ---- Astro ----

    static void CostruisciAstro()
    {
        GameObject a = new GameObject("Astro");
        a.transform.SetParent(contenitore.transform);
        a.transform.position = new Vector3(0f, 0f, 0f);
        a.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        SpriteRenderer sr = a.AddComponent<SpriteRenderer>();
        sr.sprite = FabbricaImmagini.CreaAstro();
        // sortingOrder alto: Astro deve stare sempre davanti agli altri
        sr.sortingOrder = 10;

        a.AddComponent<Astro>();
    }

    // ---- Asteroidi ----

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

            Asteroide a = go.AddComponent<Asteroide>();
            a.Inizializza(d.centro, d.dimensione);
        }
    }

    // ---- Bombe ----

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

    // ---- Caramelle ----

    static int CostruisciCaramelle(DatiLivello dati)
    {
        int totale;

        if (dati.caramelleCasuali > 0)
        {
            // Le metto a caso (livello 1)
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
            // Posizioni fisse (livelli 2 e 3)
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
        // Provo fino a 30 volte a trovare un posto buono:
        //  - non troppo vicino al centro (dove parte Astro)
        //  - non dentro un asteroide
        for (int tent = 0; tent < 30; tent++)
        {
            float x = Random.Range(-7f, 7f);
            float y = Random.Range(-3.5f, 4f);
            Vector2 p = new Vector2(x, y);

            if (p.magnitude < 1.2f) continue;
            if (DentroAsteroide(p, dati)) continue;
            return p;
        }
        // Se proprio non trovo (quasi impossibile)
        return new Vector2(3f, 3f);
    }

    static bool DentroAsteroide(Vector2 punto, DatiLivello dati)
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

    // ---- Chiave e porta (vengono create dopo) ----

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
        // y=2.0: cosi' resta sotto il "soffitto" dei livelli 2 e 3
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

    // Sceglie una posizione "buona" per la porta:
    // lontana dalla posizione attuale, fuori dagli asteroidi, lontana dalle bombe
    public static Vector2 ScegliPosizionePortaCasuale(Vector2 daEvitare)
    {
        if (livelloCorrente == null) return new Vector2(0f, 2f);

        for (int tent = 0; tent < 40; tent++)
        {
            float x = Random.Range(-5.5f, 5.5f);
            float y = Random.Range(-2.5f, 2.8f);
            Vector2 p = new Vector2(x, y);

            if (Vector2.Distance(p, daEvitare) < 3f) continue;
            if (DentroAsteroide(p, livelloCorrente)) continue;
            if (TroppoVicinoBomba(p)) continue;
            return p;
        }
        return new Vector2(4f, 2f); // fallback
    }

    static bool TroppoVicinoBomba(Vector2 punto)
    {
        for (int i = 0; i < Bomba.Tutte.Count; i++)
        {
            Bomba b = Bomba.Tutte[i];
            if (b == null) continue;
            if (Vector2.Distance(punto, b.transform.position) < 1.2f) return true;
        }
        return false;
    }

    // ---- Effetti speciali (festa ed esplosioni) ----

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


// =============================================================
// Classi che descrivono un livello (solo dati, niente logica)
// =============================================================

public class DatiLivello
{
    public string titolo;
    public string obiettivo;

    // Se caramelleCasuali > 0 le caramelle vengono messe a caso,
    // altrimenti uso la lista "caramelle" qui sotto.
    public int caramelleCasuali = 0;

    public List<DatiCaramella> caramelle = new List<DatiCaramella>();
    public List<DatiAsteroide> asteroidi = new List<DatiAsteroide>();
    public List<Vector2> bombe = new List<Vector2>();
}

public class DatiCaramella
{
    public Vector2 posizione;
    public Color colore;
}

public class DatiAsteroide
{
    public Vector2 centro;
    public Vector2 dimensione; // larghezza x altezza
    public Color colore;
}
