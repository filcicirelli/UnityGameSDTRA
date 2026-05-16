using UnityEngine;

// Costruisce la scena del gioco a partire da un SpaceLevelDef:
// sfondo (stelle e pianeti), Astro al centro, la navicella-puntatore,
// barriere/asteroidi, bombe e cristalli.
// Tutto e' figlio di un GameObject "Mission" che viene distrutto al cambio livello.
public static class SpaceLevelLoader
{
    private static GameObject root;
    private static Transform crystalsRoot;
    private static Transform barriersRoot;
    private static Transform bombsRoot;

    // Palette per cristalli generati in modo casuale (livello 1)
    static readonly Color[] CRYSTAL_COLORS =
    {
        new Color(1f, 0.85f, 0.30f),
        new Color(0.40f, 0.85f, 1f),
        new Color(0.95f, 0.45f, 0.85f),
        new Color(0.55f, 1f, 0.55f),
        new Color(1f, 0.55f, 0.30f),
    };

    public static int Load(SpaceLevelDef def)
    {
        Clear();
        root = new GameObject("Mission");

        BuildBackgroundStars();
        BuildDecorativePlanets();
        BuildAstro();
        BuildSpaceship();

        crystalsRoot = NewChild("Crystals");
        barriersRoot = NewChild("Barriers");
        bombsRoot    = NewChild("Bombs");

        BuildBarriers(def);
        BuildBombs(def);
        int total = BuildCrystals(def);
        return total;
    }

    public static void Clear()
    {
        if (root != null) Object.Destroy(root);
        root = null;
        crystalsRoot = barriersRoot = bombsRoot = null;
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

    // -------- Personaggi --------

    static void BuildAstro()
    {
        var a = new GameObject("Astro");
        a.transform.SetParent(root.transform);
        a.transform.position = new Vector3(0, -0.5f, 0);
        a.transform.localScale = new Vector3(2f, 2f, 1f);
        var sr = a.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateAstro();
        sr.sortingOrder = 1;
        a.AddComponent<Astro>();
    }

    static void BuildSpaceship()
    {
        var s = new GameObject("Spaceship");
        s.transform.SetParent(root.transform);
        s.transform.position = new Vector3(0, 2f, -1f);
        s.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
        var sr = s.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateShip();
        sr.sortingOrder = 5;
        s.AddComponent<SpaceShip>();
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

    // -------- Cristalli --------

    static int BuildCrystals(SpaceLevelDef def)
    {
        int total;
        if (def.randomCrystals > 0)
        {
            total = def.randomCrystals;
            for (int i = 0; i < total; i++)
            {
                Vector2 pos = RandomCrystalPosition(def);
                var color = CRYSTAL_COLORS[Random.Range(0, CRYSTAL_COLORS.Length)];
                CreateCrystal(pos, color, i);
            }
        }
        else
        {
            total = def.crystals.Count;
            for (int i = 0; i < total; i++)
                CreateCrystal(def.crystals[i].position, def.crystals[i].color, i);
        }
        return total;
    }

    static void CreateCrystal(Vector2 pos, Color color, int i)
    {
        var c = new GameObject("Crystal_" + i);
        c.transform.SetParent(crystalsRoot);
        c.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        var sr = c.AddComponent<SpriteRenderer>();
        sr.sprite = SpaceSpriteFactory.CreateCrystal(color);
        sr.sortingOrder = 2;
        var cr = c.AddComponent<Crystal>();
        cr.Init(new Vector3(pos.x, pos.y, 0f));
    }

    static Vector2 RandomCrystalPosition(SpaceLevelDef def)
    {
        for (int tries = 0; tries < 30; tries++)
        {
            float x = Random.Range(-7f, 7f);
            float y = Random.Range(-3.5f, 4f);
            var p = new Vector2(x, y);
            if (new Vector2(x, y + 0.5f).magnitude < 1.8f) continue; // troppo vicino ad Astro
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
