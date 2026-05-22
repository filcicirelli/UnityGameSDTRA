using UnityEngine;

// HUD del gioco con IMGUI (testi grandi e amichevoli per bambini):
//   - in alto a sinistra:   CRISTALLI: N
//   - sotto:                PUNTI: NNNN
//   - in alto al centro:    titolo del livello
//   - in alto a destra:     ENERGIA NAVICELLA (barra verde)
//   - in basso a sinistra:  distintivo obiettivo
//   - in basso a destra:    pulsanti MENU e RIAVVIA
//   - centro a missione completa:  "MISSIONE COMPLETATA!" + PROSSIMO LIVELLO
//   - centro a GAME OVER:          "BOOM!" + RIPROVA
//   - flash rosso a tutto schermo quando si tocca una barriera
public class SpaceUIManager : MonoBehaviour
{
    public static SpaceUIManager Instance { get; private set; }

    private GUIStyle bigLabel;
    private GUIStyle hugeLabel;
    private GUIStyle titleLabel;
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

        titleLabel = new GUIStyle(GUI.skin.label);
        titleLabel.fontSize = 24;
        titleLabel.fontStyle = FontStyle.Bold;
        titleLabel.alignment = TextAnchor.MiddleCenter;
        titleLabel.normal.textColor = new Color(0.85f, 0.95f, 1f);

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

        // -------- Flash rosso a schermo intero quando si tocca una barriera --------
        if (gm.BarrierFlashTimer > 0f)
        {
            var c = new Color(1f, 0.20f, 0.20f,
                              0.35f * Mathf.Clamp01(gm.BarrierFlashTimer / 0.25f));
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), SolidTex(c));
        }

        // -------- HUD principale --------

        // CARAMELLE in alto a sinistra
        GUI.Label(new Rect(20, 12, 360, 36),
                  "CARAMELLE: " + gm.CandiesCollected + " / " + gm.TotalCandies,
                  bigLabel);
        // PUNTI sotto, piu' grande
        GUI.Label(new Rect(20, 46, 360, 50),
                  "PUNTI: " + gm.Score.ToString("0000"),
                  new GUIStyle(bigLabel) { fontSize = 34 });
        // VITE: cuori rossi pieni o vuoti a seconda di quante ne restano
        var heartsStyle = new GUIStyle(bigLabel)
        {
            fontSize = 30,
            normal = { textColor = new Color(1f, 0.45f, 0.55f) },
        };
        string hearts = "VITE: ";
        for (int i = 0; i < SpaceGameManager.MaxLives; i++)
            hearts += (i < gm.Lives) ? "♥" : "♡";
        GUI.Label(new Rect(20, 96, 360, 40), hearts, heartsStyle);

        // Titolo livello in alto al centro
        string title = gm.CurrentDef != null ? gm.CurrentDef.title : "";
        GUI.Label(new Rect(Screen.width / 2f - 320, 14, 640, 36), title, titleLabel);
        GUI.Label(new Rect(Screen.width / 2f - 320, 46, 640, 28),
                  "Livello " + (gm.CurrentLevel + 1) + " di " + SpaceLevels.Count,
                  new GUIStyle(titleLabel) { fontSize = 18 });

        // ENERGIA NAVICELLA in alto a destra
        float barW = 260, barH = 28;
        float barX = Screen.width - barW - 20;
        GUI.Label(new Rect(barX, 12, barW, 28), "ENERGIA ASTRO", bigLabel);
        GUI.DrawTexture(new Rect(barX, 46, barW, barH), barBg);
        GUI.DrawTexture(new Rect(barX + 3, 49,
                                 Mathf.Max(0, (barW - 6) * Mathf.Clamp01(gm.Energy)),
                                 barH - 6),
                        barFill);
        // TEMPO sotto la barra ENERGIA: rosso lampeggiante quando <= 10s
        int secs = Mathf.CeilToInt(gm.LevelTimeLeft);
        bool urgent = gm.LevelTimeLeft <= 10f && !gm.MissionComplete && !gm.GameOver;
        Color timeColor = urgent
            ? Color.Lerp(new Color(1f, 0.95f, 0.45f), new Color(1f, 0.30f, 0.30f),
                         0.5f + 0.5f * Mathf.Sin(Time.time * 8f))
            : Color.white;
        var timeStyle = new GUIStyle(bigLabel)
        {
            fontSize = 30,
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = timeColor },
        };
        GUI.Label(new Rect(barX, 80, barW, 40), "TEMPO: " + secs + "s", timeStyle);

        // Distintivo obiettivo in basso a sinistra (cambia tra raccolta/chiave/porta)
        GUI.Box(new Rect(20, Screen.height - 70, 420, 50),
                gm.ObjectiveText, objectiveBadge);

        // Pulsanti MENU e RIAVVIA in basso a destra
        if (GUI.Button(new Rect(Screen.width - 320, Screen.height - 70, 140, 50),
                       "MENU", button))
            menuOpen = !menuOpen;
        if (GUI.Button(new Rect(Screen.width - 170, Screen.height - 70, 150, 50),
                       "RIAVVIA", button))
            gm.Restart();

        // -------- Banner "Pronti..." durante il grace period --------
        if (gm.LevelStartGrace > 0f && !gm.GameOver && !gm.MissionComplete)
        {
            GUI.DrawTexture(new Rect(0, Screen.height * 0.45f, Screen.width, 70),
                            SolidTex(new Color(0f, 0f, 0f, 0.55f)));
            GUI.Label(new Rect(0, Screen.height * 0.45f + 5, Screen.width, 60),
                      "PRONTI...  " + Mathf.CeilToInt(gm.LevelStartGrace),
                      new GUIStyle(hugeLabel) { fontSize = 40 });
        }

        // -------- Pannello "Missione Completata" --------
        if (gm.MissionComplete) DrawMissionCompletePanel(gm);

        // -------- Pannello "Game Over" --------
        if (gm.GameOver) DrawGameOverPanel(gm);

        // -------- Menu di pausa --------
        if (menuOpen) DrawMenuPanel(gm);
    }

    void DrawMissionCompletePanel(SpaceGameManager gm)
    {
        // Banda scura per dare risalto al messaggio
        GUI.DrawTexture(new Rect(0, Screen.height * 0.28f, Screen.width, 220),
                        SolidTex(new Color(0f, 0f, 0f, 0.5f)));
        GUI.Label(new Rect(0, Screen.height * 0.30f, Screen.width, 80),
                  "MISSIONE COMPLETATA!", hugeLabel);
        GUI.Label(new Rect(0, Screen.height * 0.30f + 80, Screen.width, 40),
                  "Hai aperto la porta: ottimo lavoro, Astro!",
                  new GUIStyle(bigLabel) { alignment = TextAnchor.MiddleCenter });

        // Pulsante PROSSIMO LIVELLO / RICOMINCIA
        string label = gm.HasNextLevel ? "PROSSIMO LIVELLO" : "RICOMINCIA";
        float bw = 320, bh = 60;
        if (GUI.Button(new Rect((Screen.width - bw) / 2f,
                                Screen.height * 0.30f + 140, bw, bh),
                       label, button))
        {
            if (gm.HasNextLevel) gm.NextLevel();
            else gm.Restart();
        }
    }

    void DrawGameOverPanel(SpaceGameManager gm)
    {
        // Banda scura rossastra
        GUI.DrawTexture(new Rect(0, Screen.height * 0.28f, Screen.width, 220),
                        SolidTex(new Color(0.35f, 0.05f, 0.05f, 0.85f)));
        GUI.Label(new Rect(0, Screen.height * 0.30f, Screen.width, 80),
                  "BOOM!  GAME OVER",
                  new GUIStyle(hugeLabel) { normal = { textColor = new Color(1f, 0.6f, 0.6f) } });
        string reason = string.IsNullOrEmpty(gm.GameOverReason)
            ? "Hai esaurito le vite."
            : gm.GameOverReason + "  Vite finite.";
        GUI.Label(new Rect(0, Screen.height * 0.30f + 80, Screen.width, 40),
                  reason + " Riprova con piu' calma!",
                  new GUIStyle(bigLabel) { alignment = TextAnchor.MiddleCenter });

        float bw = 220, bh = 60;
        if (GUI.Button(new Rect(Screen.width / 2f - bw - 10,
                                Screen.height * 0.30f + 140, bw, bh),
                       "RIPROVA LIVELLO", button))
            gm.RetryLevel();
        if (GUI.Button(new Rect(Screen.width / 2f + 10,
                                Screen.height * 0.30f + 140, bw, bh),
                       "DA CAPO", button))
            gm.Restart();
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

    static Texture2D SolidTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
