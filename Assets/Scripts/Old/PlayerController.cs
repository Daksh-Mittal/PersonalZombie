using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float speed = 5f;
    public int health = 3;

    private Rigidbody2D rb;
    private Camera cam;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        // Rotate player to face mouse
        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot(dir);
        }
    }

    void FixedUpdate()
    {
        // Move with WASD
        float x = 0f;
        float y = 0f;

        if (Keyboard.current.dKey.isPressed) x = 1f;
        if (Keyboard.current.aKey.isPressed) x = -1f;
        if (Keyboard.current.wKey.isPressed) y = 1f;
        if (Keyboard.current.sKey.isPressed) y = -1f;

        rb.linearVelocity = new Vector2(x, y).normalized * speed;
    }

    public void TakeDamage()
    {
        health--;
        Debug.Log("Health: " + health);

        if (health <= 0)
        {
            Debug.Log("Game Over!");
        }
    }

    void Shoot(Vector2 shootDirection)
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        spawnPos.z = 5f;
        
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, transform.rotation);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        
        if (bulletScript != null)
        {
            bulletScript.Setup(shootDirection);
        }
    }

}