using UnityEngine;

// Pianeta sorridente che appare a fine missione.
// All'inizio fa un effetto pop-in, poi sta li' a ondeggiare.
public class PianetaAmico : MonoBehaviour
{
    public float scalaFinale = 2.4f;

    private Vector3 posizioneBase;
    private float vita;

    void Awake()
    {
        posizioneBase = transform.position;
        transform.localScale = Vector3.zero; // parte invisibile
    }

    void Update()
    {
        vita = vita + Time.deltaTime;

        float scala;
        if (vita < 0.5f)
        {
            // Effetto "pop": cresce con un piccolo rimbalzo
            float t = vita / 0.5f;
            float rimbalzo = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
            scala = scalaFinale * rimbalzo * t;
        }
        else
        {
            // Dopo respira piano
            scala = scalaFinale + 0.1f * Mathf.Sin(Time.time * 3f);
        }

        transform.localScale = new Vector3(scala, scala, 1f);

        // Si muove un po' a destra e a sinistra, come a salutare
        float dx = Mathf.Sin(Time.time * 1.5f) * 0.15f;
        transform.position = posizioneBase + new Vector3(dx, 0f, 0f);
    }
}
