using UnityEngine;

public class TowerVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer turretRenderer;
    [SerializeField] private SpriteRenderer coreGlowRenderer;
    [SerializeField] private SpriteRenderer gunBarrelRenderer;
    [SerializeField] private Transform turretPivot;
    [SerializeField] private Transform wheelLeft;
    [SerializeField] private Transform wheelRight;

    private float pulseTimer;
    private float searchTimer;
    private Transform cachedTarget;
    private Color elementColor = Color.white;

    // wheel sprite is 1×1 at PPU=64, placed at Visual scale(0.35,0.35), root scale 0.7
    // world radius = 0.5 × 0.35 × 0.7 ≈ 0.1225
    private const float WheelWorldRadius = 0.1225f;

    private void Update()
    {
        pulseTimer += Time.deltaTime * 1.8f;
        float alpha = 0.6f + Mathf.Sin(pulseTimer) * 0.35f;
        if (coreGlowRenderer != null)
        {
            Color c = elementColor;
            c.a = alpha;
            coreGlowRenderer.color = c;
        }

        TrackNearestEnemy();
        SpinWheels();
    }

    private void SpinWheels()
    {
        if (wheelLeft == null && wheelRight == null) return;
        if (ConveyorBelt.Instance == null) return;

        float angSpeedRad  = ConveyorBelt.Instance.AngularSpeedDeg * Mathf.Deg2Rad;
        float linearSpeed  = angSpeedRad * ConveyorBelt.Instance.Radius;   // world units/sec
        float spinDeg      = linearSpeed / (2f * Mathf.PI * WheelWorldRadius) * 360f * Time.deltaTime;

        if (wheelLeft  != null) wheelLeft.Rotate(0f, 0f, -spinDeg, Space.Self);
        if (wheelRight != null) wheelRight.Rotate(0f, 0f, -spinDeg, Space.Self);
    }

    private void TrackNearestEnemy()
    {
        if (turretPivot == null) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            searchTimer = 0.12f;
            float best = float.MaxValue;
            cachedTarget = null;
            foreach (var e in FindObjectsOfType<Enemy>())
            {
                float d = Vector2.Distance(turretPivot.position, e.transform.position);
                if (d < best) { best = d; cachedTarget = e.transform; }
            }
        }

        Quaternion targetRot = cachedTarget != null
            ? Quaternion.Euler(0f, 0f,
                Mathf.Atan2(cachedTarget.position.y - turretPivot.position.y,
                            cachedTarget.position.x - turretPivot.position.x) * Mathf.Rad2Deg - 90f)
            : Quaternion.identity;

        turretPivot.rotation = Quaternion.Slerp(turretPivot.rotation, targetRot, Time.deltaTime * 6f);
    }

    public void SetElementColor(Color color)
    {
        elementColor = color;

        if (bodyRenderer != null)
        {
            Color bodyTint = Color.Lerp(new Color(0.58f, 0.58f, 0.68f), color, 0.20f);
            bodyTint.a = 1f;
            bodyRenderer.color = bodyTint;
        }

        if (turretRenderer != null)
            turretRenderer.color = color;

        if (gunBarrelRenderer != null)
        {
            Color dim = color * 0.65f;
            dim.a = 1f;
            gunBarrelRenderer.color = dim;
        }
    }
}
