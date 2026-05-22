using UnityEngine;

// =====================================================================
//  Chiave
// ---------------------------------------------------------------------
//  La chiave dorata compare al centro della scena dopo che Astro ha
//  raccolto tutte le caramelle del livello.
//
//  Tre comportamenti principali:
//      1) Idle: oscilla dolcemente nello spawn point
//      2) Magnetica: se Astro le si avvicina, "vola" verso di lui
//      3) Trascinata: dopo essere stata raccolta, segue Astro
//         con un piccolo ritardo (lag) per un effetto carino
//
//  Il magnetismo e' importante per la riabilitazione: il giocatore
//  non deve "centrare" la chiave con precisione, basta passare vicino.
// =====================================================================
public class Chiave : MonoBehaviour
{
    public static Chiave Istanza { get; private set; }

    public bool Raccolta { get; private set; }

    private Vector3 puntoSpawn;
    private float fase;

    // Costanti del magnetismo
    private const float RAGGIO_MAGNETE = 3.5f;   // dentro questo raggio "sente" Astro
    private const float RAGGIO_AGGANCIO = 1.8f;  // dentro: aggancio automatico

    void Awake()
    {
        Istanza = this;
        fase = Random.Range(0f, Mathf.PI * 2f);
    }

    void OnDestroy()
    {
        if (Istanza == this)
        {
            Istanza = null;
        }
    }

    public void Inizializza(Vector2 posizione)
    {
        puntoSpawn = new Vector3(posizione.x, posizione.y, -0.4f);
        transform.position = puntoSpawn;
    }

    // Chiamato da Astro quando entra nel raggio
    public void Raccogli()
    {
        if (Raccolta) return;
        Raccolta = true;

        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaChiaveRaccolta();
        }
    }

    void Update()
    {
        if (!Raccolta)
        {
            AggiornaIdle();
        }
        else
        {
            AggiornaSegui();
        }
    }

    // ------------------------------------------------------------
    //  Comportamento "idle" prima della raccolta (con magnetismo)
    // ------------------------------------------------------------
    void AggiornaIdle()
    {
        // Posizione "base": piccola oscillazione verticale
        float t = Time.time * 2f + fase;
        Vector3 idle = puntoSpawn + new Vector3(0f, Mathf.Sin(t) * 0.18f, 0f);

        Vector3 nuovaPos = idle;

        if (Astro.Istanza != null)
        {
            Vector3 posAstro = Astro.Istanza.transform.position;
            float distanza = Vector2.Distance(transform.position, posAstro);

            if (distanza <= RAGGIO_AGGANCIO)
            {
                // Astro e' molto vicino: aggancio diretto
                Raccogli();
                return;
            }

            if (distanza <= RAGGIO_MAGNETE)
            {
                // Astro e' nel raggio del magnete: la chiave si avvicina
                float fattore = Mathf.InverseLerp(RAGGIO_MAGNETE, RAGGIO_AGGANCIO, distanza);
                float velocita = Mathf.Lerp(2.5f, 9f, fattore);

                Vector3 obiettivo = new Vector3(posAstro.x, posAstro.y, idle.z);
                nuovaPos = Vector3.Lerp(transform.position, obiettivo, Time.deltaTime * velocita);
            }
            else
            {
                // Fuori dal raggio del magnete: torna piano al punto di idle
                nuovaPos = Vector3.Lerp(transform.position, idle, Time.deltaTime * 3f);
            }
        }

        transform.position = nuovaPos;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 1.5f + fase) * 12f);
        float scala = 1.2f + Mathf.Sin(Time.time * 4f + fase) * 0.08f;
        transform.localScale = new Vector3(scala, scala, 1f);
    }

    // ------------------------------------------------------------
    //  Comportamento dopo la raccolta: segue Astro con un piccolo lag
    // ------------------------------------------------------------
    void AggiornaSegui()
    {
        if (Astro.Istanza == null) return;

        Vector3 obiettivo = Astro.Istanza.transform.position
                            + new Vector3(0.55f, 0.55f, -0.4f);
        transform.position = Vector3.Lerp(transform.position, obiettivo, Time.deltaTime * 8f);

        float oscillazione = 25f + Mathf.Sin(Time.time * 6f) * 6f;
        transform.rotation = Quaternion.Euler(0f, 0f, oscillazione);
        transform.localScale = new Vector3(1.1f, 1.1f, 1f);
    }
}
