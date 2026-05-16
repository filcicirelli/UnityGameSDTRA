using UnityEngine;

// Costruisce in scena gli oggetti (Draggable) e i cestini (DropZone)
// a partire da un LevelData. Tutto e' figlio di un GameObject "Level"
// che viene distrutto quando si cambia livello.
public static class LevelLoader
{
    private static GameObject root;

    public static void Clear()
    {
        if (root != null) Object.Destroy(root);
        root = null;
    }

    public static void Load(LevelData data)
    {
        Clear();
        root = new GameObject("Level");

        // -------- Cestini --------
        foreach (var zs in data.zones)
        {
            var go = new GameObject("Zone_" + zs.label);
            go.transform.SetParent(root.transform);
            go.transform.position   = zs.position;
            go.transform.localScale = new Vector3(2.5f, 2.5f, 1f); // cestino grande

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreateZone(zs.color);
            sr.sortingOrder = 0;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            // BoxCollider2D di default copre tutto lo sprite, va bene cosi'.

            go.AddComponent<DropZone>().acceptedCategory = zs.acceptedCategory;

            // Etichetta testuale sotto il cestino
            var label = new GameObject("Label");
            label.transform.SetParent(go.transform);
            label.transform.localPosition = new Vector3(0, -0.7f, 0);
            label.transform.localScale    = new Vector3(0.4f, 0.4f, 1f);
            var tm = label.AddComponent<TextMesh>();
            tm.text         = zs.label;
            tm.anchor       = TextAnchor.MiddleCenter;
            tm.alignment    = TextAlignment.Center;
            tm.fontSize     = 60;
            tm.characterSize = 0.08f;
            tm.color        = Color.white;
            var mr = label.GetComponent<MeshRenderer>();
            mr.sortingOrder = 2;
        }

        // -------- Oggetti trascinabili --------
        // Disposti su una o due righe in alto, equidistanziati.
        int n = data.items.Count;
        int perRow = Mathf.Min(n, 6);
        float spacing = 1.6f;
        for (int i = 0; i < n; i++)
        {
            int row = i / perRow;
            int col = i % perRow;
            int rowCount = Mathf.Min(n - row * perRow, perRow);
            float startX = -(rowCount - 1) * spacing * 0.5f;

            var spec = data.items[i];
            var go = new GameObject("Item_" + i);
            go.transform.SetParent(root.transform);
            go.transform.position = new Vector3(startX + col * spacing, 3f - row * 1.6f, -1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreateShape(spec.shape, spec.color);
            sr.sortingOrder = 1;

            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = Vector2.one; // sprite 16px / PPU 16 = 1 unita'

            go.AddComponent<Draggable>().category = spec.category;
        }
    }
}
