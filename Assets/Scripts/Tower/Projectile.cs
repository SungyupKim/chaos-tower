using UnityEngine;

public enum BeamShape
{
    Straight,
    SineWave,
    Zigzag,
    Lightning
}

public class Beam : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 origin;
    private Vector3 target;
    private BeamShape shape;
    private Color beamColor;
    private float duration = 0.25f;
    private float elapsed;
    private int pointCount = 20;
    private float amplitude = 0.4f;

    public static Beam Create(Vector3 origin, Enemy targetEnemy, Element element, float damage, BeamShape shape)
    {
        if (targetEnemy == null) return null;

        float multiplier = ElementSystem.GetDamageMultiplier(element, targetEnemy.CurrentElement);
        targetEnemy.TakeDamage(damage * multiplier);

        GameObject obj = new GameObject("Beam");
        Beam beam = obj.AddComponent<Beam>();
        beam.origin = origin;
        beam.target = targetEnemy.transform.position;
        beam.shape = shape;
        beam.beamColor = ElementSystem.GetElementColor(element);
        beam.BuildBeam();
        return beam;
    }

    private void BuildBeam()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = beamColor;
        lr.endColor = beamColor;
        lr.startWidth = 0.12f;
        lr.endWidth = 0.04f;
        lr.sortingOrder = 15;
        lr.positionCount = pointCount;

        Vector3 direction = target - origin;
        Vector3 perp = new Vector3(-direction.normalized.y, direction.normalized.x, 0);

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            Vector3 basePos = Vector3.Lerp(origin, target, t);
            float offset = GetOffset(t, i);
            lr.SetPosition(i, basePos + perp * offset);
        }
    }

    private float GetOffset(float t, int index)
    {
        float edge = 1f - Mathf.Pow(2f * t - 1f, 2f);

        return shape switch
        {
            BeamShape.SineWave =>
                Mathf.Sin(t * Mathf.PI * Random.Range(3f, 6f)) * amplitude * edge,

            BeamShape.Zigzag =>
                ((index % 2 == 0) ? amplitude : -amplitude) * edge,

            BeamShape.Lightning =>
                Random.Range(-amplitude, amplitude) * edge,

            _ => 0f
        };
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float fade = 1f - (elapsed / duration);

        if (fade <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Color c = beamColor;
        c.a = fade;
        lr.startColor = c;
        lr.endColor = new Color(c.r, c.g, c.b, fade * 0.3f);
        lr.startWidth = 0.12f * fade;
        lr.endWidth = 0.04f * fade;
    }
}
