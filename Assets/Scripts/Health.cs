using Assets.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    public int maxHealth = 3;
    public int currentHealth;
    public TextMeshProUGUI healthText;
    public UnityEvent onDeath;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateText();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        UpdateText();

        if (currentHealth <= 0)
        {
            onDeath.Invoke();
            Destroy(gameObject);
        }
    }

    void UpdateText()
    {
        if (healthText != null)
            healthText.text = currentHealth.ToString();
    }
}