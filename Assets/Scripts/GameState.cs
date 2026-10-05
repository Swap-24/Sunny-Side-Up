using UnityEngine;

public class GameState : MonoBehaviour
{
    private static GameState instance;

    public static GameState Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameState>();

                if (instance == null)
                {
                    GameObject go = new GameObject("GameState");
                    instance = go.AddComponent<GameState>();
                }
            }

            return instance;
        }
    }

    public bool HasReadJournal { get; private set; }
    public string TargetSpawnPoint { get; private set; }

    private void Awake()
    {
        // Make sure there is only one GameState
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Keep GameState when changing scenes
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void MarkJournalAsRead()
    {
        HasReadJournal = true;
    }

    public void SetTargetSpawnPoint(string spawnPointID)
    {
        TargetSpawnPoint = spawnPointID;
    }

    public void ClearTargetSpawnPoint()
    {
        TargetSpawnPoint = null;
    }
}