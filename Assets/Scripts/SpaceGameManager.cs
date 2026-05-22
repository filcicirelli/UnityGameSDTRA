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
    // Vittoria finale: settata quando si completa l'ultimo livello.
    // In quel caso si salta la fase chiave/porta e si mostra la schermata
    // di fine gioco con i crediti.
    public bool Victory          { get; private set; }
    public float CelebrationTime { get; private set; }

    // Vite del giocatore (resettate a ogni livello)
    public const int MaxLives = 3;
    public int Lives { get; private set; }

    // Tempo a disposizione per completare il livello
    public const float LevelDuration = 60f;
    public float LevelTimeLeft { get; private set; }

    // Motivo dell'ultimo Game Over (mostrato nell'HUD)
    public string GameOverReason { get; private set; }

    // Fase chiave/porta
    public bool KeySpawned     { get; private set; }
    public bool KeyPickedUp    { get; private set; }

    // Flash di penalita' (barriera)
    public float BarrierFlashTimer { get; private set; }

    // Grace period a inizio livello (bombe inerti, banner "PRONTI...")
    public float LevelStartGrace { get; private set; }
    public bool  BombsArmed => LevelStartGrace <= 0f;

    // Periodo di invulnerabilita' breve dopo ogni hit (barriera/bomba),
    // per non drenare tutte le vite in un solo contatto prolungato.
    private float hitCooldown;

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
        if (hitCooldown > 0f)        hitCooldown        -= Time.deltaTime;
        if (BarrierFlashTimer > 0f)  BarrierFlashTimer  -= Time.deltaTime;
        if (LevelStartGrace > 0f)    LevelStartGrace    -= Time.deltaTime;

        // Countdown del tempo solo quando il livello e' "vivo"
        // (no grace period iniziale, no game over, no missione completata, no vittoria finale)
        if (!GameOver && !MissionComplete && !Victory && LevelStartGrace <= 0f)
        {
            LevelTimeLeft -= Time.deltaTime;
            if (LevelTimeLeft <= 0f)
            {
                LevelTimeLeft = 0f;
                OnTimeOut();
            }
        }
    }

    // -------- Gestione livelli --------

    public void LoadLevel(int index)
    {
        CurrentLevel = Mathf.Clamp(index, 0, SpaceLevels.Count - 1);
        CurrentDef = SpaceLevels.Get(CurrentLevel);
        CandiesCollected = 0;
        MissionComplete = false;
        GameOver = false;
        Victory = false;
        GameOverReason = "";
        KeySpawned = false;
        KeyPickedUp = false;
        CelebrationTime = 0f;
        BarrierFlashTimer = 0f;
        hitCooldown = 0f;
        LevelStartGrace = 1.5f;
        Lives = MaxLives;
        LevelTimeLeft = LevelDuration;
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
        if (GameOver || MissionComplete || Victory) return;
        CandiesCollected++;
        Score += 10;

        // Tutte raccolte:
        //   - sull'ultimo livello => vittoria finale (schermata credits)
        //   - sugli altri livelli  => spawna chiave + porta per la fase finale
        if (CandiesCollected >= TotalCandies && !KeySpawned && !Victory)
        {
            if (!HasNextLevel) TriggerFinalVictory();
            else SpawnKeyAndDoor();
        }
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
        if (hitCooldown > 0f) return;
        hitCooldown = 1.0f;
        BarrierFlashTimer = 0.30f;
        Score = Mathf.Max(0, Score - 5);
        LoseLife("Hai toccato un asteroide!");
    }

    public void OnBombHit()
    {
        if (GameOver || MissionComplete) return;
        if (hitCooldown > 0f) return;
        hitCooldown = 1.0f;
        BarrierFlashTimer = 0.30f;
        LoseLife("Hai toccato una bomba!");
    }

    void OnTimeOut()
    {
        if (GameOver || MissionComplete) return;
        // Penalita' per tempo scaduto: perde una vita e (se restano vite)
        // ricomincia il timer per dare un'altra possibilita'.
        LoseLife("Tempo scaduto!");
        if (!GameOver) LevelTimeLeft = LevelDuration;
    }

    void LoseLife(string reason)
    {
        Lives = Mathf.Max(0, Lives - 1);
        if (Lives <= 0)
        {
            GameOver = true;
            GameOverReason = reason;
        }
    }

    void SpawnKeyAndDoor()
    {
        KeySpawned = true;
        SpaceLevelLoader.SpawnKey();
        SpaceLevelLoader.SpawnDoor();
    }

    void TriggerFinalVictory()
    {
        Victory = true;
        CelebrationTime = 0f;
        SpaceLevelLoader.SpawnConfetti();
        SpaceLevelLoader.SpawnFriendPlanet();
    }
}
