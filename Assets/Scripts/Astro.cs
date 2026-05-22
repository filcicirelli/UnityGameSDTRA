using UnityEngine;

// Astro, l'alieno verde con lo zaino-aspirapolvere.
// Ora e' lui il "puntatore": segue il mouse, raccoglie le caramelle al
// contatto, prende la chiave e la porta verso la porta-portale.
// Si occupa anche di rilevare le collisioni con barriere e bombe.
public class Astro : MonoBehaviour
{
    public static Astro Instance { get; private set; }

    // Raggi di "tocco" (in unita' di mondo).
    // Chiave/porta hanno raggio generoso: e' una raccolta "a passaggio",
    // pensata per bambini che possono avere problemi di motricita' fine.
    public float candyTouchRadius   = 0.85f;
    public float keyTouchRadius     = 1.80f;   // generoso: la chiave e' anche "magnetica"
    public float doorTouchRadius    = 1.80f;   // anche la porta e' magnetica/generosa
    public float bombTouchRadius    = 0.55f;

    public bool HasKey { get; private set; }
    public Vector2 Velocity { get; private set; }

    private Camera cam;
    private float zToCam;
    private Vector3 baseScale;
    private SpriteRenderer sr;
    private Color baseColor;
    private float happyTimer;
    private float redFlash;
    private float tilt;          // rotazione corrente in gradi (per smoothing)

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
        zToCam = Mathf.Abs(cam.transform.position.z);
        baseScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        var gm = SpaceGameManager.Instance;
        if (gm == null) return;

        // 1) Posizione: segui il mouse (Astro E' il cursore)
        Vector3 prev = transform.position;
        Vector3 m = Input.mousePosition;
        m.z = zToCam;
        Vector3 world = cam.ScreenToWorldPoint(m);
        transform.position = new Vector3(world.x, world.y, 0f);
        Velocity = ((Vector2)(transform.position - prev)) / Mathf.Max(Time.deltaTime, 1e-6f);

        // 2) Animazioni: respiro + saltino di gioia + tilt verso il movimento
        float breath = 1f + Mathf.Sin(Time.time * 2f) * 0.04f;
        float happy = 1f;
        if (happyTimer > 0f)
        {
            happyTimer -= Time.deltaTime;
            float k = Mathf.Clamp01(happyTimer / 0.35f);
            happy = 1f + k * 0.20f;
        }
        float s = breath * happy;
        transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, 1f);

        float target = Mathf.Clamp(-Velocity.x * 3f, -18f, 18f);
        tilt = Mathf.Lerp(tilt, target, Time.deltaTime * 8f);
        transform.localRotation = Quaternion.Euler(0, 0, tilt);

        // 3) Interazioni: solo se il gioco e' "vivo"
        bool gameLive = !gm.MissionComplete && !gm.GameOver && !gm.Victory;
        if (gameLive)
        {
            CheckCandyTouch();
            CheckBarrierTouch();
            if (gm.BombsArmed) CheckBombTouch();

            // Fase chiave-porta: si attiva dopo aver raccolto tutte le caramelle.
            if (Key.Instance != null && !HasKey) CheckKeyTouch();
            if (HasKey && Door.Instance != null) CheckDoorTouch();
        }

        UpdateFlash();
    }

    // -------- Tocchi --------

    void CheckCandyTouch()
    {
        Vector2 p = transform.position;
        foreach (var c in Candy.Active)
        {
            if (c == null) continue;
            if (Vector2.Distance(p, c.transform.position) <= candyTouchRadius)
                c.Collect();
        }
    }

    void CheckBarrierTouch()
    {
        Vector2 p = transform.position;
        foreach (var b in Barrier.All)
        {
            if (b != null && b.Contains(p))
            {
                SpaceGameManager.Instance.OnBarrierTouched();
                redFlash = 0.20f;
                return;
            }
        }
    }

    void CheckBombTouch()
    {
        Vector2 p = transform.position;
        foreach (var b in Bomb.All)
        {
            if (b == null) continue;
            if (Vector2.Distance(p, b.transform.position) <= bombTouchRadius + b.dangerRadius)
            {
                b.Detonate();
                return;
            }
        }
    }

    void CheckKeyTouch()
    {
        var k = Key.Instance;
        if (Vector2.Distance(transform.position, k.transform.position) <= keyTouchRadius)
        {
            k.PickUp();
            HasKey = true;
        }
    }

    void CheckDoorTouch()
    {
        var d = Door.Instance;
        if (Vector2.Distance(transform.position, d.transform.position) <= doorTouchRadius)
            d.Unlock();
    }

    // -------- Feedback --------

    void UpdateFlash()
    {
        if (sr == null) return;
        if (redFlash > 0f)
        {
            redFlash -= Time.deltaTime;
            sr.color = Color.Lerp(baseColor, new Color(1f, 0.3f, 0.3f, 1f),
                                  Mathf.Clamp01(redFlash / 0.20f));
        }
        else sr.color = baseColor;
    }

    // Saltino di gioia: chiamato dalle caramelle al momento della raccolta.
    public static void NotifyHappyBounce()
    {
        if (Instance != null) Instance.happyTimer = 0.35f;
    }
}
