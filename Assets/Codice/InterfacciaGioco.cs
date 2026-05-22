using UnityEngine;

// =====================================================================
//  InterfacciaGioco
// ---------------------------------------------------------------------
//  Disegna l'HUD del gioco usando le API "IMGUI" di Unity (OnGUI).
//  Ho scelto IMGUI perche':
//      - non serve creare Canvas/UI prefab,
//      - il codice e' tutto qui dentro, leggibile riga per riga,
//      - va benissimo per testi grandi e amichevoli (questo gioco
//        e' pensato per bambini in riabilitazione: caratteri grossi,
//        colori vivi, pulsanti larghi).
//
//  Cosa disegno:
//      - in alto a sinistra:   CARAMELLE: N, PUNTI: NNNN, VITE: cuori
//      - in alto al centro:    titolo del livello + "Livello X di Y"
//      - in alto a destra:     barra ENERGIA + TEMPO
//      - in basso a sinistra:  badge con l'obiettivo del momento
//      - in basso a destra:    pulsanti MENU e RIAVVIA
//      - al centro:            pannelli "Missione completata!" /
//                              "Game over" / "Vittoria finale" (credits)
//      - flash rosso su tutto lo schermo quando si tocca un asteroide
// =====================================================================
public class InterfacciaGioco : MonoBehaviour
{
    public static InterfacciaGioco Istanza { get; private set; }

    // Stili GUI riusati per tutto l'HUD (li costruisco una sola volta)
    private GUIStyle stileGrande;
    private GUIStyle stileEnorme;
    private GUIStyle stileTitolo;
    private GUIStyle stileBadge;
    private GUIStyle stileBottone;

    // Texture monocolore usate per sfondi/barre
    private Texture2D texSfondoBarra;
    private Texture2D texRiempimentoBarra;
    private Texture2D texPannello;

    // Stato locale dell'interfaccia
    private bool menuAperto;

    void Awake()
    {
        Istanza = this;
    }

    // -----------------------------------------------------------------
    //  Costruzione una-tantum degli stili
    // -----------------------------------------------------------------
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

        texSfondoBarra      = TexturaPiena(new Color(0.10f, 0.10f, 0.15f, 0.85f));
        texRiempimentoBarra = TexturaPiena(new Color(0.30f, 0.95f, 0.40f, 1f));
        texPannello         = TexturaPiena(new Color(0.05f, 0.05f, 0.15f, 0.92f));
    }

    // -----------------------------------------------------------------
    //  Disegno principale (chiamato da Unity ogni frame di GUI)
    // -----------------------------------------------------------------
    void OnGUI()
    {
        if (stileGrande == null)
        {
            CostruisciStili();
        }

        GestoreGioco gm = GestoreGioco.Istanza;
        if (gm == null) return;

        // ---------- Flash rosso su tutto lo schermo ----------
        if (gm.TimerLampeggio > 0f)
        {
            float intensita = 0.35f * Mathf.Clamp01(gm.TimerLampeggio / 0.25f);
            Color colore = new Color(1f, 0.20f, 0.20f, intensita);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), TexturaPiena(colore));
        }

        // ---------- HUD principale ----------
        DisegnaContatori(gm);
        DisegnaTitoloLivello(gm);
        DisegnaBarraEnergia(gm);

        if (!gm.VittoriaFinale)
        {
            // Badge obiettivo + pulsanti in basso solo se NON sono
            // nella schermata di vittoria finale (altrimenti il pannello
            // credits coprirebbe questi pulsanti e ci sarebbero click "fantasma").
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
        }

        // ---------- Banner "PRONTI..." durante il grace period ----------
        if (gm.TempoIniziale > 0f && !gm.PartitaFinita && !gm.MissioneCompletata)
        {
            DisegnaBannerPronti(gm);
        }

        // ---------- Pannelli centrali ----------
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
    }

    // -----------------------------------------------------------------
    //  Sezioni dell'HUD (le tengo separate per leggibilita')
    // -----------------------------------------------------------------

    void DisegnaContatori(GestoreGioco gm)
    {
        // CARAMELLE in alto a sinistra
        GUI.Label(
            new Rect(20, 12, 360, 36),
            "CARAMELLE: " + gm.CaramelleRaccolte + " / " + gm.TotaleCaramelle,
            stileGrande);

        // PUNTI sotto, in grande
        GUIStyle stilePunti = new GUIStyle(stileGrande);
        stilePunti.fontSize = 34;
        GUI.Label(
            new Rect(20, 46, 360, 50),
            "PUNTI: " + gm.Punteggio.ToString("0000"),
            stilePunti);

        // VITE: cuori pieni o vuoti
        GUIStyle stileCuori = new GUIStyle(stileGrande);
        stileCuori.fontSize = 30;
        stileCuori.normal.textColor = new Color(1f, 0.45f, 0.55f);

        string cuori = "VITE: ";
        for (int i = 0; i < GestoreGioco.ViteMassime; i++)
        {
            if (i < gm.Vite) cuori += "♥";   // cuore pieno
            else             cuori += "♡";   // cuore vuoto
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

        GUIStyle stileSottoTitolo = new GUIStyle(stileTitolo);
        stileSottoTitolo.fontSize = 18;
        GUI.Label(
            new Rect(Screen.width / 2f - 320, 46, 640, 28),
            "Livello " + (gm.LivelloCorrente + 1) + " di " + DefinizioneLivelli.Conteggio,
            stileSottoTitolo);
    }

    void DisegnaBarraEnergia(GestoreGioco gm)
    {
        float larghezza = 260f;
        float altezza   = 28f;
        float x = Screen.width - larghezza - 20f;

        // Etichetta
        GUI.Label(new Rect(x, 12, larghezza, 28), "ENERGIA ASTRO", stileGrande);

        // Sfondo della barra
        GUI.DrawTexture(new Rect(x, 46, larghezza, altezza), texSfondoBarra);

        // Riempimento proporzionale all'energia
        float riempimento = Mathf.Max(0, (larghezza - 6) * Mathf.Clamp01(gm.Energia));
        GUI.DrawTexture(
            new Rect(x + 3, 49, riempimento, altezza - 6),
            texRiempimentoBarra);

        // ----- Tempo restante: rosso lampeggiante sotto i 10 secondi -----
        int secondi = Mathf.CeilToInt(gm.TempoRimasto);
        bool urgente = gm.TempoRimasto <= 10f && !gm.MissioneCompletata && !gm.PartitaFinita;

        Color coloreTempo;
        if (urgente)
        {
            // Interpolo tra giallo chiaro e rosso "sfarfallando" a 8 Hz
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

    // -----------------------------------------------------------------
    //  Pannelli centrali
    // -----------------------------------------------------------------

    void DisegnaPannelloVittoriaFinale(GestoreGioco gm)
    {
        // Sfondo a tutto schermo (blu-viola spaziale)
        GUI.DrawTexture(
            new Rect(0, 0, Screen.width, Screen.height),
            TexturaPiena(new Color(0.04f, 0.05f, 0.20f, 0.92f)));

        // Pannello centrale
        float larghezzaPannello = Mathf.Min(820f, Screen.width - 60f);
        float altezzaPannello = 520f;
        Rect pannello = new Rect(
            (Screen.width - larghezzaPannello) / 2f,
            (Screen.height - altezzaPannello) / 2f,
            larghezzaPannello, altezzaPannello);

        GUI.DrawTexture(pannello, TexturaPiena(new Color(0.08f, 0.10f, 0.30f, 0.95f)));

        // Bordo giallo "luminoso" (quattro rettangoli sottili)
        Texture2D bordo = TexturaPiena(new Color(1f, 0.85f, 0.30f, 1f));
        GUI.DrawTexture(new Rect(pannello.x, pannello.y, pannello.width, 4), bordo);
        GUI.DrawTexture(new Rect(pannello.x, pannello.y + pannello.height - 4, pannello.width, 4), bordo);
        GUI.DrawTexture(new Rect(pannello.x, pannello.y, 4, pannello.height), bordo);
        GUI.DrawTexture(new Rect(pannello.x + pannello.width - 4, pannello.y, 4, pannello.height), bordo);

        // ----- Titolo -----
        GUIStyle stileTitoloVittoria = new GUIStyle(stileEnorme);
        stileTitoloVittoria.fontSize = 44;
        GUI.Label(
            new Rect(pannello.x, pannello.y + 24, pannello.width, 70),
            "CONGRATULAZIONI!  HAI VINTO!",
            stileTitoloVittoria);

        // ----- Messaggio principale -----
        GUIStyle stileMessaggio = new GUIStyle(stileGrande);
        stileMessaggio.alignment = TextAnchor.MiddleCenter;
        stileMessaggio.fontSize = 26;
        stileMessaggio.normal.textColor = new Color(0.85f, 0.95f, 1f);
        GUI.Label(
            new Rect(pannello.x + 30, pannello.y + 110, pannello.width - 60, 50),
            "Sei sulla buona strada per la tua guarigione.",
            stileMessaggio);

        // ----- Crediti (autore / esame / professore) -----
        GUIStyle stileEtichetta = new GUIStyle(stileGrande);
        stileEtichetta.alignment = TextAnchor.MiddleCenter;
        stileEtichetta.fontSize = 22;
        stileEtichetta.normal.textColor = new Color(1f, 0.95f, 0.75f);

        GUIStyle stileValore = new GUIStyle(stileGrande);
        stileValore.alignment = TextAnchor.MiddleCenter;
        stileValore.fontSize = 22;
        stileValore.normal.textColor = Color.white;

        float riga = pannello.y + 200f;
        GUI.Label(new Rect(pannello.x, riga,         pannello.width, 32), "Autore", stileEtichetta);
        GUI.Label(new Rect(pannello.x, riga +  30f,  pannello.width, 32), "Filippo Cicirelli", stileValore);
        GUI.Label(new Rect(pannello.x, riga +  76f,  pannello.width, 32), "Esame", stileEtichetta);
        GUI.Label(new Rect(pannello.x, riga + 106f,  pannello.width, 32),
                  "Sistemi per la Riabilitazione e la Terapia Assistita", stileValore);
        GUI.Label(new Rect(pannello.x, riga + 152f,  pannello.width, 32), "Professore", stileEtichetta);
        GUI.Label(new Rect(pannello.x, riga + 182f,  pannello.width, 32), "Vitantonio Bevilacqua", stileValore);

        // ----- Pulsante per ricominciare -----
        float lp = 420f, hp = 70f;
        Rect bottoneRect = new Rect(
            pannello.x + (pannello.width - lp) / 2f,
            pannello.y + pannello.height - hp - 24f,
            lp, hp);

        GUIStyle stileBottoneGrande = new GUIStyle(stileBottone);
        stileBottoneGrande.fontSize = 24;

        if (GUI.Button(bottoneRect, "CLICCA PER GIOCARE DI NUOVO", stileBottoneGrande))
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

        // Pulsante per avanzare
        string etichetta;
        if (gm.ProssimoLivelloDisponibile) etichetta = "PROSSIMO LIVELLO";
        else                               etichetta = "RICOMINCIA";

        float lp = 320f, hp = 60f;
        if (GUI.Button(
            new Rect((Screen.width - lp) / 2f, Screen.height * 0.30f + 140, lp, hp),
            etichetta, stileBottone))
        {
            if (gm.ProssimoLivelloDisponibile) gm.ProssimoLivello();
            else                               gm.RicominciaTutto();
        }
    }

    void DisegnaPannelloGameOver(GestoreGioco gm)
    {
        // Striscia rossa scura
        GUI.DrawTexture(
            new Rect(0, Screen.height * 0.28f, Screen.width, 220),
            TexturaPiena(new Color(0.35f, 0.05f, 0.05f, 0.85f)));

        GUIStyle stileTitoloGO = new GUIStyle(stileEnorme);
        stileTitoloGO.normal.textColor = new Color(1f, 0.6f, 0.6f);
        GUI.Label(
            new Rect(0, Screen.height * 0.30f, Screen.width, 80),
            "BOOM!  GAME OVER", stileTitoloGO);

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
        float lp = 220f, hp = 60f;
        if (GUI.Button(
            new Rect(Screen.width / 2f - lp - 10, Screen.height * 0.30f + 140, lp, hp),
            "RIPROVA LIVELLO", stileBottone))
        {
            gm.RipetiLivello();
        }
        if (GUI.Button(
            new Rect(Screen.width / 2f + 10, Screen.height * 0.30f + 140, lp, hp),
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

        GUIStyle stileSottoMenu = new GUIStyle(stileGrande);
        stileSottoMenu.alignment = TextAnchor.MiddleCenter;
        stileSottoMenu.fontSize = 18;
        GUI.Label(
            new Rect(r.x, r.y + 90, r.width, 30),
            "Premi RIPRENDI per continuare la missione",
            stileSottoMenu);

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

    // -----------------------------------------------------------------
    //  Utility: piccola texture monocolore 1x1
    // -----------------------------------------------------------------
    static Texture2D TexturaPiena(Color colore)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, colore);
        t.Apply();
        return t;
    }
}
