using UnityEngine;

// Disegna l'HUD del gioco con le API OnGUI di Unity.
// Uso OnGUI perche' e' semplice: non serve creare Canvas o UI prefab,
// scrivo il codice riga per riga in questo file.
//
// In alto a sinistra:  caramelle raccolte, punti, vite (cuori)
// In alto al centro:   titolo del livello
// In alto a destra:    barra energia + tempo
// In basso a sinistra: obiettivo del momento
// In basso a destra:   pulsanti MENU e RIAVVIA
// Al centro:           pannelli di vittoria / game over / vittoria finale
public class InterfacciaGioco : MonoBehaviour
{
    public static InterfacciaGioco Istanza;

    // Stili GUI riusati per tutto l'HUD (li creo una volta sola)
    private GUIStyle stileGrande;
    private GUIStyle stileEnorme;
    private GUIStyle stileTitolo;
    private GUIStyle stileBadge;
    private GUIStyle stileBottone;

    // Texture monocolore per sfondi e barre
    private Texture2D texSfondoBarra;
    private Texture2D texRiempimentoBarra;
    private Texture2D texPannello;

    // Se il menu e' aperto o no
    private bool menuAperto;

    // Pannello informazioni tecniche (tasto F3, come la schermata di Minecraft)
    private bool debugAperto;
    private float fps = 60f;          // fotogrammi al secondo (valore medio)
    private float fpsMinimo = 99999f; // FPS piu' basso visto da quando l'ho aperto
    private GUIStyle stileDebug;

    void Awake()
    {
        Istanza = this;
    }

    // Unity chiama Update ogni frame: qui apro/chiudo il pannello e conto gli FPS
    void Update()
    {
        // F3 (o il tasto ` ) apre e chiude il pannello
        if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            debugAperto = !debugAperto;
            fpsMinimo = 99999f; // riparto a contare il minimo
        }

        // FPS = 1 diviso il tempo passato dall'ultimo frame.
        // Uso unscaledDeltaTime e una media morbida cosi' il numero non balla.
        float fpsOra = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.000001f);
        fps = Mathf.Lerp(fps, fpsOra, 0.1f);
        if (fps < fpsMinimo) fpsMinimo = fps;
    }

    // Costruisco gli stili la prima volta che servono
    void CostruisciStili()
    {
        stileGrande = new GUIStyle(GUI.skin.label);
        stileGrande.fontSize = 24;
        stileGrande.fontStyle = FontStyle.Bold;
        stileGrande.normal.textColor = Color.white;

        stileEnorme = new GUIStyle(GUI.skin.label);
        stileEnorme.fontSize = 48;
        stileEnorme.fontStyle = FontStyle.Bold;
        stileEnorme.alignment = TextAnchor.MiddleCenter;
        stileEnorme.normal.textColor = new Color(1f, 0.95f, 0.45f);

        stileTitolo = new GUIStyle(GUI.skin.label);
        stileTitolo.fontSize = 24;
        stileTitolo.fontStyle = FontStyle.Bold;
        stileTitolo.alignment = TextAnchor.MiddleCenter;
        stileTitolo.normal.textColor = new Color(0.85f, 0.95f, 1f);

        stileBadge = new GUIStyle(GUI.skin.box);
        stileBadge.fontSize = 22;
        stileBadge.fontStyle = FontStyle.Bold;
        stileBadge.alignment = TextAnchor.MiddleCenter;
        stileBadge.normal.textColor = Color.white;
        stileBadge.normal.background = TexturaPiena(new Color(0.10f, 0.20f, 0.45f, 0.95f));

        stileBottone = new GUIStyle(GUI.skin.button);
        stileBottone.fontSize = 22;
        stileBottone.fontStyle = FontStyle.Bold;

        texSfondoBarra = TexturaPiena(new Color(0.10f, 0.10f, 0.15f, 0.85f));
        texRiempimentoBarra = TexturaPiena(new Color(0.30f, 0.95f, 0.40f, 1f));
        texPannello = TexturaPiena(new Color(0.05f, 0.05f, 0.15f, 0.92f));

        // Stile del testo del pannello informazioni (verdino, piccolo)
        stileDebug = new GUIStyle(GUI.skin.label);
        stileDebug.fontSize = 15;
        stileDebug.normal.textColor = new Color(0.80f, 1f, 0.80f);
        stileDebug.alignment = TextAnchor.UpperLeft;
        stileDebug.wordWrap = false;
    }

    // Disegno principale: Unity chiama questo ogni frame
    void OnGUI()
    {
        if (stileGrande == null)
        {
            CostruisciStili();
        }

        GestoreGioco gm = GestoreGioco.Istanza;
        if (gm == null) return;

        // Flash rosso su tutto lo schermo quando si prende un colpo
        if (gm.TimerLampeggio > 0f)
        {
            float intensita = 0.35f * Mathf.Clamp01(gm.TimerLampeggio / 0.25f);
            Color colore = new Color(1f, 0.20f, 0.20f, intensita);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), TexturaPiena(colore));
        }

        // HUD principale
        DisegnaContatori(gm);
        DisegnaTitoloLivello(gm);
        DisegnaBarraEnergia(gm);

        if (!gm.VittoriaFinale)
        {
            // Obiettivo + pulsanti in basso (non li mostro nella schermata finale)
            GUI.Box(
                new Rect(20, Screen.height - 70, 420, 50),
                gm.TestoObiettivo,
                stileBadge);

            if (GUI.Button(new Rect(Screen.width - 320, Screen.height - 70, 140, 50),
                           "MENU", stileBottone))
            {
                menuAperto = !menuAperto;
            }
            if (GUI.Button(new Rect(Screen.width - 170, Screen.height - 70, 150, 50),
                           "RIAVVIA", stileBottone))
            {
                gm.RicominciaTutto();
            }

            // Bottone che apre il pannello informazioni: serve soprattutto nella
            // build WebGL, dove il browser puo' rubare il tasto F3 (ricerca pagina).
            if (GUI.Button(new Rect(450, Screen.height - 70, 150, 50),
                           "INFO (F3)", stileBottone))
            {
                debugAperto = !debugAperto;
                fpsMinimo = 99999f;
            }
        }

        // Banner "PRONTI..." durante il periodo iniziale
        if (gm.TempoIniziale > 0f && !gm.PartitaFinita && !gm.MissioneCompletata)
        {
            DisegnaBannerPronti(gm);
        }

        // Pannelli al centro
        if (gm.VittoriaFinale)
        {
            DisegnaPannelloVittoriaFinale(gm);
        }
        else if (gm.MissioneCompletata)
        {
            DisegnaPannelloMissioneCompletata(gm);
        }

        if (gm.PartitaFinita)
        {
            DisegnaPannelloGameOver(gm);
        }

        if (menuAperto && !gm.VittoriaFinale)
        {
            DisegnaPannelloMenu(gm);
        }

        // Pannello informazioni tecniche (sopra a tutto)
        if (debugAperto)
        {
            DisegnaPannelloDebug(gm);
        }
    }

    // ---- Pezzi dell'HUD ----

    void DisegnaContatori(GestoreGioco gm)
    {
        // Caramelle in alto a sinistra
        GUI.Label(
            new Rect(20, 12, 360, 36),
            "CARAMELLE: " + gm.CaramelleRaccolte + " / " + gm.TotaleCaramelle,
            stileGrande);

        // Punti
        GUIStyle stilePunti = new GUIStyle(stileGrande);
        stilePunti.fontSize = 34;
        GUI.Label(
            new Rect(20, 46, 360, 50),
            "PUNTI: " + gm.Punteggio.ToString("0000"),
            stilePunti);

        // Vite (cuori)
        GUIStyle stileCuori = new GUIStyle(stileGrande);
        stileCuori.fontSize = 30;
        stileCuori.normal.textColor = new Color(1f, 0.45f, 0.55f);

        string cuori = "VITE: ";
        for (int i = 0; i < Impostazioni.VITE; i++)
        {
            if (i < gm.Vite) cuori = cuori + "♥"; // pieno
            else cuori = cuori + "♡";              // vuoto
        }
        GUI.Label(new Rect(20, 96, 360, 40), cuori, stileCuori);
    }

    void DisegnaTitoloLivello(GestoreGioco gm)
    {
        string titolo = "";
        if (gm.LivelloAttuale != null)
        {
            titolo = gm.LivelloAttuale.titolo;
        }

        GUI.Label(
            new Rect(Screen.width / 2f - 320, 14, 640, 36),
            titolo,
            stileTitolo);

        GUIStyle stileSotto = new GUIStyle(stileTitolo);
        stileSotto.fontSize = 18;
        GUI.Label(
            new Rect(Screen.width / 2f - 320, 46, 640, 28),
            "Livello " + (gm.LivelloCorrente + 1) + " di " + DefinizioneLivelli.Conteggio,
            stileSotto);
    }

    void DisegnaBarraEnergia(GestoreGioco gm)
    {
        float larghezza = 260f;
        float altezza = 28f;
        float x = Screen.width - larghezza - 20f;

        GUI.Label(new Rect(x, 12, larghezza, 28), "ENERGIA ASTRO", stileGrande);

        // Sfondo della barra
        GUI.DrawTexture(new Rect(x, 46, larghezza, altezza), texSfondoBarra);

        // Riempimento in base all'energia
        float riemp = Mathf.Max(0, (larghezza - 6) * Mathf.Clamp01(gm.Energia));
        GUI.DrawTexture(new Rect(x + 3, 49, riemp, altezza - 6), texRiempimentoBarra);

        // Tempo: rosso e lampeggiante sotto i 10 secondi
        int secondi = Mathf.CeilToInt(gm.TempoRimasto);
        bool urgente = gm.TempoRimasto <= 10f && !gm.MissioneCompletata && !gm.PartitaFinita;

        Color coloreTempo;
        if (urgente)
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.time * 8f);
            coloreTempo = Color.Lerp(
                new Color(1f, 0.95f, 0.45f),
                new Color(1f, 0.30f, 0.30f),
                k);
        }
        else
        {
            coloreTempo = Color.white;
        }

        GUIStyle stileTempo = new GUIStyle(stileGrande);
        stileTempo.fontSize = 30;
        stileTempo.alignment = TextAnchor.MiddleRight;
        stileTempo.normal.textColor = coloreTempo;

        GUI.Label(
            new Rect(x, 80, larghezza, 40),
            "TEMPO: " + secondi + "s",
            stileTempo);
    }

    void DisegnaBannerPronti(GestoreGioco gm)
    {
        // Striscia scura
        GUI.DrawTexture(
            new Rect(0, Screen.height * 0.45f, Screen.width, 70),
            TexturaPiena(new Color(0f, 0f, 0f, 0.55f)));

        GUIStyle stileBanner = new GUIStyle(stileEnorme);
        stileBanner.fontSize = 40;
        GUI.Label(
            new Rect(0, Screen.height * 0.45f + 5, Screen.width, 60),
            "PRONTI...  " + Mathf.CeilToInt(gm.TempoIniziale),
            stileBanner);
    }

    // ---- Pannelli centrali ----

    void DisegnaPannelloVittoriaFinale(GestoreGioco gm)
    {
        // Sfondo blu-viola a tutto schermo
        GUI.DrawTexture(
            new Rect(0, 0, Screen.width, Screen.height),
            TexturaPiena(new Color(0.04f, 0.05f, 0.20f, 0.92f)));

        // Pannello centrale
        float wp = Mathf.Min(820f, Screen.width - 60f);
        float hp = 520f;
        Rect pannello = new Rect(
            (Screen.width - wp) / 2f,
            (Screen.height - hp) / 2f,
            wp, hp);

        GUI.DrawTexture(pannello, TexturaPiena(new Color(0.08f, 0.10f, 0.30f, 0.95f)));

        // Bordo giallo (4 rettangoli sottili)
        Texture2D bordo = TexturaPiena(new Color(1f, 0.85f, 0.30f, 1f));
        GUI.DrawTexture(new Rect(pannello.x, pannello.y, pannello.width, 4), bordo);
        GUI.DrawTexture(new Rect(pannello.x, pannello.y + pannello.height - 4, pannello.width, 4), bordo);
        GUI.DrawTexture(new Rect(pannello.x, pannello.y, 4, pannello.height), bordo);
        GUI.DrawTexture(new Rect(pannello.x + pannello.width - 4, pannello.y, 4, pannello.height), bordo);

        // Titolo
        GUIStyle stileTitoloV = new GUIStyle(stileEnorme);
        stileTitoloV.fontSize = 44;
        GUI.Label(
            new Rect(pannello.x, pannello.y + 24, pannello.width, 70),
            "CONGRATULAZIONI!  HAI VINTO!",
            stileTitoloV);

        // Messaggio principale
        GUIStyle stileMsg = new GUIStyle(stileGrande);
        stileMsg.alignment = TextAnchor.MiddleCenter;
        stileMsg.fontSize = 26;
        stileMsg.normal.textColor = new Color(0.85f, 0.95f, 1f);
        GUI.Label(
            new Rect(pannello.x + 30, pannello.y + 110, pannello.width - 60, 50),
            "Sei sulla buona strada per la tua guarigione.",
            stileMsg);

        // Crediti
        GUIStyle stileEtichetta = new GUIStyle(stileGrande);
        stileEtichetta.alignment = TextAnchor.MiddleCenter;
        stileEtichetta.fontSize = 22;
        stileEtichetta.normal.textColor = new Color(1f, 0.95f, 0.75f);

        GUIStyle stileValore = new GUIStyle(stileGrande);
        stileValore.alignment = TextAnchor.MiddleCenter;
        stileValore.fontSize = 22;
        stileValore.normal.textColor = Color.white;

        float riga = pannello.y + 200f;
        GUI.Label(new Rect(pannello.x, riga, pannello.width, 32), "Autore", stileEtichetta);
        GUI.Label(new Rect(pannello.x, riga + 30f, pannello.width, 32), "Filippo Cicirelli", stileValore);
        GUI.Label(new Rect(pannello.x, riga + 76f, pannello.width, 32), "Esame", stileEtichetta);
        GUI.Label(new Rect(pannello.x, riga + 106f, pannello.width, 32),
                  "Sistemi per la Riabilitazione e la Terapia Assistita", stileValore);
        GUI.Label(new Rect(pannello.x, riga + 152f, pannello.width, 32), "Professore", stileEtichetta);
        GUI.Label(new Rect(pannello.x, riga + 182f, pannello.width, 32), "Vitantonio Bevilacqua", stileValore);

        // Pulsante per ricominciare
        float lp = 420f, hpb = 70f;
        Rect rectBottone = new Rect(
            pannello.x + (pannello.width - lp) / 2f,
            pannello.y + pannello.height - hpb - 24f,
            lp, hpb);

        GUIStyle stileBottoneGrande = new GUIStyle(stileBottone);
        stileBottoneGrande.fontSize = 24;

        if (GUI.Button(rectBottone, "CLICCA PER GIOCARE DI NUOVO", stileBottoneGrande))
        {
            gm.RicominciaTutto();
        }
    }

    void DisegnaPannelloMissioneCompletata(GestoreGioco gm)
    {
        // Striscia scura
        GUI.DrawTexture(
            new Rect(0, Screen.height * 0.28f, Screen.width, 220),
            TexturaPiena(new Color(0f, 0f, 0f, 0.5f)));

        GUI.Label(
            new Rect(0, Screen.height * 0.30f, Screen.width, 80),
            "MISSIONE COMPLETATA!", stileEnorme);

        GUIStyle stileSotto = new GUIStyle(stileGrande);
        stileSotto.alignment = TextAnchor.MiddleCenter;
        GUI.Label(
            new Rect(0, Screen.height * 0.30f + 80, Screen.width, 40),
            "Hai aperto la porta: ottimo lavoro, Astro!",
            stileSotto);

        // Pulsante per andare avanti
        string etichetta;
        if (gm.ProssimoLivelloDisponibile) etichetta = "PROSSIMO LIVELLO";
        else etichetta = "RICOMINCIA";

        float lp = 320f, hpb = 60f;
        if (GUI.Button(
            new Rect((Screen.width - lp) / 2f, Screen.height * 0.30f + 140, lp, hpb),
            etichetta, stileBottone))
        {
            if (gm.ProssimoLivelloDisponibile) gm.ProssimoLivello();
            else gm.RicominciaTutto();
        }
    }

    void DisegnaPannelloGameOver(GestoreGioco gm)
    {
        // Striscia rossa scura
        GUI.DrawTexture(
            new Rect(0, Screen.height * 0.28f, Screen.width, 220),
            TexturaPiena(new Color(0.35f, 0.05f, 0.05f, 0.85f)));

        GUIStyle stileGO = new GUIStyle(stileEnorme);
        stileGO.normal.textColor = new Color(1f, 0.6f, 0.6f);
        GUI.Label(
            new Rect(0, Screen.height * 0.30f, Screen.width, 80),
            "BOOM!  GAME OVER", stileGO);

        // Motivo
        string motivo;
        if (string.IsNullOrEmpty(gm.MotivoSconfitta))
        {
            motivo = "Hai esaurito le vite.";
        }
        else
        {
            motivo = gm.MotivoSconfitta + "  Vite finite.";
        }

        GUIStyle stileMotivo = new GUIStyle(stileGrande);
        stileMotivo.alignment = TextAnchor.MiddleCenter;
        GUI.Label(
            new Rect(0, Screen.height * 0.30f + 80, Screen.width, 40),
            motivo + " Riprova con piu' calma!",
            stileMotivo);

        // Pulsanti
        float lp = 220f, hpb = 60f;
        if (GUI.Button(
            new Rect(Screen.width / 2f - lp - 10, Screen.height * 0.30f + 140, lp, hpb),
            "RIPROVA LIVELLO", stileBottone))
        {
            gm.RipetiLivello();
        }
        if (GUI.Button(
            new Rect(Screen.width / 2f + 10, Screen.height * 0.30f + 140, lp, hpb),
            "DA CAPO", stileBottone))
        {
            gm.RicominciaTutto();
        }
    }

    void DisegnaPannelloMenu(GestoreGioco gm)
    {
        float w = 480f, h = 280f;
        Rect r = new Rect(
            (Screen.width - w) / 2f,
            (Screen.height - h) / 2f,
            w, h);

        GUI.DrawTexture(r, texPannello);

        GUIStyle stileTitoloMenu = new GUIStyle(stileEnorme);
        stileTitoloMenu.fontSize = 40;
        GUI.Label(new Rect(r.x, r.y + 30, r.width, 50), "MENU", stileTitoloMenu);

        GUIStyle stileSotto = new GUIStyle(stileGrande);
        stileSotto.alignment = TextAnchor.MiddleCenter;
        stileSotto.fontSize = 18;
        GUI.Label(
            new Rect(r.x, r.y + 90, r.width, 30),
            "Premi RIPRENDI per continuare la missione",
            stileSotto);

        if (GUI.Button(new Rect(r.x + w / 2f - 200, r.y + 150, 180, 60),
                       "RIPRENDI", stileBottone))
        {
            menuAperto = false;
        }
        if (GUI.Button(new Rect(r.x + w / 2f + 20, r.y + 150, 180, 60),
                       "RIAVVIA", stileBottone))
        {
            gm.RicominciaTutto();
            menuAperto = false;
        }
    }

    // Pannello con tutte le informazioni tecniche, come la schermata di
    // debug di Minecraft (tasto F3). Lo uso per spiegare a colpo d'occhio
    // i valori del gioco, le prestazioni e il dispositivo su cui gira.
    void DisegnaPannelloDebug(GestoreGioco gm)
    {
        // Sfondo nero semitrasparente, cosi' il testo si legge bene
        GUI.DrawTexture(
            new Rect(0, 0, Screen.width, Screen.height),
            TexturaPiena(new Color(0f, 0f, 0f, 0.55f)));

        // --- Dati di Astro e del mouse ---
        Vector2 posAstro = Vector2.zero;
        float velAstro = 0f;
        bool haChiave = false;
        if (Astro.Istanza != null)
        {
            posAstro = Astro.Istanza.transform.position;
            velAstro = Astro.Istanza.Velocita.magnitude;
            haChiave = Astro.Istanza.HaChiave;
        }
        Vector3 mouse = Input.mousePosition;

        string stato;
        if (gm.VittoriaFinale)         stato = "VITTORIA FINALE";
        else if (gm.MissioneCompletata) stato = "missione completata";
        else if (gm.PartitaFinita)      stato = "game over";
        else                            stato = "in gioco";

        // ===== COLONNA SINISTRA: valori del gioco =====
        string sinistra =
            "=== GIOCO ===\n" +
            "Livello: " + (gm.LivelloCorrente + 1) + " / " + DefinizioneLivelli.Conteggio + "\n" +
            "Stato: " + stato + "\n" +
            "Obiettivo: " + gm.TestoObiettivo + "\n" +
            "Punti: " + gm.Punteggio + "\n" +
            "Caramelle: " + gm.CaramelleRaccolte + " / " + gm.TotaleCaramelle + "\n" +
            "Vite: " + gm.Vite + " / " + Impostazioni.VITE + "\n" +
            "Tempo rimasto: " + gm.TempoRimasto.ToString("0.0") + " s\n" +
            "\n" +
            "Astro X: " + posAstro.x.ToString("0.00") + "\n" +
            "Astro Y: " + posAstro.y.ToString("0.00") + "\n" +
            "Velocita' Astro: " + velAstro.ToString("0.0") + "\n" +
            "Mouse (pixel): " + (int)mouse.x + " , " + (int)mouse.y + "\n" +
            "Ha la chiave: " + (haChiave ? "si" : "no") + "\n" +
            "Bombe attive: " + (gm.BombeAttive ? "si" : "no") + "\n" +
            "\n" +
            "Oggetti in scena:\n" +
            "  caramelle: " + Caramella.Attive.Count + "\n" +
            "  asteroidi: " + Asteroide.Tutti.Count + "\n" +
            "  bombe:     " + Bomba.Tutte.Count;

        GUI.Label(new Rect(20, 18, 470, Screen.height - 36), sinistra, stileDebug);

        // ===== COLONNA DESTRA: prestazioni e dispositivo =====
        string destra =
            "=== PRESTAZIONI ===\n" +
            "FPS: " + Mathf.RoundToInt(fps) + "\n" +
            "FPS minimo: " + Mathf.RoundToInt(fpsMinimo) + "\n" +
            "Lag (ms per frame): " + (Time.unscaledDeltaTime * 1000f).ToString("0.0") + "\n" +
            "Target FPS: " + Application.targetFrameRate + "\n" +
            "VSync: " + QualitySettings.vSyncCount + "\n" +
            "timeScale (1=normale): " + Time.timeScale.ToString("0.0") + "\n" +
            "\n" +
            "=== DISPOSITIVO ===\n" +
            "Sistema: " + SystemInfo.operatingSystem + "\n" +
            "Dispositivo: " + SystemInfo.deviceModel + "\n" +
            "CPU: " + SystemInfo.processorType + "\n" +
            "Core CPU: " + SystemInfo.processorCount + "\n" +
            "RAM: " + SystemInfo.systemMemorySize + " MB\n" +
            "Scheda video: " + SystemInfo.graphicsDeviceName + "\n" +
            "Memoria video: " + SystemInfo.graphicsMemorySize + " MB\n" +
            "API grafica: " + SystemInfo.graphicsDeviceType + "\n" +
            "\n" +
            "=== SCHERMO ===\n" +
            "Risoluzione: " + Screen.width + " x " + Screen.height + "\n" +
            "Refresh: " + Screen.currentResolution.refreshRateRatio.value.ToString("0") + " Hz\n" +
            "Schermo intero: " + (Screen.fullScreen ? "si" : "no") + "\n" +
            "\n" +
            "=== ALTRO ===\n" +
            "Versione Unity: " + Application.unityVersion + "\n" +
            "Piattaforma: " + Application.platform + "\n" +
            "Tempo di gioco: " + Time.realtimeSinceStartup.ToString("0") + " s\n" +
            "\n" +
            "(premi di nuovo F3 per chiudere)";

        // La colonna destra parte dopo la sinistra (che finisce a x=490) e adatta
        // la sua larghezza: cosi' non si sovrappongono su schermi stretti, come
        // la build WebGL a 960x600.
        float xDestra = Mathf.Max(510f, Screen.width - 520f);
        GUI.Label(new Rect(xDestra, 18, Screen.width - xDestra - 20f, Screen.height - 36), destra, stileDebug);
    }

    // Piccola texture 1x1 di un colore (utile per sfondi e barre)
    static Texture2D TexturaPiena(Color colore)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, colore);
        t.Apply();
        return t;
    }
}
