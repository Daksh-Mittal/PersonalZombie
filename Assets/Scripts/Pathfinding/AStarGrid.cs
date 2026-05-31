// Adapted from: https://github.com/SebLague/Pathfinding-2D

using UnityEngine;
using System.Collections.Generic;

public class AStarGrid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float gridSize;
    public float overlapCircleRadius;

    // TODO: This is unused, and can be added as part of the workshop
    public bool includeDiagonalNeighbours;

    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;



    [System.Serializable]
    public struct TerrainType {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }

    public TerrainType[] walkableRegions;
    public LayerMask dynamicObstacleMask;




    void Awake()
    {
        nodeDiameter = gridSize * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    public void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = (Vector2)transform.position - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + gridSize) + Vector2.up * (y * nodeDiameter + gridSize);
                bool walkable = (Physics2D.OverlapCircle(worldPoint, overlapCircleRadius, unwalkableMask) == null);
                int movementPenalty = 0;
                if (walkable) {
                    foreach (TerrainType region in walkableRegions) {
                        if (Physics2D.OverlapCircle(worldPoint, overlapCircleRadius, region.terrainMask)) {
                            movementPenalty += region.terrainPenalty;
                        }
                    }
                    if (Physics2D.OverlapCircle(worldPoint, overlapCircleRadius * 2f, dynamicObstacleMask)) {
                        movementPenalty += 50; // Dynamic Obstacle avoidance
                    }
                }
                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                if (!includeDiagonalNeighbours && Mathf.Abs(x) + Mathf.Abs(y) == 2) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public Node ClosestWalkableNode(Node node)
    {
        int maxRadius = Mathf.Max(gridSizeX, gridSizeY) / 2;
        for (int i = 1; i < maxRadius; i++)
        {
            Node n = FindWalkableInRadius(node.gridX, node.gridY, i);
            if (n != null)
            {
                return n;
            }
        }
        return null;
    }

    Node FindWalkableInRadius(int centreX, int centreY, int radius)
    {
        for (int i = -radius; i <= radius; i++)
        {
            int verticalSearchX = i + centreX;
            int horizontalSearchY = i + centreY;

            // Top
            if (InBounds(verticalSearchX, centreY + radius))
            {
                if (grid[verticalSearchX, centreY + radius].walkable)
                {
                    return grid[verticalSearchX, centreY + radius];
                }
            }

            // Bottom
            if (InBounds(verticalSearchX, centreY - radius))
            {
                if (grid[verticalSearchX, centreY - radius].walkable)
                {
                    return grid[verticalSearchX, centreY - radius];
                }
            }

            // Right
            if (InBounds(centreY + radius, horizontalSearchY))
            {
                if (grid[centreX + radius, horizontalSearchY].walkable)
                {
                    return grid[centreX + radius, horizontalSearchY];
                }
            }

            // Left
            if (InBounds(centreY - radius, horizontalSearchY))
            {
                if (grid[centreX - radius, horizontalSearchY].walkable)
                {
                    return grid[centreX - radius, horizontalSearchY];
                }
            }

        }

        return null;
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                if (!n.walkable) 
                {
                    Gizmos.color = Color.red;
                }
                else if (n.movementPenalty >= 30) 
                {
                    Gizmos.color = Color.blue; 
                }
                else if (n.movementPenalty > 0) 
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 1f); 
                }
                else 
                {
                    Gizmos.color = Color.grey; 
                }

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }

    public void UpdateGridRegion(Bounds bounds)
    {
        if (grid == null) return;

        float expandBy = overlapCircleRadius * 2f;
        Vector2 min = (Vector2)bounds.min - new Vector2(expandBy, expandBy);
        Vector2 max = (Vector2)bounds.max + new Vector2(expandBy, expandBy);

        Node minNode = NodeFromWorldPoint(min);
        Node maxNode = NodeFromWorldPoint(max);

        int startX = Mathf.Clamp(Mathf.Min(minNode.gridX, maxNode.gridX), 0, gridSizeX - 1);
        int endX = Mathf.Clamp(Mathf.Max(minNode.gridX, maxNode.gridX), 0, gridSizeX - 1);
        int startY = Mathf.Clamp(Mathf.Min(minNode.gridY, maxNode.gridY), 0, gridSizeY - 1);
        int endY = Mathf.Clamp(Mathf.Max(minNode.gridY, maxNode.gridY), 0, gridSizeY - 1);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Node n = grid[x, y];
                Vector2 worldPoint = n.worldPosition;
                
                bool walkable = (Physics2D.OverlapCircle(worldPoint, overlapCircleRadius, unwalkableMask) == null);
                int movementPenalty = 0;
                
                if (walkable) 
                {
                    foreach (TerrainType region in walkableRegions) 
                    {
                        if (Physics2D.OverlapCircle(worldPoint, overlapCircleRadius, region.terrainMask)) 
                        {
                            movementPenalty += region.terrainPenalty;
                        }
                    }
                    if (Physics2D.OverlapCircle(worldPoint, overlapCircleRadius * 2f, dynamicObstacleMask)) 
                    {
                        movementPenalty += 50; // Dynamic Obstacle avoidance
                    }
                }
                
                // Update the existing node
                n.walkable = walkable;
                n.movementPenalty = movementPenalty;
            }
        }
    }
}
