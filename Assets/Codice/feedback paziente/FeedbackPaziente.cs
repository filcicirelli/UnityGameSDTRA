using UnityEngine;

// ============================================================================
// FEEDBACK PER IL PAZIENTE  (il "cervello" del feedback)
// ----------------------------------------------------------------------------
// Questo oggetto da' il "premio" o il "no" al paziente:
//   - AZIONE GIUSTA    -> suono gradevole + Astro che si GONFIA (e brilla)
//   - AZIONE SBAGLIATA -> suono basso/sgradevole + Astro che si SCHIACCIA
//
// PERCHE' SERVE, nella riabilitazione:
// un feedback immediato e chiaro (orecchio + occhio insieme) aiuta il paziente
// a capire SUBITO se il movimento e' corretto, e quindi a migliorare prima.
//
// SI INSTALLA DA SOLO: non serve trascinarlo nella scena in Unity.
// Gli altri file lo chiamano con i metodi statici qui sotto, per esempio:
//     FeedbackPaziente.CaramellaPresa();
//     FeedbackPaziente.MissioneCompiuta();
//     FeedbackPaziente.AzioneSbagliata();
//
// I numeri che regolano tutto stanno in ParametriFeedback.cs (la "pagina").
// I suoni sono file audio gratuiti (CC0) dentro Assets/Resources.
// ============================================================================
public class FeedbackPaziente : MonoBehaviour
{
    // Riferimento unico, cosi' gli altri script lo trovano facilmente
    public static FeedbackPaziente Istanza;

    // L'altoparlante del gioco
    private AudioSource sorgente;

    // I tre suoni, creati una sola volta all'avvio
    private AudioClip suonoCaramella;
    private AudioClip suonoVittoria;
    private AudioClip suonoErrore;

    // ---- Installazione automatica (parte da sola insieme al gioco) ----
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Installa()
    {
        if (Istanza == null)
        {
            new GameObject("FeedbackPaziente").AddComponent<FeedbackPaziente>();
        }
    }

    void Awake()
    {
        Istanza = this;

        // Servono delle "orecchie" (un AudioListener) per sentire i suoni.
        // La telecamera di default (con le sue orecchie) viene distrutta e
        // ricreata senza. Per essere SEMPRE sicuri di averne esattamente una,
        // tolgo quelle che trovo e ne metto una su questo oggetto (che resta
        // vivo per tutta la partita). Cosi' non dipendo dall'ordine di avvio.
        AudioListener[] orecchieVecchie =
            Object.FindObjectsByType<AudioListener>();
        for (int i = 0; i < orecchieVecchie.Length; i++)
        {
            Object.Destroy(orecchieVecchie[i]);
        }
        gameObject.AddComponent<AudioListener>();

        // Altoparlante 2D (il suono non dipende dalla posizione di Astro)
        sorgente = gameObject.AddComponent<AudioSource>();
        sorgente.playOnAwake = false;
        sorgente.spatialBlend = 0f;

        // Carico i suoni veri (file audio) dalla cartella Assets/Resources.
        // I nomi dei file stanno in ParametriFeedback (la "pagina parametri").
        // Se un file manca, il clip resta null: il gioco NON si blocca,
        // semplicemente quel feedback sonoro non parte (vedi metodo Suona).
        suonoCaramella = Resources.Load<AudioClip>(ParametriFeedback.SUONO_RACCOLTA);
        suonoVittoria  = Resources.Load<AudioClip>(ParametriFeedback.SUONO_VITTORIA);
        suonoErrore    = Resources.Load<AudioClip>(ParametriFeedback.SUONO_ERRORE);
        if (suonoCaramella == null || suonoVittoria == null || suonoErrore == null)
        {
            Debug.LogWarning("FeedbackPaziente: manca un file audio in Assets/Resources " +
                             "(servono: raccolta, vittoria, errore).");
        }
    }

    void OnDestroy()
    {
        if (Istanza == this)
        {
            Istanza = null;
        }
    }

    // ========================================================================
    // METODI PUBBLICI (li chiamano gli altri file del gioco).
    // Sono statici e controllano da soli che il feedback esista, cosi' chi li
    // chiama scrive una riga sola, p.es.  FeedbackPaziente.CaramellaPresa();
    // ========================================================================

    public static void CaramellaPresa()   { if (Istanza != null) Istanza.Giusto(Istanza.suonoCaramella); }
    public static void MissioneCompiuta()  { if (Istanza != null) Istanza.Giusto(Istanza.suonoVittoria); }
    public static void AzioneSbagliata()   { if (Istanza != null) Istanza.Sbagliato(); }

    // ---- Logica interna ----

    // Azione GIUSTA: suono gradevole + Astro si gonfia e brilla
    void Giusto(AudioClip suono)
    {
        if (ParametriFeedback.SUONO_ATTIVO)
        {
            Suona(suono, ParametriFeedback.VOLUME_GIUSTO);
        }
        if (ParametriFeedback.VISIVO_ATTIVO && Astro.Istanza != null)
        {
            Astro.Istanza.Gonfia();
        }
    }

    // Azione SBAGLIATA: suono basso/sgradevole + Astro si schiaccia
    void Sbagliato()
    {
        if (ParametriFeedback.SUONO_ATTIVO)
        {
            Suona(suonoErrore, ParametriFeedback.VOLUME_SBAGLIATO);
        }
        if (ParametriFeedback.VISIVO_ATTIVO && Astro.Istanza != null)
        {
            Astro.Istanza.Schiaccia();
        }
    }

    void Suona(AudioClip clip, float volume)
    {
        if (clip == null || sorgente == null) return;
        // PlayOneShot permette ai suoni di sovrapporsi senza tagliarsi a vicenda
        sorgente.PlayOneShot(clip, volume * ParametriFeedback.VOLUME_GENERALE);
    }
}
