using UnityEngine;
using System.Collections;
using System.Linq;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private TowerData towerData;
    [SerializeField] private Element element = Element.Fire;

    [Header("Visual")]
    [SerializeField] private TowerVisual towerVisual;
    [SerializeField] private Transform firePoint;
    [SerializeField] private SpriteRenderer hpBarFill;
    [SerializeField] private SpriteRenderer xpBarFill;

    // Combat multipliers (cumulative upgrades)
    private float damageMultiplier = 1f;
    private float fireRateMultiplier = 1f;
    private float rangeMultiplier = 1f;
    private AttackMode attackMode;
    private bool inBurst;

    // HP
    private const int BaseMaxHP = 80;
    private int maxHP = BaseMaxHP;
    private int currentHP;
    private float hpBarOriginalWidth = 1f;

    // XP / Level
    private float currentXP;
    private int level = 1;
    private float xpBarOriginalWidth = 1f;

    private float fireCooldown;
    private ConveyorBelt belt;

    public Element CurrentElement => element;
    public float Damage => BaseDamage * damageMultiplier * (attackMode?.damageMultiplier ?? 1f);
    public float Range  => BaseRange  * rangeMultiplier;
    public float FireRate => BaseFireRate * fireRateMultiplier * (attackMode?.fireRateMultiplier ?? 1f);
    public int   Level    => level;
    public float CurrentXP     => currentXP;
    public float XPToNextLevel => 60f + level * 40f;
    public int   CurrentHP => currentHP;
    public int   MaxHP    => maxHP;
    public bool  IsAlive  => currentHP > 0;
    public AttackMode CurrentAttackMode => attackMode;

    private float BaseDamage   => towerData != null ? towerData.baseDamage   : 10f;
    private float BaseRange    => towerData != null ? towerData.range         : 5f;
    private float BaseFireRate => towerData != null ? towerData.fireRate      : 1f;

    public static event System.Action<Tower> OnTowerLevelUp;
    public static event System.Action<Tower> OnTowerHPChanged;

    private void Start()
    {
        belt = FindObjectOfType<ConveyorBelt>();
        belt?.RegisterTower(this);

        currentHP = maxHP;
        if (hpBarFill != null) hpBarOriginalWidth = hpBarFill.transform.localScale.x;
        if (xpBarFill != null) xpBarOriginalWidth = xpBarFill.transform.localScale.x;

        attackMode = AttackMode.GetStandard();
        UpdateVisual();
        UpdateHPBar();
        UpdateXPBar();
    }

    private void OnDestroy()
    {
        belt?.UnregisterTower(this);

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        bool anyAlive = System.Array.Exists(FindObjectsOfType<Tower>(), t => t != this && t.IsAlive);
        if (!anyAlive)
            GameManager.Instance.TriggerGameOver();
    }

    private void Update()
    {
        if (!IsAlive) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        if (inBurst) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
            TryFire();
    }

    private void TryFire()
    {
        switch (attackMode?.type)
        {
            case AttackModeType.Multishot:
                FireMultishot();
                break;
            case AttackModeType.Burst:
                StartCoroutine(FireBurst());
                break;
            default:
                var target = FindClosestEnemy();
                if (target == null) return;
                FireAt(target);
                break;
        }
        fireCooldown = 1f / Mathf.Max(0.1f, FireRate);
    }

    private void FireAt(Enemy target)
    {
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        BeamShape shape = attackMode?.beamShape ?? BeamShape.Clean;
        Beam.Create(origin, target, element, Damage, shape, this);

        if (attackMode?.type == AttackModeType.Explosion && attackMode.aoeRadius > 0f)
            ApplyExplosion(target.transform.position);
        else if (attackMode?.type == AttackModeType.Chain && attackMode.chainRange > 0f)
            ApplyChain(target, origin);
    }

    private void FireMultishot()
    {
        int count = attackMode?.targetCount ?? 1;
        var targets = FindObjectsOfType<Enemy>()
            .Where(e => Vector2.Distance(transform.position, e.transform.position) <= Range)
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .Take(count)
            .ToArray();
        foreach (var t in targets)
            FireAt(t);
    }

    private IEnumerator FireBurst()
    {
        inBurst = true;
        int shots = attackMode?.burstCount ?? 3;
        for (int i = 0; i < shots; i++)
        {
            if (!IsAlive) break;
            var t = FindClosestEnemy();
            if (t != null) FireAt(t);
            yield return new WaitForSeconds(0.11f);
        }
        yield return new WaitForSeconds(1.3f);
        inBurst = false;
    }

    private void ApplyExplosion(Vector3 pos)
    {
        float aoeDmg = Damage * 0.5f;
        foreach (var e in FindObjectsOfType<Enemy>())
            if (Vector2.Distance(e.transform.position, pos) <= attackMode.aoeRadius)
                e.TakeDamage(aoeDmg);
    }

    private void ApplyChain(Enemy primary, Vector3 origin)
    {
        Enemy chain = null;
        float best = attackMode.chainRange;
        foreach (var e in FindObjectsOfType<Enemy>())
        {
            if (e == primary) continue;
            float d = Vector2.Distance(primary.transform.position, e.transform.position);
            if (d < best) { best = d; chain = e; }
        }
        if (chain != null)
            Beam.Create(origin, chain, element, Damage * 0.6f, BeamShape.Crackle, this);
    }

    private Enemy FindClosestEnemy()
    {
        Enemy closest = null;
        float best = Range;
        foreach (var e in FindObjectsOfType<Enemy>())
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < best) { best = d; closest = e; }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;
        if (other.GetComponent<Enemy>() != null)
            TakeDamage(8);
    }

    public void GainXP(float amount)
    {
        if (!IsAlive) return;
        currentXP += amount;
        while (currentXP >= XPToNextLevel)
        {
            currentXP -= XPToNextLevel;
            level++;
            OnTowerLevelUp?.Invoke(this);
        }
        UpdateXPBar();
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        currentHP = Mathf.Max(0, currentHP - damage);
        OnTowerHPChanged?.Invoke(this);
        UpdateHPBar();
        if (currentHP <= 0)
            Destroy(gameObject);
    }

    public void RepairHP(float fraction)
    {
        currentHP = Mathf.Min(maxHP, currentHP + Mathf.RoundToInt(maxHP * fraction));
        OnTowerHPChanged?.Invoke(this);
        UpdateHPBar();
    }

    public void BoostDamage(float bonus)    => damageMultiplier   += bonus;
    public void BoostFireRate(float bonus)  => fireRateMultiplier += bonus;
    public void BoostRange(float bonus)     => rangeMultiplier    += bonus;

    public void SetAttackMode(AttackMode mode) => attackMode = mode;

    public void SetElement(Element newElement)
    {
        element = newElement;
        UpdateVisual();
    }

    // Kept for backwards compatibility with any Unity event bindings
    public void RandomizeTrajectory() { }

    private void UpdateVisual()
    {
        towerVisual?.SetElementColor(ElementSystem.GetElementColor(element));
    }

    private void UpdateHPBar()
    {
        if (hpBarFill == null) return;
        float ratio = (float)currentHP / maxHP;
        Vector3 s = hpBarFill.transform.localScale;
        s.x = hpBarOriginalWidth * ratio;
        hpBarFill.transform.localScale = s;
        hpBarFill.color = Color.Lerp(Color.red, Color.green, ratio);
    }

    private void UpdateXPBar()
    {
        if (xpBarFill == null) return;
        float ratio = XPToNextLevel > 0f ? Mathf.Clamp01(currentXP / XPToNextLevel) : 0f;
        Vector3 s = xpBarFill.transform.localScale;
        s.x = xpBarOriginalWidth * ratio;
        xpBarFill.transform.localScale = s;
    }
}
