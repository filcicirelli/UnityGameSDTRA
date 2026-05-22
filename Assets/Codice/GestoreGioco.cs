using UnityEngine;

// =====================================================================
//  GestoreGioco
// ---------------------------------------------------------------------
//  E' il "cervello" del gioco: tiene lo stato globale della partita
//  (punteggio, vite, tempo, livello corrente) e decide cosa succede
//  quando il giocatore raccoglie una caramella, prende la chiave,
//  tocca un asteroide o una bomba.
//
//  Ho organizzato il livello in tre fasi:
//      Fase 1 - Raccolta:   raccogli tutte le caramelle
//      Fase 2 - Chiave:     prendi la chiave che appare al centro
//      Fase 3 - Porta:      porta la chiave alla porta che si sposta
//
//  Uso il pattern "Singleton" (proprieta' statica Istanza) cosi' gli
//  altri script possono chiamare GestoreGioco.Istanza senza doverlo
//  cercare ogni volta nella scena.
// =====================================================================
public class GestoreGioco : MonoBehaviour
{
    // ----- Singleton -----
    public static GestoreGioco Istanza { get; private set; }

    // ----- Stato della partita -----
    public int LivelloCorrente      { get; private set; }
    public int CaramelleRaccolte    { get; private set; }
    public int Punteggio            { get; private set; }
    public int TotaleCaramelle      { get; private set; }
    public bool MissioneCompletata  { get; private set; }
    public bool PartitaFinita       { get; private set; }
    public bool VittoriaFinale      { get; private set; }
    public float TempoFesta         { get; private set; }

    // ----- Vite del giocatore -----
    // Ad ogni livello ricomincio da 3 vite: e' un gioco per bambini in
    // riabilitazione, quindi voglio che possano sbagliare qualche volta
    // senza perdere subito la partita.
    public const int ViteMassime = 3;
    public int Vite { get; private set; }

    // ----- Tempo limite del livello -----
    // 60 secondi: abbastanza per esplorare, ma serve un po' di fretta
    // per stimolare la concentrazione (obiettivo riabilitativo).
    public const float DurataLivello = 60f;
    public float TempoRimasto { get; private set; }

    // Motivo dell'ultima sconfitta (lo mostro nel pannello di Game Over)
    public string MotivoSconfitta { get; private set; }

    // ----- Fase finale: chiave + porta -----
    public bool ChiaveApparsa { get; private set; }
    public bool ChiavePresa   { get; private set; }

    // Lampeggio rosso quando si tocca un asteroide
    public float TimerLampeggio { get; private set; }

    // Periodo di grazia iniziale ("PRONTI..."): bombe inerti per un attimo
    public float TempoIniziale { get; private set; }
    public bool BombeAttive
    {
        get { return TempoIniziale <= 0f; }
    }

    // Piccolo cooldown dopo ogni hit per non perdere tutte le vite
    // restando incollati ad un asteroide.
    private float cooldownDanno;

    // Definizione del livello che sto giocando in questo momento
    public DatiLivello LivelloAttuale { get; private set; }

    // Energia = percentuale di caramelle raccolte. La uso per la barra in HUD.
    public float Energia
    {
        get
        {
            if (TotaleCaramelle == 0) return 0f;
            return (float)CaramelleRaccolte / (float)TotaleCaramelle;
        }
    }

    // Testo dell'obiettivo: cambia a seconda della fase del livello.
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

    // -----------------------------------------------------------------
    //  Ciclo di vita
    // -----------------------------------------------------------------

    void Awake()
    {
        Istanza = this;
    }

    void Start()
    {
        // Inizio sempre dal primo livello
        CaricaLivello(0);
    }

    void Update()
    {
        // Aggiorno i vari timer "che scendono"
        if (MissioneCompletata)
        {
            TempoFesta += Time.deltaTime;
        }
        if (cooldownDanno > 0f)
        {
            cooldownDanno -= Time.deltaTime;
        }
        if (TimerLampeggio > 0f)
        {
            TimerLampeggio -= Time.deltaTime;
        }
        if (TempoIniziale > 0f)
        {
            TempoIniziale -= Time.deltaTime;
        }

        // Il timer del livello scorre solo quando il gioco e' "vivo":
        // niente countdown durante il PRONTI iniziale, ne' dopo la vittoria
        // o un game over.
        bool livelloVivo = !PartitaFinita && !MissioneCompletata
                           && !VittoriaFinale && TempoIniziale <= 0f;
        if (livelloVivo)
        {
            TempoRimasto -= Time.deltaTime;
            if (TempoRimasto <= 0f)
            {
                TempoRimasto = 0f;
                GestisciTempoScaduto();
            }
        }
    }

    // -----------------------------------------------------------------
    //  Gestione dei livelli
    // -----------------------------------------------------------------

    public void CaricaLivello(int indice)
    {
        // Mi assicuro che l'indice sia valido
        LivelloCorrente = Mathf.Clamp(indice, 0, DefinizioneLivelli.Conteggio - 1);
        LivelloAttuale = DefinizioneLivelli.Ottieni(LivelloCorrente);

        // Azzero tutto lo stato di livello
        CaramelleRaccolte = 0;
        MissioneCompletata = false;
        PartitaFinita = false;
        VittoriaFinale = false;
        MotivoSconfitta = "";
        ChiaveApparsa = false;
        ChiavePresa = false;
        TempoFesta = 0f;
        TimerLampeggio = 0f;
        cooldownDanno = 0f;
        TempoIniziale = 1.5f;
        Vite = ViteMassime;
        TempoRimasto = DurataLivello;

        // Chiedo al caricatore di costruire la scena del livello
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

    // -----------------------------------------------------------------
    //  Eventi che arrivano dal gameplay
    // -----------------------------------------------------------------

    public void SegnalaCaramellaRaccolta()
    {
        // Se la partita non e' piu' "viva", ignoro
        if (PartitaFinita || MissioneCompletata || VittoriaFinale) return;

        CaramelleRaccolte = CaramelleRaccolte + 1;
        Punteggio = Punteggio + 10;

        // Se ho raccolto tutto, passo alla fase finale (chiave + porta)
        // oppure scateno la vittoria definitiva se ero all'ultimo livello.
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
    }

    public void SegnalaPortaRaggiunta()
    {
        // La porta si apre solo se sto trasportando la chiave
        if (!ChiavePresa) return;
        if (MissioneCompletata) return;

        MissioneCompletata = true;
        TempoFesta = 0f;

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
        Punteggio = Mathf.Max(0, Punteggio - 5);
        PerdiUnaVita("Hai toccato un asteroide!");
    }

    public void SegnalaBombaColpita()
    {
        if (PartitaFinita || MissioneCompletata) return;
        if (cooldownDanno > 0f) return;

        cooldownDanno = 1.0f;
        TimerLampeggio = 0.30f;
        PerdiUnaVita("Hai toccato una bomba!");
    }

    // -----------------------------------------------------------------
    //  Metodi interni di supporto
    // -----------------------------------------------------------------

    void GestisciTempoScaduto()
    {
        if (PartitaFinita || MissioneCompletata) return;

        // Il tempo finito costa una vita. Se restano vite, faccio
        // ripartire il timer per dare un'altra possibilita'.
        PerdiUnaVita("Tempo scaduto!");
        if (!PartitaFinita)
        {
            TempoRimasto = DurataLivello;
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
        TempoFesta = 0f;
        CaricatoreLivelli.GeneraCoriandoli();
        CaricatoreLivelli.GeneraPianetaAmico();
    }
}
