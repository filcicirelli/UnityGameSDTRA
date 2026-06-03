using System.Collections.Generic;
using UnityEngine;

// In questo file ci sono tutti gli "oggetti" del gioco, cioe' le cose
// che si muovono o con cui Astro interagisce:
//   - Astro        (il personaggio del giocatore)
//   - Caramella    (da raccogliere)
//   - Chiave       (appare alla fine, va portata alla porta)
//   - Porta        (si apre con la chiave)
//   - Bomba        (da non toccare)
//   - Asteroide    (rettangolo che fa male)
//   - Esplosione, PianetaAmico, Coriandoli  (effetti)


// =============================================================
// ASTRO: il personaggio del giocatore (un alieno verde).
// Astro segue il mouse: dove sta il mouse, sta lui.
// Se passa vicino a una caramella la prende, se tocca un asteroide o
// una bomba perde una vita. Quando ha tutte le caramelle deve prendere
// la chiave e portarla alla porta.
// =============================================================
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


// =============================================================
// CARAMELLA spaziale. Si raccoglie passando vicino con Astro.
// Galleggia nello spazio con un piccolo movimento sinusoidale.
// =============================================================
public class Caramella : MonoBehaviour
{
    // Lista di tutte le caramelle attive nel livello
    public static List<Caramella> Attive = new List<Caramella>();

    private Vector3 punto;   // posizione attorno a cui oscilla
    private float fase;
    private bool raccolta;

    void OnEnable()
    {
        Attive.Add(this);
    }

    void OnDisable()
    {
        Attive.Remove(this);
    }

    public void Inizializza(Vector3 posizione)
    {
        punto = posizione;
        transform.position = posizione;
        // Fase diversa per ogni caramella, cosi' non si muovono tutte uguali
        fase = Random.Range(0f, Mathf.PI * 2f);
    }

    // Chiamata da Astro quando si avvicina abbastanza
    public void Raccogli()
    {
        if (raccolta) return;
        raccolta = true;

        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaCaramellaRaccolta();
        }
        Astro.NotificaSaltoFelice();
        Destroy(gameObject);
    }

    void Update()
    {
        // Galleggiamento + scintillio
        float t = Time.time * 1.5f + fase;

        float dx = Mathf.Sin(t) * 0.10f;
        float dy = Mathf.Cos(t * 0.8f) * 0.12f;
        transform.position = punto + new Vector3(dx, dy, 0f);

        // Piccola rotazione avanti-indietro
        float angolo = Mathf.Sin(Time.time * 3f + fase) * 8f;
        transform.rotation = Quaternion.Euler(0f, 0f, angolo);

        // Pulsa di dimensione, sembra che brilli
        float scala = 1f + Mathf.Sin(Time.time * 6f + fase) * 0.08f;
        transform.localScale = new Vector3(scala, scala, 1f);
    }
}


// =============================================================
// CHIAVE dorata: appare dopo aver raccolto tutte le caramelle.
// Astro deve prenderla e portarla alla porta.
// =============================================================
public class Chiave : MonoBehaviour
{
    // Riferimento alla chiave del livello (ce n'e' una sola alla volta)
    public static Chiave Istanza;

    public bool Raccolta;

    private Vector3 puntoSpawn;
    private float fase;

    // Se Astro entra in questo raggio, la chiave gli va incontro (effetto calamita)
    private const float RAGGIO_MAGNETE = 3.5f;
    // Se entra in questo raggio, viene presa direttamente
    private const float RAGGIO_PRESA = 1.8f;

    void Awake()
    {
        Istanza = this;
        fase = Random.Range(0f, Mathf.PI * 2f);
    }

    void OnDestroy()
    {
        if (Istanza == this)
        {
            Istanza = null;
        }
    }

    public void Inizializza(Vector2 posizione)
    {
        puntoSpawn = new Vector3(posizione.x, posizione.y, -0.4f);
        transform.position = puntoSpawn;
    }

    // Chiamata da Astro
    public void Raccogli()
    {
        if (Raccolta) return;
        Raccolta = true;

        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaChiaveRaccolta();
        }
    }

    void Update()
    {
        if (!Raccolta)
        {
            ComportamentoLibera();
        }
        else
        {
            SeguiAstro();
        }
    }

    // Prima di essere raccolta: ondeggia e si fa attirare da Astro
    void ComportamentoLibera()
    {
        // Posizione "ferma" con oscillazione verticale
        float t = Time.time * 2f + fase;
        Vector3 ferma = puntoSpawn + new Vector3(0f, Mathf.Sin(t) * 0.18f, 0f);

        Vector3 nuovaPos = ferma;

        if (Astro.Istanza != null)
        {
            Vector3 posAstro = Astro.Istanza.transform.position;
            float distanza = Vector2.Distance(transform.position, posAstro);

            if (distanza <= RAGGIO_PRESA)
            {
                // Astro molto vicino: la prendo subito
                Raccogli();
                return;
            }

            if (distanza <= RAGGIO_MAGNETE)
            {
                // Astro entro il raggio del magnete: la chiave gli va incontro
                float fattore = Mathf.InverseLerp(RAGGIO_MAGNETE, RAGGIO_PRESA, distanza);
                float velocita = Mathf.Lerp(2.5f, 9f, fattore);

                Vector3 obiettivo = new Vector3(posAstro.x, posAstro.y, ferma.z);
                nuovaPos = Vector3.Lerp(transform.position, obiettivo, Time.deltaTime * velocita);
            }
            else
            {
                // Fuori dal raggio: la chiave torna al punto di partenza
                nuovaPos = Vector3.Lerp(transform.position, ferma, Time.deltaTime * 3f);
            }
        }

        transform.position = nuovaPos;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 1.5f + fase) * 12f);
        float scala = 1.2f + Mathf.Sin(Time.time * 4f + fase) * 0.08f;
        transform.localScale = new Vector3(scala, scala, 1f);
    }

    // Dopo la raccolta: la chiave segue Astro con un piccolo ritardo
    void SeguiAstro()
    {
        if (Astro.Istanza == null) return;

        Vector3 obiettivo = Astro.Istanza.transform.position + new Vector3(0.55f, 0.55f, -0.4f);
        transform.position = Vector3.Lerp(transform.position, obiettivo, Time.deltaTime * 8f);

        float oscillazione = 25f + Mathf.Sin(Time.time * 6f) * 6f;
        transform.rotation = Quaternion.Euler(0f, 0f, oscillazione);
        transform.localScale = new Vector3(1.1f, 1.1f, 1f);
    }
}


// =============================================================
// PORTA che si sblocca quando Astro la tocca portando la chiave.
// Per renderla un po' difficile si sposta ogni tot secondi.
// =============================================================
public class Porta : MonoBehaviour
{
    public static Porta Istanza;

    // Quanti secondi sta ferma in un posto prima di spostarsi
    public float secondiFerma = Impostazioni.PORTA_SECONDI_FERMA;

    // Durata dello spostamento
    public float secondiSpostamento = 0.5f;

    private Vector3 partenza;
    private Vector3 destinazione;
    private float timer;
    private bool inMovimento;
    private bool aperta;

    void Awake()
    {
        Istanza = this;
    }

    void OnDestroy()
    {
        if (Istanza == this)
        {
            Istanza = null;
        }
    }

    public void Inizializza(Vector2 posizione)
    {
        transform.position = new Vector3(posizione.x, posizione.y, -0.5f);
        partenza = transform.position;
        destinazione = transform.position;
        inMovimento = false;
        timer = 0f;
    }

    void Update()
    {
        if (aperta) return;

        // Piccolo "respiro" della porta
        float wobble = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
        transform.localScale = new Vector3(1.8f * wobble, 1.8f * wobble, 1f);

        timer = timer + Time.deltaTime;

        if (!inMovimento)
        {
            // Aspetto qualche secondo e poi mi sposto
            if (timer >= secondiFerma)
            {
                AvviaSpostamento();
            }
        }
        else
        {
            // In viaggio fra partenza e destinazione
            float t = Mathf.Clamp01(timer / secondiSpostamento);
            float smussato = Mathf.SmoothStep(0f, 1f, t);

            Vector3 nuova = Vector3.Lerp(partenza, destinazione, smussato);
            transform.position = new Vector3(nuova.x, nuova.y, -0.5f);

            if (t >= 1f)
            {
                inMovimento = false;
                timer = 0f;
            }
        }
    }

    void AvviaSpostamento()
    {
        partenza = transform.position;
        Vector2 prossima = CaricatoreLivelli.ScegliPosizionePortaCasuale(transform.position);
        destinazione = new Vector3(prossima.x, prossima.y, -0.5f);

        inMovimento = true;
        timer = 0f;
    }

    // Chiamata da Astro quando entra in contatto.
    // Si apre solo se Astro porta la chiave.
    public void Sblocca()
    {
        if (aperta) return;
        aperta = true;

        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaPortaRaggiunta();
        }
    }
}


// =============================================================
// BOMBA: da non toccare. Pulsa di rosso per farsi notare.
// =============================================================
public class Bomba : MonoBehaviour
{
    // Lista di tutte le bombe del livello
    public static List<Bomba> Tutte = new List<Bomba>();

    // Zona di pericolo attorno alla bomba (oltre lo sprite)
    public float raggioPericolo = 0.5f;

    private Vector3 posizioneBase;
    private SpriteRenderer sr;
    private SpriteRenderer alone;
    private float fase;
    private bool esplosa;

    void OnEnable()
    {
        Tutte.Add(this);
    }

    void OnDisable()
    {
        Tutte.Remove(this);
    }

    public void Inizializza(Vector2 posizione)
    {
        posizioneBase = new Vector3(posizione.x, posizione.y, 0f);
        transform.position = posizioneBase;
        fase = Random.Range(0f, Mathf.PI * 2f);
        sr = GetComponent<SpriteRenderer>();

        // Aggiungo un alone rosso che pulsa: cosi' si vede meglio
        GameObject aloneGo = new GameObject("Alone");
        aloneGo.transform.SetParent(transform, false);
        aloneGo.transform.localPosition = Vector3.zero;
        aloneGo.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

        alone = aloneGo.AddComponent<SpriteRenderer>();
        alone.sprite = FabbricaImmagini.CreaQuadratoPieno(new Color(1f, 0.20f, 0.20f, 0.30f));
        int ordine;
        if (sr != null) ordine = sr.sortingOrder;
        else ordine = 2;
        alone.sortingOrder = ordine - 1;
    }

    void Update()
    {
        if (esplosa) return;

        // Pulsazione dell'alone
        float pulse = 0.85f + Mathf.Sin(Time.time * 5f + fase) * 0.25f;
        if (alone != null)
        {
            alone.transform.localScale = new Vector3(2.4f * pulse, 2.4f * pulse, 1f);
            Color col = alone.color;
            col.a = 0.20f + 0.20f * Mathf.Sin(Time.time * 5f + fase);
            alone.color = col;
        }

        // Piccolo movimento della bomba
        float t = Time.time * 1.2f + fase;
        float dx = Mathf.Sin(t) * 0.06f;
        float dy = Mathf.Cos(t * 0.7f) * 0.08f;
        transform.position = posizioneBase + new Vector3(dx, dy, 0f);
    }

    // Esplode e fa perdere una vita
    public void Detona()
    {
        if (esplosa) return;
        esplosa = true;

        CaricatoreLivelli.GeneraEsplosione(transform.position);
        if (GestoreGioco.Istanza != null)
        {
            GestoreGioco.Istanza.SegnalaBombaColpita();
        }
        Destroy(gameObject);
    }
}


// =============================================================
// ASTEROIDE: rettangolo che fa male se viene toccato.
// Non uso la fisica di Unity, faccio io il controllo "punto dentro rettangolo".
// =============================================================
public class Asteroide : MonoBehaviour
{
    // Lista di tutti gli asteroidi presenti nel livello
    public static List<Asteroide> Tutti = new List<Asteroide>();

    // Rettangolo nello spazio del gioco
    public Rect Rettangolo;

    void OnEnable()
    {
        Tutti.Add(this);
    }

    void OnDisable()
    {
        Tutti.Remove(this);
    }

    public void Inizializza(Vector2 centro, Vector2 dimensione)
    {
        transform.position = new Vector3(centro.x, centro.y, 0f);
        // Lo sprite e' 1x1, lo ridimensiono in base a quanto deve essere grande
        transform.localScale = new Vector3(dimensione.x, dimensione.y, 1f);

        Rettangolo = new Rect(
            centro.x - dimensione.x / 2f,
            centro.y - dimensione.y / 2f,
            dimensione.x,
            dimensione.y);
    }

    public bool Contiene(Vector2 punto)
    {
        return Rettangolo.Contains(punto);
    }
}


// =============================================================
// ESPLOSIONE: anello luminoso che cresce e svanisce.
// Lo creo quando una bomba viene toccata.
// =============================================================
public class Esplosione : MonoBehaviour
{
    public float durata = 0.7f;
    public float scalaMax = 4.5f;

    private SpriteRenderer sr;
    private float eta;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * 0.2f; // parto piccolino
    }

    void Update()
    {
        eta = eta + Time.deltaTime;
        float k = Mathf.Clamp01(eta / durata);

        // Cresce velocemente all'inizio, poi rallenta
        float velocita = 1f - (1f - k) * (1f - k);
        float scala = Mathf.Lerp(0.2f, scalaMax, velocita);
        transform.localScale = new Vector3(scala, scala, 1f);

        // Sparisce piano piano (alpha da 1 a 0)
        if (sr != null)
        {
            Color col = sr.color;
            col.a = 1f - k;
            sr.color = col;
        }

        if (eta >= durata)
        {
            Destroy(gameObject);
        }
    }
}


// =============================================================
// PIANETA AMICO: pianeta sorridente che appare a fine missione.
// All'inizio fa un effetto pop-in, poi sta li' a ondeggiare.
// =============================================================
public class PianetaAmico : MonoBehaviour
{
    public float scalaFinale = 2.4f;

    private Vector3 posizioneBase;
    private float vita;

    void Awake()
    {
        posizioneBase = transform.position;
        transform.localScale = Vector3.zero; // parte invisibile
    }

    void Update()
    {
        vita = vita + Time.deltaTime;

        float scala;
        if (vita < 0.5f)
        {
            // Effetto "pop": cresce con un piccolo rimbalzo
            float t = vita / 0.5f;
            float rimbalzo = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
            scala = scalaFinale * rimbalzo * t;
        }
        else
        {
            // Dopo respira piano
            scala = scalaFinale + 0.1f * Mathf.Sin(Time.time * 3f);
        }

        transform.localScale = new Vector3(scala, scala, 1f);

        // Si muove un po' a destra e a sinistra, come a salutare
        float dx = Mathf.Sin(Time.time * 1.5f) * 0.15f;
        transform.position = posizioneBase + new Vector3(dx, 0f, 0f);
    }
}


// =============================================================
// CORIANDOLI: pioggia di coriandoli colorati per festeggiare
// la missione completata.
// =============================================================
public class Coriandoli : MonoBehaviour
{
    public int quantita = 80;
    public float durata = 4f;

    // Colori dei coriandoli
    static readonly Color[] COLORI = new Color[]
    {
        new Color(1f,    0.30f, 0.40f),
        new Color(0.40f, 0.80f, 1f),
        new Color(1f,    0.85f, 0.20f),
        new Color(0.55f, 1f,    0.55f),
        new Color(0.95f, 0.55f, 1f),
    };

    void Start()
    {
        // Creo tutti i coriandoli subito
        for (int i = 0; i < quantita; i++)
        {
            CreaUno();
        }
        // Dopo un po' la festa finisce e tutto sparisce
        Destroy(gameObject, durata + 1.5f);
    }

    void CreaUno()
    {
        GameObject go = new GameObject("Coriandolo");
        go.transform.SetParent(transform, false);

        // Parto dall'alto, posizione orizzontale a caso
        float x = Random.Range(-8f, 8f);
        go.transform.position = new Vector3(x, 6.5f, -3f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Color colore = COLORI[Random.Range(0, COLORI.Length)];
        sr.sprite = FabbricaImmagini.CreaQuadratoPieno(colore);
        sr.sortingOrder = 10;

        float scala = Random.Range(0.12f, 0.22f);
        go.transform.localScale = new Vector3(scala, scala * 0.6f, 1f);

        PezzoCoriandolo p = go.AddComponent<PezzoCoriandolo>();
        p.velocita = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1f, -3f));
        p.velocitaAngolare = Random.Range(-360f, 360f);
        p.vita = durata;
    }
}

// Singolo coriandolo: cade, ondeggia e gira.
public class PezzoCoriandolo : MonoBehaviour
{
    public Vector2 velocita;
    public float velocitaAngolare;
    public float vita;

    private float eta;
    private float fase;

    void Awake()
    {
        fase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        eta = eta + Time.deltaTime;

        // Gravita' leggera
        velocita.y = velocita.y - 1.2f * Time.deltaTime;

        // Movimento + un po' di ondeggio
        float dx = velocita.x * Time.deltaTime + Mathf.Sin(Time.time * 4f + fase) * 0.01f;
        float dy = velocita.y * Time.deltaTime;
        transform.position = transform.position + new Vector3(dx, dy, 0f);
        transform.Rotate(0f, 0f, velocitaAngolare * Time.deltaTime);

        if (eta >= vita)
        {
            Destroy(gameObject);
        }
    }
}
