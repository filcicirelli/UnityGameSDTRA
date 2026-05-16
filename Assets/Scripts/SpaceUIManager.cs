using UnityEngine;

// HUD del gioco con IMGUI (testi grandi e amichevoli per bambini):
//   - in alto a sinistra:  CRISTALLI: N
//   - sotto:               PUNTI: NNNN
//   - in alto a destra:    ENERGIA NAVICELLA (barra verde)
//   - in basso a sinistra: distintivo obiettivo
//   - in basso a destra:   pulsanti MENU e RIAVVIA
//   - centro:              "MISSIONE COMPLETATA!" con celebrazione
public class SpaceUIManager : MonoBehaviour
{
    public static SpaceUIManager Instance { get; private set; }

    private GUIStyle bigLabel;
    private GUIStyle hugeLabel;
    private GUIStyle objectiveBadge;
    private GUIStyle button;
    private Texture2D barBg, barFill, panelBg;

    private bool menuOpen;

    void Awake() { Instance = this; }

    void InitStyles()
    {
        bigLabel = new GUIStyle(GUI.skin.label);
        bigLabel.fontSize = 24;
        bigLabel.fontStyle = FontStyle.Bold;
        bigLabel.normal.textColor = Color.white;

        hugeLabel = new GUIStyle(GUI.skin.label);
        hugeLabel.fontSize = 48;
        hugeLabel.fontStyle = FontStyle.Bold;
        hugeLabel.alignment = TextAnchor.MiddleCenter;
        hugeLabel.normal.textColor = new Color(1f, 0.95f, 0.45f);

        objectiveBadge = new GUIStyle(GUI.skin.box);
        objectiveBadge.fontSize = 22;
        objectiveBadge.fontStyle = FontStyle.Bold;
        objectiveBadge.alignment = TextAnchor.MiddleCenter;
        objectiveBadge.normal.textColor = Color.white;
        objectiveBadge.normal.background = SolidTex(new Color(0.10f, 0.20f, 0.45f, 0.95f));

        button = new GUIStyle(GUI.skin.button);
        button.fontSize = 22;
        button.fontStyle = FontStyle.Bold;

        barBg   = SolidTex(new Color(0.10f, 0.10f, 0.15f, 0.85f));
        barFill = SolidTex(new Color(0.30f, 0.95f, 0.40f, 1f));
        panelBg = SolidTex(new Color(0.05f, 0.05f, 0.15f, 0.92f));
    }

    void OnGUI()
    {
        if (bigLabel == null) InitStyles();
        var gm = SpaceGameManager.Instance;
        if (gm == null) return;

        // -------- HUD principale --------

        // CRISTALLI in alto a sinistra
        GUI.Label(new Rect(20, 12, 360, 36),
                  "CRISTALLI: " + gm.CrystalsCollected, bigLabel);
        // PUNTI subito sotto, piu' grande
        GUI.Label(new Rect(20, 46, 360, 50),
                  "PUNTI: " + gm.Score.ToString("0000"),
                  new GUIStyle(bigLabel) { fontSize = 34 });

        // ENERGIA NAVICELLA in alto a destra (etichetta + barra)
        float barW = 260, barH = 28;
        float barX = Screen.width - barW - 20;
        GUI.Label(new Rect(barX, 12, barW, 28), "ENERGIA NAVICELLA", bigLabel);
        GUI.DrawTexture(new Rect(barX, 46, barW, barH), barBg);
        GUI.DrawTexture(new Rect(barX + 3, 49,
                                 Mathf.Max(0, (barW - 6) * Mathf.Clamp01(gm.Energy)),
                                 barH - 6),
                        barFill);

        // Distintivo obiettivo in basso a sinistra
        GUI.Box(new Rect(20, Screen.height - 70, 320, 50),
                gm.ObjectiveText, objectiveBadge);

        // Pulsanti MENU e RIAVVIA in basso a destra
        if (GUI.Button(new Rect(Screen.width - 320, Screen.height - 70, 140, 50),
                       "MENU", button))
            menuOpen = !menuOpen;
        if (GUI.Button(new Rect(Screen.width - 170, Screen.height - 70, 150, 50),
                       "RIAVVIA", button))
            gm.Restart();

        // -------- Schermata "Missione Completata" --------

        if (gm.MissionComplete)
        {
            // Sfondo semitrasparente per dare risalto al messaggio
            GUI.DrawTexture(new Rect(0, Screen.height * 0.30f, Screen.width, 160),
                            SolidTex(new Color(0f, 0f, 0f, 0.45f)));
            GUI.Label(new Rect(0, Screen.height * 0.32f, Screen.width, 80),
                      "MISSIONE COMPLETATA!", hugeLabel);
            GUI.Label(new Rect(0, Screen.height * 0.32f + 80, Screen.width, 40),
                      "Hai raccolto tutti i cristalli: ben fatto, Astro!",
                      new GUIStyle(bigLabel) { alignment = TextAnchor.MiddleCenter });
        }

        // -------- Menu di pausa --------

        if (menuOpen) DrawMenuPanel(gm);
    }

    void DrawMenuPanel(SpaceGameManager gm)
    {
        float w = 480, h = 280;
        var r = new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
        GUI.DrawTexture(r, panelBg);
        GUI.Label(new Rect(r.x, r.y + 30, r.width, 50),
                  "MENU",
                  new GUIStyle(hugeLabel) { fontSize = 40 });
        GUI.Label(new Rect(r.x, r.y + 90, r.width, 30),
                  "Premi RIPRENDI per continuare la missione",
                  new GUIStyle(bigLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 18 });

        if (GUI.Button(new Rect(r.x + w / 2 - 200, r.y + 150, 180, 60),
                       "RIPRENDI", button))
            menuOpen = false;
        if (GUI.Button(new Rect(r.x + w / 2 + 20, r.y + 150, 180, 60),
                       "RIAVVIA", button))
        {
            gm.Restart();
            menuOpen = false;
        }
    }

    // Crea una texture 1x1 di un certo colore (per sfondi e barre).
    static Texture2D SolidTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
