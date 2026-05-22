using UnityEngine;

// Anello luminoso che cresce e svanisce.
// Lo creo quando una bomba viene toccata.
public class Esplosione : MonoBehaviour
{
    public float durata = 0.7f;
    public float scalaMax = 4.5f;

    private SpriteRenderer sr;
    private float eta;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * 0.2f; // parto piccolino
    }

    void Update()
    {
        eta = eta + Time.deltaTime;
        float k = Mathf.Clamp01(eta / durata);

        // Cresce velocemente all'inizio, poi rallenta
        float velocita = 1f - (1f - k) * (1f - k);
        float scala = Mathf.Lerp(0.2f, scalaMax, velocita);
        transform.localScale = new Vector3(scala, scala, 1f);

        // Sparisce piano piano (alpha da 1 a 0)
        if (sr != null)
        {
            Color col = sr.color;
            col.a = 1f - k;
            sr.color = col;
        }

        if (eta >= durata)
        {
            Destroy(gameObject);
        }
    }
}
