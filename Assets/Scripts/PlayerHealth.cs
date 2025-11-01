using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health;

    public int maxHealth = 3;

    public TextMeshProUGUI livesText;
    
    private Animator _animator;

    private bool _isDead = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0 && !_isDead)
        {
            GameManager.Instance.GameOver(false);
            _isDead = true;
        }
        livesText.text = health.ToString();
    }

    public void Damage(int damage)
    {
        health -= damage;
        _animator.SetTrigger("Damage");
    }
}
