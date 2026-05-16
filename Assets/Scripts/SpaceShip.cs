using System.Collections.Generic;
using UnityEngine;

// La navicella che fa da "puntatore del mouse".
// - Ogni frame si sposta dove punta il mouse.
// - Tenendo premuto il tasto sinistro attiva il "raggio traente":
//   un fascio di tessere disegnato verso Astro che attira
//   cristalli vicini... e bombe!
// - Controlla anche le collisioni con le barriere (penalita')
//   e con le bombe (GAME OVER).
public class SpaceShip : MonoBehaviour
{
    public float beamRadius = 3.5f;
    public float beamPullSpeed = 6f;
    public int beamTileCount = 12;
    public float beamTileSpacing = 0.18f;

    // Soglia di "tocca-bomba" sommata al dangerRadius della bomba.
    public float shipBombRadius = 0.4f;

    public bool BeamActive { get; private set; }
    public Vector2 BeamDirection { get; private set; } = Vector2.down;

    private Camera cam;
    private float zToCam;
    private SpriteRenderer sr;
    private Color baseColor;
    private float redFlash; // > 0 = lampeggia rosso
    private readonly List<GameObject> beamTiles = new List<GameObject>();
    private Sprite beamTileSprite;

    void Awake()
    {
        cam = Camera.main;
        zToCam = Mathf.Abs(cam.transform.position.z);
        beamTileSprite = SpaceSpriteFactory.CreateBeamTile();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    void Update()
    {
        var gm = SpaceGameManager.Instance;

        // 1) Segui il mouse (anche a missione completa: lascialo libero)
        Vector3 m = Input.mousePosition;
        m.z = zToCam;
        Vector3 world = cam.ScreenToWorldPoint(m);
        transform.position = new Vector3(world.x, world.y, -1f);

        // 2) Direzione del raggio: verso Astro (centro)
        Vector2 toAstro = (Vector2)(-transform.position);
        BeamDirection = toAstro.sqrMagnitude > 0.0001f ? toAstro.normalized : Vector2.down;

        // 3) Raggio attivo solo se gioco "vivo"
        bool gameLive = gm != null && !gm.MissionComplete && !gm.GameOver;
        BeamActive = gameLive && Input.GetMouseButton(0);

        UpdateBeamVisual();

        if (gameLive)
        {
            if (BeamActive)
            {
                PullNearbyCrystals();
                if (gm.BombsArmed) PullNearbyBombs();
            }
            CheckBarrierHit();
            if (gm.BombsArmed) CheckBombContact();
        }

        UpdateFlash();
    }

    // -------- Visual del raggio --------

    void UpdateBeamVisual()
    {
        if (beamTiles.Count == 0)
        {
            for (int i = 0; i < beamTileCount; i++)
            {
                var t = new GameObject("BeamTile_" + i);
                t.transform.SetParent(transform, false);
                var trs = t.AddComponent<SpriteRenderer>();
                trs.sprite = beamTileSprite;
                trs.sortingOrder = 4;
                t.SetActive(false);
                beamTiles.Add(t);
            }
        }
        for (int i = 0; i < beamTiles.Count; i++)
        {
            var t = beamTiles[i];
            if (!BeamActive) { t.SetActive(false); continue; }
            t.SetActive(true);
            float offset = (i + 1) * beamTileSpacing
                           + Mathf.Sin(Time.time * 8f + i * 0.5f) * 0.05f;
            t.transform.localPosition = (Vector3)(BeamDirection * offset);
            float angle = Mathf.Atan2(BeamDirection.y, BeamDirection.x) * Mathf.Rad2Deg - 90f;
            t.transform.localRotation = Quaternion.Euler(0, 0, angle);
            float fade = 1f - (float)i / beamTiles.Count;
            t.transform.localScale = new Vector3(0.8f * fade + 0.2f, 1f, 1f);
        }
    }

    // -------- Aspirazione --------

    void PullNearbyCrystals()
    {
        Vector2 mouth = (Vector2)transform.position + BeamDirection * 0.4f;
        foreach (var c in Crystal.Active)
        {
            if (c == null) continue;
            Vector2 cp = c.transform.position;
            if (Vector2.Distance(cp, mouth) <= beamRadius)
                c.PullToward(transform.position, beamPullSpeed);
        }
    }

    void PullNearbyBombs()
    {
        Vector2 mouth = (Vector2)transform.position + BeamDirection * 0.4f;
        foreach (var b in Bomb.All)
        {
            if (b == null) continue;
            Vector2 bp = b.transform.position;
            if (Vector2.Distance(bp, mouth) <= beamRadius)
                b.PullToward(transform.position, beamPullSpeed);
        }
    }

    // -------- Collisioni --------

    void CheckBarrierHit()
    {
        Vector2 p = transform.position;
        foreach (var b in Barrier.All)
        {
            if (b != null && b.Contains(p))
            {
                if (SpaceGameManager.Instance != null)
                    SpaceGameManager.Instance.OnBarrierTouched();
                redFlash = 0.20f;
                return;
            }
        }
    }

    void CheckBombContact()
    {
        Vector2 p = transform.position;
        foreach (var b in Bomb.All)
        {
            if (b == null) continue;
            if (Vector2.Distance(p, b.transform.position) <= shipBombRadius + b.dangerRadius)
            {
                b.Detonate();
                return;
            }
        }
    }

    // -------- Flash rosso (penalita' barriera) --------

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
}
