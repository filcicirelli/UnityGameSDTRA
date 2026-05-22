using UnityEngine;

// File che si avvia da solo quando premo Play in Unity.
// Crea la telecamera e i due oggetti principali del gioco.
public static class Avvio
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inizia()
    {
        // Pulisco eventuali telecamere e luci di default
        Camera[] cam = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        for (int i = 0; i < cam.Length; i++)
        {
            Object.Destroy(cam[i].gameObject);
        }

        Light[] luci = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < luci.Length; i++)
        {
            Object.Destroy(luci[i].gameObject);
        }

        // Telecamera 2D, sfondo viola scuro (sembra lo spazio)
        GameObject ogCam = new GameObject("TelecameraPrincipale");
        ogCam.tag = "MainCamera";

        Camera c = ogCam.AddComponent<Camera>();
        c.orthographic = true;
        c.orthographicSize = 6f;
        c.backgroundColor = new Color(0.05f, 0.04f, 0.12f);
        c.transform.position = new Vector3(0f, 0f, -10f);

        // Nascondo il cursore: nel gioco e' Astro che fa da puntatore
        Cursor.visible = false;

        // Creo i due oggetti che fanno funzionare tutto
        new GameObject("GestoreGioco").AddComponent<GestoreGioco>();
        new GameObject("InterfacciaGioco").AddComponent<InterfacciaGioco>();
    }
}
