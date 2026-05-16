using UnityEngine;

// Genera sprite "8-bit" da codice usando una piccola griglia di pixel.
// Cosi' non servono asset esterni (PNG, ecc.) e il look pixel resta autentico.
public static class SpriteFactory
{
    private const int SIZE = 16;        // griglia 16x16 (classico 8-bit)
    private const int PPU  = 16;        // 16 pixel = 1 unita' Unity

    // Sprite di una forma (per gli oggetti trascinabili).
    public static Sprite CreateShape(ShapeType shape, Color color)
    {
        Color[] px = NewTransparent();
        Color outline = DarkenColor(color, 0.5f);

        switch (shape)
        {
            case ShapeType.Circle:   DrawCircle(px, color, outline);   break;
            case ShapeType.Square:   DrawSquare(px, color, outline);   break;
            case ShapeType.Triangle: DrawTriangle(px, color, outline); break;
            case ShapeType.Bar:      DrawBar(px, color, outline);      break;
        }
        return Bake(px);
    }

    // Sprite del cestino (rettangolo con bordo scuro).
    public static Sprite CreateZone(Color color)
    {
        Color[] px = NewTransparent();
        Color outline = DarkenColor(color, 0.4f);
        // riempimento
        for (int y = 1; y < SIZE - 1; y++)
            for (int x = 1; x < SIZE - 1; x++) px[y * SIZE + x] = color;
        // bordo
        for (int i = 0; i < SIZE; i++)
        {
            px[i] = outline;                       // riga in basso
            px[(SIZE - 1) * SIZE + i] = outline;   // riga in alto
            px[i * SIZE] = outline;                // colonna sinistra
            px[i * SIZE + SIZE - 1] = outline;     // colonna destra
        }
        return Bake(px);
    }

    // -------- helpers di disegno --------

    static void DrawCircle(Color[] px, Color fill, Color outline)
    {
        float cx = (SIZE - 1) / 2f, cy = (SIZE - 1) / 2f;
        float r = SIZE / 2f - 1;
        for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d <= r - 1) px[y * SIZE + x] = fill;
                else if (d <= r) px[y * SIZE + x] = outline;
            }
    }

    static void DrawSquare(Color[] px, Color fill, Color outline)
    {
        int m = 2; // margine
        for (int y = m; y < SIZE - m; y++)
            for (int x = m; x < SIZE - m; x++) px[y * SIZE + x] = fill;
        for (int i = m; i < SIZE - m; i++)
        {
            px[m * SIZE + i] = outline;
            px[(SIZE - m - 1) * SIZE + i] = outline;
            px[i * SIZE + m] = outline;
            px[i * SIZE + SIZE - m - 1] = outline;
        }
    }

    static void DrawTriangle(Color[] px, Color fill, Color outline)
    {
        // Triangolo isoscele con vertice in alto.
        for (int y = 1; y < SIZE - 1; y++)
        {
            float t = (float)y / (SIZE - 2);            // 0 in basso, 1 in alto
            int half = Mathf.RoundToInt((1 - t) * (SIZE / 2 - 1));
            int cx = SIZE / 2;
            for (int x = cx - half; x <= cx + half; x++)
            {
                if (x < 0 || x >= SIZE) continue;
                bool edge = (x == cx - half) || (x == cx + half) || (y == 1);
                px[y * SIZE + x] = edge ? outline : fill;
            }
        }
    }

    static void DrawBar(Color[] px, Color fill, Color outline)
    {
        // Rettangolo orizzontale (es. banana / zucchina).
        int yMin = 6, yMax = 10;
        for (int y = yMin; y <= yMax; y++)
            for (int x = 2; x < SIZE - 2; x++) px[y * SIZE + x] = fill;
        for (int x = 2; x < SIZE - 2; x++)
        {
            px[yMin * SIZE + x] = outline;
            px[yMax * SIZE + x] = outline;
        }
        for (int y = yMin; y <= yMax; y++)
        {
            px[y * SIZE + 2] = outline;
            px[y * SIZE + SIZE - 3] = outline;
        }
    }

    // -------- utilita' --------

    static Color[] NewTransparent()
    {
        var px = new Color[SIZE * SIZE];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        return px;
    }

    static Color DarkenColor(Color c, float factor)
    {
        return new Color(c.r * factor, c.g * factor, c.b * factor, 1f);
    }

    static Sprite Bake(Color[] px)
    {
        var tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;   // niente smussatura: look pixel
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), PPU);
    }
}
