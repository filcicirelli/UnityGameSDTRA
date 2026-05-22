using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  Caramella
// ---------------------------------------------------------------------
//  Le caramelle galleggiano nello spazio facendo piccoli movimenti.
//  Astro le raccoglie semplicemente toccandole: non c'e' bisogno di
//  click ne' di precisione fine (e' un gioco per riabilitazione).
//
//  Tengo una lista statica "Attive" delle caramelle ancora presenti
//  in scena, cosi' Astro ad ogni frame puo' scorrerla senza dover
//  cercare gli oggetti con Find ogni volta.
// =====================================================================
public class Caramella : MonoBehaviour
{
    // Tutte le caramelle ancora in scena. Astro la legge ogni Update.
    public static readonly HashSet<Caramella> Attive = new HashSet<Caramella>();

    // Posizione "di riposo" attorno a cui la caramella oscilla
    private Vector3 punto;
    private float fase;
    private bool gia_raccolta;

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
        // Fase casuale: cosi' le caramelle non oscillano tutte all'unisono
        fase = Random.Range(0f, Mathf.PI * 2f);
    }

    // Chiamato da Astro quando entra nel raggio di tocco.
    public void Raccogli()
    {
        if (gia_raccolta) return;
        gia_raccolta = true;

        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaCaramellaRaccolta();
        }
        Astro.NotificaSaltoFelice();
        Destroy(gameObject);
    }

    void Update()
    {
        // Piccolo "galleggiamento" + scintillio
        float t = Time.time * 1.5f + fase;

        // Movimento orizzontale e verticale a forma di figura di Lissajous
        float dx = Mathf.Sin(t) * 0.10f;
        float dy = Mathf.Cos(t * 0.8f) * 0.12f;
        transform.position = punto + new Vector3(dx, dy, 0f);

        // Rotazione leggera che oscilla avanti-indietro
        float angolo = Mathf.Sin(Time.time * 3f + fase) * 8f;
        transform.rotation = Quaternion.Euler(0f, 0f, angolo);

        // Scala che "pulsa": effetto scintillio
        float scala = 1f + Mathf.Sin(Time.time * 6f + fase) * 0.08f;
        transform.localScale = new Vector3(scala, scala, 1f);
    }
}
