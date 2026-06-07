using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Speed = 10f;
    public float Lifetime = 2f; // To auto destroy if it misses and flies off-screen

    void Start()
    {
        Destroy(gameObject, Lifetime);
    }

    public void Setup(Vector2 shootDirection)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * Speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Zombie"))
        {
            FindObjectOfType<WaveManager>().OnZombieDied();

            Destroy(other.gameObject);

            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}