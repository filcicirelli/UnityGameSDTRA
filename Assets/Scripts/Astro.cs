using UnityEngine;

// Astro, l'alieno verde al centro dello schermo.
// E' fermo, ma fa un piccolo "respiro" continuo e una breve animazione
// di gioia ogni volta che un cristallo viene raccolto.
public class Astro : MonoBehaviour
{
    private static Astro instance;

    private Vector3 basePos;
    private float happyTimer;

    void Awake()
    {
        instance = this;
        basePos = transform.position;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void Update()
    {
        // Respiro: scala leggermente in modo sinusoidale
        float breath = 1f + Mathf.Sin(Time.time * 2f) * 0.04f;

        // Saltino di gioia quando raccoglie un cristallo
        float happyScale = 1f;
        float happyOffsetY = 0f;
        if (happyTimer > 0f)
        {
            happyTimer -= Time.deltaTime;
            float k = Mathf.Clamp01(happyTimer / 0.35f);
            happyScale = 1f + (1f - k) * 0.0f + k * 0.20f;
            happyOffsetY = Mathf.Sin(k * Mathf.PI) * 0.25f;
        }

        transform.localScale = new Vector3(breath * happyScale, breath * happyScale, 1f);
        transform.position = basePos + new Vector3(0, happyOffsetY, 0);
    }

    // Punto unico di ingresso per "Astro fai un saltino di gioia".
    public static void NotifyHappyBounce()
    {
        if (instance != null) instance.happyTimer = 0.35f;
    }
}
