using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Grid Dimensions")]
    public int width = 30;
    public int height = 20;
    public float tileSize = 1f;

    [Header("Tile Prefab Arrays")]
    public GameObject[] floorPrefabs;
    public GameObject[] wallPrefabs;
    public GameObject[] borderPrefabs;

    [Header("Generation Settings")]
    [Range(0f, 1f)] public float obstacleDensity = 0.2f;
    public int seed;
    public bool useRandomSeed = true;

    [Header("Hazard Settings")]
    public GameObject[] hazardPrefabs;
    [Range(0f, 0.1f)] public float hazardDensity = 0.03f;

    [Header("Obstacle Spacing")]
    public int obstacleClearance = 2;

    private int[,] mapData;
    
    [HideInInspector] public List<Vector2> validFloorPositions = new List<Vector2>();

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()   
    {
        if (useRandomSeed) seed = Random.Range(0, 10000);
        Random.InitState(seed);

        mapData = new int[width, height];
        validFloorPositions.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    mapData[x, y] = 2; // Border
                }
                else
                {
                    bool isCenterZone = (x > width / 2 - 4 && x < width / 2 + 4 && y > height / 2 - 4 && y < height / 2 + 4);
                    
                    if (isCenterZone)
                    {
                        mapData[x, y] = 0; 
                    }
                    else
                    {
                        float spawnRoll = Random.value;

 
                        if (spawnRoll < obstacleDensity)
                        {
                            if (!HasNearbyObstacle(x, y))
                            {
                                mapData[x, y] = 1;
                            }
                            else
                            {
                                mapData[x, y] = 0;
                            }
                        }
                        else if (spawnRoll < (obstacleDensity + hazardDensity))
                        {
                            mapData[x, y] = 3; 
                        }
                        else
                        {
                            mapData[x, y] = 0; 
                        }
                    }
                }
            }
        }

        InstantiateTiles();

        AStarGrid aStarGrid = FindFirstObjectByType<AStarGrid>();
        if (aStarGrid != null)
        {
            float totalWorldX = width * tileSize; 
            float totalWorldY = height * tileSize; 

            float matchingGridSize = tileSize / 2f; 

            aStarGrid.InitializeDynamicGrid(new Vector2(totalWorldX, totalWorldY), matchingGridSize);
        }
    }

    void InstantiateTiles()
    {
        Vector2 origin = (Vector2)transform.position - new Vector2((width * tileSize) / 2f, (height * tileSize) / 2f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 spawnPos = origin + new Vector2(x * tileSize + (tileSize / 2f), y * tileSize + (tileSize / 2f));

                if (mapData[x, y] == 2)
                {
                    SpawnRandomTileFrom(borderPrefabs, spawnPos);
                }
                else if (mapData[x, y] == 1)
                {
                    SpawnRandomTileFrom(wallPrefabs, spawnPos);
                }
                else if (mapData[x, y] == 3)
                {
                    SpawnRandomTileFrom(hazardPrefabs, spawnPos);
                    validFloorPositions.Add(spawnPos); 
                }
                else
                {
                    SpawnRandomTileFrom(floorPrefabs, spawnPos);
                    validFloorPositions.Add(spawnPos); 
                }
            }
        }
    }

    void SpawnRandomTileFrom(GameObject[] prefabArray, Vector2 position)
    {
        if (prefabArray == null || prefabArray.Length == 0) return;
        
        // Pick an image randomy
        GameObject pickedPrefab = prefabArray[Random.Range(0, prefabArray.Length)];
        Instantiate(pickedPrefab, position, Quaternion.identity, transform);
    }

    public Vector2 GetRandomWalkablePosition()
    {
        if (validFloorPositions.Count == 0) return Vector2.zero;
        int randomIndex = Random.Range(0, validFloorPositions.Count);
        return validFloorPositions[randomIndex];
    }

    private bool HasNearbyObstacle(int x, int y)
    {
        for (int dx = -obstacleClearance; dx <= obstacleClearance; dx++)
        {
            for (int dy = -obstacleClearance; dy <= obstacleClearance; dy++)
            {
                int checkX = x + dx;
                int checkY = y + dy;

                if (checkX < 0 || checkX >= width ||
                    checkY < 0 || checkY >= height)
                    continue;

                if (mapData[checkX, checkY] == 1)
                    return true;
            }
        }

        return false;
    }


}