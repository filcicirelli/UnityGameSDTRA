using System.Collections.Generic;
using UnityEngine;

// Forme disponibili per gli sprite generati a runtime.
public enum ShapeType { Circle, Square, Triangle, Bar }

// Tutto cio' che serve per descrivere un livello.
// Non e' un ScriptableObject per restare puramente in codice
// (piu' facile da spiegare all'esame).
public class LevelData
{
    public string title;                                // titolo mostrato a schermo
    public List<ItemSpec> items = new List<ItemSpec>(); // oggetti da trascinare
    public List<ZoneSpec> zones = new List<ZoneSpec>(); // cestini di destinazione
    public float timeLimit = 0f;                        // 0 = nessun timer
}

// Descrive un oggetto trascinabile.
public class ItemSpec
{
    public ShapeType shape;
    public Color color;
    public string category;   // deve coincidere con ZoneSpec.acceptedCategory
}

// Descrive un cestino in cui rilasciare gli oggetti.
public class ZoneSpec
{
    public string label;
    public Color color;
    public string acceptedCategory;
    public Vector2 position;
}
