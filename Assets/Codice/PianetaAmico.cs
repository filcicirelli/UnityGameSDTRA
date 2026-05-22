using UnityEngine;

// =====================================================================
//  PianetaAmico
// ---------------------------------------------------------------------
//  Pianeta sorridente che appare quando si completa la missione.
//  Serve come "ricompensa visiva" e rinforzo positivo per il
//  giocatore in riabilitazione.
//
//  Animazione in due fasi:
//      - Pop-in: nei primi 0.5 secondi cresce da 0 alla scala
//        finale con un piccolo "rimbalzo" (overshoot).
//      - Idle: ondeggia dolcemente avanti-indietro come a salutare.
// =====================================================================
public class PianetaAmico : MonoBehaviour
{
    public float scalaFinale = 2.4f;

    private Vector3 posizioneBase;
    private float tempoDiVita;

    void Awake()
    {
        posizioneBase = transform.position;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        tempoDiVita += Time.deltaTime;

        float scalaAttuale;
        if (tempoDiVita < 0.5f)
        {
            // ----- Fase 1: pop-in con overshoot -----
            float t = Mathf.Clamp01(tempoDiVita / 0.5f);
            float overshoot = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
            scalaAttuale = scalaFinale * overshoot * t;
        }
        else
        {
            // ----- Fase 2: idle "respirante" -----
            scalaAttuale = scalaFinale + 0.1f * Mathf.Sin(Time.time * 3f);
        }

        transform.localScale = new Vector3(scalaAttuale, scalaAttuale, 1f);

        // Saluto: ondeggio orizzontale leggero
        float dx = Mathf.Sin(Time.time * 1.5f) * 0.15f;
        transform.position = posizioneBase + new Vector3(dx, 0f, 0f);
    }
}
