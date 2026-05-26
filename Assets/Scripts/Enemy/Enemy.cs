using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer healthBarFill;

    private EnemyData data;
    private Element element;
    private float currentHealth;
    private float moveSpeed;
    private int damage;
    private int scoreValue;

    public Element CurrentElement => element;
    public bool IsAlive => currentHealth > 0;

    public static event System.Action<Enemy> OnEnemyDied;

    public void Init(EnemyData data, Element element, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        this.data = data;
        this.element = element;
        this.currentHealth = data.maxHealth * healthMultiplier;
        this.moveSpeed = data.moveSpeed * speedMultiplier;
        this.damage = data.damage;
        this.scoreValue = data.scoreValue;

        if (spriteRenderer != null)
            spriteRenderer.color = ElementSystem.GetElementColor(element);
    }

    public void InitProcedural(int wave, Element element)
    {
        this.element = element;
        float waveScale = 1f + (wave - 1) * 0.2f;
        this.currentHealth = 30f * waveScale;
        this.moveSpeed = 1.5f + wave * 0.1f;
        this.damage = 1;
        this.scoreValue = 10 + wave * 2;

        if (spriteRenderer != null)
            spriteRenderer.color = ElementSystem.GetElementColor(element);
    }

    private void Update()
    {
        if (!IsAlive) return;

        Vector3 target = GameManager.Instance.BasePosition;
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float dist = Vector2.Distance(transform.position, target);
        if (dist < 0.5f)
        {
            ReachBase();
        }

        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.AddScore(scoreValue);
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    private void ReachBase()
    {
        GameManager.Instance.DamageBase(damage);
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null || data == null) return;

        float ratio = currentHealth / data.maxHealth;
        healthBarFill.transform.localScale = new Vector3(ratio, 1, 1);
    }
}
