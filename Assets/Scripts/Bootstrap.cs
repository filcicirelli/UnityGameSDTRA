using UnityEngine;

// Bootstrap: punto di ingresso automatico del gioco.
// Premendo Play in Unity viene avviato il gioco "Missione Spaziale".
// Tutto il setup di scena e' costruito qui da codice (niente prefab/scene).
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        // Pulisci eventuali camere/luci della scena di default
        foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            Object.Destroy(c.gameObject);
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            Object.Destroy(l.gameObject);

        // Camera ortografica 2D, sfondo "nebulosa" viola scuro 8-bit
        var camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.05f, 0.04f, 0.12f);
        cam.transform.position = new Vector3(0, 0, -10);

        // Nascondi il cursore di sistema: la navicella e' il puntatore
        Cursor.visible = false;

        // Manager (logica) e UI
        new GameObject("SpaceGameManager").AddComponent<SpaceGameManager>();
        new GameObject("SpaceUIManager").AddComponent<SpaceUIManager>();
    }
}
