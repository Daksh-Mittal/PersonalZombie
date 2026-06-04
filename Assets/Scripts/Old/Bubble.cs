using UnityEngine;
using UnityEngine.InputSystem;
using SteeringCalcs;
using Globals;

public class Bubble : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    public float speed = 1f;
    public float timeToLive = 2f;

    private float distanceToClosestSnake;
    private Snake closestSnake;
    private float _repathTimer = 0f;
    private Node[] _currentPath;
    private int _targetWaypointIndex;
    public float MaxSpeed;
    private float _arriveRadius;
    public float MaxAccel;
    public float AccelTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        //Find the closest snake
        findClosestSnake();

        if (closestSnake != null)
        {
            //Calculate a path to the closest snake using the A* Algorithm every 0.3 seconds, providing tracking behaviour
            _repathTimer += Time.fixedDeltaTime;
            if (_currentPath == null || _repathTimer > 0.3f)
            {
                _currentPath = Pathfinding.RequestPath(transform.position, closestSnake.transform.position);
                _targetWaypointIndex = 0;
                _repathTimer = 0f;
            }

            //Applying velocity to the bubble
            Vector2 desiredVel = getVelocityTowardsPath();
            Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
            _rb.AddForce(steering);

        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Frog") && !collider.gameObject.CompareTag("Fly") && !collider.gameObject.CompareTag("Obstacle"))
        {   
            Destroy(gameObject); // Destroy the bubble if it collides with anything other than the frog and fly
        }
    }

    public void fire(Vector2 direction, float speed) //Fire method that sends the bubble in the direction the frog is facing at a certain speed, and destroys the bubble after a certain amount of time
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();

        _rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, timeToLive);
    }

    private void findClosestSnake()
    {
        distanceToClosestSnake = Mathf.Infinity;
        closestSnake = null;

        foreach (Snake snake in (Snake[])GameObject.FindObjectsByType(typeof(Snake), FindObjectsSortMode.None))
        {
            float d = (snake.transform.position - transform.position).magnitude;
            if (d < distanceToClosestSnake)
            {
                distanceToClosestSnake = d;
                closestSnake = snake;
            }
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

}