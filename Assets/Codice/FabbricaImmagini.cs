using UnityEngine;

// Disegna da codice tutti gli sprite del gioco (pixel art 16x16).
// Cosi' il progetto non ha bisogno di file immagine esterni.
// Per cambiare l'aspetto di un oggetto basta cambiare i pixel qui.
public static class FabbricaImmagini
{
    private const int LATO = 16; // griglia 16x16
    private const int PPU = 16;  // 16 pixel = 1 unita' di Unity

    // ---- ASTRO (alieno verde con zaino) ----
    public static Sprite CreaAstro()
    {
        Color[] pixel = NuovaGriglia();

        Color pelle = new Color(0.40f, 0.85f, 0.40f);
        Color pelleScura = new Color(0.20f, 0.55f, 0.25f);
        Color pancia = new Color(0.70f, 1f, 0.70f);
        Color bianco = Color.white;
        Color nero = Color.black;
        Color serbatoio = new Color(0.85f, 0.85f, 0.95f);
        Color serbatoioScuro = new Color(0.40f, 0.45f, 0.60f);
        Color antenna = new Color(1f, 0.85f, 0.20f);

        // Corpo tondo
        for (int y = 2; y <= 12; y++)
        {
            for (int x = 3; x <= 12; x++)
            {
                float dx = x - 7.5f;
                float dy = y - 7.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 4.2f) pixel[y * LATO + x] = pelle;
                else if (dist < 5.0f) pixel[y * LATO + x] = pelleScura;
            }
        }

        // Pancia chiara
        for (int y = 4; y <= 7; y++)
        {
            for (int x = 6; x <= 9; x++)
            {
                pixel[y * LATO + x] = pancia;
            }
        }

        // Occhi
        pixel[9 * LATO + 5] = bianco;
        pixel[9 * LATO + 6] = bianco;
        pixel[9 * LATO + 9] = bianco;
        pixel[9 * LATO + 10] = bianco;
        pixel[9 * LATO + 6] = nero; // pupille
        pixel[9 * LATO + 9] = nero;

        // Bocca
        pixel[7 * LATO + 6] = pelleScura;
        pixel[7 * LATO + 7] = pelleScura;
        pixel[7 * LATO + 8] = pelleScura;
        pixel[7 * LATO + 9] = pelleScura;

        // Antenna con pallina gialla in cima
        pixel[13 * LATO + 8] = pelleScura;
        pixel[14 * LATO + 8] = pelleScura;
        pixel[15 * LATO + 7] = antenna;
        pixel[15 * LATO + 8] = antenna;
        pixel[15 * LATO + 9] = antenna;

        // Zainetto ai lati
        for (int y = 5; y <= 10; y++)
        {
            pixel[y * LATO + 1] = serbatoioScuro;
            pixel[y * LATO + 2] = serbatoio;
            pixel[y * LATO + 13] = serbatoio;
            pixel[y * LATO + 14] = serbatoioScuro;
        }

        return Cuoci(pixel);
    }

    // ---- CARAMELLA (cerchio con ali ai lati) ----
    public static Sprite CreaCaramella(Color tinta)
    {
        Color[] pixel = NuovaGriglia();

        Color corpo = tinta;
        Color bordo = Scurisci(tinta, 0.45f);
        Color luce = Color.white;
        Color incarto = new Color(
            0.5f + tinta.r * 0.5f,
            0.5f + tinta.g * 0.5f,
            0.5f + tinta.b * 0.5f, 1f);

        // Cerchio
        float cx = 7.5f, cy = 7.5f, r = 3.8f;
        for (int y = 0; y < LATO; y++)
        {
            for (int x = 0; x < LATO; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist <= r - 1) pixel[y * LATO + x] = corpo;
                else if (dist <= r) pixel[y * LATO + x] = bordo;
            }
        }

        // Riflesso bianco
        pixel[10 * LATO + 6] = luce;
        pixel[10 * LATO + 7] = luce;
        pixel[9 * LATO + 6] = luce;

        // Ala sinistra
        pixel[8 * LATO + 3] = incarto;
        pixel[7 * LATO + 3] = incarto;
        pixel[9 * LATO + 2] = incarto;
        pixel[6 * LATO + 2] = incarto;
        pixel[8 * LATO + 1] = incarto;
        pixel[7 * LATO + 1] = incarto;
        pixel[10 * LATO + 2] = bordo;
        pixel[5 * LATO + 2] = bordo;
        pixel[9 * LATO + 1] = bordo;
        pixel[6 * LATO + 1] = bordo;

        // Ala destra
        pixel[8 * LATO + 12] = incarto;
        pixel[7 * LATO + 12] = incarto;
        pixel[9 * LATO + 13] = incarto;
        pixel[6 * LATO + 13] = incarto;
        pixel[8 * LATO + 14] = incarto;
        pixel[7 * LATO + 14] = incarto;
        pixel[10 * LATO + 13] = bordo;
        pixel[5 * LATO + 13] = bordo;
        pixel[9 * LATO + 14] = bordo;
        pixel[6 * LATO + 14] = bordo;

        return Cuoci(pixel);
    }

    // ---- CHIAVE DORATA ----
    public static Sprite CreaChiave()
    {
        Color[] pixel = NuovaGriglia();

        Color oro = new Color(1f, 0.85f, 0.20f);
        Color oroScuro = new Color(0.70f, 0.50f, 0.10f);
        Color luce = Color.white;

        // Anello (cerchio cavo a sinistra)
        float cx = 4.5f, cy = 7.5f, raggioEsterno = 3.2f, raggioInterno = 1.6f;
        for (int y = 0; y < LATO; y++)
        {
            for (int x = 0; x < LATO; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist <= raggioEsterno && dist > raggioInterno)
                {
                    if (dist > raggioEsterno - 0.7f)
                        pixel[y * LATO + x] = oroScuro;
                    else
                        pixel[y * LATO + x] = oro;
                }
            }
        }
        pixel[9 * LATO + 3] = luce; // riflesso

        // Asta verso destra
        for (int x = 7; x <= 13; x++)
        {
            pixel[7 * LATO + x] = oro;
            pixel[8 * LATO + x] = oro;
            pixel[6 * LATO + x] = oroScuro;
            pixel[9 * LATO + x] = oroScuro;
        }

        // Denti
        pixel[5 * LATO + 11] = oro;
        pixel[5 * LATO + 12] = oro;
        pixel[5 * LATO + 13] = oro;
        pixel[4 * LATO + 11] = oroScuro;
        pixel[4 * LATO + 13] = oroScuro;

        return Cuoci(pixel);
    }

    // ---- PORTA / PORTALE ----
    public static Sprite CreaPorta()
    {
        Color[] pixel = NuovaGriglia();

        Color cornice = new Color(0.75f, 0.55f, 0.30f);
        Color corniceScura = new Color(0.35f, 0.20f, 0.08f);
        Color portale = new Color(0.55f, 0.30f, 0.85f);
        Color portaleLuce = new Color(0.85f, 0.65f, 1f);
        Color stella = Color.white;

        // Cornice rettangolare verticale
        for (int y = 1; y <= 14; y++)
        {
            for (int x = 3; x <= 11; x++)
            {
                bool esterno = (x == 3 || x == 11 || y == 1 || y == 14);
                bool interno = (x == 4 || x == 10 || y == 2 || y == 13);

                if (esterno) pixel[y * LATO + x] = corniceScura;
                else if (interno) pixel[y * LATO + x] = cornice;
                else pixel[y * LATO + x] = portale;
            }
        }

        // Luce centrale del portale
        for (int y = 5; y <= 10; y++)
        {
            for (int x = 6; x <= 8; x++)
            {
                pixel[y * LATO + x] = portaleLuce;
            }
        }

        // Stellina centrale
        pixel[8 * LATO + 7] = stella;
        pixel[7 * LATO + 6] = stella;
        pixel[7 * LATO + 8] = stella;
        pixel[6 * LATO + 7] = stella;

        return Cuoci(pixel);
    }

    // ---- STELLINA DI SFONDO ----
    public static Sprite CreaStella(Color tinta)
    {
        Color[] pixel = NuovaGriglia();
        // Una crocetta di 5 pixel
        pixel[8 * LATO + 8] = tinta;
        pixel[8 * LATO + 7] = tinta;
        pixel[8 * LATO + 9] = tinta;
        pixel[7 * LATO + 8] = tinta;
        pixel[9 * LATO + 8] = tinta;
        return Cuoci(pixel);
    }

    // ---- PIANETA DECORATIVO (sullo sfondo) ----
    public static Sprite CreaPianeta(Color principale, Color screziato)
    {
        Color[] pixel = NuovaGriglia();

        float cx = 7.5f, cy = 7.5f, raggio = 6f;
        for (int y = 0; y < LATO; y++)
        {
            for (int x = 0; x < LATO; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= raggio - 1)
                {
                    // Pattern a chiazze
                    if (((x + y) % 3) == 0) pixel[y * LATO + x] = screziato;
                    else pixel[y * LATO + x] = principale;
                }
                else if (dist <= raggio)
                {
                    pixel[y * LATO + x] = Scurisci(principale, 0.5f);
                }
            }
        }
        return Cuoci(pixel);
    }

    // ---- PIANETA AMICO (compare a fine missione) ----
    public static Sprite CreaPianetaAmico()
    {
        Color[] pixel = NuovaGriglia();

        Color rosa = new Color(0.95f, 0.55f, 0.85f);
        Color azzurro = new Color(0.55f, 0.80f, 1f);
        Color bordo = new Color(0.40f, 0.20f, 0.55f);
        Color occhio = Color.black;
        Color bocca = new Color(0.30f, 0.10f, 0.10f);

        float cx = 7.5f, cy = 7.5f, raggio = 6.5f;
        for (int y = 0; y < LATO; y++)
        {
            for (int x = 0; x < LATO; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= raggio - 1)
                {
                    if (y > 7) pixel[y * LATO + x] = rosa;
                    else pixel[y * LATO + x] = azzurro;
                }
                else if (dist <= raggio)
                {
                    pixel[y * LATO + x] = bordo;
                }
            }
        }
        // Occhioni
        pixel[9 * LATO + 5] = occhio;
        pixel[9 * LATO + 6] = occhio;
        pixel[9 * LATO + 9] = occhio;
        pixel[9 * LATO + 10] = occhio;
        // Sorriso
        pixel[6 * LATO + 5] = bocca;
        pixel[6 * LATO + 10] = bocca;
        pixel[5 * LATO + 6] = bocca;
        pixel[5 * LATO + 7] = bocca;
        pixel[5 * LATO + 8] = bocca;
        pixel[5 * LATO + 9] = bocca;

        return Cuoci(pixel);
    }

    // ---- TESSERA ASTEROIDE (la "barriera") ----
    public static Sprite CreaTesseraAsteroide(Color coloreBase)
    {
        Color[] pixel = NuovaGriglia();

        Color chiaro = coloreBase;
        Color scuro = Scurisci(coloreBase, 0.55f);
        Color bordo = Scurisci(coloreBase, 0.30f);

        for (int y = 0; y < LATO; y++)
        {
            for (int x = 0; x < LATO; x++)
            {
                bool sulBordo = (x == 0 || x == LATO - 1 || y == 0 || y == LATO - 1);
                int rumore = ((x * 13 + y * 7) % 9); // crateri "finti casuali"

                Color c;
                if (sulBordo) c = bordo;
                else if (rumore < 3) c = scuro;
                else c = chiaro;
                pixel[y * LATO + x] = c;
            }
        }
        return Cuoci(pixel);
    }

    // ---- BOMBA (sfera scura con miccia) ----
    public static Sprite CreaBomba()
    {
        Color[] pixel = NuovaGriglia();

        Color corpo = new Color(0.12f, 0.10f, 0.18f);
        Color luce = new Color(0.55f, 0.55f, 0.70f);
        Color miccia = new Color(0.45f, 0.30f, 0.20f);
        Color fiamma = new Color(1f, 0.55f, 0.20f);

        // Sfera
        float cx = 7.5f, cy = 6.5f, raggio = 5.5f;
        for (int y = 0; y < LATO; y++)
        {
            for (int x = 0; x < LATO; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist <= raggio) pixel[y * LATO + x] = corpo;
            }
        }

        // Riflesso in alto a sinistra
        pixel[9 * LATO + 5] = luce;
        pixel[9 * LATO + 6] = luce;
        pixel[10 * LATO + 5] = luce;

        // Miccia con scintilla
        pixel[12 * LATO + 8] = miccia;
        pixel[13 * LATO + 9] = miccia;
        pixel[14 * LATO + 9] = miccia;
        pixel[15 * LATO + 9] = fiamma;
        pixel[14 * LATO + 10] = fiamma;

        return Cuoci(pixel);
    }

    // ---- ESPLOSIONE (anello luminoso a 3 strati) ----
    public static Sprite CreaEsplosione()
    {
        Color[] pixel = NuovaGriglia();

        Color interno = new Color(1f, 1f, 0.6f);
        Color medio = new Color(1f, 0.55f, 0.20f);
        Color esterno = new Color(0.85f, 0.20f, 0.20f);

        float cx = 7.5f, cy = 7.5f;
        for (int y = 0; y < LATO; y++)
        {
            for (int x = 0; x < LATO; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist <= 3f) pixel[y * LATO + x] = interno;
                else if (dist <= 5.5f) pixel[y * LATO + x] = medio;
                else if (dist <= 7f) pixel[y * LATO + x] = esterno;
            }
        }
        return Cuoci(pixel);
    }

    // ---- QUADRATO PIENO (lo uso per coriandoli e barre dell'HUD) ----
    public static Sprite CreaQuadratoPieno(Color colore)
    {
        Color[] pixel = new Color[LATO * LATO];
        for (int i = 0; i < pixel.Length; i++)
        {
            pixel[i] = colore;
        }
        return Cuoci(pixel);
    }

    // ---- Funzioni di supporto ----

    static Color[] NuovaGriglia()
    {
        // Griglia di pixel tutti trasparenti
        Color[] pixel = new Color[LATO * LATO];
        for (int i = 0; i < pixel.Length; i++)
        {
            pixel[i] = Color.clear;
        }
        return pixel;
    }

    static Color Scurisci(Color c, float fattore)
    {
        // Moltiplico i canali RGB per "fattore"
        return new Color(c.r * fattore, c.g * fattore, c.b * fattore, c.a);
    }

    static Sprite Cuoci(Color[] pixel)
    {
        // Trasformo l'array di colori in una Texture e poi in uno Sprite.
        // filterMode = Point cosi' la pixel art resta "a quadretti".
        Texture2D tex = new Texture2D(LATO, LATO, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixel);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, LATO, LATO),
            new Vector2(0.5f, 0.5f),
            PPU);
    }
}
