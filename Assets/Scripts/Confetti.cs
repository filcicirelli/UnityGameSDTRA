using UnityEngine;

// Pioggia di coriandoli colorati alla "missione completata".
// Spawna tanti piccoli quadrati con velocita' casuale, gravita' leggera
// e rotazione: niente sistemi particellari, solo SpriteRenderer in piu'.
public class Confetti : MonoBehaviour
{
    public int count = 80;
    public float duration = 4f;

    static readonly Color[] COLORS =
    {
        new Color(1f, 0.30f, 0.40f),
        new Color(0.40f, 0.80f, 1f),
        new Color(1f, 0.85f, 0.20f),
        new Color(0.55f, 1f, 0.55f),
        new Color(0.95f, 0.55f, 1f),
    };

    void Start()
    {
        for (int i = 0; i < count; i++) Spawn();
        Destroy(gameObject, duration + 1.5f);
    }

    void Spawn()
    {
        var go = new GameObject("Confetto");
        go.transform.SetParent(transform, false);
        // Parte dal bordo alto della camera, in una posizione orizzontale casuale
        go.transform.position = new Vector3(Random.Range(-8f, 8f), 6.5f, -3f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateSolid(COLORS[Random.Range(0, COLORS.Length)]);
        sr.sortingOrder = 10;

        float scale = Random.Range(0.12f, 0.22f);
        go.transform.localScale = new Vector3(scale, scale * 0.6f, 1f);

        var p = go.AddComponent<ConfettoPiece>();
        p.velocity = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1f, -3f));
        p.angularSpeed = Random.Range(-360f, 360f);
        p.lifetime = duration;
    }
}

// Singolo coriandolo: gravita' debole + ondeggio orizzontale.
public class ConfettoPiece : MonoBehaviour
{
    public Vector2 velocity;
    public float angularSpeed;
    public float lifetime;

    private float age;
    private float phase;

    void Awake() { phase = Random.Range(0f, Mathf.PI * 2f); }

    void Update()
    {
        age += Time.deltaTime;
        // gravita' leggera
        velocity.y -= 1.2f * Time.deltaTime;
        // movimento + ondeggio
        Vector3 step = new Vector3(
            velocity.x * Time.deltaTime + Mathf.Sin(Time.time * 4f + phase) * 0.01f,
            velocity.y * Time.deltaTime,
            0f);
        transform.position += step;
        transform.Rotate(0, 0, angularSpeed * Time.deltaTime);

        if (age >= lifetime) Destroy(gameObject);
    }
}
