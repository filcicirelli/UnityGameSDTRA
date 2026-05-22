using System.Collections.Generic;
using UnityEngine;

// Caramella spaziale. Si raccoglie passando vicino con Astro.
// La galleggiare nello spazio con un piccolo movimento sinusoidale.
public class Caramella : MonoBehaviour
{
    // Lista di tutte le caramelle attive nel livello
    public static List<Caramella> Attive = new List<Caramella>();

    private Vector3 punto;   // posizione attorno a cui oscilla
    private float fase;
    private bool raccolta;

    void OnEnable()
    {
        Attive.Add(this);
    }

    void OnDisable()
    {
        Attive.Remove(this);
    }

    public void Inizializza(Vector3 posizione)
    {
        punto = posizione;
        transform.position = posizione;
        // Fase diversa per ogni caramella, cosi' non si muovono tutte uguali
        fase = Random.Range(0f, Mathf.PI * 2f);
    }

    // Chiamata da Astro quando si avvicina abbastanza
    public void Raccogli()
    {
        if (raccolta) return;
        raccolta = true;

        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaCaramellaRaccolta();
        }
        Astro.NotificaSaltoFelice();
        Destroy(gameObject);
    }

    void Update()
    {
        // Galleggiamento + scintillio
        float t = Time.time * 1.5f + fase;

        float dx = Mathf.Sin(t) * 0.10f;
        float dy = Mathf.Cos(t * 0.8f) * 0.12f;
        transform.position = punto + new Vector3(dx, dy, 0f);

        // Piccola rotazione avanti-indietro
        float angolo = Mathf.Sin(Time.time * 3f + fase) * 8f;
        transform.rotation = Quaternion.Euler(0f, 0f, angolo);

        // Pulsa di dimensione, sembra che brilli
        float scala = 1f + Mathf.Sin(Time.time * 6f + fase) * 0.08f;
        transform.localScale = new Vector3(scala, scala, 1f);
    }
}
