using UnityEngine;

// Pioggia di coriandoli colorati per festeggiare la missione completata.
public class Coriandoli : MonoBehaviour
{
    public int quantita = 80;
    public float durata = 4f;

    // Colori dei coriandoli
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
        // Creo tutti i coriandoli subito
        for (int i = 0; i < quantita; i++)
        {
            CreaUno();
        }
        // Dopo un po' la festa finisce e tutto sparisce
        Destroy(gameObject, durata + 1.5f);
    }

    void CreaUno()
    {
        GameObject go = new GameObject("Coriandolo");
        go.transform.SetParent(transform, false);

        // Parto dall'alto, posizione orizzontale a caso
        float x = Random.Range(-8f, 8f);
        go.transform.position = new Vector3(x, 6.5f, -3f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Color colore = COLORI[Random.Range(0, COLORI.Length)];
        sr.sprite = FabbricaImmagini.CreaQuadratoPieno(colore);
        sr.sortingOrder = 10;

        float scala = Random.Range(0.12f, 0.22f);
        go.transform.localScale = new Vector3(scala, scala * 0.6f, 1f);

        PezzoCoriandolo p = go.AddComponent<PezzoCoriandolo>();
        p.velocita = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1f, -3f));
        p.velocitaAngolare = Random.Range(-360f, 360f);
        p.vita = durata;
    }
}

// Singolo coriandolo: cade, ondeggia e gira.
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
        eta = eta + Time.deltaTime;

        // Gravita' leggera
        velocita.y = velocita.y - 1.2f * Time.deltaTime;

        // Movimento + un po' di ondeggio
        float dx = velocita.x * Time.deltaTime + Mathf.Sin(Time.time * 4f + fase) * 0.01f;
        float dy = velocita.y * Time.deltaTime;
        transform.position = transform.position + new Vector3(dx, dy, 0f);
        transform.Rotate(0f, 0f, velocitaAngolare * Time.deltaTime);

        if (eta >= vita)
        {
            Destroy(gameObject);
        }
    }
}
