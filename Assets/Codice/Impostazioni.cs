// Qui ci sono tutti i numeri del gioco che si possono modificare.
// Se il gioco e' troppo difficile o troppo facile cambia i valori qua sotto.
public static class Impostazioni
{
    // ---- VITE E TEMPO ----
    public const int VITE = 5;             // vite all'inizio di ogni livello
    public const float TEMPO_LIVELLO = 60f; // secondi a disposizione

    // ---- ASTRO ----
    // Sono i "raggi" entro i quali Astro raccoglie/tocca un oggetto.
    // Piu' grandi = piu' facile.
    public const float RAGGIO_CARAMELLA = 0.85f;
    public const float RAGGIO_CHIAVE    = 1.80f;
    public const float RAGGIO_PORTA     = 1.80f;
    public const float RAGGIO_BOMBA     = 0.55f;

    // ---- PUNTI ----
    public const int PUNTI_CARAMELLA   = 10; // punti guadagnati
    public const int PUNTI_PERSI_HIT   = 5;  // punti persi se prendi un asteroide

    // ---- CARAMELLE NEL LIVELLO 1 (generate a caso) ----
    public const int CARAMELLE_LIV1 = 10;

    // ---- PORTA ----
    // Ogni quanti secondi la porta si sposta da sola
    public const float PORTA_SECONDI_FERMA = 3f;

    // ---- PERIODO INIZIALE "PRONTI..." ----
    public const float TEMPO_PRONTI = 1.5f;
}
