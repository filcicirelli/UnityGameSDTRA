using UnityEngine;

// ============================================================================
// PAGINA DEI PARAMETRI DEL COMANDO CON LA WEBCAM
// ----------------------------------------------------------------------------
// Questa e' la "pagina dei valori" della modalita' in cui si gioca muovendo
// il dito davanti alla webcam, al posto del mouse.
// Qui dentro NON c'e' logica: ci sono SOLO i numeri/valori che si possono
// cambiare, esattamente come fa Impostazioni.cs per il gioco.
//
// COME FUNZIONA (in breve):
// non "riconosciamo il dito" (servirebbe l'intelligenza artificiale), ma
// seguiamo un EVIDENZIATORE FLUO tenuto in mano: il paziente muove un
// evidenziatore (verde, giallo o fucsia) e il gioco segue quel colore.
// Gli evidenziatori vanno benissimo perche' sono molto piu' ACCESI (saturi)
// della pelle: cosi' il gioco non confonde piu' il colore con il viso.
// Semplice, robusto e senza librerie esterne.
//
// La modalita' "dito" si sceglie nella SCHERMATA INIZIALE (vedi SchermataStart).
//
// Un terapista puo' regolare questi valori senza toccare il resto del codice.
// ============================================================================
public static class ParametriWebcam
{
    // ---- COLORI DA SEGUIRE ----
    // Il gioco segue UNO QUALSIASI di questi colori (l'evidenziatore che hai):
    // verde fluo, giallo fluo o fucsia. Sono colori molto accesi, ben diversi
    // dalla pelle. Per aggiungerne/cambiarne uno basta modificare la lista.
    public static readonly Color[] COLORI_EVIDENZIATORE = new Color[]
    {
        new Color(0.20f, 1f,    0.20f), // verde fluo
        new Color(0.85f, 1f,    0.10f), // giallo fluo
        new Color(1f,    0.10f, 0.70f), // fucsia
    };

    // Quanto la TINTA del pixel puo' essere diversa da un colore della lista e
    // venire comunque accettata. La tinta e' un cerchio (0..1): 0 = solo
    // identico, ~0.08 = un po' di margine, 0.5 = mezza ruota dei colori.
    public const float TOLLERANZA_TINTA = 0.08f;
    // Il colore deve essere abbastanza ACCESO (saturo). Tenuta ALTA apposta:
    // la pelle/il viso sono poco saturi e cosi' vengono scartati, mentre un
    // evidenziatore fluo e' molto saturo e passa.
    public const float SATURAZIONE_MINIMA = 0.55f;
    // ...e abbastanza luminoso: cosi' scarto le ombre nere.
    public const float LUMINOSITA_MINIMA = 0.25f;

    // ---- WEBCAM ----
    // Risoluzione bassa = analisi veloce (e basta e avanza per seguire un colore).
    public const int LARGHEZZA_RICHIESTA = 320;
    public const int ALTEZZA_RICHIESTA   = 240;
    public const int FPS_RICHIESTI       = 30;

    // Specchia l'immagine come uno specchio: muovi il dito a destra -> il
    // puntatore va a destra (senza, andrebbe al contrario).
    public const bool SPECCHIA = true;

    // Analizzo 1 pixel ogni PASSO_ANALISI: piu' alto = piu' veloce ma meno preciso.
    public const int PASSO_ANALISI = 2;
    // Quanti pixel del colore giusto servono per dire "ho visto il dito".
    // Piu' alto = ignora puntini sparsi, ma chiede un oggetto piu' grande.
    public const int PIXEL_MINIMI = 12;

    // ---- MOVIMENTO ----
    // Morbidezza del puntatore: 0 = segue di colpo (scattoso e reattivo),
    // valori vicini a 1 = molto morbido e calmo (utile se la mano trema).
    public const float MORBIDEZZA = 0.5f;

    // ---- ANTEPRIMA A SCHERMO ----
    // Mostra in un angolo cio' che vede la webcam, con un mirino sul dito:
    // aiuta il paziente a capire dove sta puntando.
    public const bool MOSTRA_ANTEPRIMA   = true;
    public const float ANTEPRIMA_LARGHEZZA = 240f; // larghezza in pixel sullo schermo
    public const int ANTEPRIMA_MARGINE     = 20;   // distanza dal bordo
}
