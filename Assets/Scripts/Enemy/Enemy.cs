using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer healthBarFill;
    [SerializeField] private AirshipVisual airshipVisual;

    private EnemyData data;
    private Element element;
    private float currentHealth;
    private float maxHealth;
    private float moveSpeed;
    private int damage;
    private int scoreValue;
    private float spawnDist = -1f;
    private Tower lastHitTower;
    private float healthBarOrigWidth;

    public Element CurrentElement => element;
    public bool IsAlive => currentHealth > 0;
    public int Damage => damage;

    public static event System.Action<Enemy> OnEnemyDied;

    private void Start()
    {
        if (healthBarFill != null) healthBarOrigWidth = healthBarFill.transform.localScale.x;
    }

    public void Init(EnemyData data, Element element, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        this.data = data;
        this.element = element;
        this.maxHealth = data.maxHealth * healthMultiplier;
        this.currentHealth = this.maxHealth;
        this.moveSpeed = data.moveSpeed * speedMultiplier;
        this.damage = data.damage;
        this.scoreValue = data.scoreValue;

        ApplyElementColor(element);
    }

    public void InitProcedural(int wave, Element element)
    {
        this.element = element;
        float waveScale = 1f + (wave - 1) * 0.2f;
        this.maxHealth = 30f * waveScale;
        this.currentHealth = this.maxHealth;
        this.moveSpeed = 1.5f + wave * 0.1f;
        this.damage = 1;
        this.scoreValue = 10 + wave * 2;

        ApplyElementColor(element);
    }

    private void ApplyElementColor(Element el)
    {
        Color c = ElementSystem.GetElementColor(el);
        if (spriteRenderer != null) spriteRenderer.color = c;
        if (airshipVisual != null) airshipVisual.SetElementColor(c);
    }

    public void SetLastHit(Tower tower) => lastHitTower = tower;

    private void Update()
    {
        if (!IsAlive) return;

        Vector3 target = GameManager.Instance.BasePosition;
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float dist = Vector2.Distance(transform.position, target);
        UpdateApproachScale(dist);

        if (dist < 0.5f)
        {
            ReachBase();
            return;
        }

        UpdateHealthBar();
    }

    private void UpdateApproachScale(float distToBase)
    {
        if (spawnDist < 0f) spawnDist = distToBase;
        if (spawnDist <= 0f) return;
        float t = 1f - Mathf.Clamp01(distToBase / spawnDist);
        transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.05f, t);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        lastHitTower?.GainXP(scoreValue);
        GameManager.Instance.AddScore(scoreValue);
        BoostTowersOfSameElement();
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    private void BoostTowersOfSameElement()
    {
        float bonus = 0.05f;
        foreach (var tower in FindObjectsOfType<Tower>())
            if (tower.CurrentElement == element)
                tower.BoostDamage(bonus);
    }

    private void ReachBase()
    {
        GameManager.Instance.DamageBase(damage);
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null || maxHealth <= 0f) return;
        float ratio = Mathf.Clamp01(currentHealth / maxHealth);
        Vector3 s = healthBarFill.transform.localScale;
        s.x = healthBarOrigWidth * ratio;
        healthBarFill.transform.localScale = s;
    }
}
