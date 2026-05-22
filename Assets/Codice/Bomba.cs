using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  Bomba
// ---------------------------------------------------------------------
//  Oggetto che NON si deve toccare. Visivamente pulsa di rosso per
//  attirare l'attenzione del giocatore: la pulsazione e' importante
//  per chi ha difficolta' a riconoscere oggetti statici.
//
//  Al contatto: parte un'esplosione e il GestoreGioco viene avvisato
//  con SegnalaBombaColpita (che fa perdere una vita).
// =====================================================================
public class Bomba : MonoBehaviour
{
    // Lista globale: Astro la legge per testare i contatti.
    public static readonly List<Bomba> Tutte = new List<Bomba>();

    // Raggio "di pericolo" attorno alla bomba (oltre lo sprite)
    public float raggioPericolo = 0.5f;

    private Vector3 posizioneBase;
    private SpriteRenderer sr;
    private SpriteRenderer alone;
    private float fase;
    private bool esplosa;

    void OnEnable()
    {
        Tutte.Add(this);
    }

    void OnDisable()
    {
        Tutte.Remove(this);
    }

    public void Inizializza(Vector2 posizione)
    {
        posizioneBase = new Vector3(posizione.x, posizione.y, 0f);
        transform.position = posizioneBase;
        fase = Random.Range(0f, Mathf.PI * 2f);
        sr = GetComponent<SpriteRenderer>();

        // Aggiungo un "alone" rosso pulsante come SpriteRenderer figlio.
        // Cosi' la bomba e' subito riconoscibile dai bambini.
        GameObject aloneGo = new GameObject("Alone");
        aloneGo.transform.SetParent(transform, false);
        aloneGo.transform.localPosition = Vector3.zero;
        aloneGo.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

        alone = aloneGo.AddComponent<SpriteRenderer>();
        alone.sprite = FabbricaImmagini.CreaQuadratoPieno(new Color(1f, 0.20f, 0.20f, 0.30f));
        int ordine = (sr != null) ? sr.sortingOrder : 2;
        alone.sortingOrder = ordine - 1;
    }

    void Update()
    {
        if (esplosa) return;

        // ----- Pulsazione dell'alone -----
        float pulse = 0.85f + Mathf.Sin(Time.time * 5f + fase) * 0.25f;
        if (alone != null)
        {
            alone.transform.localScale = new Vector3(2.4f * pulse, 2.4f * pulse, 1f);
            Color c = alone.color;
            c.a = 0.20f + 0.20f * Mathf.Sin(Time.time * 5f + fase);
            alone.color = c;
        }

        // ----- Piccola fluttuazione della bomba stessa -----
        float t = Time.time * 1.2f + fase;
        float dx = Mathf.Sin(t) * 0.06f;
        float dy = Mathf.Cos(t * 0.7f) * 0.08f;
        transform.position = posizioneBase + new Vector3(dx, dy, 0f);
    }

    // Innesca l'esplosione e segnala l'evento al gestore.
    public void Detona()
    {
        if (esplosa) return;
        esplosa = true;

        CaricatoreLivelli.GeneraEsplosione(transform.position);
        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaBombaColpita();
        }
        Destroy(gameObject);
    }
}
