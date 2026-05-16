using UnityEngine;

// "Direttore d'orchestra" di Missione Spaziale.
// Tiene lo stato (cristalli raccolti, punti, energia, missione)
// e reagisce agli eventi del giocatore (cristallo aspirato).
public class SpaceGameManager : MonoBehaviour
{
    public static SpaceGameManager Instance { get; private set; }

    // Stato corrente
    public int CrystalsCollected { get; private set; }
    public int Score             { get; private set; }
    public int TotalCrystals     { get; private set; }
    public bool MissionComplete  { get; private set; }
    public float CelebrationTime { get; private set; } // tempo trascorso dal completamento

    // Energia 0..1 (riempie la barra "ENERGIA NAVICELLA")
    public float Energy => (TotalCrystals == 0) ? 0f : (float)CrystalsCollected / TotalCrystals;

    // Obiettivo del livello (per il distintivo in basso a sinistra)
    public string ObjectiveText => "RACCOGLI " + TotalCrystals + " CRISTALLI";

    void Awake() { Instance = this; }

    void Start() { StartMission(); }

    void Update()
    {
        if (MissionComplete) CelebrationTime += Time.deltaTime;
    }

    public void StartMission()
    {
        CrystalsCollected = 0;
        Score = 0;
        MissionComplete = false;
        CelebrationTime = 0f;
        TotalCrystals = SpaceLevelLoader.Load();
    }

    // Chiamato da Crystal quando viene "aspirato" dalla navicella.
    public void OnCrystalCollected()
    {
        CrystalsCollected++;
        Score += 10;
        if (CrystalsCollected >= TotalCrystals && !MissionComplete)
            CompleteMission();
    }

    void CompleteMission()
    {
        MissionComplete = true;
        CelebrationTime = 0f;
        // Festa: Pianeta Amico al centro e coriandoli
        SpaceLevelLoader.SpawnFriendPlanet();
        SpaceLevelLoader.SpawnConfetti();
    }

    public void Restart() { StartMission(); }
}
