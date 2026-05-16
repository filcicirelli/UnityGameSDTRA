using System.Collections.Generic;
using UnityEngine;

// Bomba: oggetto da NON toccare e da NON aspirare.
// Il contatto con la navicella o l'aspirazione col raggio = GAME OVER.
// Visivamente pulsa di rosso per essere immediatamente riconoscibile dai bambini.
public class Bomb : MonoBehaviour
{
    public static readonly List<Bomb> All = new List<Bomb>();

    public float dangerRadius = 0.5f; // raggio di "contatto" con la navicella

    private Vector3 basePos;
    private SpriteRenderer sr;
    private SpriteRenderer halo;
    private float phase;
    private bool pulling;
    private Vector3 pullTarget;
    private float pullSpeed;
    private bool exploded;

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public void Init(Vector2 position)
    {
        basePos = new Vector3(position.x, position.y, 0f);
        transform.position = basePos;
        phase = Random.Range(0f, Mathf.PI * 2f);
        sr = GetComponent<SpriteRenderer>();

        // Alone rosso pulsante: secondo SpriteRenderer figlio per dare allerta visiva.
        var haloGO = new GameObject("Halo");
        haloGO.transform.SetParent(transform, false);
        haloGO.transform.localPosition = Vector3.zero;
        haloGO.transform.localScale = new Vector3(2.4f, 2.4f, 1f);
        halo = haloGO.AddComponent<SpriteRenderer>();
        halo.sprite = SpaceSpriteFactory.CreateSolid(new Color(1f, 0.20f, 0.20f, 0.30f));
        halo.sortingOrder = (sr != null ? sr.sortingOrder : 2) - 1;
    }

    // Chiamato da SpaceShip quando il raggio traente la "tocca".
    public void PullToward(Vector3 target, float speed)
    {
        pulling = true;
        pullTarget = target;
        pullSpeed = speed;
    }

    void Update()
    {
        if (exploded) return;

        // Pulsazione: l'alone cresce e cala
        float pulse = 0.85f + Mathf.Sin(Time.time * 5f + phase) * 0.25f;
        if (halo != null)
        {
            halo.transform.localScale = new Vector3(2.4f * pulse, 2.4f * pulse, 1f);
            var c = halo.color;
            c.a = 0.20f + 0.20f * Mathf.Sin(Time.time * 5f + phase);
            halo.color = c;
        }

        if (pulling)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, pullTarget, pullSpeed * Time.deltaTime);
            pulling = false;
        }
        else
        {
            // Lieve fluttuazione
            float t = Time.time * 1.2f + phase;
            transform.position = basePos + new Vector3(
                Mathf.Sin(t) * 0.06f, Mathf.Cos(t * 0.7f) * 0.08f, 0f);
        }
    }

    // Innesca l'esplosione e segnala il GAME OVER al manager.
    public void Detonate()
    {
        if (exploded) return;
        exploded = true;
        SpaceLevelLoader.SpawnExplosion(transform.position);
        if (SpaceGameManager.Instance != null)
            SpaceGameManager.Instance.OnBombHit();
        Destroy(gameObject);
    }
}
