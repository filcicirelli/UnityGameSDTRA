using System.Collections.Generic;
using UnityEngine;

// Qui ci sono i livelli del gioco.
// Per ogni livello dico: il titolo, l'obiettivo, dove vanno
// le caramelle, gli asteroidi e le bombe.
//
// Per AGGIUNGERE un livello: aggiungere un case nello switch sotto e
// scrivere un nuovo metodo CostruisciLivelloN() simile agli altri.
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
        l.indice = 0;
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
        l.indice = 1;
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
        l.indice = 2;
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

// ---- Classi per descrivere un livello ----

public class DatiLivello
{
    public int indice;
    public string titolo;
    public string obiettivo;

    // Se caramelleCasuali > 0 le caramelle vengono messe a caso,
    // altrimenti uso la lista "caramelle" qui sotto.
    public int caramelleCasuali = 0;

    public List<DatiCaramella> caramelle = new List<DatiCaramella>();
    public List<DatiAsteroide> asteroidi = new List<DatiAsteroide>();
    public List<Vector2> bombe = new List<Vector2>();

    public int NumeroCaramelle
    {
        get
        {
            if (caramelleCasuali > 0) return caramelleCasuali;
            return caramelle.Count;
        }
    }
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
