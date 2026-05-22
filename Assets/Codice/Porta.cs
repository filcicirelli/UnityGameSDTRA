using UnityEngine;

// =====================================================================
//  Porta
// ---------------------------------------------------------------------
//  La porta-portale compare insieme alla chiave. Per renderla un po'
//  piu' difficile, ogni 3 secondi si sposta in una nuova posizione
//  casuale (con una transizione morbida di circa mezzo secondo).
//
//  La porta si "sblocca" solo quando Astro la tocca PORTANDO la chiave.
//  Senza la chiave non succede niente.
// =====================================================================
public class Porta : MonoBehaviour
{
    public static Porta Istanza { get; private set; }

    // Quanto resta ferma in una posizione prima di spostarsi di nuovo
    public float secondiFerma = 3f;

    // Durata dell'animazione di spostamento
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

        // Piccolo "wobble" (la porta respira leggermente)
        float wobble = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
        transform.localScale = new Vector3(1.8f * wobble, 1.8f * wobble, 1f);

        timer += Time.deltaTime;

        if (!inMovimento)
        {
            // Sta ferma: aspetto i secondiFerma e poi avvio lo spostamento
            if (timer >= secondiFerma)
            {
                AvviaSpostamento();
            }
        }
        else
        {
            // Sta facendo il viaggio fra partenza e destinazione
            float progresso = Mathf.Clamp01(timer / secondiSpostamento);
            float smussato = Mathf.SmoothStep(0f, 1f, progresso);

            Vector3 nuovaPos = Vector3.Lerp(partenza, destinazione, smussato);
            transform.position = new Vector3(nuovaPos.x, nuovaPos.y, -0.5f);

            if (progresso >= 1f)
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

    // Chiamato da Astro: la porta si apre solo se Astro porta la chiave
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
