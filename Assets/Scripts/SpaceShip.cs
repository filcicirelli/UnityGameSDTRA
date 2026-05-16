using System.Collections.Generic;
using UnityEngine;

// La navicella che fa da "puntatore del mouse".
// - Ogni frame si sposta dove punta il mouse (smooth).
// - Tenendo premuto il tasto sinistro attiva il "raggio traente":
//   un fascio di tessere disegnato verso il basso (verso Astro)
//   che attira i cristalli entro un raggio.
public class SpaceShip : MonoBehaviour
{
    public float beamRadius = 3.5f;       // raggio entro cui i cristalli vengono attratti
    public float beamPullSpeed = 6f;      // velocita' di trascinamento dei cristalli
    public int beamTileCount = 12;        // quante tessere disegnano il fascio
    public float beamTileSpacing = 0.18f; // distanza tra tessere

    public bool BeamActive { get; private set; }
    public Vector2 BeamDirection { get; private set; } = Vector2.down;

    private Camera cam;
    private float zToCam;
    private readonly List<GameObject> beamTiles = new List<GameObject>();
    private Sprite beamTileSprite;

    void Awake()
    {
        cam = Camera.main;
        zToCam = Mathf.Abs(cam.transform.position.z);
        beamTileSprite = SpaceSpriteFactory.CreateBeamTile();
    }

    void Update()
    {
        // 1) Segui il mouse
        Vector3 m = Input.mousePosition;
        m.z = zToCam;
        Vector3 world = cam.ScreenToWorldPoint(m);
        transform.position = new Vector3(world.x, world.y, -1f);

        // 2) Direzione del raggio: dalla navicella verso Astro (centro scena)
        Vector2 toAstro = (Vector2)(-transform.position);
        BeamDirection = toAstro.sqrMagnitude > 0.0001f ? toAstro.normalized : Vector2.down;

        // 3) Attiva/disattiva il raggio col tasto sinistro
        BeamActive = Input.GetMouseButton(0)
                     && SpaceGameManager.Instance != null
                     && !SpaceGameManager.Instance.MissionComplete;

        UpdateBeamVisual();
        if (BeamActive) PullNearbyCrystals();
    }

    void UpdateBeamVisual()
    {
        // Crea le tessere la prima volta che serve
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
            // Pulsazione + dispone le tessere lungo la direzione del raggio
            float offset = (i + 1) * beamTileSpacing
                           + Mathf.Sin(Time.time * 8f + i * 0.5f) * 0.05f;
            t.transform.localPosition = (Vector3)(BeamDirection * offset);
            float angle = Mathf.Atan2(BeamDirection.y, BeamDirection.x) * Mathf.Rad2Deg - 90f;
            t.transform.localRotation = Quaternion.Euler(0, 0, angle);
            // Tessere piu' lontane piu' piccole/trasparenti = effetto cono
            float fade = 1f - (float)i / beamTiles.Count;
            t.transform.localScale = new Vector3(0.8f * fade + 0.2f, 1f, 1f);
        }
    }

    void PullNearbyCrystals()
    {
        // Punto "bocca" del raggio: appena davanti alla navicella
        Vector2 mouth = (Vector2)transform.position + BeamDirection * 0.4f;
        foreach (var c in Crystal.Active)
        {
            if (c == null) continue;
            Vector2 cp = c.transform.position;
            if (Vector2.Distance(cp, mouth) <= beamRadius)
                c.PullToward(transform.position, beamPullSpeed);
        }
    }
}
