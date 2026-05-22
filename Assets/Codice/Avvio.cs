using UnityEngine;

// =====================================================================
//  Avvio del gioco
// ---------------------------------------------------------------------
//  Questo e' il punto di partenza dell'applicazione: quando premo Play
//  dentro Unity, il metodo "Inizia" viene chiamato automaticamente.
//  Da qui costruisco a mano la telecamera e creo gli oggetti che fanno
//  girare il gioco (il gestore della partita e l'interfaccia).
//
//  Ho scelto di NON usare Scene/Prefab gia' pronti: tutto e' creato
//  da codice. Cosi' all'esame posso spiegare riga per riga cosa
//  succede quando il gioco parte.
// =====================================================================
public static class Avvio
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inizia()
    {
        // 1) Pulisco eventuali telecamere o luci gia' presenti nella scena
        //    vuota di default, cosi' parto sempre da una situazione pulita.
        Camera[] telecamereEsistenti = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        for (int i = 0; i < telecamereEsistenti.Length; i++)
        {
            Object.Destroy(telecamereEsistenti[i].gameObject);
        }

        Light[] luciEsistenti = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < luciEsistenti.Length; i++)
        {
            Object.Destroy(luciEsistenti[i].gameObject);
        }

        // 2) Creo la telecamera principale: ortografica (2D),
        //    sfondo viola scuro per dare l'idea dello spazio profondo.
        GameObject oggettoTelecamera = new GameObject("TelecameraPrincipale");
        oggettoTelecamera.tag = "MainCamera";

        Camera telecamera = oggettoTelecamera.AddComponent<Camera>();
        telecamera.orthographic = true;
        telecamera.orthographicSize = 6f;
        telecamera.backgroundColor = new Color(0.05f, 0.04f, 0.12f);
        telecamera.transform.position = new Vector3(0f, 0f, -10f);

        // 3) Nascondo il cursore di sistema: nel mio gioco e' Astro che
        //    fa da puntatore (segue il mouse) e quindi non serve la freccia.
        Cursor.visible = false;

        // 4) Creo i due "oggetti di servizio":
        //    - GestoreGioco: tiene lo stato della partita (punteggio, vite, ecc.)
        //    - InterfacciaGioco: disegna l'HUD e i pannelli (vittoria, game over...)
        new GameObject("GestoreGioco").AddComponent<GestoreGioco>();
        new GameObject("InterfacciaGioco").AddComponent<InterfacciaGioco>();
    }
}
