using Globals;
using SteeringCalcs;
using UnityEngine;

public class SmartZombie : MonoBehaviour
{
    public float MaxSpeed;
    public float MaxAccel;
    public float AccelTime;
    public GameObject Player;
    private Vector2 _target;
    public AvoidanceParams AvoidParams;
    private float _repathTimer = 0f;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _animator;
    private Node[] _currentPath;
    private int _targetWaypointIndex;
    private float _arriveRadius = 0.5f;
    private MapGenerator _mapGen;
    
    WaveManager waveManager;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        //Setting the player character in the code
        Player = GameObject.Find("player_Character");

        waveManager = FindFirstObjectByType<WaveManager>();
        _mapGen = FindFirstObjectByType<MapGenerator>();
    }

    void FixedUpdate()
    {
        if (Player == null) return;

        Rigidbody2D playerRb = Player.GetComponent<Rigidbody2D>();
        Vector2 playerVelocity = playerRb != null ? playerRb.linearVelocity : Vector2.zero;

        // Set the target to ahead of the player
        _target = (Vector2)Player.transform.position + playerVelocity * 0.5f;

        _repathTimer += Time.fixedDeltaTime;
        if (_currentPath == null || _repathTimer > 0.3f)
        {
            _currentPath = Pathfinding.RequestPath(transform.position, _target);
            _targetWaypointIndex = 0;
            _repathTimer = 0f;
        }

        if (_currentPath != null && _currentPath.Length > 0 && _targetWaypointIndex < _currentPath.Length)
        {
            Vector2 desiredVel = getVelocityTowardsPath();

            // Keeps the physics body pushed away from corners even at high momentum
            Vector2 travelDirection = _rb.linearVelocity.normalized;
            if (travelDirection == Vector2.zero) travelDirection = ((Vector2)_target - (Vector2)transform.position).normalized;

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.4f, travelDirection, 0.8f, Pathfinding.grid.unwalkableMask);
            if (hit.collider != null)
            {
                Vector2 avoidanceForce = hit.normal * MaxSpeed * 1.5f;
                desiredVel += avoidanceForce;
            }

            Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
            _rb.AddForce(steering);
        }
        else
        {                
            // If no path, slow down to a stop
            _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, Vector2.zero, MaxAccel * Time.fixedDeltaTime);
        }

        //Rotating the zombie to face the direction of movement
        float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        CheckBounds();
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Spike"))
    {
        if (waveManager != null)
            waveManager.OnZombieDied();

        Destroy(gameObject);
    }
}

    private Vector2 getVelocityTowardsPath()
    {
        Vector2 targetPos = _currentPath[_targetWaypointIndex].worldPosition;
        float distToWaypoint = Vector2.Distance(transform.position, targetPos);

        if (distToWaypoint < Constants.TARGET_REACHED_TOLERANCE)
        {
            _targetWaypointIndex++;
            if (_targetWaypointIndex >= _currentPath.Length)
            {
                _currentPath = null;
                return Vector2.zero;
            }
            targetPos = _currentPath[_targetWaypointIndex].worldPosition;
        }

        if (_targetWaypointIndex == _currentPath.Length - 1)
            return Steering.ArriveDirect(transform.position, targetPos, _arriveRadius, MaxSpeed);
        else
            return Steering.SeekDirect(transform.position, targetPos, MaxSpeed);
    }

    private void CheckBounds()
    {
        if (_mapGen == null) return;

        float halfWidth  = (_mapGen.width  * _mapGen.tileSize) / 2f;
        float halfHeight = (_mapGen.height * _mapGen.tileSize) / 2f;

        Vector2 mapCenter = _mapGen.transform.position;
        Vector2 pos = transform.position;

        bool outOfBounds = pos.x < mapCenter.x - halfWidth  || pos.x > mapCenter.x + halfWidth  || pos.y < mapCenter.y - halfHeight || pos.y > mapCenter.y + halfHeight;

        if (outOfBounds)
        {
            if (waveManager != null) waveManager.OnZombieDied();

            print("Smart Zombie went out of bounds and was destroyed.");
            Destroy(gameObject);
        }
    }
    
}   