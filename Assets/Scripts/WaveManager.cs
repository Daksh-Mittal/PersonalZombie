using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("UI")]
    public UIManager uiManager;

    [Header("Prefabs")]
    public GameObject dumbZombiePrefab;
    public GameObject smartZombiePrefab;
    
    [Header("Spawn Settings")]
    public Transform zombieHoardParent; 
    public Transform player;
    
    [Header("Wave Stats")]
    public int currentWave = 1;
    public int maxWaves = 10;
    private int zombiesAlive = 0;
    private bool gameActive = true;

    void Start()
    {
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        if (currentWave > maxWaves)
        {
            Debug.Log("Player Wins! Survived 10 waves.");
            gameActive = false;
            
            if (uiManager != null) uiManager.ShowWinScreen(); 
            
            yield break;
        }

        Debug.Log("Starting Wave: " + currentWave);

        // Calculate zombie counts: Lots of dumb zombies, very few smart ones
        int dumbCount = 5 + (currentWave * 3); 
        int smartCount = 1 + (currentWave / 3); 

        // 1. Spawn Dumb Zombies in a "Flock"
        Vector2 flockCenter = GetRandomSpawnPosition();
        for (int i = 0; i < dumbCount; i++)
        {
            Vector2 spawnPos = flockCenter + Random.insideUnitCircle * 2.5f; 
            GameObject dumbZ = Instantiate(dumbZombiePrefab, spawnPos, Quaternion.identity, zombieHoardParent);
                        
            zombiesAlive++;
        }

        // 2. Spawn Smart Zombies scattered individually
        for (int i = 0; i < smartCount; i++)
        {
            Vector2 spawnPos = GetRandomSpawnPosition();
            GameObject smartZ = Instantiate(smartZombiePrefab, spawnPos, Quaternion.identity);
            zombiesAlive++;
        }

        yield return null;
    }

    Vector2 GetRandomSpawnPosition()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        return (Vector2)player.position + (randomDir * 4f); // Spawns 12 units away
    }

    public void OnZombieDied()
    {
        zombiesAlive--;
        
        // Safely check if the player exists and has the PlayerHealth script active
        if (player != null)
        {
            PlayerHealth healthScript = player.GetComponent<PlayerHealth>();
            if (healthScript != null)
            {
                healthScript.AddKill();
            }
        }

        if (zombiesAlive <= 0 && gameActive)
        {
            currentWave++;
            StartCoroutine(StartWave());
        }
    }
}