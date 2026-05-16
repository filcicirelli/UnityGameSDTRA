using UnityEngine;

// Bootstrap: punto di ingresso automatico del gioco.
// Si avvia da solo dopo il caricamento della scena, cosi' non serve
// configurare nulla nell'Editor: basta premere Play.
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        // Rimuove camera e luci della scena di default
        foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            Object.Destroy(c.gameObject);
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            Object.Destroy(l.gameObject);

        // Camera ortografica (2D)
        var camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.10f, 0.10f, 0.16f); // viola scuro 8-bit
        cam.transform.position = new Vector3(0, 0, -10);

        // Manager (logica) e UI
        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("UIManager").AddComponent<UIManager>();
    }
}
