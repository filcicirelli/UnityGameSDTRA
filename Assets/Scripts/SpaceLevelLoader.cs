using UnityEngine;

// Costruisce la scena del gioco a partire da un SpaceLevelDef:
// sfondo (stelle e pianeti), Astro (controllato dal mouse),
// barriere/asteroidi, bombe e caramelle.
// La chiave e la porta vengono spawnati dopo la raccolta totale.
// Tutto e' figlio di un GameObject "Mission" distrutto al cambio livello.
public static class SpaceLevelLoader
{
    private static GameObject root;
    private static Transform candiesRoot;
    private static Transform barriersRoot;
    private static Transform bombsRoot;
    private static SpaceLevelDef currentDef;

    // Palette per caramelle generate in modo casuale (livello 1)
    static readonly Color[] CANDY_COLORS =
    {
        new Color(1f, 0.35f, 0.55f),     // fragola
        new Color(0.40f, 0.85f, 1f),     // menta-blu
        new Color(0.95f, 0.45f, 0.85f),  // anguria
        new Color(0.55f, 1f, 0.55f),     // mela verde
        new Color(1f, 0.65f, 0.20f),     // arancia
        new Color(0.95f, 0.85f, 0.30f),  // limone
    };

    public static int Load(SpaceLevelDef def)
    {
        Clear();
        currentDef = def;
        root = new GameObject("Mission");

        BuildBackgroundStars();
        BuildDecorativePlanets();
        BuildAstro();

        candiesRoot  = NewChild("Candies");
        barriersRoot = NewChild("Barriers");
        bombsRoot    = NewChild("Bombs");

        BuildBarriers(def);
        BuildBombs(def);
        return BuildCandies(def);
    }

    public static void Clear()
    {
        if (root != null) Object.Destroy(root);
        root = null;
        candiesRoot = barriersRoot = bombsRoot = null;
        currentDef = null;
    }

    static Transform NewChild(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root.transform);
        return go.transform;
    }

    // -------- Sfondo --------

    static void BuildBackgroundStars()
    {
        var starRoot = NewChild("Stars");
        for (int i = 0; i < 120; i++)
        {
            float x = Random.Range(-9.5f, 9.5f);
            float y = Random.Range(-5.5f, 5.5f);
            float b = Random.Range(0.4f, 1f);
            var c = new Color(b, b, b * 0.9f + 0.1f, 1f);

            var s = new GameObject("Star");
            s.transform.SetParent(starRoot);
            s.transform.position = new Vector3(x, y, 5f);
            var sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = SpaceSpriteFactory.CreateStar(c);
            sr.sortingOrder = -5;
            float k = Random.Range(0.4f, 1.1f);
            s.transform.localScale = new Vector3(k, k, 1f);
        }
    }

    static void BuildDecorativePlanets()
    {
        var planets = NewChild("Planets");
        var palette = new (Color, Color)[]
        {
            (new Color(0.95f, 0.45f, 0.45f), new Color(0.60f, 0.20f, 0.30f)),
            (new Color(0.50f, 0.80f, 1f),    new Color(0.20f, 0.40f, 0.70f)),
            (new Color(0.95f, 0.85f, 0.40f), new Color(0.70f, 0.40f, 0.10f)),
            (new Color(0.70f, 0.55f, 0.95f), new Color(0.30f, 0.20f, 0.55f)),
        };
        Vector2[] positions =
        {
            new Vector2(-7f,  3.2f), new Vector2( 7.2f, 2.5f),
            new Vector2(-6.2f,-3.0f),new Vector2( 6.5f,-3.3f),
        };
        for (int i = 0; i < positions.Length; i++)
        {
            var p = new GameObject("Planet_" + i);
            p.transform.SetParent(planets);
            p.transform.position = positions[i];
            float k = Random.Range(1.1f, 1.6f);
            p.transform.localScale = new Vector3(k, k, 1f);
            var sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = SpaceSpriteFactory.CreatePlanet(palette[i].Item1, palette[i].Item2);
            sr.sortingOrder = -2;
        }
    }

    // -------- Astro (puntatore + protagonista) --------

    static void BuildAstro()
    {
        var a = new GameObject("Astro");
        a.transform.SetParent(root.transform);
        a.transform.position = new Vector3(0, 0, 0);     // partenza al centro
        a.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        var sr = a.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateAstro();
        // Astro e' il puntatore: deve sempre essere visibile sopra
        // caramelle, chiave e porta. sortingOrder alto.
        sr.sortingOrder = 10;
        a.AddComponent<Astro>();
    }

    // -------- Barriere --------

    static void BuildBarriers(SpaceLevelDef def)
    {
        foreach (var b in def.barriers)
        {
            var go = new GameObject("Barrier");
            go.transform.SetParent(barriersRoot);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpaceSpriteFactory.CreateBarrierTile(b.color);
            sr.sortingOrder = 0;
            var br = go.AddComponent<Barrier>();
            br.Init(b.center, b.size);
        }
    }

    // -------- Bombe --------

    static void BuildBombs(SpaceLevelDef def)
    {
        foreach (var pos in def.bombs)
        {
            var go = new GameObject("Bomb");
            go.transform.SetParent(bombsRoot);
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpaceSpriteFactory.CreateBomb();
            sr.sortingOrder = 3;
            var b = go.AddComponent<Bomb>();
            b.Init(pos);
        }
    }

    // -------- Caramelle --------

    static int BuildCandies(SpaceLevelDef def)
    {
        int total;
        if (def.randomCandies > 0)
        {
            total = def.randomCandies;
            for (int i = 0; i < total; i++)
                CreateCandyAt(RandomCandyPosition(def),
                              CANDY_COLORS[Random.Range(0, CANDY_COLORS.Length)], i);
        }
        else
        {
            total = def.candies.Count;
            for (int i = 0; i < total; i++)
                CreateCandyAt(def.candies[i].position, def.candies[i].color, i);
        }
        return total;
    }

    static void CreateCandyAt(Vector2 pos, Color color, int i)
    {
        var c = new GameObject("Candy_" + i);
        c.transform.SetParent(candiesRoot);
        c.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
        var sr = c.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateCandy(color);
        sr.sortingOrder = 2;
        var cr = c.AddComponent<Candy>();
        cr.Init(new Vector3(pos.x, pos.y, 0f));
    }

    static Vector2 RandomCandyPosition(SpaceLevelDef def)
    {
        for (int tries = 0; tries < 30; tries++)
        {
            float x = Random.Range(-7f, 7f);
            float y = Random.Range(-3.5f, 4f);
            var p = new Vector2(x, y);
            if (p.magnitude < 1.2f) continue;
            if (IsInsideBarrier(p, def)) continue;
            return p;
        }
        return new Vector2(3f, 3f);
    }

    static bool IsInsideBarrier(Vector2 p, SpaceLevelDef def)
    {
        foreach (var b in def.barriers)
        {
            var r = new Rect(b.center.x - b.size.x / 2f,
                             b.center.y - b.size.y / 2f,
                             b.size.x, b.size.y);
            if (r.Contains(p)) return true;
        }
        return false;
    }

    // -------- Chiave + Porta (fase finale del livello) --------

    public static void SpawnKey()
    {
        if (root == null) return;
        var go = new GameObject("Key");
        go.transform.SetParent(root.transform);
        go.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateKey();
        sr.sortingOrder = 5;
        var k = go.AddComponent<Key>();
        // y=2.0 invece di 3.0: ben sotto il muro superiore di L2/L3 (y>=4),
        // cosi' Astro la puo' prendere senza dover sfiorare le barriere.
        k.Init(new Vector2(0f, 2.0f));
    }

    public static void SpawnDoor()
    {
        if (root == null) return;
        var go = new GameObject("Door");
        go.transform.SetParent(root.transform);
        go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateDoor();
        sr.sortingOrder = 5;
        var d = go.AddComponent<Door>();
        d.Init(PickRandomDoorPos(Vector2.zero));
    }

    // Sceglie una posizione casuale per la porta, evitando barriere,
    // bombe e la posizione corrente (per garantire un cambio reale).
    public static Vector2 PickRandomDoorPos(Vector2 avoidPos)
    {
        if (currentDef == null) return new Vector2(0f, 2f);
        for (int tries = 0; tries < 40; tries++)
        {
            // Range stretto: lascia margine ai muri laterali (x=+-7.5) e
            // al muro superiore (y=4). La porta e' larga ~1.8 unita'.
            float x = Random.Range(-5.5f, 5.5f);
            float y = Random.Range(-2.5f, 2.8f);
            var p = new Vector2(x, y);
            if (Vector2.Distance(p, avoidPos) < 3f) continue;
            if (IsInsideBarrier(p, currentDef)) continue;
            if (NearBomb(p)) continue;
            return p;
        }
        return new Vector2(4f, 2f);
    }

    static bool NearBomb(Vector2 p)
    {
        foreach (var b in Bomb.All)
            if (b != null && Vector2.Distance(p, b.transform.position) < 1.2f)
                return true;
        return false;
    }

    // -------- Effetti --------

    public static void SpawnFriendPlanet()
    {
        if (root == null) return;
        var p = new GameObject("FriendPlanet");
        p.transform.SetParent(root.transform);
        p.transform.position = new Vector3(0, 1.5f, -2f);
        var sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateFriendPlanet();
        sr.sortingOrder = 6;
        p.AddComponent<FriendPlanet>();
    }

    public static void SpawnConfetti()
    {
        if (root == null) return;
        var c = new GameObject("Confetti");
        c.transform.SetParent(root.transform);
        c.AddComponent<Confetti>();
    }

    public static void SpawnExplosion(Vector3 position)
    {
        if (root == null) return;
        var e = new GameObject("Explosion");
        e.transform.SetParent(root.transform);
        e.transform.position = new Vector3(position.x, position.y, -3f);
        var sr = e.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateExplosion();
        sr.sortingOrder = 8;
        e.AddComponent<Explosion>();
    }
}
