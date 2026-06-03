using UnityEngine;

public enum BeamShape
{
    Clean,
    Flicker,
    Pulse,
    Crackle
}

public class Beam : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 origin;
    private Vector3 target;
    private BeamShape shape;
    private Color beamColor;
    private float duration = 0.2f;
    private float elapsed;
    private int pointCount = 12;
    private float baseWidth = 0.1f;

    public static Beam Create(Vector3 origin, Enemy targetEnemy, Element element, float damage, BeamShape shape, Tower attacker = null)
    {
        if (targetEnemy == null) return null;

        targetEnemy.SetLastHit(attacker);
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
        lr.sortingOrder = 15;
        lr.positionCount = pointCount;

        Vector3 dir = (target - origin).normalized;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0);

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            Vector3 pos = Vector3.Lerp(origin, target, t);
            float noise = GetNoise(t, i);
            lr.SetPosition(i, pos + perp * noise);
        }

        ApplyWidth();
    }

    private float GetNoise(float t, int index)
    {
        float edge = 1f - Mathf.Pow(2f * t - 1f, 2f);
        float tiny = 0.03f;

        return shape switch
        {
            BeamShape.Flicker => Random.Range(-tiny, tiny) * edge,
            BeamShape.Crackle => Random.Range(-tiny * 2f, tiny * 2f) * edge,
            _ => 0f
        };
    }

    private void ApplyWidth()
    {
        switch (shape)
        {
            case BeamShape.Pulse:
                AnimationCurve curve = new AnimationCurve(
                    new Keyframe(0f, 0.6f),
                    new Keyframe(0.3f, 1.2f),
                    new Keyframe(0.7f, 0.8f),
                    new Keyframe(1f, 0.3f));
                lr.widthCurve = curve;
                lr.widthMultiplier = baseWidth;
                break;
            case BeamShape.Crackle:
                lr.startWidth = baseWidth * 1.3f;
                lr.endWidth = baseWidth * 0.5f;
                break;
            default:
                lr.startWidth = baseWidth;
                lr.endWidth = baseWidth * 0.6f;
                break;
        }
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
        lr.endColor = new Color(c.r, c.g, c.b, fade * 0.5f);
    }
}
