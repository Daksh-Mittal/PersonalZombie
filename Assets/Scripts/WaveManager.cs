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

    IEnumerator Start()
    {
        yield return null;
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {

        if (currentWave > maxWaves)
        {
            Debug.Log("Player Wins! Survived all waves. Resetting for ML training.");
        
            if (player != null)
            {
                PlayerMLBridge mlBridge = player.GetComponent<PlayerMLBridge>();
                if (mlBridge != null)
                {
                    mlBridge.AddReward(10.0f); 
                    mlBridge.EndEpisode();
                }
            }

            currentWave = 1; 
        }

        if (uiManager != null) uiManager.UpdateWaveUI(currentWave, maxWaves);

        Debug.Log("Starting Wave: " + currentWave);

        int dumbCount = 5 + (currentWave * 3); 
        int smartCount = 1 + (currentWave / 3); 

        MapGenerator mapGen = FindFirstObjectByType<MapGenerator>();
        float minX = float.MinValue, maxX = float.MaxValue;
        float minY = float.MinValue, maxY = float.MaxValue;

        if (mapGen != null)
        {
            float halfWidth = (mapGen.width * mapGen.tileSize) / 2f;
            float halfHeight = (mapGen.height * mapGen.tileSize) / 2f;

            float mapCenterX = mapGen.transform.position.x;
            float mapCenterY = mapGen.transform.position.y;

            float safetyBuffer = mapGen.tileSize * 2.5f + 1.2f;

            minX = mapCenterX - halfWidth + safetyBuffer;
            maxX = mapCenterX + halfWidth - safetyBuffer;
            minY = mapCenterY - halfHeight + safetyBuffer;
            maxY = mapCenterY + halfHeight - safetyBuffer;
        }

        Vector2 flockCenter = GetRandomSpawnPosition();
        for (int i = 0; i < dumbCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 1.2f;
            Vector2 spawnPos = flockCenter + offset;

            if (mapGen != null)
            {
                spawnPos.x = Mathf.Clamp(spawnPos.x, minX, maxX);
                spawnPos.y = Mathf.Clamp(spawnPos.y, minY, maxY);
            }
            
            GameObject dumbZ = Instantiate(dumbZombiePrefab, spawnPos, Quaternion.identity, zombieHoardParent);
            zombiesAlive++;
        }

        for (int i = 0; i < smartCount; i++)
        {
            Vector2 spawnPos = GetRandomSpawnPosition();

            if (mapGen != null)
            {
                spawnPos.x = Mathf.Clamp(spawnPos.x, minX, maxX);
                spawnPos.y = Mathf.Clamp(spawnPos.y, minY, maxY);
            }

            GameObject smartZ = Instantiate(smartZombiePrefab, spawnPos, Quaternion.identity);
            zombiesAlive++;
        }

        yield return null;
    }

    Vector2 GetRandomSpawnPosition()
    {
        MapGenerator mapGen = FindFirstObjectByType<MapGenerator>();
        
        if (mapGen != null && mapGen.validFloorPositions.Count > 0)
        {
            Vector2 safePos = Vector2.zero;
            float minimumDistanceFromPlayer = 5f;
            int attempts = 0;

            do
            {
                safePos = mapGen.GetRandomWalkablePosition();
                attempts++;
            } 
            while (Vector2.Distance(safePos, player.position) < minimumDistanceFromPlayer && attempts < 100);

            return safePos;
        }

        //fallback
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        return (Vector2)player.position + (randomDir * 4f);
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

    public void ResetWaves()
    {
        StopAllCoroutines();
        
        currentWave = 1;
        zombiesAlive = 0;
        gameActive = true;

        GameObject[] remainingZombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject zombie in remainingZombies)
        {
            Destroy(zombie);
        }


        StartCoroutine(StartWave());
    }


}