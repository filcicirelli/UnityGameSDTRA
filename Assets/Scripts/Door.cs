using UnityEngine;

// Porta che permette di passare al livello successivo.
// Compare insieme alla chiave. Ogni 3 secondi si sposta in una nuova
// posizione (lerp morbido sull'ultimo mezzo secondo). Si attiva solo
// se Astro la tocca mentre sta gia' trascinando la chiave.
public class Door : MonoBehaviour
{
    public static Door Instance { get; private set; }

    public float idleSeconds = 3f;     // tempo fermo prima del prossimo spostamento
    public float moveSeconds = 0.5f;   // durata dell'animazione

    private Vector3 fromPos, toPos;
    private float timer;               // tempo trascorso nella fase corrente
    private bool moving;
    private bool unlocked;

    void Awake() { Instance = this; }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Init(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, -0.5f);
        fromPos = toPos = transform.position;
        moving = false;
        timer = 0f;
    }

    void Update()
    {
        if (unlocked) return;

        // Idle wobble + leggera pulsazione luminosa
        float wobble = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
        transform.localScale = new Vector3(1.8f * wobble, 1.8f * wobble, 1f);

        timer += Time.deltaTime;
        if (!moving)
        {
            if (timer >= idleSeconds) StartMove();
        }
        else
        {
            float p = Mathf.Clamp01(timer / moveSeconds);
            float t = Mathf.SmoothStep(0f, 1f, p);
            var pos = Vector3.Lerp(fromPos, toPos, t);
            transform.position = new Vector3(pos.x, pos.y, -0.5f);
            if (p >= 1f) { moving = false; timer = 0f; }
        }
    }

    void StartMove()
    {
        fromPos = transform.position;
        Vector2 next = SpaceLevelLoader.PickRandomDoorPos(transform.position);
        toPos = new Vector3(next.x, next.y, -0.5f);
        moving = true;
        timer = 0f;
    }

    // Chiamato da Astro al contatto se sta trasportando la chiave.
    public void Unlock()
    {
        if (unlocked) return;
        unlocked = true;
        if (SpaceGameManager.Instance != null)
            SpaceGameManager.Instance.OnDoorReached();
    }
}
