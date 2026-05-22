using UnityEngine;

// Porta che si sblocca quando Astro la tocca portando la chiave.
// Per renderla un po' difficile si sposta ogni tot secondi.
public class Porta : MonoBehaviour
{
    public static Porta Istanza;

    // Quanti secondi sta ferma in un posto prima di spostarsi
    public float secondiFerma = Impostazioni.PORTA_SECONDI_FERMA;

    // Durata dello spostamento
    public float secondiSpostamento = 0.5f;

    private Vector3 partenza;
    private Vector3 destinazione;
    private float timer;
    private bool inMovimento;
    private bool aperta;

    void Awake()
    {
        Istanza = this;
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
        transform.position = new Vector3(posizione.x, posizione.y, -0.5f);
        partenza = transform.position;
        destinazione = transform.position;
        inMovimento = false;
        timer = 0f;
    }

    void Update()
    {
        if (aperta) return;

        // Piccolo "respiro" della porta
        float wobble = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
        transform.localScale = new Vector3(1.8f * wobble, 1.8f * wobble, 1f);

        timer = timer + Time.deltaTime;

        if (!inMovimento)
        {
            // Aspetto qualche secondo e poi mi sposto
            if (timer >= secondiFerma)
            {
                AvviaSpostamento();
            }
        }
        else
        {
            // In viaggio fra partenza e destinazione
            float t = Mathf.Clamp01(timer / secondiSpostamento);
            float smussato = Mathf.SmoothStep(0f, 1f, t);

            Vector3 nuova = Vector3.Lerp(partenza, destinazione, smussato);
            transform.position = new Vector3(nuova.x, nuova.y, -0.5f);

            if (t >= 1f)
            {
                inMovimento = false;
                timer = 0f;
            }
        }
    }

    void AvviaSpostamento()
    {
        partenza = transform.position;
        Vector2 prossima = CaricatoreLivelli.ScegliPosizionePortaCasuale(transform.position);
        destinazione = new Vector3(prossima.x, prossima.y, -0.5f);

        inMovimento = true;
        timer = 0f;
    }

    // Chiamata da Astro quando entra in contatto.
    // Si apre solo se Astro porta la chiave.
    public void Sblocca()
    {
        if (aperta) return;
        aperta = true;

        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaPortaRaggiunta();
        }
    }
}
