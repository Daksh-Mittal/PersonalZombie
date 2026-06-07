using Globals;
using SteeringCalcs;
using UnityEngine;

public class DumbZombie : MonoBehaviour
{
    public float MaxSpeed;
    public float MaxAccel;
    public float AccelTime;
    public float ArriveRadius = 0.5f;
    public GameObject Player;
    private Vector2 _target;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _animator;
    private FlockSettings _settings;
    private System.Collections.Generic.List<Transform> _neighbours;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        //Setting the player character in the code
        Player = GameObject.Find("player_Character");

        //Getting the flock settings and neighbour list from the parent object
        _settings = transform.parent.GetComponent<FlockSettings>();
        _neighbours = new System.Collections.Generic.List<Transform>();
    }

    void FixedUpdate()
    {
        UpdateNeighbours();

        //Setting the target to the player character's position
        _target = Player.transform.position;

        Vector2 desiredVel = Vector2.zero;

        //Using the seek steering behavior to calculate the desired velocity towards the target
        desiredVel = Steering.ArriveDirect(transform.position, _target, ArriveRadius, MaxSpeed);

        if (_settings != null && _neighbours.Count > 0)
        {
            Vector2 sep = _settings.SeparationWeight * Steering.GetSeparation(transform.position, _neighbours, MaxSpeed);
            Vector2 coh = _settings.CohesionWeight * Steering.GetCohesion(transform.position, _neighbours, MaxSpeed);
            Vector2 ali = _settings.AlignmentWeight * Steering.GetAlignment(_neighbours, MaxSpeed);

            desiredVel = (desiredVel * 2f + sep + coh + ali).normalized * MaxSpeed;
        }

        Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
        _rb.AddForce(steering);

        //Rotating the zombie to face the direction of movement
        float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    private void UpdateNeighbours()
    {
        _neighbours.Clear();

        // 1. SAFETY CHECK: If there is no parent, stop here to prevent a crash
        if (transform.parent == null) return;

        // 2. SAFETY CHECK: If the parent is missing the FlockSettings script, stop here
        if (_settings == null) return;

        foreach (Transform member in transform.parent)
        {
            if (member != transform &&
                (transform.position - member.position).magnitude < _settings.FlockRadius)
            {
                _neighbours.Add(member);
            }
        }
    }
    
}   