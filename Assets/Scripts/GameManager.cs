using System.Collections.Generic;
using UnityEngine;

// "Direttore d'orchestra" del gioco:
// - tiene lo stato (punteggio, livello corrente, oggetti rimasti, tempo)
// - reagisce ai drop chiamati da Draggable
// - passa al livello successivo quando il livello e' completato
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score          { get; private set; }
    public int CurrentLevel   { get; private set; }
    public int ItemsRemaining { get; private set; }
    public float TimeLeft     { get; private set; } // 0 se livello senza timer
    public bool GameOver      { get; private set; }
    public bool GameComplete  { get; private set; }

    private List<LevelData> levels;

    void Awake()
    {
        Instance = this;
        levels = LevelDefinitions.BuildAll();
    }

    void Start()
    {
        LoadLevel(0);
    }

    void Update()
    {
        if (GameOver || GameComplete) return;
        if (TimeLeft > 0f)
        {
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                GameOver = true;
            }
        }
    }

    public void LoadLevel(int index)
    {
        if (index >= levels.Count)
        {
            GameComplete = true;
            LevelLoader.Clear();
            return;
        }
        CurrentLevel    = index;
        ItemsRemaining  = levels[index].items.Count;
        TimeLeft        = levels[index].timeLimit;
        GameOver        = false;
        LevelLoader.Load(levels[index]);
    }

    public void OnCorrectDrop()
    {
        Score += 10;
        ItemsRemaining--;
        if (ItemsRemaining <= 0)
            LoadLevel(CurrentLevel + 1);
    }

    public void OnWrongDrop()
    {
        Score = Mathf.Max(0, Score - 5);
    }

    public void Restart()
    {
        Score = 0;
        GameComplete = false;
        LoadLevel(0);
    }

    public string CurrentTitle =>
        (CurrentLevel < levels.Count) ? levels[CurrentLevel].title : "";
}
