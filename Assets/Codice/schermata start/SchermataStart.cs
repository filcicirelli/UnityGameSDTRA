using UnityEngine;

// ============================================================================
// SCHERMATA INIZIALE (START)
// ----------------------------------------------------------------------------
// E' la prima cosa che si vede quando parte il gioco. Da qui si:
//   1) SCEGLIE COME GIOCARE: mouse, dito (webcam) o joystick;
//   2) si legge un riepilogo delle IMPOSTAZIONI del gioco;
//   3) si leggono i PARAMETRI TECNICI (FPS, refresh, schermo, dispositivo...),
//      utili da mostrare e spiegare all'esame.
// Premendo GIOCA si avvia la partita con il comando scelto.
//
// Disegno tutto con OnGUI, come l'HUD del gioco (InterfacciaGioco): cosi' non
// servono Canvas o prefab, e il codice si legge riga per riga.
//
// Questo oggetto si installa DA SOLO all'avvio, non va messo in scena.
// ============================================================================
public class SchermataStart : MonoBehaviour
{
    // Il comando selezionato adesso nella schermata (parte dal mouse)
    private Comandi.Modalita scelta = Comandi.Modalita.Mouse;

    // FPS medi (li calcolo come fa il pannello F3 del gioco)
    private float fps = 60f;

    // Stili e immagini, creati una volta sola
    private GUIStyle stileTitolo;
    private GUIStyle stileSotto;
    private GUIStyle stileSezione;
    private GUIStyle stileBottone;
    private GUIStyle stileBottoneGrande;
    private GUIStyle stileInfo;
    private GUIStyle stileDescrizione;
    private Texture2D texVelo;     // velo scuro su tutto lo schermo
    private Texture2D texPannello; // sfondo dei riquadri

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Installa()
    {
        new GameObject("SchermataStart").AddComponent<SchermataStart>();
    }

    void Awake()
    {
        // Parto mostrando il comando gia' attivo (utile se torno qui dal menu)
        scelta = Comandi.Attuale;
    }

    void Update()
    {
        // Media morbida degli FPS, cosi' il numero non "balla"
        float fpsOra = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.000001f);
        fps = Mathf.Lerp(fps, fpsOra, 0.1f);
    }

    void OnGUI()
    {
        GestoreGioco gm = GestoreGioco.Istanza;
        if (gm == null) return;

        // Disegno la schermata SOLO quando il menu iniziale e' aperto
        if (!gm.MenuInizialeAperto) return;

        if (stileTitolo == null) CostruisciStili();

        // Velo scuro sopra lo sfondo, cosi' i testi si leggono bene
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texVelo);

        // --- Titolo ---
        GUI.Label(new Rect(0, 24, Screen.width, 70), "ASTRO", stileTitolo);
        GUI.Label(new Rect(0, 96, Screen.width, 30),
                  "Aiuta Astro a raccogliere le caramelle e ad aprire la porta",
                  stileSotto);

        // --- Tre colonne sotto al titolo ---
        float margine = 30f;
        float top = 145f;
        float larghColonna = (Screen.width - margine * 4f) / 3f;
        float altColonna = Screen.height - top - margine;

        float x1 = margine;
        float x2 = margine * 2f + larghColonna;
        float x3 = margine * 3f + larghColonna * 2f;

        DisegnaColonnaComandi(gm, new Rect(x1, top, larghColonna, altColonna));
        PannelloInfo(new Rect(x2, top, larghColonna, altColonna),
                     "IMPOSTAZIONI DEL GIOCO", TestoImpostazioni());
        PannelloInfo(new Rect(x3, top, larghColonna, altColonna),
                     "PARAMETRI TECNICI", TestoParametriTecnici());
    }

    // ---- Colonna 1: scelta del comando + pulsante GIOCA ----

    void DisegnaColonnaComandi(GestoreGioco gm, Rect area)
    {
        GUI.Label(new Rect(area.x, area.y, area.width, 30), "1) SCEGLI COME GIOCARE", stileSezione);

        float y = area.y + 40f;
        float h = 56f;
        float gap = 12f;

        if (BottoneModalita(new Rect(area.x, y, area.width, h), "MOUSE", Comandi.Modalita.Mouse))
        {
            scelta = Comandi.Modalita.Mouse;
        }
        y += h + gap;
        if (BottoneModalita(new Rect(area.x, y, area.width, h), "DITO (WEBCAM)", Comandi.Modalita.Dito))
        {
            scelta = Comandi.Modalita.Dito;
        }
        y += h + gap;
        if (BottoneModalita(new Rect(area.x, y, area.width, h), "JOYSTICK", Comandi.Modalita.Joystick))
        {
            scelta = Comandi.Modalita.Joystick;
        }
        y += h + gap + 6f;

        // Descrizione del comando selezionato
        GUI.Box(new Rect(area.x, y, area.width, 70), DescrizioneComando(scelta), stileDescrizione);
        y += 70f + 20f;

        // Pulsante GIOCA
        GUI.Label(new Rect(area.x, y, area.width, 26), "2) AVVIA LA MISSIONE", stileSezione);
        y += 34f;
        if (GUI.Button(new Rect(area.x, y, area.width, 64), "GIOCA", stileBottoneGrande))
        {
            gm.IniziaPartita(scelta);
        }
    }

    // Un pulsante per scegliere un comando: quello selezionato e' verde.
    bool BottoneModalita(Rect r, string testo, Comandi.Modalita modalita)
    {
        bool selezionata = (scelta == modalita);

        Color vecchio = GUI.backgroundColor;
        if (selezionata) GUI.backgroundColor = new Color(0.30f, 0.85f, 0.40f);

        string etichetta = (selezionata ? "▶ " : "") + testo;
        bool premuto = GUI.Button(r, etichetta, stileBottone);

        GUI.backgroundColor = vecchio;
        return premuto;
    }

    string DescrizioneComando(Comandi.Modalita modalita)
    {
        switch (modalita)
        {
            case Comandi.Modalita.Dito:
                return "Muovi davanti alla webcam un evidenziatore fluo\n(verde, giallo o fucsia).";
            case Comandi.Modalita.Joystick:
                return "Usa il joystick.\nVanno bene anche le frecce o i tasti WASD.";
            default:
                return "Muovi Astro con il puntatore del mouse.";
        }
    }

    // ---- Colonna 2: testo con tutte le impostazioni del gioco ----

    string TestoImpostazioni()
    {
        return
            "=== PARTITA ===\n" +
            "Livelli totali: " + DefinizioneLivelli.Conteggio + "\n" +
            "Vite per livello: " + Impostazioni.VITE + "\n" +
            "Tempo per livello: " + Impostazioni.TEMPO_LIVELLO + " s\n" +
            "Conto \"PRONTI\": " + Impostazioni.TEMPO_PRONTI + " s\n" +
            "Caramelle livello 1: " + Impostazioni.CARAMELLE_LIV1 + "\n" +
            "\n" +
            "=== PUNTI ===\n" +
            "Punti per caramella: " + Impostazioni.PUNTI_CARAMELLA + "\n" +
            "Punti persi per colpo: " + Impostazioni.PUNTI_PERSI_HIT + "\n" +
            "\n" +
            "=== RAGGI DI PRESA ===\n" +
            "Caramella: " + Impostazioni.RAGGIO_CARAMELLA + "\n" +
            "Chiave: " + Impostazioni.RAGGIO_CHIAVE + "\n" +
            "Porta: " + Impostazioni.RAGGIO_PORTA + "\n" +
            "Bomba: " + Impostazioni.RAGGIO_BOMBA + "\n" +
            "Porta si sposta ogni: " + Impostazioni.PORTA_SECONDI_FERMA + " s\n" +
            "\n" +
            "=== FEEDBACK PAZIENTE ===\n" +
            "Suono: " + (ParametriFeedback.SUONO_ATTIVO ? "acceso" : "spento") + "\n" +
            "Effetti visivi: " + (ParametriFeedback.VISIVO_ATTIVO ? "acceso" : "spento") + "\n" +
            "\n" +
            "=== COMANDI ===\n" +
            "Webcam, tolleranza tinta: " + ParametriWebcam.TOLLERANZA_TINTA + "\n" +
            "Webcam, morbidezza: " + ParametriWebcam.MORBIDEZZA + "\n" +
            "Joystick, velocita': " + ParametriComandi.JOYSTICK_VELOCITA + " px/s\n" +
            "Joystick, zona morta: " + ParametriComandi.JOYSTICK_ZONA_MORTA;
    }

    // ---- Colonna 3: parametri tecnici (FPS, schermo, dispositivo...) ----

    string TestoParametriTecnici()
    {
        return
            "=== PRESTAZIONI ===\n" +
            "FPS (ora): " + Mathf.RoundToInt(fps) + "\n" +
            "Target FPS: " + Application.targetFrameRate + "\n" +
            "VSync: " + QualitySettings.vSyncCount + "\n" +
            "Lag (ms per frame): " + (Time.unscaledDeltaTime * 1000f).ToString("0.0") + "\n" +
            "\n" +
            "=== SCHERMO ===\n" +
            "Risoluzione: " + Screen.width + " x " + Screen.height + "\n" +
            "Refresh rate: " + Screen.currentResolution.refreshRateRatio.value.ToString("0") + " Hz\n" +
            "Schermo intero: " + (Screen.fullScreen ? "si" : "no") + "\n" +
            "DPI schermo: " + Screen.dpi.ToString("0") + "\n" +
            "\n" +
            "=== DISPOSITIVO ===\n" +
            "Sistema: " + SystemInfo.operatingSystem + "\n" +
            "Modello: " + SystemInfo.deviceModel + "\n" +
            "CPU: " + SystemInfo.processorType + "\n" +
            "Core CPU: " + SystemInfo.processorCount + "\n" +
            "RAM: " + SystemInfo.systemMemorySize + " MB\n" +
            "Scheda video: " + SystemInfo.graphicsDeviceName + "\n" +
            "Memoria video: " + SystemInfo.graphicsMemorySize + " MB\n" +
            "API grafica: " + SystemInfo.graphicsDeviceType + "\n" +
            "Webcam trovate: " + WebCamTexture.devices.Length + "\n" +
            "\n" +
            "=== SOFTWARE ===\n" +
            "Versione Unity: " + Application.unityVersion + "\n" +
            "Piattaforma: " + Application.platform;
    }

    // ---- Aiutanti per il disegno ----

    // Disegna un riquadro con un titolo e sotto un testo su piu' righe
    void PannelloInfo(Rect r, string titolo, string testo)
    {
        GUI.DrawTexture(r, texPannello);
        GUI.Label(new Rect(r.x + 12, r.y + 8, r.width - 24, 26), titolo, stileSezione);
        GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, r.height - 46), testo, stileInfo);
    }

    void CostruisciStili()
    {
        stileTitolo = new GUIStyle(GUI.skin.label);
        stileTitolo.fontSize = 64;
        stileTitolo.fontStyle = FontStyle.Bold;
        stileTitolo.alignment = TextAnchor.MiddleCenter;
        stileTitolo.normal.textColor = new Color(1f, 0.95f, 0.45f);

        stileSotto = new GUIStyle(GUI.skin.label);
        stileSotto.fontSize = 18;
        stileSotto.alignment = TextAnchor.MiddleCenter;
        stileSotto.normal.textColor = new Color(0.85f, 0.95f, 1f);

        stileSezione = new GUIStyle(GUI.skin.label);
        stileSezione.fontSize = 18;
        stileSezione.fontStyle = FontStyle.Bold;
        stileSezione.normal.textColor = new Color(1f, 0.85f, 0.45f);

        stileBottone = new GUIStyle(GUI.skin.button);
        stileBottone.fontSize = 20;
        stileBottone.fontStyle = FontStyle.Bold;

        stileBottoneGrande = new GUIStyle(GUI.skin.button);
        stileBottoneGrande.fontSize = 28;
        stileBottoneGrande.fontStyle = FontStyle.Bold;

        stileDescrizione = new GUIStyle(GUI.skin.box);
        stileDescrizione.fontSize = 14;
        stileDescrizione.alignment = TextAnchor.MiddleCenter;
        stileDescrizione.wordWrap = true;
        stileDescrizione.normal.textColor = Color.white;

        stileInfo = new GUIStyle(GUI.skin.label);
        stileInfo.fontSize = 14;
        stileInfo.alignment = TextAnchor.UpperLeft;
        stileInfo.wordWrap = false;
        stileInfo.normal.textColor = new Color(0.80f, 1f, 0.80f);

        texVelo = TexturaPiena(new Color(0f, 0f, 0.05f, 0.72f));
        texPannello = TexturaPiena(new Color(0.05f, 0.07f, 0.18f, 0.85f));
    }

    static Texture2D TexturaPiena(Color colore)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, colore);
        t.Apply();
        return t;
    }
}
