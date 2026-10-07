using System;
using System.Collections.Generic;
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
                instance = FindAnyObjectByType<GameState>();

                if (instance == null)
                {
                    GameObject go = new GameObject("GameState");
                    instance = go.AddComponent<GameState>();
                }
            }

            return instance;
        }
    }

    public bool HasReadJournal => HasFlag("ReadJournal");
    public string TargetSpawnPoint { get; private set; }

    private readonly HashSet<string> flags = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> interactedObjectIDs = new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ==========================================
    // GENERIC GAME FLAGS (MODULAR QUEST & STORY)
    // ==========================================

    public bool HasFlag(string flagName)
    {
        return !string.IsNullOrEmpty(flagName) && flags.Contains(flagName);
    }

    public void SetFlag(string flagName, bool value = true)
    {
        if (string.IsNullOrEmpty(flagName))
            return;

        if (value)
        {
            flags.Add(flagName);
        }
        else
        {
            flags.Remove(flagName);
        }
    }

    public void ClearFlag(string flagName)
    {
        if (!string.IsNullOrEmpty(flagName))
        {
            flags.Remove(flagName);
        }
    }

    // Convenience wrapper for journal
    public void MarkJournalAsRead()
    {
        SetFlag("ReadJournal", true);
    }

    // ==========================================
    // SPAWN POINT POSITIONING
    // ==========================================

    public void SetTargetSpawnPoint(string spawnPointID)
    {
        TargetSpawnPoint = spawnPointID;
    }

    public void ClearTargetSpawnPoint()
    {
        TargetSpawnPoint = null;
    }

    // ==========================================
    // INTERACTION PERSISTENCE
    // ==========================================

    public bool HasInteractedWith(string id)
    {
        return !string.IsNullOrEmpty(id) && interactedObjectIDs.Contains(id);
    }

    public void RecordInteraction(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            interactedObjectIDs.Add(id);
        }
    }
}