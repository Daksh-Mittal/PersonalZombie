using UnityEngine;
using System.Collections.Generic;
using System;

public class Pathfinding : MonoBehaviour
{
    public static AStarGrid grid;
    static Pathfinding instance;

    public enum HeuristicType { Manhattan, Euclidean, Octile }
    public HeuristicType currentHeuristic = HeuristicType.Octile;

    void Awake()
    {
        grid = GetComponent<AStarGrid>();
        instance = this;
    }

    public static Node[] RequestPath(Vector2 from, Vector2 to)
    {
        return instance.FindPath(from, to);
    }

    Node[] FindPath(Vector2 from, Vector2 to)
    {
        Node[] waypoints = new Node[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(from);
        Node targetNode = grid.NodeFromWorldPoint(to);

        if (!startNode.walkable) startNode = grid.ClosestWalkableNode(startNode);
        if (!targetNode.walkable) targetNode = grid.ClosestWalkableNode(targetNode);

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            startNode.gCost = 0;
            startNode.hCost = Heuristic(startNode, targetNode);
            startNode.parent = startNode;
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

                    float newCostToNeighbour = currentNode.gCost + GCost(currentNode, neighbour) + neighbour.movementPenalty;

                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = Heuristic(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                        else openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        if (pathSuccess) waypoints = RetracePath(startNode, targetNode);
        return waypoints;
    }

    Node[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);
        path.Reverse();

        //path = SmoothPath(path);
        return path.ToArray();
    }

    private float GCost(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private float Heuristic(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        switch (currentHeuristic)
        {
            case HeuristicType.Manhattan: return 10 * (dstX + dstY);
            case HeuristicType.Euclidean: return 10f * Mathf.Sqrt(dstX * dstX + dstY * dstY);
            case HeuristicType.Octile:
            default:
                if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
                return 14 * dstX + 10 * (dstY - dstX);
        }
    }

List<Node> SmoothPath(List<Node> path)
    {
        if (path.Count <= 2) return path;

        List<Node> smoothed = new List<Node>();
        smoothed.Add(path[0]);

        int current = 0;
        while (current < path.Count - 1)
        {
            int furthest = current + 1;

            for (int i = path.Count - 1; i > current; i--)
            {
                Vector2 start = path[current].worldPosition;
                Vector2 end   = path[i].worldPosition;
                Vector2 dir   = end - start;
                float   dist  = dir.magnitude;

                RaycastHit2D hit = Physics2D.CircleCast(
                    start, 
                    grid.overlapCircleRadius, 
                    dir.normalized, 
                    dist, 
                    grid.unwalkableMask
                );

                if (hit.collider != null) continue; // obstacle in the way

                bool cutsThroughWorseTerrain = false;
                foreach (var region in grid.walkableRegions)
                {
                    if (region.terrainPenalty > path[current].movementPenalty)
                    {
                        RaycastHit2D terrainHit = Physics2D.CircleCast(
                            start, 
                            grid.overlapCircleRadius, 
                            dir.normalized, 
                            dist, 
                            region.terrainMask
                        );

                        if (terrainHit.collider != null)
                        {
                            cutsThroughWorseTerrain = true;
                            break;
                        }
                    }
                }

                if (cutsThroughWorseTerrain) continue; 

                furthest = i;
                break;
            }

            smoothed.Add(path[furthest]);
            current = furthest;
        }

        return smoothed;
    }
}
