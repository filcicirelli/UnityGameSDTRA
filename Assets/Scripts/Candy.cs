using System.Collections.Generic;
using UnityEngine;

// Caramella che Astro raccoglie semplicemente toccandola.
// Niente raggio traente: la raccolta e' a contatto diretto (vedi Astro.cs).
public class Candy : MonoBehaviour
{
    // Lista globale: Astro ne controlla la collisione ogni frame.
    public static readonly HashSet<Candy> Active = new HashSet<Candy>();

    private Vector3 anchor;   // posizione "di riposo" attorno a cui fluttua
    private float phase;
    private bool collected;

    void OnEnable()  { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    public void Init(Vector3 position)
    {
        anchor = position;
        transform.position = position;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    // Chiamato da Astro al contatto: punto +10 e salto di gioia.
    public void Collect()
    {
        if (collected) return;
        collected = true;
        if (SpaceGameManager.Instance != null)
            SpaceGameManager.Instance.OnCandyCollected();
        Astro.NotifyHappyBounce();
        Destroy(gameObject);
    }

    void Update()
    {
        // Fluttuazione + scintillio
        float t = Time.time * 1.5f + phase;
        transform.position = anchor + new Vector3(
            Mathf.Sin(t) * 0.10f,
            Mathf.Cos(t * 0.8f) * 0.12f,
            0f);
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 3f + phase) * 8f);
        float s = 1f + Mathf.Sin(Time.time * 6f + phase) * 0.08f;
        transform.localScale = new Vector3(s, s, 1f);
    }
}
