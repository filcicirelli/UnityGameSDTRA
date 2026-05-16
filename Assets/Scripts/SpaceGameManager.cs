using UnityEngine;

// "Direttore d'orchestra" di Missione Spaziale.
// Tiene lo stato globale (livello, cristalli raccolti, punti, energia, esiti)
// e reagisce agli eventi (cristallo aspirato, barriera toccata, bomba).
public class SpaceGameManager : MonoBehaviour
{
    public static SpaceGameManager Instance { get; private set; }

    // Stato corrente
    public int CurrentLevel      { get; private set; }
    public int CrystalsCollected { get; private set; }
    public int Score             { get; private set; }
    public int TotalCrystals     { get; private set; }
    public bool MissionComplete  { get; private set; }
    public bool GameOver         { get; private set; }
    public float CelebrationTime { get; private set; }

    // Penalita' barriera (mostrata per qualche frame come flash rosso)
    public float BarrierFlashTimer { get; private set; }

    // Grace period a inizio livello: le bombe restano "disarmate" cosi' il
    // giocatore puo' vedere la mappa prima di rischiare un GAME OVER.
    public float LevelStartGrace { get; private set; }
    public bool  BombsArmed => LevelStartGrace <= 0f;

    // Cooldown: dopo un colpo a una barriera, ignora altri colpi per un po'
    private float barrierCooldown;

    // Definizione del livello corrente, per accesso a obiettivo/titolo dall'UI
    public SpaceLevelDef CurrentDef { get; private set; }

    public float Energy => (TotalCrystals == 0) ? 0f : (float)CrystalsCollected / TotalCrystals;

    void Awake() { Instance = this; }
    void Start() { LoadLevel(0); }

    void Update()
    {
        if (MissionComplete) CelebrationTime += Time.deltaTime;
        if (barrierCooldown > 0f)    barrierCooldown    -= Time.deltaTime;
        if (BarrierFlashTimer > 0f)  BarrierFlashTimer  -= Time.deltaTime;
        if (LevelStartGrace > 0f)    LevelStartGrace    -= Time.deltaTime;
    }

    // -------- Gestione livelli --------

    public void LoadLevel(int index)
    {
        CurrentLevel = Mathf.Clamp(index, 0, SpaceLevels.Count - 1);
        CurrentDef = SpaceLevels.Get(CurrentLevel);
        CrystalsCollected = 0;
        MissionComplete = false;
        GameOver = false;
        CelebrationTime = 0f;
        BarrierFlashTimer = 0f;
        barrierCooldown = 0f;
        LevelStartGrace = 1.5f; // mezzo secondo + un secondo: le bombe sono inerti
        TotalCrystals = SpaceLevelLoader.Load(CurrentDef);
    }

    public void NextLevel()
    {
        if (CurrentLevel + 1 < SpaceLevels.Count) LoadLevel(CurrentLevel + 1);
        else Restart(); // dopo l'ultimo livello, ricomincia dal primo
    }

    public bool HasNextLevel => CurrentLevel + 1 < SpaceLevels.Count;

    // Reset completo: torna al livello 1 azzerando i punti.
    public void Restart()
    {
        Score = 0;
        LoadLevel(0);
    }

    // Riavvia solo il livello corrente senza azzerare i punti totali.
    public void RetryLevel() { LoadLevel(CurrentLevel); }

    // -------- Eventi dal gameplay --------

    public void OnCrystalCollected()
    {
        if (GameOver) return;
        CrystalsCollected++;
        Score += 10;
        if (CrystalsCollected >= TotalCrystals && !MissionComplete)
            CompleteMission();
    }

    public void OnBarrierTouched()
    {
        if (GameOver || MissionComplete) return;
        if (barrierCooldown > 0f) return; // gia' segnato di recente
        barrierCooldown = 0.5f;
        BarrierFlashTimer = 0.25f;
        Score = Mathf.Max(0, Score - 5);
    }

    public void OnBombHit()
    {
        if (GameOver) return;
        GameOver = true;
    }

    void CompleteMission()
    {
        MissionComplete = true;
        CelebrationTime = 0f;
        SpaceLevelLoader.SpawnFriendPlanet();
        SpaceLevelLoader.SpawnConfetti();
    }
}
