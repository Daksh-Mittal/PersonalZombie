using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class PlayerMLBridge : Unity.MLAgents.Agent
{
    [Header("References")]
    public Transform zombie;          
    public GameObject bulletPrefab;  
    public Transform firePoint;        

    [Header("Settings")]
    public float moveSpeed = 5f;
    public float fireRate = 0.25f;


    public float spawnRange = 4f;
    public float minSpawnDistance = 2f; 

    private Rigidbody2D _rb;
    private PlayerController _playerController;
    private float _nextFireTime = 0f;


    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerController = GetComponent<PlayerController>();

        if (_playerController != null)
            _playerController.enabled = false;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
        transform.rotation = Quaternion.identity;

        if (zombie != null)
        {
            Vector2 randomPos;
            do
            {
                randomPos = new Vector2(
                    Random.Range(-spawnRange, spawnRange),
                    Random.Range(-spawnRange, spawnRange)
                );
            }
            while (Vector2.Distance(randomPos, Vector2.zero) < minSpawnDistance);

            zombie.position = randomPos;

            Rigidbody2D zombieRb = zombie.GetComponent<Rigidbody2D>();
            if (zombieRb != null) zombieRb.linearVelocity = Vector2.zero;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (zombie == null)
        {
            for (int i = 0; i < 6; i++) sensor.AddObservation(0f);
            return;
        }

        Vector2 toZombie = (Vector2)zombie.position - (Vector2)transform.position;
        sensor.AddObservation(toZombie.x);
        sensor.AddObservation(toZombie.y);

        sensor.AddObservation(_rb.linearVelocity.x);
        sensor.AddObservation(_rb.linearVelocity.y);


        Rigidbody2D zombieRb = zombie.GetComponent<Rigidbody2D>();
        if (zombieRb != null)
        {
            sensor.AddObservation(zombieRb.linearVelocity.x);
            sensor.AddObservation(zombieRb.linearVelocity.y);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        Vector2 move = new Vector2(moveX, moveY);
        if (move.magnitude > 1f) move = move.normalized; 
        _rb.linearVelocity = move * moveSpeed;

        if (zombie != null && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + fireRate;
            ShootAtZombie();
        }

        AddReward(0.001f);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuous = actionsOut.ContinuousActions;

        float x = 0f;
        float y = 0f;

        if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed) x = 1f;
        if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed) x = -1f;
        if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed) y = 1f;
        if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed) y = -1f;

        continuous[0] = x;
        continuous[1] = y;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Zombie"))
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    private void ShootAtZombie()
    {
        if (bulletPrefab == null) return;

        Vector2 shootDir = ((Vector2)zombie.position - (Vector2)transform.position).normalized;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, transform.rotation);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.Setup(shootDir);
    }
}