using UnityEngine;

// "Direttore d'orchestra" di Missione Spaziale.
// Tiene lo stato globale e gestisce le tre fasi di ciascun livello:
//   1) Raccolta caramelle    (raccogli tutte le caramelle)
//   2) Prendi la chiave      (la chiave appare al centro)
//   3) Porta la chiave alla porta che si sposta ogni 3 secondi
public class SpaceGameManager : MonoBehaviour
{
    public static SpaceGameManager Instance { get; private set; }

    // Stato corrente
    public int CurrentLevel      { get; private set; }
    public int CandiesCollected  { get; private set; }
    public int Score             { get; private set; }
    public int TotalCandies      { get; private set; }
    public bool MissionComplete  { get; private set; }
    public bool GameOver         { get; private set; }
    public float CelebrationTime { get; private set; }

    // Fase chiave/porta
    public bool KeySpawned     { get; private set; }
    public bool KeyPickedUp    { get; private set; }

    // Flash di penalita' (barriera)
    public float BarrierFlashTimer { get; private set; }

    // Grace period a inizio livello (bombe inerti, banner "PRONTI...")
    public float LevelStartGrace { get; private set; }
    public bool  BombsArmed => LevelStartGrace <= 0f;

    private float barrierCooldown;

    // Definizione del livello corrente
    public SpaceLevelDef CurrentDef { get; private set; }

    public float Energy => (TotalCandies == 0) ? 0f : (float)CandiesCollected / TotalCandies;

    // Obiettivo dinamico mostrato sul distintivo in basso a sinistra
    public string ObjectiveText
    {
        get
        {
            if (KeySpawned && !KeyPickedUp) return "PRENDI LA CHIAVE";
            if (KeyPickedUp && !MissionComplete) return "PORTA LA CHIAVE ALLA PORTA";
            return CurrentDef != null ? CurrentDef.objective : "";
        }
    }

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
        CandiesCollected = 0;
        MissionComplete = false;
        GameOver = false;
        KeySpawned = false;
        KeyPickedUp = false;
        CelebrationTime = 0f;
        BarrierFlashTimer = 0f;
        barrierCooldown = 0f;
        LevelStartGrace = 1.5f;
        TotalCandies = SpaceLevelLoader.Load(CurrentDef);
    }

    public bool HasNextLevel => CurrentLevel + 1 < SpaceLevels.Count;
    public void NextLevel()
    {
        if (HasNextLevel) LoadLevel(CurrentLevel + 1);
        else Restart();
    }

    public void Restart()       { Score = 0; LoadLevel(0); }
    public void RetryLevel()    { LoadLevel(CurrentLevel); }

    // -------- Eventi dal gameplay --------

    public void OnCandyCollected()
    {
        if (GameOver || MissionComplete) return;
        CandiesCollected++;
        Score += 10;

        // Tutte raccolte: spawna chiave + porta. Niente "missione complete"
        // ancora: serve raggiungere la porta con la chiave.
        if (CandiesCollected >= TotalCandies && !KeySpawned)
            SpawnKeyAndDoor();
    }

    public void OnKeyPickedUp()
    {
        if (!KeySpawned) return;
        KeyPickedUp = true;
    }

    public void OnDoorReached()
    {
        if (!KeyPickedUp || MissionComplete) return;
        MissionComplete = true;
        CelebrationTime = 0f;
        SpaceLevelLoader.SpawnFriendPlanet();
        SpaceLevelLoader.SpawnConfetti();
    }

    public void OnBarrierTouched()
    {
        if (GameOver || MissionComplete) return;
        if (barrierCooldown > 0f) return;
        barrierCooldown = 0.5f;
        BarrierFlashTimer = 0.25f;
        Score = Mathf.Max(0, Score - 5);
    }

    public void OnBombHit()
    {
        if (GameOver) return;
        GameOver = true;
    }

    void SpawnKeyAndDoor()
    {
        KeySpawned = true;
        SpaceLevelLoader.SpawnKey();
        SpaceLevelLoader.SpawnDoor();
    }
}
