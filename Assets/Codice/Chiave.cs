using UnityEngine;

// Chiave dorata: appare dopo aver raccolto tutte le caramelle.
// Astro deve prenderla e portarla alla porta.
public class Chiave : MonoBehaviour
{
    // Riferimento alla chiave del livello (ce n'e' una sola alla volta)
    public static Chiave Istanza;

    public bool Raccolta;

    private Vector3 puntoSpawn;
    private float fase;

    // Se Astro entra in questo raggio, la chiave gli va incontro (effetto calamita)
    private const float RAGGIO_MAGNETE = 3.5f;
    // Se entra in questo raggio, viene presa direttamente
    private const float RAGGIO_PRESA = 1.8f;

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

    // Chiamata da Astro
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
            ComportamentoLibera();
        }
        else
        {
            SeguiAstro();
        }
    }

    // Prima di essere raccolta: ondeggia e si fa attirare da Astro
    void ComportamentoLibera()
    {
        // Posizione "ferma" con oscillazione verticale
        float t = Time.time * 2f + fase;
        Vector3 ferma = puntoSpawn + new Vector3(0f, Mathf.Sin(t) * 0.18f, 0f);

        Vector3 nuovaPos = ferma;

        if (Astro.Istanza != null)
        {
            Vector3 posAstro = Astro.Istanza.transform.position;
            float distanza = Vector2.Distance(transform.position, posAstro);

            if (distanza <= RAGGIO_PRESA)
            {
                // Astro molto vicino: la prendo subito
                Raccogli();
                return;
            }

            if (distanza <= RAGGIO_MAGNETE)
            {
                // Astro entro il raggio del magnete: la chiave gli va incontro
                float fattore = Mathf.InverseLerp(RAGGIO_MAGNETE, RAGGIO_PRESA, distanza);
                float velocita = Mathf.Lerp(2.5f, 9f, fattore);

                Vector3 obiettivo = new Vector3(posAstro.x, posAstro.y, ferma.z);
                nuovaPos = Vector3.Lerp(transform.position, obiettivo, Time.deltaTime * velocita);
            }
            else
            {
                // Fuori dal raggio: la chiave torna al punto di partenza
                nuovaPos = Vector3.Lerp(transform.position, ferma, Time.deltaTime * 3f);
            }
        }

        transform.position = nuovaPos;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 1.5f + fase) * 12f);
        float scala = 1.2f + Mathf.Sin(Time.time * 4f + fase) * 0.08f;
        transform.localScale = new Vector3(scala, scala, 1f);
    }

    // Dopo la raccolta: la chiave segue Astro con un piccolo ritardo
    void SeguiAstro()
    {
        if (Astro.Istanza == null) return;

        Vector3 obiettivo = Astro.Istanza.transform.position + new Vector3(0.55f, 0.55f, -0.4f);
        transform.position = Vector3.Lerp(transform.position, obiettivo, Time.deltaTime * 8f);

        float oscillazione = 25f + Mathf.Sin(Time.time * 6f) * 6f;
        transform.rotation = Quaternion.Euler(0f, 0f, oscillazione);
        transform.localScale = new Vector3(1.1f, 1.1f, 1f);
    }
}
