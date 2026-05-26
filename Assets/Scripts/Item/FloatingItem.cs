using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatFrequency = 2f;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private float blinkStartTime = 10f;

    private ItemEffect effect;
    private Vector3 basePosition;
    private float spawnTime;

    public void Init(ItemEffect effect)
    {
        this.effect = effect;
        this.basePosition = transform.position;
        this.spawnTime = Time.time;

        UpdateVisual();
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        float elapsed = Time.time - spawnTime;
        float yOffset = Mathf.Sin(elapsed * floatFrequency) * floatAmplitude;
        transform.position = basePosition + Vector3.up * yOffset;

        if (elapsed > blinkStartTime)
        {
            bool visible = Mathf.Sin(elapsed * 10f) > 0;
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Tower tower = other.GetComponent<Tower>();
        if (tower == null) return;

        effect.Apply(tower);
        Destroy(gameObject);
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        switch (effect.type)
        {
            case ItemType.ElementPowerUp:
            case ItemType.ElementChange:
                spriteRenderer.color = ElementSystem.GetElementColor(effect.element);
                break;
            case ItemType.TrajectoryChange:
                spriteRenderer.color = Color.white;
                break;
        }
    }
}
