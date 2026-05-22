using UnityEngine;

// Chiave dorata. Compare quando tutte le caramelle del livello
// sono state raccolte. Astro la prende toccandola; una volta presa
// segue Astro con un lieve ritardo (effetto "trascinata").
public class Key : MonoBehaviour
{
    public static Key Instance { get; private set; }

    public bool PickedUp { get; private set; }

    private Vector3 spawnPos;
    private float phase;

    void Awake()
    {
        Instance = this;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Init(Vector2 position)
    {
        spawnPos = new Vector3(position.x, position.y, -0.4f);
        transform.position = spawnPos;
    }

    // Astro l'ha toccata: ora la "trascina" lui.
    public void PickUp()
    {
        if (PickedUp) return;
        PickedUp = true;
        if (SpaceGameManager.Instance != null)
            SpaceGameManager.Instance.OnKeyPickedUp();
    }

    void Update()
    {
        if (!PickedUp)
        {
            // Posizione "idle" base: oscilla intorno allo spawn.
            float t = Time.time * 2f + phase;
            Vector3 idle = spawnPos + new Vector3(0, Mathf.Sin(t) * 0.18f, 0);

            // Effetto magnetico: se Astro e' vicino, la chiave gli vola incontro.
            // Cosi' i bambini non devono "centrare" il bersaglio: basta passargli
            // vicino e la chiave si attacca da sola.
            Vector3 pos = idle;
            if (Astro.Instance != null)
            {
                Vector3 ap = Astro.Instance.transform.position;
                float dist = Vector2.Distance(transform.position, ap);
                const float magnetRange = 3.5f;   // entro questo raggio la chiave "sente" Astro
                const float snapRange   = 1.8f;   // dentro: aggancio automatico

                if (dist <= snapRange)
                {
                    PickUp();
                    return;
                }
                if (dist <= magnetRange)
                {
                    // Piu' Astro e' vicino, piu' la chiave accelera verso di lui.
                    float pull = Mathf.InverseLerp(magnetRange, snapRange, dist); // 0..1
                    float speed = Mathf.Lerp(2.5f, 9f, pull);
                    pos = Vector3.Lerp(transform.position,
                                       new Vector3(ap.x, ap.y, idle.z),
                                       Time.deltaTime * speed);
                }
                else
                {
                    // Fuori range: torna dolcemente verso la posizione idle.
                    pos = Vector3.Lerp(transform.position, idle, Time.deltaTime * 3f);
                }
            }
            transform.position = pos;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 1.5f + phase) * 12f);
            float s = 1.2f + Mathf.Sin(Time.time * 4f + phase) * 0.08f;
            transform.localScale = new Vector3(s, s, 1f);
        }
        else if (Astro.Instance != null)
        {
            // Segue Astro con un piccolo lag: si vede "trascinare".
            Vector3 target = Astro.Instance.transform.position + new Vector3(0.55f, 0.55f, -0.4f);
            transform.position = Vector3.Lerp(transform.position, target,
                                              Time.deltaTime * 8f);
            transform.rotation = Quaternion.Euler(0, 0,
                25f + Mathf.Sin(Time.time * 6f) * 6f);
            transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        }
    }
}
