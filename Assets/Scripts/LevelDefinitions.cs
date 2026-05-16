using System.Collections.Generic;
using UnityEngine;

// Definizione "in chiaro" dei livelli del gioco.
// Aggiungere un livello = aggiungere un metodo qui e una riga in BuildAll().
public static class LevelDefinitions
{
    // Palette 8-bit (colori vivaci, pochi toni per livello).
    static readonly Color RED    = new Color(0.90f, 0.20f, 0.20f);
    static readonly Color BLUE   = new Color(0.20f, 0.45f, 0.95f);
    static readonly Color GREEN  = new Color(0.30f, 0.80f, 0.30f);
    static readonly Color YELLOW = new Color(1.00f, 0.85f, 0.20f);
    static readonly Color ORANGE = new Color(1.00f, 0.55f, 0.10f);
    static readonly Color GRAY   = new Color(0.70f, 0.70f, 0.75f);

    public static List<LevelData> BuildAll()
    {
        return new List<LevelData>
        {
            Level1_Colors(),
            Level2_Shapes(),
            Level3_Categories(),
            Level4_Mixed()
        };
    }

    // L1: smista per COLORE (3 cestini colorati, oggetti tutti cerchi).
    static LevelData Level1_Colors()
    {
        var lv = new LevelData { title = "Smista per COLORE" };

        lv.zones.Add(new ZoneSpec { label = "ROSSO", color = RED,   acceptedCategory = "red",   position = new Vector2(-5, -3.5f) });
        lv.zones.Add(new ZoneSpec { label = "BLU",   color = BLUE,  acceptedCategory = "blue",  position = new Vector2( 0, -3.5f) });
        lv.zones.Add(new ZoneSpec { label = "VERDE", color = GREEN, acceptedCategory = "green", position = new Vector2( 5, -3.5f) });

        for (int i = 0; i < 2; i++) lv.items.Add(new ItemSpec { shape = ShapeType.Circle, color = RED,   category = "red"   });
        for (int i = 0; i < 2; i++) lv.items.Add(new ItemSpec { shape = ShapeType.Circle, color = BLUE,  category = "blue"  });
        for (int i = 0; i < 2; i++) lv.items.Add(new ItemSpec { shape = ShapeType.Circle, color = GREEN, category = "green" });

        return lv;
    }

    // L2: smista per FORMA (3 cestini, oggetti tutti grigi).
    static LevelData Level2_Shapes()
    {
        var lv = new LevelData { title = "Smista per FORMA" };

        lv.zones.Add(new ZoneSpec { label = "CERCHIO",   color = GRAY, acceptedCategory = "circle",   position = new Vector2(-5, -3.5f) });
        lv.zones.Add(new ZoneSpec { label = "QUADRATO",  color = GRAY, acceptedCategory = "square",   position = new Vector2( 0, -3.5f) });
        lv.zones.Add(new ZoneSpec { label = "TRIANGOLO", color = GRAY, acceptedCategory = "triangle", position = new Vector2( 5, -3.5f) });

        for (int i = 0; i < 2; i++) lv.items.Add(new ItemSpec { shape = ShapeType.Circle,   color = GRAY, category = "circle"   });
        for (int i = 0; i < 2; i++) lv.items.Add(new ItemSpec { shape = ShapeType.Square,   color = GRAY, category = "square"   });
        for (int i = 0; i < 2; i++) lv.items.Add(new ItemSpec { shape = ShapeType.Triangle, color = GRAY, category = "triangle" });

        return lv;
    }

    // L3: smista per CATEGORIA semantica (frutta vs verdura).
    // Le forme + colori rappresentano simbolicamente cibi.
    static LevelData Level3_Categories()
    {
        var lv = new LevelData { title = "FRUTTA o VERDURA?" };

        lv.zones.Add(new ZoneSpec { label = "FRUTTA",  color = RED,   acceptedCategory = "frutta",  position = new Vector2(-3, -3.5f) });
        lv.zones.Add(new ZoneSpec { label = "VERDURA", color = GREEN, acceptedCategory = "verdura", position = new Vector2( 3, -3.5f) });

        // "frutta": mela (cerchio rosso), banana (barra gialla), arancia (cerchio arancione)
        lv.items.Add(new ItemSpec { shape = ShapeType.Circle, color = RED,    category = "frutta" });
        lv.items.Add(new ItemSpec { shape = ShapeType.Bar,    color = YELLOW, category = "frutta" });
        lv.items.Add(new ItemSpec { shape = ShapeType.Circle, color = ORANGE, category = "frutta" });
        // "verdura": carota (triangolo arancione), insalata (cerchio verde), zucchina (barra verde)
        lv.items.Add(new ItemSpec { shape = ShapeType.Triangle, color = ORANGE, category = "verdura" });
        lv.items.Add(new ItemSpec { shape = ShapeType.Circle,   color = GREEN,  category = "verdura" });
        lv.items.Add(new ItemSpec { shape = ShapeType.Bar,      color = GREEN,  category = "verdura" });

        return lv;
    }

    // L4: regole combinate + timer leggero (sfida finale).
    static LevelData Level4_Mixed()
    {
        var lv = new LevelData { title = "SFIDA FINALE - sbrigati!" };
        lv.timeLimit = 30f;

        lv.zones.Add(new ZoneSpec { label = "ROSSO", color = RED,   acceptedCategory = "red",   position = new Vector2(-5, -3.5f) });
        lv.zones.Add(new ZoneSpec { label = "BLU",   color = BLUE,  acceptedCategory = "blue",  position = new Vector2( 0, -3.5f) });
        lv.zones.Add(new ZoneSpec { label = "VERDE", color = GREEN, acceptedCategory = "green", position = new Vector2( 5, -3.5f) });

        // Mix di forme, smistate sempre per colore.
        ShapeType[] shapes = { ShapeType.Circle, ShapeType.Square, ShapeType.Triangle, ShapeType.Bar };
        Color[] cols = { RED, BLUE, GREEN };
        string[] cats = { "red", "blue", "green" };
        for (int i = 0; i < 9; i++)
        {
            int c = i % 3;
            lv.items.Add(new ItemSpec { shape = shapes[i % shapes.Length], color = cols[c], category = cats[c] });
        }
        return lv;
    }
}
