using UnityEngine;

// UI molto semplice usando OnGUI (IMGUI).
// Una sola funzione, facile da spiegare: viene chiamata da Unity
// ogni frame e disegna direttamente sullo schermo.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GUIStyle bigLabel;
    private GUIStyle titleLabel;
    private GUIStyle button;

    void Awake() { Instance = this; }

    void InitStyles()
    {
        bigLabel = new GUIStyle(GUI.skin.label);
        bigLabel.fontSize = 22;
        bigLabel.normal.textColor = Color.white;
        bigLabel.fontStyle = FontStyle.Bold;

        titleLabel = new GUIStyle(GUI.skin.label);
        titleLabel.fontSize = 30;
        titleLabel.alignment = TextAnchor.MiddleCenter;
        titleLabel.normal.textColor = new Color(1f, 0.9f, 0.4f);
        titleLabel.fontStyle = FontStyle.Bold;

        button = new GUIStyle(GUI.skin.button);
        button.fontSize = 22;
    }

    void OnGUI()
    {
        if (bigLabel == null) InitStyles();
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Barra in alto: titolo livello centrato
        GUI.Label(new Rect(0, 10, Screen.width, 40), gm.CurrentTitle, titleLabel);

        // Punteggio (sx) e Livello (dx)
        GUI.Label(new Rect(20, 50, 300, 30), "Punti: " + gm.Score, bigLabel);
        GUI.Label(new Rect(Screen.width - 220, 50, 200, 30),
                  "Livello " + (gm.CurrentLevel + 1), bigLabel);

        // Timer (solo se il livello ne ha uno)
        if (gm.TimeLeft > 0f)
        {
            GUI.Label(new Rect(Screen.width / 2 - 60, Screen.height - 40, 120, 30),
                      "Tempo: " + Mathf.CeilToInt(gm.TimeLeft) + "s", bigLabel);
        }

        // Schermata di fine gioco
        if (gm.GameComplete) DrawCenterPanel("HAI FINITO!  Punti: " + gm.Score, "Ricomincia");
        else if (gm.GameOver) DrawCenterPanel("TEMPO SCADUTO  Punti: " + gm.Score, "Riprova");
    }

    void DrawCenterPanel(string message, string buttonText)
    {
        float w = 500, h = 200;
        var r = new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
        GUI.Box(r, GUIContent.none);
        GUI.Label(new Rect(r.x, r.y + 40, w, 40), message, titleLabel);
        if (GUI.Button(new Rect(r.x + w / 2 - 80, r.y + 110, 160, 50), buttonText, button))
            GameManager.Instance.Restart();
    }
}
