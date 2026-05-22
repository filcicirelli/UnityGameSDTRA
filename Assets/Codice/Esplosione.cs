using UnityEngine;

// =====================================================================
//  Esplosione
// ---------------------------------------------------------------------
//  Anellino luminoso che cresce velocemente e poi svanisce.
//  Niente Animator, niente sistema particellare: faccio tutto con
//  due animazioni "a mano" (scala + alpha), cosi' resto fedele alla
//  filosofia "tutto da codice".
// =====================================================================
public class Esplosione : MonoBehaviour
{
    public float durata = 0.7f;
    public float scalaMax = 4.5f;

    private SpriteRenderer sr;
    private float eta;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // Parto piccolo piccolo
        transform.localScale = Vector3.one * 0.2f;
    }

    void Update()
    {
        eta += Time.deltaTime;
        float k = Mathf.Clamp01(eta / durata);

        // Easing "out-quadratic": parte veloce, poi rallenta
        float easing = 1f - Mathf.Pow(1f - k, 2f);
        float scala = Mathf.Lerp(0.2f, scalaMax, easing);
        transform.localScale = new Vector3(scala, scala, 1f);

        // Fade out: l'alpha va da 1 a 0
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, k);
            sr.color = c;
        }

        if (eta >= durata)
        {
            Destroy(gameObject);
        }
    }
}
