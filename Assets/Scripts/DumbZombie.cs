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
    private MapGenerator _mapGen;
    
    WaveManager waveManager;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        Player = GameObject.Find("player_Character");

        _settings = transform.parent.GetComponent<FlockSettings>();
        _neighbours = new System.Collections.Generic.List<Transform>();

        waveManager = FindFirstObjectByType<WaveManager>();

        _mapGen = FindFirstObjectByType<MapGenerator>();
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

    void FixedUpdate()
    {
        UpdateNeighbours();

        _target = Player.transform.position;

        Vector2 desiredVel = Vector2.zero;

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

        float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        CheckBounds();
    }


    private void UpdateNeighbours()
    {
        _neighbours.Clear();

        if (transform.parent == null) return;

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

            print("Dumb Zombie went out of bounds and was destroyed.");
            Destroy(gameObject);
        }
    }
    
}   