using UnityEngine;

// Costruisce la scena del gioco a partire da nulla: sfondo (stelle e
// piccoli pianeti), Astro al centro, la navicella-puntatore e i cristalli
// sparsi sul "percorso spaziale".
// Tutto e' figlio di un GameObject "Mission" che viene distrutto al riavvio.
public static class SpaceLevelLoader
{
    private static GameObject root;
    private static Transform crystalsRoot;

    // Palette colorata e luminosa per i cristalli (8-bit ma invitante)
    static readonly Color[] CRYSTAL_COLORS =
    {
        new Color(1f, 0.85f, 0.30f),   // giallo
        new Color(0.40f, 0.85f, 1f),   // azzurro
        new Color(0.95f, 0.45f, 0.85f),// rosa
        new Color(0.55f, 1f, 0.55f),   // verde chiaro
        new Color(1f, 0.55f, 0.30f),   // arancio
    };

    public static int Load()
    {
        Clear();
        root = new GameObject("Mission");

        BuildBackgroundStars();
        BuildDecorativePlanets();
        BuildAstro();
        BuildSpaceship();

        crystalsRoot = new GameObject("Crystals").transform;
        crystalsRoot.SetParent(root.transform);
        int total = BuildCrystals(10);
        return total;
    }

    public static void Clear()
    {
        if (root != null) Object.Destroy(root);
        root = null;
        crystalsRoot = null;
    }

    // -------- Sfondo --------

    static void BuildBackgroundStars()
    {
        var starRoot = new GameObject("Stars");
        starRoot.transform.SetParent(root.transform);
        // ~120 stelline distribuite sullo schermo
        for (int i = 0; i < 120; i++)
        {
            float x = Random.Range(-9.5f, 9.5f);
            float y = Random.Range(-5.5f, 5.5f);
            float brightness = Random.Range(0.4f, 1f);
            var c = new Color(brightness, brightness, brightness * 0.9f + 0.1f, 1f);

            var s = new GameObject("Star");
            s.transform.SetParent(starRoot.transform);
            s.transform.position = new Vector3(x, y, 5f);
            var sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = SpaceSpriteFactory.CreateStar(c);
            sr.sortingOrder = -5;
            // Scala random per varieta' di "lontananza"
            float k = Random.Range(0.4f, 1.1f);
            s.transform.localScale = new Vector3(k, k, 1f);
        }
    }

    static void BuildDecorativePlanets()
    {
        var planets = new GameObject("Planets");
        planets.transform.SetParent(root.transform);

        var palette = new (Color, Color)[]
        {
            (new Color(0.95f, 0.45f, 0.45f), new Color(0.60f, 0.20f, 0.30f)),
            (new Color(0.50f, 0.80f, 1f),    new Color(0.20f, 0.40f, 0.70f)),
            (new Color(0.95f, 0.85f, 0.40f), new Color(0.70f, 0.40f, 0.10f)),
            (new Color(0.70f, 0.55f, 0.95f), new Color(0.30f, 0.20f, 0.55f)),
        };
        Vector2[] positions =
        {
            new Vector2(-7f,  3.2f),
            new Vector2( 7.2f, 2.5f),
            new Vector2(-6.2f,-3.0f),
            new Vector2( 6.5f,-3.3f),
        };
        for (int i = 0; i < positions.Length; i++)
        {
            var p = new GameObject("Planet_" + i);
            p.transform.SetParent(planets.transform);
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

    // -------- Cristalli --------

    static int BuildCrystals(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = RandomCrystalPosition();
            var color = CRYSTAL_COLORS[Random.Range(0, CRYSTAL_COLORS.Length)];

            var c = new GameObject("Crystal_" + i);
            c.transform.SetParent(crystalsRoot);
            c.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            var sr = c.AddComponent<SpriteRenderer>();
            sr.sprite = SpaceSpriteFactory.CreateCrystal(color);
            sr.sortingOrder = 2;

            var cr = c.AddComponent<Crystal>();
            cr.Init(new Vector3(pos.x, pos.y, 0f));
        }
        return count;
    }

    // Pesca una posizione casuale ragionevole: dentro alla "mappa" ma
    // non troppo vicina ad Astro (cosi' il giocatore deve muovere il mouse).
    static Vector2 RandomCrystalPosition()
    {
        for (int tries = 0; tries < 20; tries++)
        {
            float x = Random.Range(-7f, 7f);
            float y = Random.Range(-3.5f, 4f);
            if (new Vector2(x, y + 0.5f).magnitude > 1.8f) return new Vector2(x, y);
        }
        return new Vector2(3f, 3f);
    }

    // -------- Festa di completamento --------

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
}
