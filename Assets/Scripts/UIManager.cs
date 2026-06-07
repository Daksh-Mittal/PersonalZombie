using UnityEngine;
using UnityEngine.UI; 

public class UIManager : MonoBehaviour
{
    [Header("Health UI")]
    public Image[] hearts; // Array to hold the 3 heart images

    [Header("End Game Screens")]
    public GameObject gameOverScreen;
    public GameObject winScreen;

    void Start()
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);
    }

    public void UpdateHealthUI(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentHealth;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
    }

    public void ShowWinScreen()
    {
        if (winScreen != null) winScreen.SetActive(true);
    }
}