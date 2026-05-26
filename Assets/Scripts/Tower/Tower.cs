using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private TowerData towerData;
    [SerializeField] private Element element = Element.Fire;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform firePoint;

    private float fireCooldown;
    private float damageMultiplier = 1f;
    private BeamShape currentBeamShape;
    private ConveyorBelt belt;

    public Element CurrentElement => element;
    public float Damage => towerData.baseDamage * damageMultiplier;
    public float Range => towerData.range;

    private void Start()
    {
        belt = GetComponentInParent<ConveyorBelt>();
        if (belt == null)
            belt = FindObjectOfType<ConveyorBelt>();
        belt?.RegisterTower(this);

        RandomizeBeamShape();
        UpdateVisual();
    }

    private void OnDestroy()
    {
        belt?.UnregisterTower(this);
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        Enemy target = FindClosestEnemy();
        if (target == null) return;

        fireCooldown = 1f / towerData.fireRate;
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Beam.Create(origin, target, element, Damage, currentBeamShape);
    }

    private Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float closestDist = towerData.range;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    public void SetElement(Element newElement)
    {
        element = newElement;
        UpdateVisual();
    }

    public void BoostDamage(float bonus)
    {
        damageMultiplier += bonus;
    }

    public void RandomizeTrajectory()
    {
        RandomizeBeamShape();
    }

    private void RandomizeBeamShape()
    {
        BeamShape prev = currentBeamShape;
        int count = System.Enum.GetValues(typeof(BeamShape)).Length;
        do
        {
            currentBeamShape = (BeamShape)Random.Range(0, count);
        } while (currentBeamShape == prev && count > 1);
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = ElementSystem.GetElementColor(element);
    }
}
