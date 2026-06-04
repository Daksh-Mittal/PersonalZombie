using Globals;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float interval;
    public bool spikesUp = false;
    private float _timer = 0f;

    //Actual collider that deals damage
    private Collider2D _collider;

    //Bigger Seperate collider just for the AStarGrid so that the A* algorithm has a better chance
    public Collider2D obstacleCollider;

    void Start()
    {
        _collider = GetComponent<Collider2D>();
        _collider.enabled = spikesUp;
        obstacleCollider.enabled = spikesUp;
    }

    // void Update()
    // {
    //     _timer += Time.deltaTime;
    //     if (_timer >= interval)
    //     {
    //         spikesUp = !spikesUp;
    //         obstacleCollider.enabled = spikesUp;
    //         _collider.enabled = spikesUp;
    //         Pathfinding.grid.CreateGrid();
    //         _timer = 0f;
    //     }
    // }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= interval)
        {
            spikesUp = !spikesUp;
            obstacleCollider.enabled = spikesUp;
            _collider.enabled = spikesUp;
            
            if (Pathfinding.grid != null) 
            {
                Pathfinding.grid.UpdateGridRegion(obstacleCollider.bounds);
            }
            
            _timer = 0f;
        }
    }
}