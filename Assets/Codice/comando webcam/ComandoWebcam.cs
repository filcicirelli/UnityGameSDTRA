using System.Collections;
using UnityEngine;

// ============================================================================
// COMANDO CON LA WEBCAM
// ----------------------------------------------------------------------------
// E' una delle tre modalita' di gioco: invece di muovere Astro con il mouse,
// lo si muove muovendo il dito davanti alla webcam. La modalita' si sceglie
// nella SCHERMATA INIZIALE (vedi SchermataStart e Comandi).
//
// Non riconosciamo davvero "il dito" (servirebbe l'intelligenza artificiale):
// seguiamo un EVIDENZIATORE FLUO tenuto in mano (verde, giallo o fucsia) e,
// per ogni fotogramma della webcam, cerchiamo i pixel di quel colore e ne
// calcoliamo il "centro". Quel centro diventa la posizione del puntatore.
// Gli evidenziatori sono molto piu' saturi della pelle: cosi' non si rischia
// piu' di confondere il colore con il viso.
//
// Questo file:
//  - si installa DA SOLO all'avvio (come FeedbackPaziente), non va messo in scena;
//  - accende la webcam solo quando serve (modalita' "dito" attiva);
//  - espone agli altri file la posizione del dito (Posizione) e se lo vede
//    (DitoVisto); ci pensa Comandi a passarla ad Astro;
//  - disegna in un angolo un'anteprima della webcam con un mirino sul dito.
// ============================================================================
public class ComandoWebcam : MonoBehaviour
{
    public static ComandoWebcam Istanza;

    // ---- Stato letto dagli altri file (da Comandi) ----
    // Posizione del dito in PIXEL dello schermo (stesso sistema di Input.mousePosition)
    public static Vector2 Posizione;
    // true se in questo fotogramma ho trovato il colore
    public static bool DitoVisto;

    // La webcam e' accesa e ha gia' un'immagine valida?
    public static bool Pronta
    {
        get
        {
            return Istanza != null
                && Istanza.webcam != null
                && Istanza.webcam.isPlaying
                && Istanza.webcam.width > 16; // finche' e' 16 il primo frame non e' arrivato
        }
    }

    // ---- Variabili interne ----
    private WebCamTexture webcam;
    private Color32[] pixel;        // i pixel della webcam, riusati ad ogni frame
    private bool avvioInCorso;      // sto gia' accendendo la webcam?
    private string messaggio = "";  // messaggio da mostrare (es. "nessuna webcam")
    private float[] tinteBersaglio; // la tinta (hue) di ogni colore dell'evidenziatore
    private float ultimaNx = 0.5f;  // ultima posizione del dito nell'immagine (0..1)
    private float ultimaNy = 0.5f;
    private Texture2D texBianca;    // 1x1 bianca, per riquadri e mirino

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Installa()
    {
        if (Istanza != null) return;
        GameObject go = new GameObject("ComandoWebcam");
        Istanza = go.AddComponent<ComandoWebcam>();
    }

    void Awake()
    {
        Istanza = this;

        // Calcolo una volta sola la tinta (hue) di ogni colore dell'evidenziatore.
        // Confrontare la tinta funziona anche se la luce cambia (un verde resta
        // verde sia in ombra che al sole).
        tinteBersaglio = new float[ParametriWebcam.COLORI_EVIDENZIATORE.Length];
        for (int i = 0; i < tinteBersaglio.Length; i++)
        {
            float s, v;
            Color.RGBToHSV(ParametriWebcam.COLORI_EVIDENZIATORE[i], out tinteBersaglio[i], out s, out v);
        }

        texBianca = new Texture2D(1, 1);
        texBianca.SetPixel(0, 0, Color.white);
        texBianca.Apply();

        Posizione = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void OnDestroy()
    {
        if (Istanza == this) Istanza = null;
        if (webcam != null) webcam.Stop();
    }

    // La modalita' "dito" e' attiva solo se l'ho scelta E la partita e' iniziata
    // (durante la schermata iniziale tengo la webcam spenta).
    static bool ModalitaDitoAttiva()
    {
        return Comandi.Attuale == Comandi.Modalita.Dito
            && GestoreGioco.Istanza != null
            && !GestoreGioco.Istanza.MenuInizialeAperto;
    }

    void Update()
    {
        if (ModalitaDitoAttiva())
        {
            // Accendo la webcam (la prima volta) e analizzo i fotogrammi
            AccendiWebcam();
            if (Pronta && webcam.didUpdateThisFrame)
            {
                AnalizzaFrame();
            }
        }
        else
        {
            // Modalita' non attiva: tengo la webcam in pausa per liberarla
            if (webcam != null && webcam.isPlaying) webcam.Pause();
        }
    }

    // ---- Accensione della webcam ----

    void AccendiWebcam()
    {
        if (webcam != null)
        {
            if (!webcam.isPlaying) webcam.Play(); // era in pausa: la riavvio
            return;
        }
        if (!avvioInCorso)
        {
            StartCoroutine(AvviaWebcam());
        }
    }

    // Chiedo il permesso (serve su alcuni sistemi/browser) e poi avvio la webcam.
    IEnumerator AvviaWebcam()
    {
        avvioInCorso = true;

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            messaggio = "permesso negato";
            avvioInCorso = false;
            yield break;
        }

        if (WebCamTexture.devices.Length == 0)
        {
            messaggio = "nessuna webcam trovata";
            avvioInCorso = false;
            yield break;
        }

        string nome = WebCamTexture.devices[0].name; // la prima webcam disponibile
        webcam = new WebCamTexture(
            nome,
            ParametriWebcam.LARGHEZZA_RICHIESTA,
            ParametriWebcam.ALTEZZA_RICHIESTA,
            ParametriWebcam.FPS_RICHIESTI);
        webcam.Play();

        avvioInCorso = false;
    }

    // ---- Analisi del fotogramma: trovo il "centro" del colore ----

    void AnalizzaFrame()
    {
        int w = webcam.width;
        int h = webcam.height;

        // Copio i pixel in un array (lo riuso per non sprecare memoria)
        if (pixel == null || pixel.Length != w * h)
        {
            pixel = new Color32[w * h];
        }
        webcam.GetPixels32(pixel);

        // Sommo le posizioni di tutti i pixel del colore giusto: la media e'
        // il loro "centro di massa", cioe' dove sta il dito.
        long sommaX = 0;
        long sommaY = 0;
        int conta = 0;

        int passo = Mathf.Max(1, ParametriWebcam.PASSO_ANALISI);
        for (int y = 0; y < h; y += passo)
        {
            int inizioRiga = y * w;
            for (int x = 0; x < w; x += passo)
            {
                if (ColoreGiusto(pixel[inizioRiga + x]))
                {
                    sommaX += x;
                    sommaY += y;
                    conta++;
                }
            }
        }

        if (conta >= ParametriWebcam.PIXEL_MINIMI)
        {
            DitoVisto = true;

            // Centro in coordinate 0..1 dentro l'immagine
            float nx = (float)sommaX / conta / w;
            float ny = (float)sommaY / conta / h;
            if (ParametriWebcam.SPECCHIA) nx = 1f - nx; // effetto specchio

            ultimaNx = nx;
            ultimaNy = ny;

            // Trasformo in pixel dello schermo
            Vector2 bersaglio = new Vector2(nx * Screen.width, ny * Screen.height);

            // Movimento morbido: il puntatore insegue il bersaglio senza scatti.
            // Calcolo indipendente dagli FPS, cosi' va uguale su PC lenti e veloci.
            float morbidezza = Mathf.Clamp01(ParametriWebcam.MORBIDEZZA);
            float t = 1f - Mathf.Pow(morbidezza, Time.deltaTime * 60f);
            Posizione = Vector2.Lerp(Posizione, bersaglio, t);
        }
        else
        {
            // Non vedo il colore: lascio il puntatore dov'era (non lo sparo a caso)
            DitoVisto = false;
        }
    }

    // Un pixel e' "del colore giusto" se e' abbastanza acceso (saturo) e luminoso
    // e la sua TINTA e' vicina a UNO QUALSIASI dei colori dell'evidenziatore.
    bool ColoreGiusto(Color32 c)
    {
        Color col = new Color(c.r / 255f, c.g / 255f, c.b / 255f);
        float h, s, v;
        Color.RGBToHSV(col, out h, out s, out v);

        if (s < ParametriWebcam.SATURAZIONE_MINIMA) return false; // poco saturo (es. pelle)
        if (v < ParametriWebcam.LUMINOSITA_MINIMA) return false;  // troppo scuro

        // Provo tutti i colori dell'evidenziatore: basta che ne combaci uno.
        for (int i = 0; i < tinteBersaglio.Length; i++)
        {
            // La tinta e' un cerchio: 0 e 1 sono lo stesso colore (rosso),
            // quindi misuro la distanza "girando dalla parte piu' corta".
            float dh = Mathf.Abs(h - tinteBersaglio[i]);
            if (dh > 0.5f) dh = 1f - dh;
            if (dh <= ParametriWebcam.TOLLERANZA_TINTA) return true;
        }
        return false;
    }

    // ---- Disegno a schermo (anteprima webcam) ----

    void OnGUI()
    {
        // Disegno solo quando si sta giocando con il dito
        if (!ModalitaDitoAttiva()) return;

        if (!Pronta)
        {
            // Webcam non ancora pronta: spiego perche' (intanto uso il mouse)
            string testo;
            if (!string.IsNullOrEmpty(messaggio)) testo = "Webcam: " + messaggio + " (uso il mouse)";
            else testo = "Avvio webcam...";
            DisegnaSuggerimento(testo);
            return;
        }

        if (ParametriWebcam.MOSTRA_ANTEPRIMA)
        {
            DisegnaAnteprima();
        }
    }

    void DisegnaAnteprima()
    {
        // Riquadro in alto a destra, sotto la barra dell'energia
        float pw = ParametriWebcam.ANTEPRIMA_LARGHEZZA;
        float ph = pw * webcam.height / webcam.width; // mantengo le proporzioni
        float px = Screen.width - pw - ParametriWebcam.ANTEPRIMA_MARGINE;
        float py = 130f;

        // Bordo bianco
        GUI.DrawTexture(new Rect(px - 2, py - 2, pw + 4, ph + 4), texBianca);

        // Immagine della webcam (specchiata se SPECCHIA, cosi' combacia col mirino)
        Rect riquadro = new Rect(px, py, pw, ph);
        Rect coordinate = ParametriWebcam.SPECCHIA ? new Rect(1f, 0f, -1f, 1f)
                                                   : new Rect(0f, 0f, 1f, 1f);
        GUI.DrawTextureWithTexCoords(riquadro, webcam, coordinate);

        // Mirino sul dito (verde se lo vedo, rosso se non lo vedo)
        float mx = px + ultimaNx * pw;
        float my = py + (1f - ultimaNy) * ph; // la GUI ha la y verso il basso
        Color coloreMirino = DitoVisto ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f);
        DisegnaMirino(mx, my, coloreMirino);

        // Etichetta sopra l'anteprima
        string stato = DitoVisto ? "evidenziatore: OK" : "mostra l'evidenziatore";
        DisegnaEtichetta(new Rect(px, py - 26, pw, 22), "WEBCAM   " + stato);
    }

    // Mirino = una crocetta fatta con due rettangolini
    void DisegnaMirino(float x, float y, Color colore)
    {
        Color vecchio = GUI.color;
        GUI.color = colore;
        GUI.DrawTexture(new Rect(x - 12, y - 2, 24, 4), texBianca); // barra orizzontale
        GUI.DrawTexture(new Rect(x - 2, y - 12, 4, 24), texBianca); // barra verticale
        GUI.color = vecchio;
    }

    void DisegnaEtichetta(Rect r, string testo)
    {
        GUIStyle stile = new GUIStyle(GUI.skin.box);
        stile.fontSize = 14;
        stile.fontStyle = FontStyle.Bold;
        stile.alignment = TextAnchor.MiddleCenter;
        stile.normal.textColor = Color.white;
        GUI.Box(r, testo, stile);
    }

    void DisegnaSuggerimento(string testo)
    {
        GUIStyle stile = new GUIStyle(GUI.skin.label);
        stile.fontSize = 16;
        stile.fontStyle = FontStyle.Bold;
        stile.alignment = TextAnchor.MiddleCenter;
        stile.normal.textColor = new Color(0.85f, 0.95f, 1f);
        // In basso al centro, fra l'obiettivo (a sinistra) e i pulsanti (a destra)
        GUI.Label(new Rect(Screen.width / 2f - 220, Screen.height - 32, 440, 24), testo, stile);
    }
}
