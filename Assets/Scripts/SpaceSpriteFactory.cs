using UnityEngine;

// Genera tutti gli sprite "8-bit" del gioco Missione Spaziale da codice.
// Una sola griglia di pixel (16x16) per ogni elemento: niente asset esterni,
// look pixel-art autentico, perfetto per spiegare il funzionamento all'esame.
public static class SpaceSpriteFactory
{
    private const int SIZE = 16;
    private const int PPU  = 16;

    // -------- Navicella (puntatore) --------
    // Triangolino col motore acceso, vista dall'alto.
    public static Sprite CreateShip()
    {
        var px = NewTransparent();
        Color body = new Color(0.85f, 0.90f, 1f);
        Color shade = new Color(0.45f, 0.55f, 0.85f);
        Color glass = new Color(0.30f, 0.85f, 1f);
        Color flame = new Color(1f, 0.75f, 0.20f);

        // Scafo (triangolo con punta in alto)
        for (int y = 2; y <= 12; y++)
        {
            int t = 12 - y;                       // 0 in alto -> 10 in basso
            int half = Mathf.Min(6, 1 + (10 - t) / 2);
            for (int x = 8 - half; x <= 8 + half; x++)
            {
                bool edge = (x == 8 - half) || (x == 8 + half) || (y == 12) || (y == 2);
                px[y * SIZE + x] = edge ? shade : body;
            }
        }
        // Cupola del pilota
        px[9 * SIZE + 8] = glass;
        px[8 * SIZE + 7] = glass; px[8 * SIZE + 8] = glass; px[8 * SIZE + 9] = glass;
        // Fiamma del motore (sotto)
        px[1 * SIZE + 7] = flame; px[1 * SIZE + 8] = flame; px[1 * SIZE + 9] = flame;
        px[0 * SIZE + 8] = flame;
        return Bake(px);
    }

    // -------- Astro, l'alieno verde con lo zaino-aspirapolvere --------
    public static Sprite CreateAstro()
    {
        var px = NewTransparent();
        Color skin = new Color(0.40f, 0.85f, 0.40f);
        Color skinDark = new Color(0.20f, 0.55f, 0.25f);
        Color belly = new Color(0.70f, 1f, 0.70f);
        Color eyeW = Color.white;
        Color eyeB = Color.black;
        Color tank = new Color(0.85f, 0.85f, 0.95f);
        Color tankDark = new Color(0.40f, 0.45f, 0.60f);
        Color antenna = new Color(1f, 0.85f, 0.20f);

        // Corpo tondo
        for (int y = 2; y <= 12; y++)
            for (int x = 3; x <= 12; x++)
            {
                float dx = x - 7.5f, dy = y - 7.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < 4.2f) px[y * SIZE + x] = skin;
                else if (d < 5.0f) px[y * SIZE + x] = skinDark;
            }
        // Pancia chiara
        for (int y = 4; y <= 7; y++)
            for (int x = 6; x <= 9; x++) px[y * SIZE + x] = belly;
        // Occhi
        px[9 * SIZE + 5] = eyeW; px[9 * SIZE + 6] = eyeW;
        px[9 * SIZE + 9] = eyeW; px[9 * SIZE + 10] = eyeW;
        px[9 * SIZE + 6] = eyeB; px[9 * SIZE + 9] = eyeB;
        // Bocca sorridente
        px[7 * SIZE + 6] = skinDark; px[7 * SIZE + 7] = skinDark;
        px[7 * SIZE + 8] = skinDark; px[7 * SIZE + 9] = skinDark;
        // Antenna con pallina gialla
        px[13 * SIZE + 8] = skinDark;
        px[14 * SIZE + 8] = skinDark;
        px[15 * SIZE + 8] = antenna; px[15 * SIZE + 7] = antenna; px[15 * SIZE + 9] = antenna;
        // Zaino aspirapolvere (lati)
        for (int y = 5; y <= 10; y++)
        {
            px[y * SIZE + 1] = tankDark; px[y * SIZE + 2] = tank;
            px[y * SIZE + 13] = tank;    px[y * SIZE + 14] = tankDark;
        }
        return Bake(px);
    }

    // -------- Cristallo di polvere cosmica --------
    // Piccolo rombo brillante, colore variabile per dare vita alla scena.
    public static Sprite CreateCrystal(Color tint)
    {
        var px = NewTransparent();
        Color core = tint;
        Color edge = Darken(tint, 0.45f);
        Color spark = Color.white;

        // Rombo
        for (int y = 0; y < SIZE; y++)
        {
            int half = Mathf.Max(0, 6 - Mathf.Abs(y - 8));
            for (int x = 8 - half; x <= 8 + half; x++)
            {
                if (x < 0 || x >= SIZE) continue;
                bool e = (x == 8 - half) || (x == 8 + half);
                px[y * SIZE + x] = e ? edge : core;
            }
        }
        // Scintilla
        px[10 * SIZE + 9] = spark;
        px[11 * SIZE + 9] = spark;
        return Bake(px);
    }

    // -------- Stella di sfondo (3x3 di pixel chiari) --------
    public static Sprite CreateStar(Color tint)
    {
        var px = NewTransparent();
        px[8 * SIZE + 8] = tint;
        px[8 * SIZE + 7] = tint;
        px[8 * SIZE + 9] = tint;
        px[7 * SIZE + 8] = tint;
        px[9 * SIZE + 8] = tint;
        return Bake(px);
    }

    // -------- Piccolo pianeta decorativo --------
    public static Sprite CreatePlanet(Color a, Color b)
    {
        var px = NewTransparent();
        float cx = 7.5f, cy = 7.5f, r = 6f;
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                float dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= r - 1)
                    px[y * SIZE + x] = ((x + y) % 3 == 0) ? b : a;
                else if (d <= r)
                    px[y * SIZE + x] = Darken(a, 0.5f);
            }
        return Bake(px);
    }

    // -------- "Pianeta Amico" che appare a missione completata --------
    // Pianeta sorridente, piu' grande e colorato.
    public static Sprite CreateFriendPlanet()
    {
        var px = NewTransparent();
        Color a = new Color(0.95f, 0.55f, 0.85f);   // rosa
        Color b = new Color(0.55f, 0.80f, 1f);      // azzurro
        Color edge = new Color(0.40f, 0.20f, 0.55f);
        Color eye = Color.black;
        Color mouth = new Color(0.30f, 0.10f, 0.10f);

        float cx = 7.5f, cy = 7.5f, r = 6.5f;
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                float dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= r - 1)
                    px[y * SIZE + x] = (y > 7) ? a : b;
                else if (d <= r)
                    px[y * SIZE + x] = edge;
            }
        // Occhioni
        px[9 * SIZE + 5] = eye; px[9 * SIZE + 6] = eye;
        px[9 * SIZE + 9] = eye; px[9 * SIZE + 10] = eye;
        // Sorrisone
        px[6 * SIZE + 5] = mouth; px[6 * SIZE + 10] = mouth;
        px[5 * SIZE + 6] = mouth; px[5 * SIZE + 7] = mouth;
        px[5 * SIZE + 8] = mouth; px[5 * SIZE + 9] = mouth;
        return Bake(px);
    }

    // -------- Tessera 1x1 del raggio traente (rettangolino chiaro) --------
    public static Sprite CreateBeamTile()
    {
        var px = NewTransparent();
        Color a = new Color(0.55f, 0.95f, 1f, 0.85f);
        Color b = new Color(1f, 1f, 1f, 1f);
        for (int y = 0; y < SIZE; y++)
            for (int x = 5; x <= 10; x++)
                px[y * SIZE + x] = ((x + y) % 2 == 0) ? a : b;
        return Bake(px);
    }

    // -------- Barriera-asteroide (tessera) --------
    // Pixel a "roccia" con bordo scuro: scalata dal loader sulla dimensione richiesta.
    public static Sprite CreateBarrierTile(Color baseColor)
    {
        var px = NewTransparent();
        Color light = baseColor;
        Color dark = Darken(baseColor, 0.55f);
        Color edge = Darken(baseColor, 0.30f);

        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                bool border = (x == 0 || x == SIZE - 1 || y == 0 || y == SIZE - 1);
                // crateri pseudo-casuali ma deterministici
                int h = ((x * 13 + y * 7) % 9);
                Color c = border ? edge : (h < 3 ? dark : light);
                px[y * SIZE + x] = c;
            }
        return Bake(px);
    }

    // -------- Bomba (sfera scura con miccia rossa) --------
    public static Sprite CreateBomb()
    {
        var px = NewTransparent();
        Color body = new Color(0.12f, 0.10f, 0.18f);
        Color shine = new Color(0.55f, 0.55f, 0.70f);
        Color fuseDark = new Color(0.45f, 0.30f, 0.20f);
        Color spark = new Color(1f, 0.55f, 0.20f);

        // Sfera
        float cx = 7.5f, cy = 6.5f, r = 5.5f;
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d <= r) px[y * SIZE + x] = body;
            }
        // Riflesso in alto-sinistra
        px[9 * SIZE + 5] = shine; px[9 * SIZE + 6] = shine;
        px[10 * SIZE + 5] = shine;

        // Miccia con scintilla in cima
        px[12 * SIZE + 8] = fuseDark;
        px[13 * SIZE + 9] = fuseDark;
        px[14 * SIZE + 9] = fuseDark;
        px[15 * SIZE + 9] = spark;
        px[14 * SIZE + 10] = spark;
        return Bake(px);
    }

    // -------- Esplosione (anello luminoso, scalata dall'animazione) --------
    public static Sprite CreateExplosion()
    {
        var px = NewTransparent();
        Color inner = new Color(1f, 1f, 0.6f);
        Color mid = new Color(1f, 0.55f, 0.20f);
        Color outer = new Color(0.85f, 0.20f, 0.20f);
        float cx = 7.5f, cy = 7.5f;
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d <= 3f) px[y * SIZE + x] = inner;
                else if (d <= 5.5f) px[y * SIZE + x] = mid;
                else if (d <= 7f) px[y * SIZE + x] = outer;
            }
        return Bake(px);
    }

    // -------- Quadratino pieno (per coriandoli e barre HUD in-world) --------
    public static Sprite CreateSolid(Color color)
    {
        var px = new Color[SIZE * SIZE];
        for (int i = 0; i < px.Length; i++) px[i] = color;
        return Bake(px);
    }

    // -------- utility --------

    static Color[] NewTransparent()
    {
        var px = new Color[SIZE * SIZE];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        return px;
    }

    static Color Darken(Color c, float f)
    {
        return new Color(c.r * f, c.g * f, c.b * f, c.a);
    }

    static Sprite Bake(Color[] px)
    {
        var tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), PPU);
    }
}
