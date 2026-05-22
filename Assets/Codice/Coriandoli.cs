using UnityEngine;

// =====================================================================
//  Coriandoli
// ---------------------------------------------------------------------
//  Pioggia di coriandoli colorati che parte alla "missione completata".
//  Per restare nello stile semplice del progetto NON uso un sistema
//  particellare: creo tanti piccoli quadrati (SpriteRenderer) e li
//  animo a mano nel componente PezzoCoriandolo.
// =====================================================================
public class Coriandoli : MonoBehaviour
{
    public int quantita = 80;
    public float durata = 4f;

    static readonly Color[] COLORI = new Color[]
    {
        new Color(1f,    0.30f, 0.40f),
        new Color(0.40f, 0.80f, 1f),
        new Color(1f,    0.85f, 0.20f),
        new Color(0.55f, 1f,    0.55f),
        new Color(0.95f, 0.55f, 1f),
    };

    void Start()
    {
        // Creo i coriandoli in un colpo solo all'inizio
        for (int i = 0; i < quantita; i++)
        {
            CreaPezzo();
        }
        // Auto-distruzione dopo un po' (la festa non dura per sempre)
        Destroy(gameObject, durata + 1.5f);
    }

    void CreaPezzo()
    {
        GameObject go = new GameObject("Coriandolo");
        go.transform.SetParent(transform, false);

        // Parto dal bordo alto, posizione orizzontale casuale
        float x = Random.Range(-8f, 8f);
        go.transform.position = new Vector3(x, 6.5f, -3f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Color colore = COLORI[Random.Range(0, COLORI.Length)];
        sr.sprite = FabbricaImmagini.CreaQuadratoPieno(colore);
        sr.sortingOrder = 10;

        float scala = Random.Range(0.12f, 0.22f);
        go.transform.localScale = new Vector3(scala, scala * 0.6f, 1f);

        PezzoCoriandolo pezzo = go.AddComponent<PezzoCoriandolo>();
        pezzo.velocita = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1f, -3f));
        pezzo.velocitaAngolare = Random.Range(-360f, 360f);
        pezzo.vita = durata;
    }
}

// =====================================================================
//  PezzoCoriandolo
// ---------------------------------------------------------------------
//  Singolo coriandolo: cade per gravita' debole, ondeggia orizzontalmente
//  e ruota su se stesso. Quando il suo tempo finisce, sparisce.
// =====================================================================
public class PezzoCoriandolo : MonoBehaviour
{
    public Vector2 velocita;
    public float velocitaAngolare;
    public float vita;

    private float eta;
    private float fase;

    void Awake()
    {
        fase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        eta += Time.deltaTime;

        // Gravita' leggera (acceleratore verso il basso)
        velocita.y -= 1.2f * Time.deltaTime;

        // Movimento + leggero ondeggio orizzontale
        Vector3 passo = new Vector3(
            velocita.x * Time.deltaTime + Mathf.Sin(Time.time * 4f + fase) * 0.01f,
            velocita.y * Time.deltaTime,
            0f);
        transform.position += passo;
        transform.Rotate(0f, 0f, velocitaAngolare * Time.deltaTime);

        if (eta >= vita)
        {
            Destroy(gameObject);
        }
    }
}
