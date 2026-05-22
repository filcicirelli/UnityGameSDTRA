using UnityEngine;

// Astro: il personaggio del giocatore (un alieno verde).
// Astro segue il mouse: dove sta il mouse, sta lui.
// Se passa vicino a una caramella la prende, se tocca un asteroide o
// una bomba perde una vita. Quando ha tutte le caramelle deve prendere
// la chiave e portarla alla porta.
public class Astro : MonoBehaviour
{
    public static Astro Istanza;

    // Raggi entro i quali Astro "tocca" gli oggetti
    public float raggioCaramella = Impostazioni.RAGGIO_CARAMELLA;
    public float raggioChiave    = Impostazioni.RAGGIO_CHIAVE;
    public float raggioPorta     = Impostazioni.RAGGIO_PORTA;
    public float raggioBomba     = Impostazioni.RAGGIO_BOMBA;

    // Sta gia' portando la chiave?
    public bool HaChiave;

    // Velocita' attuale (la calcolo dalla differenza di posizione)
    public Vector2 Velocita;

    // Variabili interne
    private Camera telecamera;
    private float distanzaZ;
    private Vector3 scalaBase;
    private SpriteRenderer sr;
    private Color coloreBase;
    private float timerSalto;
    private float timerLampeggio;
    private float inclinazione;

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

        // 1) Sposto Astro dove sta il mouse
        Vector3 posPrecedente = transform.position;
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = distanzaZ;
        Vector3 mouseMondo = telecamera.ScreenToWorldPoint(mouseScreen);
        transform.position = new Vector3(mouseMondo.x, mouseMondo.y, 0f);

        // Calcolo la velocita' (mi serve per inclinare lo sprite)
        float dt = Mathf.Max(Time.deltaTime, 0.000001f);
        Velocita = ((Vector2)(transform.position - posPrecedente)) / dt;

        // 2) Animazioni: respiro, saltino quando raccoglie, inclinazione
        float respiro = 1f + Mathf.Sin(Time.time * 2f) * 0.04f;

        float salto = 1f;
        if (timerSalto > 0f)
        {
            timerSalto = timerSalto - Time.deltaTime;
            float k = Mathf.Clamp01(timerSalto / 0.35f);
            salto = 1f + k * 0.20f;
        }

        float scalaTot = respiro * salto;
        transform.localScale = new Vector3(scalaBase.x * scalaTot, scalaBase.y * scalaTot, 1f);

        // Inclinazione dovuta al movimento orizzontale
        float inclinObiettivo = Mathf.Clamp(-Velocita.x * 3f, -18f, 18f);
        inclinazione = Mathf.Lerp(inclinazione, inclinObiettivo, Time.deltaTime * 8f);
        transform.localRotation = Quaternion.Euler(0f, 0f, inclinazione);

        // 3) Controllo le interazioni solo se il gioco e' "vivo"
        bool giocoVivo = !gm.MissioneCompletata && !gm.PartitaFinita && !gm.VittoriaFinale;
        if (giocoVivo)
        {
            ControllaCaramelle();
            ControllaAsteroidi();

            if (gm.BombeAttive)
            {
                ControllaBombe();
            }

            // Fase chiave/porta
            if (Chiave.Istanza != null && !HaChiave)
            {
                ControllaChiave();
            }
            if (HaChiave && Porta.Istanza != null)
            {
                ControllaPorta();
            }
        }

        AggiornaLampeggioRosso();
    }

    // ---- Controlli di tocco ----

    void ControllaCaramelle()
    {
        Vector2 pos = transform.position;
        // Faccio una copia perche' Raccogli() modifica la lista
        for (int i = Caramella.Attive.Count - 1; i >= 0; i--)
        {
            Caramella c = Caramella.Attive[i];
            if (c == null) continue;
            float dist = Vector2.Distance(pos, c.transform.position);
            if (dist <= raggioCaramella)
            {
                c.Raccogli();
            }
        }
    }

    void ControllaAsteroidi()
    {
        Vector2 pos = transform.position;
        for (int i = 0; i < Asteroide.Tutti.Count; i++)
        {
            Asteroide a = Asteroide.Tutti[i];
            if (a != null && a.Contiene(pos))
            {
                GestoreGioco.Istanza.SegnalaAsteroideToccato();
                timerLampeggio = 0.20f;
                return; // basta un asteroide alla volta
            }
        }
    }

    void ControllaBombe()
    {
        Vector2 pos = transform.position;
        for (int i = 0; i < Bomba.Tutte.Count; i++)
        {
            Bomba b = Bomba.Tutte[i];
            if (b == null) continue;
            float dist = Vector2.Distance(pos, b.transform.position);
            if (dist <= raggioBomba + b.raggioPericolo)
            {
                b.Detona();
                return;
            }
        }
    }

    void ControllaChiave()
    {
        Chiave k = Chiave.Istanza;
        float dist = Vector2.Distance(transform.position, k.transform.position);
        if (dist <= raggioChiave)
        {
            k.Raccogli();
            HaChiave = true;
        }
    }

    void ControllaPorta()
    {
        Porta p = Porta.Istanza;
        float dist = Vector2.Distance(transform.position, p.transform.position);
        if (dist <= raggioPorta)
        {
            p.Sblocca();
        }
    }

    // Quando Astro prende un colpo lampeggia di rosso per un istante
    void AggiornaLampeggioRosso()
    {
        if (sr == null) return;

        if (timerLampeggio > 0f)
        {
            timerLampeggio = timerLampeggio - Time.deltaTime;
            float k = Mathf.Clamp01(timerLampeggio / 0.20f);
            sr.color = Color.Lerp(coloreBase, new Color(1f, 0.3f, 0.3f, 1f), k);
        }
        else
        {
            sr.color = coloreBase;
        }
    }

    // La caramella chiama questo metodo: Astro fa un piccolo saltino
    public static void NotificaSaltoFelice()
    {
        if (Istanza != null)
        {
            Istanza.timerSalto = 0.35f;
        }
    }
}
