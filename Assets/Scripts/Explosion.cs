using UnityEngine;

// Animazione di esplosione: l'anello cresce rapidamente e svanisce.
// Niente Animator o particelle: solo trasformazioni di scala e alpha
// per restare nello stile pixel-art "da spiegare a parole".
public class Explosion : MonoBehaviour
{
    public float duration = 0.7f;
    public float maxScale = 4.5f;

    private SpriteRenderer sr;
    private float age;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * 0.2f;
    }

    void Update()
    {
        age += Time.deltaTime;
        float k = Mathf.Clamp01(age / duration);

        // Scala con easing (parte veloce, poi rallenta)
        float ease = 1f - Mathf.Pow(1f - k, 2f);
        float s = Mathf.Lerp(0.2f, maxScale, ease);
        transform.localScale = new Vector3(s, s, 1f);

        // Fade out
        if (sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, k);
            sr.color = c;
        }

        if (age >= duration) Destroy(gameObject);
    }
}
