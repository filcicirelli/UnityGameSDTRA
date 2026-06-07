using UnityEngine;

// ============================================================================
// FABBRICA DEI SUONI
// ----------------------------------------------------------------------------
// Crea i suoni del feedback DA CODICE, esattamente come FabbricaImmagini crea
// le immagini da codice: non serve nessun file audio esterno.
//
// COME FUNZIONA (semplice):
// Un suono e' una lunga lista di numeri (i "campioni"), ognuno fra -1 e +1.
// Se quei numeri disegnano un'onda che sale e scende tante volte al secondo,
// l'orecchio sente una nota. Piu' l'onda e' veloce (piu' Hz), piu' la nota
// e' acuta. Per fare una piccola melodia mettiamo in fila piu' note.
// ============================================================================
public static class FabbricaSuoni
{
    private const int FREQUENZA = 44100; // campioni al secondo (qualita' CD)

    // Crea un suono fatto da una sequenza di note (una piccola melodia).
    //   nome      : nome del suono (serve solo a Unity)
    //   frequenze : le note in Hz, suonate una dopo l'altra
    //   durataNota: quanti secondi dura ogni nota
    //   ruvido    : se true il suono diventa un po' "sporco" (per l'errore)
    public static AudioClip CreaMelodia(string nome, float[] frequenze, float durataNota, bool ruvido)
    {
        int campioniNota = Mathf.Max(1, Mathf.RoundToInt(FREQUENZA * durataNota));
        int totale = campioniNota * frequenze.Length;
        float[] dati = new float[totale];

        for (int n = 0; n < frequenze.Length; n++)
        {
            float f = frequenze[n];
            for (int i = 0; i < campioniNota; i++)
            {
                float t = (float)i / FREQUENZA;     // tempo dentro la nota (secondi)
                float p = (float)i / campioniNota;  // avanzamento nella nota (0..1)

                // Onda base: una "sinusoide" (suono morbido e pulito)
                float onda = Mathf.Sin(2f * Mathf.PI * f * t);

                // Per l'errore aggiungo una seconda onda leggermente stonata:
                // le due onde "battono" insieme e il suono diventa sgradevole.
                if (ruvido)
                {
                    onda = 0.6f * onda + 0.4f * Mathf.Sin(2f * Mathf.PI * (f * 1.05f) * t);
                }

                // Inviluppo: il volume sale in fretta e poi scende piano, cosi'
                // la nota non fa "click" all'inizio e alla fine.
                onda = onda * Inviluppo(p);

                dati[n * campioniNota + i] = onda;
            }
        }

        AudioClip clip = AudioClip.Create(nome, totale, 1, FREQUENZA, false);
        clip.SetData(dati, 0);
        return clip;
    }

    // Forma del volume di una nota: parte da 0, sale, resta piena, poi torna a 0.
    static float Inviluppo(float p)
    {
        const float attacco  = 0.06f; // prima parte: il volume sale
        const float rilascio = 0.55f; // ultima parte: il volume scende

        if (p < attacco)       return p / attacco;           // salita
        if (p > 1f - rilascio) return (1f - p) / rilascio;   // discesa
        return 1f;                                            // pieno
    }
}
