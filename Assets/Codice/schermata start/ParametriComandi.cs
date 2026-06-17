using UnityEngine;

// ============================================================================
// PAGINA DEI PARAMETRI DEI COMANDI
// ----------------------------------------------------------------------------
// Questa e' la "pagina dei valori" per il modo in cui si comanda il gioco
// (vedi Comandi.cs). Qui dentro NON c'e' logica: ci sono SOLO i numeri che si
// possono cambiare, come fa Impostazioni.cs per il gioco.
//
// Riguarda soprattutto il JOYSTICK (che muove un puntatore "a velocita'").
// Il mouse e la webcam non hanno bisogno di questi valori.
// ============================================================================
public static class ParametriComandi
{
    // ---- JOYSTICK (vale anche per le frecce e i tasti WASD) ----

    // Quanti pixel al secondo si muove il puntatore con la leva a fondo corsa.
    // Piu' alto = puntatore piu' veloce.
    public const float JOYSTICK_VELOCITA = 1000f;

    // Zona morta: ignoro i piccoli movimenti della leva, cosi' una leva un po'
    // storta a riposo non fa muovere il puntatore da solo.
    public const float JOYSTICK_ZONA_MORTA = 0.15f;

    // Se true inverto il su/giu' (alcuni preferiscono il comando "stile aereo").
    public static readonly bool JOYSTICK_INVERTI_Y = false;

    // ---- PULSANTI DEI PANNELLI (fine livello, game over, menu...) ----
    // Quei pulsanti si possono premere col clic del mouse OPPURE tenendoci sopra
    // Astro per un po'. Questo "dwell" serve a chi gioca con webcam o joystick,
    // che non ha il clic del mouse. Qui dico quanti secondi tenerlo fermo.
    public const float DWELL_SECONDI = 1.2f;
}
