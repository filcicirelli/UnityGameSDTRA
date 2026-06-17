using UnityEngine;

// ============================================================================
// COMANDI: il punto unico che decide DOVE STA IL PUNTATORE
// ----------------------------------------------------------------------------
// Il gioco si puo' comandare in tre modi (scelti nella schermata iniziale):
//   - MOUSE     : il puntatore e' il mouse;
//   - DITO      : il puntatore e' la punta del dito vista dalla webcam
//                 (vedi la cartella "comando webcam");
//   - JOYSTICK  : un joystick (o le frecce / WASD) muovono il puntatore.
//
// Astro non sa quale modo e' attivo: chiede solo "dove sta il puntatore?"
// chiamando Comandi.PuntatoreSchermo(). Cosi' il resto del gioco non cambia.
//
// Questo oggetto si installa DA SOLO all'avvio (come FeedbackPaziente e
// ComandoWebcam), quindi non va trascinato in scena.
// ============================================================================
public class Comandi : MonoBehaviour
{
    // I tre modi di gioco
    public enum Modalita { Mouse, Dito, Joystick }

    // Il modo scelto adesso (parte dal mouse)
    public static Modalita Attuale = Modalita.Mouse;

    public static Comandi Istanza;

    // Posizione del puntatore quando si gioca col joystick (in pixel dello
    // schermo, stesso sistema di Input.mousePosition). Col joystick non ho una
    // posizione "assoluta" come il mouse: parto dal centro e mi sposto.
    private static Vector2 posizioneJoystick;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Installa()
    {
        if (Istanza != null) return;
        Istanza = new GameObject("Comandi").AddComponent<Comandi>();
    }

    void Awake()
    {
        Istanza = this;
        CentraPuntatore();
    }

    // La schermata iniziale chiama questo quando premi GIOCA.
    public static void Imposta(Modalita modalita)
    {
        Attuale = modalita;
        CentraPuntatore(); // col joystick si riparte sempre dal centro
    }

    static void CentraPuntatore()
    {
        posizioneJoystick = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    // ------------------------------------------------------------------------
    // METODO USATO DA ASTRO: dove sta il puntatore in questo fotogramma?
    // ------------------------------------------------------------------------
    public static Vector3 PuntatoreSchermo()
    {
        switch (Attuale)
        {
            case Modalita.Dito:
                // Se la webcam e' pronta uso il dito, altrimenti torno al mouse
                if (ComandoWebcam.Pronta) return ComandoWebcam.Posizione;
                return Input.mousePosition;

            case Modalita.Joystick:
                return new Vector3(posizioneJoystick.x, posizioneJoystick.y, 0f);

            default: // Mouse
                return Input.mousePosition;
        }
    }

    void Update()
    {
        // Solo col joystick devo aggiornare la posizione fotogramma per fotogramma
        if (Attuale == Modalita.Joystick)
        {
            MuoviConJoystick();
        }
    }

    // Leggo la leva (o le frecce / WASD) e sposto il puntatore di conseguenza.
    void MuoviConJoystick()
    {
        // Assi standard di Unity: vanno da -1 a +1
        float ax = Input.GetAxisRaw("Horizontal");
        float ay = Input.GetAxisRaw("Vertical");

        // Zona morta: ignoro i movimenti piccolissimi
        if (Mathf.Abs(ax) < ParametriComandi.JOYSTICK_ZONA_MORTA) ax = 0f;
        if (Mathf.Abs(ay) < ParametriComandi.JOYSTICK_ZONA_MORTA) ay = 0f;
        if (ParametriComandi.JOYSTICK_INVERTI_Y) ay = -ay;

        // Mi muovo "a velocita'": piu' tengo la leva, piu' vado avanti
        float passo = ParametriComandi.JOYSTICK_VELOCITA * Time.deltaTime;
        posizioneJoystick.x += ax * passo;
        posizioneJoystick.y += ay * passo;

        // Resto sempre dentro lo schermo
        posizioneJoystick.x = Mathf.Clamp(posizioneJoystick.x, 0f, Screen.width);
        posizioneJoystick.y = Mathf.Clamp(posizioneJoystick.y, 0f, Screen.height);
    }
}
