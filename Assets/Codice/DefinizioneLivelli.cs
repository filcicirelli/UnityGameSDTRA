using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  DefinizioneLivelli
// ---------------------------------------------------------------------
//  Qui descrivo "come e' fatto" ciascun livello del gioco: titolo,
//  obiettivo, posizione di caramelle, asteroidi (le barriere) e bombe.
//
//  Ho preferito tenere questi dati in chiaro nel codice invece di
//  caricarli da un file: cosi' all'esame posso aprirli e mostrarli
//  riga per riga.
//
//  Ogni livello e' costruito a mano nel proprio metodo per renderlo
//  facile da capire e da modificare.
// =====================================================================
public static class DefinizioneLivelli
{
    public const int Conteggio = 3;

    public static DatiLivello Ottieni(int indice)
    {
        // Uno switch tradizionale: facile da leggere e da spiegare.
        switch (indice)
        {
            case 0:  return CostruisciLivello1();
            case 1:  return CostruisciLivello2();
            case 2:  return CostruisciLivello3();
            default: return null;
        }
    }

    // ----- Palette dei colori delle caramelle -----
    static readonly Color[] COLORI_CARAMELLE = new Color[]
    {
        new Color(1f,    0.85f, 0.30f),
        new Color(0.40f, 0.85f, 1f),
        new Color(0.95f, 0.45f, 0.85f),
        new Color(0.55f, 1f,    0.55f),
        new Color(1f,    0.55f, 0.30f),
    };

    // Colore degli asteroidi (sempre lo stesso, sembra una roccia spaziale)
    static readonly Color COLORE_ASTEROIDE = new Color(0.55f, 0.50f, 0.45f);

    // =================================================================
    //  LIVELLO 1 - Primo volo
    //  Spazio aperto: 10 caramelle posizionate in modo casuale.
    //  Niente ostacoli, serve solo a imparare a muovere Astro.
    // =================================================================
    static DatiLivello CostruisciLivello1()
    {
        DatiLivello livello = new DatiLivello();
        livello.indice = 0;
        livello.titolo = "LIVELLO 1 - PRIMO VOLO";
        livello.obiettivo = "RACCOGLI 10 CARAMELLE";
        livello.caramelleCasuali = 10;
        return livello;
    }

    // =================================================================
    //  LIVELLO 2 - Tra gli asteroidi
    //  Aggiungo delle barriere a rettangolo che fanno male se toccate.
    //  Devo passare nei "varchi" per raccogliere le caramelle.
    // =================================================================
    static DatiLivello CostruisciLivello2()
    {
        DatiLivello livello = new DatiLivello();
        livello.indice = 1;
        livello.titolo = "LIVELLO 2 - TRA GLI ASTEROIDI";
        livello.obiettivo = "EVITA GLI ASTEROIDI";

        // Cornice "a corridoio": due muri verticali ai lati,
        // un soffitto in alto e due ostacoli al centro.
        livello.asteroidi.Add(NuovoAsteroide(-7.5f,  0.0f, 1.2f, 5.0f));
        livello.asteroidi.Add(NuovoAsteroide( 7.5f,  0.0f, 1.2f, 5.0f));
        livello.asteroidi.Add(NuovoAsteroide( 0.0f,  4.5f, 9.0f, 1.0f));
        livello.asteroidi.Add(NuovoAsteroide(-2.5f,  1.0f, 1.5f, 1.5f));
        livello.asteroidi.Add(NuovoAsteroide( 2.5f, -1.5f, 1.8f, 1.0f));

        // 10 caramelle posizionate "a mano" nei varchi tra le barriere
        Vector2[] posizioniCaramelle = new Vector2[]
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
        AggiungiCaramelle(livello, posizioniCaramelle);

        return livello;
    }

    // =================================================================
    //  LIVELLO 3 - Campo minato
    //  Stesse barriere del livello 2, ma in piu' ci sono 4 bombe.
    //  Toccare una bomba fa perdere una vita.
    // =================================================================
    static DatiLivello CostruisciLivello3()
    {
        DatiLivello livello = new DatiLivello();
        livello.indice = 2;
        livello.titolo = "LIVELLO 3 - CAMPO MINATO";
        livello.obiettivo = "ATTENZIONE ALLE BOMBE";

        // Riuso le stesse barriere del livello 2
        livello.asteroidi.Add(NuovoAsteroide(-7.5f,  0.0f, 1.2f, 5.0f));
        livello.asteroidi.Add(NuovoAsteroide( 7.5f,  0.0f, 1.2f, 5.0f));
        livello.asteroidi.Add(NuovoAsteroide( 0.0f,  4.5f, 9.0f, 1.0f));
        livello.asteroidi.Add(NuovoAsteroide(-2.5f,  1.0f, 1.5f, 1.5f));
        livello.asteroidi.Add(NuovoAsteroide( 2.5f, -1.5f, 1.8f, 1.0f));

        // 4 bombe, lontane dal centro (dove Astro fa spawn).
        // Cosi' il giocatore ha sempre un po' di spazio per orientarsi.
        livello.bombe.Add(new Vector2(-6.0f,  2.5f));
        livello.bombe.Add(new Vector2( 6.0f,  2.5f));
        livello.bombe.Add(new Vector2(-4.0f, -3.5f));
        livello.bombe.Add(new Vector2( 4.0f, -3.5f));

        // Caramelle nei "vicoli" liberi (lontane dalle bombe)
        Vector2[] posizioniCaramelle = new Vector2[]
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
        AggiungiCaramelle(livello, posizioniCaramelle);

        return livello;
    }

    // -----------------------------------------------------------------
    //  Metodi di supporto
    // -----------------------------------------------------------------

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

// =====================================================================
//  Tipi di dato per descrivere un livello
//  (li tengo nello stesso file per averli tutti sotto mano)
// =====================================================================

public class DatiLivello
{
    public int indice;
    public string titolo;
    public string obiettivo;

    // Se "caramelleCasuali" e' maggiore di 0, le caramelle vengono
    // generate in posizioni random; altrimenti uso la lista "caramelle".
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
    public Vector2 dimensione;   // larghezza x altezza in unita' di mondo
    public Color colore;
}
