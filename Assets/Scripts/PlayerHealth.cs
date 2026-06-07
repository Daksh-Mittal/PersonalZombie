using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("References")]
    public UIManager uiManager; 

    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    
    [Header("Healing Settings")]
    public int killsNeededToHeal = 10;
    private int currentKills = 0;

    void Start()
    {
        currentHealth = maxHealth;
        if (uiManager != null) uiManager.UpdateHealthUI(currentHealth);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Zombie"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Player Health: " + currentHealth);
        
        if (uiManager != null) uiManager.UpdateHealthUI(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddKill()
    {
        currentKills++;
        if (currentKills >= killsNeededToHeal)
        {
            Heal(1);
            currentKills = 0;
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += amount;
            Debug.Log("Player Healed! Current Health: " + currentHealth);
            if (uiManager != null) uiManager.UpdateHealthUI(currentHealth);
        }
    }

    void Die()
    {
        Debug.Log("Game Over! Health reached 0.");
        gameObject.SetActive(false);
        
        if (uiManager != null) uiManager.ShowGameOver(); 
    }
}