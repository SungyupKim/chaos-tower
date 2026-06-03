using UnityEngine;

public class AirshipVisual : MonoBehaviour
{
    [SerializeField] private Transform propellerAnchor;
    [SerializeField] private SpriteRenderer engineGlowRenderer;

    private float timer;
    private Color engineGlowBaseColor = Color.white;

    public void SetElementColor(Color baseColor)
    {
        Color dark = baseColor * 0.45f;
        dark.a = 1f;
        Color blade = new Color(dark.r, dark.g, dark.b, 0.72f);

        // Bright element-tinted glow for engine exhaust
        engineGlowBaseColor = new Color(
            Mathf.Min(1f, baseColor.r * 1.2f + 0.4f),
            Mathf.Min(1f, baseColor.g * 1.2f + 0.4f),
            Mathf.Min(1f, baseColor.b * 1.2f + 0.4f),
            1f
        );

        SetChildColor("Hull", baseColor);
        SetChildColor("Gondola", dark);
        SetChildColor("FinTop", dark);
        SetChildColor("FinBottom", dark);
        SetChildColor("TailFin", dark);

        if (propellerAnchor != null)
            foreach (Transform child in propellerAnchor)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = blade;
            }

        if (engineGlowRenderer != null)
        {
            Color g = engineGlowBaseColor;
            g.a = engineGlowRenderer.color.a;
            engineGlowRenderer.color = g;
        }
    }

    private void SetChildColor(string childName, Color color)
    {
        Transform t = transform.Find(childName);
        if (t == null) return;
        var sr = t.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (propellerAnchor != null)
            propellerAnchor.Rotate(0f, 0f, 800f * Time.deltaTime, Space.Self);

        if (engineGlowRenderer != null)
        {
            float alpha = 0.5f + Mathf.Abs(Mathf.Sin(timer * Mathf.PI * 1.8f)) * 0.5f;
            Color c = engineGlowBaseColor;
            c.a = alpha;
            engineGlowRenderer.color = c;
        }

        float bob = Mathf.Sin(timer * Mathf.PI * 2f * 0.85f) * 0.045f;
        transform.localPosition = new Vector3(0f, bob, 0f);
    }
}
