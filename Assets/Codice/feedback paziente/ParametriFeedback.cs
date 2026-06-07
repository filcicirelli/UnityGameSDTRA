using UnityEngine;

// ============================================================================
// PAGINA DEI PARAMETRI DEL FEEDBACK
// ----------------------------------------------------------------------------
// Questa e' la "pagina dei valori" del feedback per il paziente.
// Qui dentro NON c'e' logica: ci sono SOLO i numeri che si possono cambiare
// per regolare il feedback, esattamente come fa Impostazioni.cs per il gioco.
//
// Tutto e' pensato per la RIABILITAZIONE: il feedback deve INCORAGGIARE.
// Per questo il suono dell'errore e' piu' basso e gentile di quello giusto:
// serve a far capire lo sbaglio, non a spaventare il paziente.
//
// Un terapista puo' modificare questi valori senza toccare il resto del codice.
// ============================================================================
public static class ParametriFeedback
{
    // ---- INTERRUTTORI GENERALI ----
    // Si possono spegnere separatamente suono ed effetti visivi.
    // Esempio: un paziente sensibile ai suoni -> SUONO_ATTIVO = false.
    public const bool SUONO_ATTIVO  = true;
    public const bool VISIVO_ATTIVO = true;

    // ---- VOLUMI (0 = muto, 1 = massimo) ----
    public const float VOLUME_GENERALE  = 0.90f; // volume di TUTTO il feedback
    public const float VOLUME_GIUSTO    = 0.80f; // azioni corrette (suono gradevole)
    public const float VOLUME_SBAGLIATO = 0.45f; // errori: piu' basso, cosi' e' gentile

    // ---- NOTE DEI SUONI (in Hz) ----
    // Sono semplici note musicali. Note che SALGONO = suono allegro;
    // note che SCENDONO = suono "di errore".
    // Caramella: tre note che salgono (do - mi - sol)
    public static readonly float[] NOTE_CARAMELLA = { 523.25f, 659.25f, 783.99f };
    // Chiave: piu' acute e brillanti (sol - do - mi)
    public static readonly float[] NOTE_CHIAVE    = { 783.99f, 1046.50f, 1318.51f };
    // Vittoria: una piccola fanfara (do - mi - sol - do alto)
    public static readonly float[] NOTE_VITTORIA  = { 523.25f, 659.25f, 783.99f, 1046.50f };
    // Errore: due note che SCENDONO (suono "sbagliato", ma non aggressivo)
    public static readonly float[] NOTE_ERRORE    = { 220.00f, 174.61f };

    // ---- DURATA DELLE NOTE (secondi per ogni nota) ----
    public const float DURATA_NOTA_GIUSTO = 0.10f; // note corte e vivaci
    public const float DURATA_NOTA_ERRORE = 0.18f; // note piu' lunghe e morbide

    // ---- ASTRO SI GONFIA (feedback POSITIVO) ----
    // Quando il paziente fa la cosa giusta, Astro si gonfia e poi torna normale.
    public const float GONFIA_QUANTITA = 0.45f; // 0.45 = fino a +45% di dimensione
    public const float GONFIA_DURATA   = 0.45f; // durata del gonfiamento (secondi)

    // Bagliore di gioia: per un istante Astro si colora di verde/oro.
    public static readonly Color COLORE_GIOIA = new Color(0.70f, 1f, 0.55f);
    public const float GIOIA_INTENSITA = 0.80f; // quanto e' forte il bagliore (0..1)

    // ---- ASTRO SI SGONFIA / SCHIACCIA (feedback NEGATIVO) ----
    // Quando il paziente sbaglia, Astro si appiattisce (largo e basso) e torna.
    public const float SCHIACCIA_QUANTITA = 0.30f; // 0.30 = +30% largo, -30% alto
    public const float SCHIACCIA_DURATA   = 0.30f; // durata dello schiacciamento (secondi)

    // Lampeggio rosso quando si sbaglia.
    public static readonly Color COLORE_ERRORE = new Color(1f, 0.30f, 0.30f);
}
