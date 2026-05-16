using UnityEngine;

// Il "Pianeta Amico" che appare quando la missione e' completata:
// arriva con un'animazione di pop-in e poi rimbalza dolcemente,
// salutando con dei piccoli movimenti.
public class FriendPlanet : MonoBehaviour
{
    private Vector3 basePos;
    private float lifeTime;
    public float targetScale = 2.4f;

    void Awake() { basePos = transform.position; transform.localScale = Vector3.zero; }

    void Update()
    {
        lifeTime += Time.deltaTime;

        // Pop-in: scala da 0 a targetScale nei primi 0.5s con piccolo "overshoot"
        float t = Mathf.Clamp01(lifeTime / 0.5f);
        float pop = (lifeTime < 0.5f)
            ? targetScale * (1f + 0.2f * Mathf.Sin(t * Mathf.PI)) * t
            : targetScale + 0.1f * Mathf.Sin(Time.time * 3f);

        transform.localScale = new Vector3(pop, pop, 1f);

        // Saluto: ondeggio orizzontale leggero
        transform.position = basePos + new Vector3(Mathf.Sin(Time.time * 1.5f) * 0.15f, 0, 0);
    }
}
