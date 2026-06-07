using UnityEngine;

// Avvio: parte da solo quando premo Play in Unity.
// Crea la telecamera e i due oggetti principali del gioco
// (il GestoreGioco e l'InterfacciaGioco).
public static class Avvio
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inizia()
    {
        // Pulisco eventuali telecamere e luci di default
        Camera[] cam = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        for (int i = 0; i < cam.Length; i++)
        {
            Object.Destroy(cam[i].gameObject);
        }

        Light[] luci = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < luci.Length; i++)
        {
            Object.Destroy(luci[i].gameObject);
        }

        // Telecamera 2D. Lo sfondo vero e' il wallpaper (vedi CostruisciSfondo);
        // questo nero si vede solo se l'immagine non venisse caricata.
        GameObject ogCam = new GameObject("TelecameraPrincipale");
        ogCam.tag = "MainCamera";

        Camera c = ogCam.AddComponent<Camera>();
        c.orthographic = true;
        c.orthographicSize = 6f;
        c.backgroundColor = Color.black;
        c.transform.position = new Vector3(0f, 0f, -10f);

        // Nascondo il cursore: nel gioco e' Astro che fa da puntatore
        Cursor.visible = false;

        // Creo i due oggetti che fanno funzionare tutto
        new GameObject("GestoreGioco").AddComponent<GestoreGioco>();
        new GameObject("InterfacciaGioco").AddComponent<InterfacciaGioco>();
    }
}

// Cervello del gioco: tiene punteggio, vite, tempo e livello.
// Decide cosa succede quando una caramella viene presa, quando si
// tocca un asteroide o una bomba, quando si arriva alla porta, etc.
//
// Ogni livello ha 3 fasi:
//   FASE 1: raccogli tutte le caramelle
//   FASE 2: prendi la chiave che appare al centro
//   FASE 3: porta la chiave alla porta
public class GestoreGioco : MonoBehaviour
{
    // Riferimento unico: cosi' gli altri script lo trovano facilmente
    public static GestoreGioco Istanza;

    // ---- Stato della partita ----
    public int LivelloCorrente;
    public int CaramelleRaccolte;
    public int Punteggio;
    public int TotaleCaramelle;
    public bool MissioneCompletata;
    public bool PartitaFinita;
    public bool VittoriaFinale;

    // ---- Vite ----
    public int Vite;

    // ---- Tempo del livello ----
    public float TempoRimasto;

    // Motivo per cui ho perso (lo mostro nel Game Over)
    public string MotivoSconfitta;

    // ---- Chiave e porta ----
    public bool ChiaveApparsa;
    public bool ChiavePresa;

    // Lampeggio rosso quando prendo un colpo
    public float TimerLampeggio;

    // Periodo iniziale "PRONTI..." (le bombe non fanno male in questo tempo)
    public float TempoIniziale;

    public bool BombeAttive
    {
        get { return TempoIniziale <= 0f; }
    }

    // Aspetta un secondo dopo aver perso una vita, cosi' non perdo
    // tutte le vite restando incollato a un asteroide
    private float cooldownDanno;

    // Definizione del livello in corso
    public DatiLivello LivelloAttuale;

    // Percentuale di caramelle raccolte (la uso per la barra "energia")
    public float Energia
    {
        get
        {
            if (TotaleCaramelle == 0) return 0f;
            return (float)CaramelleRaccolte / (float)TotaleCaramelle;
        }
    }

    // Testo dell'obiettivo da mostrare nell'HUD
    public string TestoObiettivo
    {
        get
        {
            if (ChiaveApparsa && !ChiavePresa)
            {
                return "PRENDI LA CHIAVE";
            }
            if (ChiavePresa && !MissioneCompletata)
            {
                return "PORTA LA CHIAVE ALLA PORTA";
            }
            if (LivelloAttuale != null)
            {
                return LivelloAttuale.obiettivo;
            }
            return "";
        }
    }

    void Awake()
    {
        Istanza = this;
    }

    void Start()
    {
        // Parto sempre dal primo livello
        CaricaLivello(0);
    }

    void Update()
    {
        // Aggiorno i vari timer
        if (cooldownDanno > 0f)
        {
            cooldownDanno = cooldownDanno - Time.deltaTime;
        }
        if (TimerLampeggio > 0f)
        {
            TimerLampeggio = TimerLampeggio - Time.deltaTime;
        }
        if (TempoIniziale > 0f)
        {
            TempoIniziale = TempoIniziale - Time.deltaTime;
        }

        // Il tempo scorre solo se il livello e' "vivo"
        bool livelloVivo = !PartitaFinita && !MissioneCompletata
                           && !VittoriaFinale && TempoIniziale <= 0f;
        if (livelloVivo)
        {
            TempoRimasto = TempoRimasto - Time.deltaTime;
            if (TempoRimasto <= 0f)
            {
                TempoRimasto = 0f;
                TempoScaduto();
            }
        }
    }

    // ---- Caricamento livelli ----

    public void CaricaLivello(int indice)
    {
        // Sicurezza: l'indice deve essere valido
        LivelloCorrente = Mathf.Clamp(indice, 0, DefinizioneLivelli.Conteggio - 1);
        LivelloAttuale = DefinizioneLivelli.Ottieni(LivelloCorrente);

        // Azzero tutto
        CaramelleRaccolte = 0;
        MissioneCompletata = false;
        PartitaFinita = false;
        VittoriaFinale = false;
        MotivoSconfitta = "";
        ChiaveApparsa = false;
        ChiavePresa = false;
        TimerLampeggio = 0f;
        cooldownDanno = 0f;
        TempoIniziale = Impostazioni.TEMPO_PRONTI;
        Vite = Impostazioni.VITE;
        TempoRimasto = Impostazioni.TEMPO_LIVELLO;

        // Chiedo al caricatore di creare gli oggetti del livello
        TotaleCaramelle = CaricatoreLivelli.Carica(LivelloAttuale);
    }

    public bool ProssimoLivelloDisponibile
    {
        get { return (LivelloCorrente + 1) < DefinizioneLivelli.Conteggio; }
    }

    public void ProssimoLivello()
    {
        if (ProssimoLivelloDisponibile)
        {
            CaricaLivello(LivelloCorrente + 1);
        }
        else
        {
            RicominciaTutto();
        }
    }

    public void RicominciaTutto()
    {
        Punteggio = 0;
        CaricaLivello(0);
    }

    public void RipetiLivello()
    {
        CaricaLivello(LivelloCorrente);
    }

    // ---- Eventi che mi mandano gli altri script ----

    public void SegnalaCaramellaRaccolta()
    {
        if (PartitaFinita || MissioneCompletata || VittoriaFinale) return;

        CaramelleRaccolte = CaramelleRaccolte + 1;
        Punteggio = Punteggio + Impostazioni.PUNTI_CARAMELLA;

        // FEEDBACK: azione giusta -> suono gradevole + Astro si gonfia
        FeedbackPaziente.CaramellaPresa();

        // Se ho raccolto tutto passo alla fase chiave/porta,
        // oppure vinco se era l'ultimo livello
        if (CaramelleRaccolte >= TotaleCaramelle && !ChiaveApparsa && !VittoriaFinale)
        {
            if (!ProssimoLivelloDisponibile)
            {
                AttivaVittoriaFinale();
            }
            else
            {
                CreaChiaveEPorta();
            }
        }
    }

    public void SegnalaChiaveRaccolta()
    {
        if (!ChiaveApparsa) return;
        ChiavePresa = true;

        // FEEDBACK: hai preso la chiave -> suono brillante + Astro si gonfia
        FeedbackPaziente.ChiavePresa();
    }

    public void SegnalaPortaRaggiunta()
    {
        // La porta si apre solo se sto portando la chiave
        if (!ChiavePresa) return;
        if (MissioneCompletata) return;

        MissioneCompletata = true;

        // FEEDBACK: missione compiuta -> piccola fanfara + Astro si gonfia
        FeedbackPaziente.MissioneCompiuta();

        // Effetti di festa
        CaricatoreLivelli.GeneraPianetaAmico();
        CaricatoreLivelli.GeneraCoriandoli();
    }

    public void SegnalaAsteroideToccato()
    {
        if (PartitaFinita || MissioneCompletata) return;
        if (cooldownDanno > 0f) return;

        cooldownDanno = 1.0f;
        TimerLampeggio = 0.30f;

        // FEEDBACK: azione sbagliata -> suono basso/gentile + Astro si schiaccia
        FeedbackPaziente.AzioneSbagliata();

        Punteggio = Mathf.Max(0, Punteggio - Impostazioni.PUNTI_PERSI_HIT);
        PerdiUnaVita("Hai toccato un asteroide!");
    }

    public void SegnalaBombaColpita()
    {
        if (PartitaFinita || MissioneCompletata) return;
        if (cooldownDanno > 0f) return;

        cooldownDanno = 1.0f;
        TimerLampeggio = 0.30f;

        // FEEDBACK: azione sbagliata -> suono basso/gentile + Astro si schiaccia
        FeedbackPaziente.AzioneSbagliata();

        PerdiUnaVita("Hai toccato una bomba!");
    }

    // ---- Metodi interni ----

    void TempoScaduto()
    {
        if (PartitaFinita || MissioneCompletata) return;

        // FEEDBACK: tempo finito -> suono di errore + Astro si schiaccia
        FeedbackPaziente.AzioneSbagliata();

        // Il tempo scaduto fa perdere una vita; se ne ho ancora
        // faccio ripartire il timer
        PerdiUnaVita("Tempo scaduto!");
        if (!PartitaFinita)
        {
            TempoRimasto = Impostazioni.TEMPO_LIVELLO;
        }
    }

    void PerdiUnaVita(string motivo)
    {
        Vite = Mathf.Max(0, Vite - 1);
        if (Vite <= 0)
        {
            PartitaFinita = true;
            MotivoSconfitta = motivo;
        }
    }

    void CreaChiaveEPorta()
    {
        ChiaveApparsa = true;
        CaricatoreLivelli.GeneraChiave();
        CaricatoreLivelli.GeneraPorta();
    }

    void AttivaVittoriaFinale()
    {
        VittoriaFinale = true;

        // FEEDBACK: hai finito tutti i livelli -> fanfara di vittoria
        FeedbackPaziente.MissioneCompiuta();

        CaricatoreLivelli.GeneraCoriandoli();
        CaricatoreLivelli.GeneraPianetaAmico();
    }
}
