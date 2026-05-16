using System.Collections.Generic;
using UnityEngine;

// Definizione "in chiaro" dei livelli di Missione Spaziale.
// Ogni livello descrive: titolo, numero/posizioni dei cristalli,
// elenco delle barriere (rettangoli) e delle bombe.
public static class SpaceLevels
{
    public const int Count = 3;

    public static SpaceLevelDef Get(int index)
    {
        switch (index)
        {
            case 0: return Level1_Open();
            case 1: return Level2_Barriers();
            case 2: return Level3_Bombs();
            default: return null;
        }
    }

    // --- Palette dei cristalli (riuso anche in altri livelli)
    static readonly Color[] CRYSTAL_COLORS =
    {
        new Color(1f, 0.85f, 0.30f),
        new Color(0.40f, 0.85f, 1f),
        new Color(0.95f, 0.45f, 0.85f),
        new Color(0.55f, 1f, 0.55f),
        new Color(1f, 0.55f, 0.30f),
    };
    static readonly Color ASTEROID = new Color(0.55f, 0.50f, 0.45f);

    // ============================================================
    // LIVELLO 1: spazio aperto, 10 cristalli sparsi
    // ============================================================
    static SpaceLevelDef Level1_Open()
    {
        var lv = new SpaceLevelDef
        {
            index = 0,
            title = "LIVELLO 1 - PRIMO VOLO",
            objective = "RACCOGLI 10 CRISTALLI",
        };
        // Posizioni random: il loader le genera al volo.
        lv.randomCrystals = 10;
        return lv;
    }

    // ============================================================
    // LIVELLO 2: corridoi tra gli asteroidi (barriere)
    // Toccare una barriera = penalita' (-5 punti, flash rosso, shake).
    // ============================================================
    static SpaceLevelDef Level2_Barriers()
    {
        var lv = new SpaceLevelDef
        {
            index = 1,
            title = "LIVELLO 2 - TRA GLI ASTEROIDI",
            objective = "EVITA LE BARRIERE",
        };

        // Cornice "a corridoio": due muri verticali ai lati + un soffitto
        // + due ostacoli interni.
        lv.barriers.Add(B(-7.5f,  0f, 1.2f, 5.0f));
        lv.barriers.Add(B( 7.5f,  0f, 1.2f, 5.0f));
        lv.barriers.Add(B( 0f,  4.5f, 9.0f, 1.0f));
        lv.barriers.Add(B(-2.5f, 1.0f, 1.5f, 1.5f));
        lv.barriers.Add(B( 2.5f,-1.5f, 1.8f, 1.0f));

        // Cristalli posizionati nei "varchi" tra le barriere.
        AddCrystals(lv, new Vector2[]
        {
            new Vector2(-5.5f,  3.0f), new Vector2(-3.0f,  3.5f),
            new Vector2( 3.0f,  3.5f), new Vector2( 5.5f,  3.0f),
            new Vector2(-5.0f,  0.0f), new Vector2( 0.0f,  2.5f),
            new Vector2( 5.0f,  0.5f), new Vector2(-3.5f, -2.0f),
            new Vector2( 0.0f, -3.0f), new Vector2( 4.5f, -3.0f),
        });

        return lv;
    }

    // ============================================================
    // LIVELLO 3: stesse barriere del livello 2 + bombe
    // Bomba toccata o aspirata = GAME OVER.
    // ============================================================
    static SpaceLevelDef Level3_Bombs()
    {
        var lv = new SpaceLevelDef
        {
            index = 2,
            title = "LIVELLO 3 - CAMPO MINATO",
            objective = "ATTENZIONE ALLE BOMBE",
        };

        // Stesse barriere del livello 2
        lv.barriers.Add(B(-7.5f,  0f, 1.2f, 5.0f));
        lv.barriers.Add(B( 7.5f,  0f, 1.2f, 5.0f));
        lv.barriers.Add(B( 0f,  4.5f, 9.0f, 1.0f));
        lv.barriers.Add(B(-2.5f, 1.0f, 1.5f, 1.5f));
        lv.barriers.Add(B( 2.5f,-1.5f, 1.8f, 1.0f));

        // Bombe (4) NON al centro: zona di spawn (raggio ~3 dall'origine)
        // deve essere libera, cosi' il giocatore puo' orientarsi.
        lv.bombs.Add(new Vector2(-6.0f,  2.5f));
        lv.bombs.Add(new Vector2( 6.0f,  2.5f));
        lv.bombs.Add(new Vector2(-4.0f, -3.5f));
        lv.bombs.Add(new Vector2( 4.0f, -3.5f));

        // 10 cristalli sparsi tra barriere e bombe (lontani dalle bombe)
        AddCrystals(lv, new Vector2[]
        {
            new Vector2(-4.0f,  3.0f), new Vector2(-1.5f,  3.0f),
            new Vector2( 1.5f,  3.0f), new Vector2( 4.0f,  3.0f),
            new Vector2(-5.0f,  0.0f), new Vector2( 0.0f,  2.5f),
            new Vector2( 5.0f,  0.0f), new Vector2(-1.5f, -3.0f),
            new Vector2( 1.5f, -3.0f), new Vector2( 0.0f, -3.5f),
        });

        return lv;
    }

    // -------- helper --------

    static BarrierSpec B(float cx, float cy, float w, float h)
    {
        return new BarrierSpec
        {
            center = new Vector2(cx, cy),
            size = new Vector2(w, h),
            color = ASTEROID,
        };
    }

    static void AddCrystals(SpaceLevelDef lv, Vector2[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            lv.crystals.Add(new CrystalSpec
            {
                position = positions[i],
                color = CRYSTAL_COLORS[i % CRYSTAL_COLORS.Length],
            });
        }
    }
}

// -------- Tipi di dato per la definizione dei livelli --------

public class SpaceLevelDef
{
    public int index;
    public string title;
    public string objective;
    public int randomCrystals = 0;             // se >0, cristalli generati random
    public List<CrystalSpec> crystals = new List<CrystalSpec>();
    public List<BarrierSpec> barriers = new List<BarrierSpec>();
    public List<Vector2> bombs = new List<Vector2>();

    public int CrystalCount => (randomCrystals > 0) ? randomCrystals : crystals.Count;
}

public class CrystalSpec
{
    public Vector2 position;
    public Color color;
}

public class BarrierSpec
{
    public Vector2 center;
    public Vector2 size;   // larghezza x altezza in unita' di mondo
    public Color color;
}
