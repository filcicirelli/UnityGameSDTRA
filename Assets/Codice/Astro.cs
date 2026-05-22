using UnityEngine;

// =====================================================================
//  Astro
// ---------------------------------------------------------------------
//  Astro e' il personaggio del giocatore: un alieno verde con uno
//  zainetto. Si comporta come un "puntatore vivente":
//      - segue il mouse in ogni momento
//      - se passa vicino a una caramella, la raccoglie
//      - se tocca un asteroide o una bomba, perde una vita
//      - dopo aver raccolto tutto, raccoglie la chiave e la porta
//        verso la porta-portale
//
//  Tutti i raggi di "tocco" sono volutamente generosi: il gioco e'
//  pensato per pazienti in riabilitazione, quindi non serve la
//  precisione millimetrica di un mouse.
// =====================================================================
public class Astro : MonoBehaviour
{
    public static Astro Istanza { get; private set; }

    // ----- Raggi di interazione (in unita' di mondo) -----
    public float raggioCaramella = 0.85f;
    public float raggioChiave    = 1.80f;   // generoso (la chiave e' "magnetica")
    public float raggioPorta     = 1.80f;   // anche la porta e' generosa
    public float raggioBomba     = 0.55f;

    // Sta gia' portando la chiave?
    public bool HaChiave { get; private set; }

    // Velocita' attuale (la calcolo a mano dal cambio di posizione)
    public Vector2 Velocita { get; private set; }

    // ----- Variabili interne -----
    private Camera telecamera;
    private float distanzaZ;
    private Vector3 scalaBase;
    private SpriteRenderer sr;
    private Color coloreBase;
    private float timerSaltoFelice;
    private float timerLampeggioRosso;
    private float inclinazione;  // angolo corrente in gradi

    // -----------------------------------------------------------------
    //  Ciclo di vita Unity
    // -----------------------------------------------------------------

    void Awake()
    {
        Istanza = this;
        telecamera = Camera.main;
        distanzaZ = Mathf.Abs(telecamera.transform.position.z);
        scalaBase = transform.localScale;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            coloreBase = sr.color;
        }
    }

    void OnDestroy()
    {
        if (Istanza == this)
        {
            Istanza = null;
        }
    }

    void Update()
    {
        GestoreGioco gm = GestoreGioco.Istanza;
        if (gm == null) return;

        // ------------------------------------------------------------
        //  1) Posizione: seguo il mouse (Astro E' il cursore)
        // ------------------------------------------------------------
        Vector3 posizionePrecedente = transform.position;
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = distanzaZ;
        Vector3 mouseMondo = telecamera.ScreenToWorldPoint(mouseScreen);
        transform.position = new Vector3(mouseMondo.x, mouseMondo.y, 0f);

        // Stima della velocita' (mi servira' per inclinare lo sprite)
        float dt = Mathf.Max(Time.deltaTime, 0.000001f);
        Velocita = ((Vector2)(transform.position - posizionePrecedente)) / dt;

        // ------------------------------------------------------------
        //  2) Animazioni: respiro + saltino di gioia + tilt
        // ------------------------------------------------------------
        float respiro = 1f + Mathf.Sin(Time.time * 2f) * 0.04f;

        float saltoFelice = 1f;
        if (timerSaltoFelice > 0f)
        {
            timerSaltoFelice -= Time.deltaTime;
            float k = Mathf.Clamp01(timerSaltoFelice / 0.35f);
            saltoFelice = 1f + k * 0.20f;
        }

        float scalaCorrente = respiro * saltoFelice;
        transform.localScale = new Vector3(
            scalaBase.x * scalaCorrente,
            scalaBase.y * scalaCorrente,
            1f);

        // Inclinazione legata alla velocita' orizzontale (effetto "lean")
        float inclinazioneObiettivo = Mathf.Clamp(-Velocita.x * 3f, -18f, 18f);
        inclinazione = Mathf.Lerp(inclinazione, inclinazioneObiettivo, Time.deltaTime * 8f);
        transform.localRotation = Quaternion.Euler(0f, 0f, inclinazione);

        // ------------------------------------------------------------
        //  3) Interazioni (solo se il gioco e' "vivo")
        // ------------------------------------------------------------
        bool giocoVivo = !gm.MissioneCompletata && !gm.PartitaFinita && !gm.VittoriaFinale;
        if (giocoVivo)
        {
            ControllaToccoCaramelle();
            ControllaToccoAsteroidi();

            if (gm.BombeAttive)
            {
                ControllaToccoBombe();
            }

            // Fase chiave/porta: attiva solo dopo la raccolta
            if (Chiave.Istanza != null && !HaChiave)
            {
                ControllaToccoChiave();
            }
            if (HaChiave && Porta.Istanza != null)
            {
                ControllaToccoPorta();
            }
        }

        AggiornaLampeggioRosso();
    }

    // -----------------------------------------------------------------
    //  Controlli di tocco
    // -----------------------------------------------------------------

    void ControllaToccoCaramelle()
    {
        Vector2 posAstro = transform.position;
        foreach (Caramella c in Caramella.Attive)
        {
            if (c == null) continue;
            float dist = Vector2.Distance(posAstro, c.transform.position);
            if (dist <= raggioCaramella)
            {
                c.Raccogli();
            }
        }
    }

    void ControllaToccoAsteroidi()
    {
        Vector2 posAstro = transform.position;
        for (int i = 0; i < Asteroide.Tutti.Count; i++)
        {
            Asteroide a = Asteroide.Tutti[i];
            if (a != null && a.Contiene(posAstro))
            {
                GestoreGioco.Istanza.SegnalaAsteroideToccato();
                timerLampeggioRosso = 0.20f;
                return; // basta un asteroide alla volta
            }
        }
    }

    void ControllaToccoBombe()
    {
        Vector2 posAstro = transform.position;
        for (int i = 0; i < Bomba.Tutte.Count; i++)
        {
            Bomba b = Bomba.Tutte[i];
            if (b == null) continue;
            float dist = Vector2.Distance(posAstro, b.transform.position);
            if (dist <= raggioBomba + b.raggioPericolo)
            {
                b.Detona();
                return;
            }
        }
    }

    void ControllaToccoChiave()
    {
        Chiave k = Chiave.Istanza;
        float dist = Vector2.Distance(transform.position, k.transform.position);
        if (dist <= raggioChiave)
        {
            k.Raccogli();
            HaChiave = true;
        }
    }

    void ControllaToccoPorta()
    {
        Porta p = Porta.Istanza;
        float dist = Vector2.Distance(transform.position, p.transform.position);
        if (dist <= raggioPorta)
        {
            p.Sblocca();
        }
    }

    // -----------------------------------------------------------------
    //  Feedback visivo (lampeggio rosso quando "fa male")
    // -----------------------------------------------------------------

    void AggiornaLampeggioRosso()
    {
        if (sr == null) return;

        if (timerLampeggioRosso > 0f)
        {
            timerLampeggioRosso -= Time.deltaTime;
            float k = Mathf.Clamp01(timerLampeggioRosso / 0.20f);
            sr.color = Color.Lerp(coloreBase, new Color(1f, 0.3f, 0.3f, 1f), k);
        }
        else
        {
            sr.color = coloreBase;
        }
    }

    // Chiamato dalla caramella quando viene raccolta: Astro fa un saltino.
    public static void NotificaSaltoFelice()
    {
        if (Istanza != null)
        {
            Istanza.timerSaltoFelice = 0.35f;
        }
    }
}
