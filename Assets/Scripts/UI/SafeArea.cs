using UnityEngine;

// Resizes this RectTransform to match Screen.safeArea.
// Place all HUD/panel UI as children of this object so they avoid
// notches and navigation bars on any device.
public class SafeArea : MonoBehaviour
{
    private RectTransform rt;
    private Rect lastSafeArea = Rect.zero;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    private void Apply()
    {
        Rect safe = Screen.safeArea;
        if (safe == lastSafeArea) return;
        lastSafeArea = safe;

        float sw = Screen.width;
        float sh = Screen.height;
        if (sw <= 0 || sh <= 0) return;

        Vector2 min = new Vector2(safe.xMin / sw, safe.yMin / sh);
        Vector2 max = new Vector2(safe.xMax / sw, safe.yMax / sh);

        // Clamp to valid range in case of transient bad values during orientation change
        min.x = Mathf.Clamp01(min.x);
        min.y = Mathf.Clamp01(min.y);
        max.x = Mathf.Clamp01(max.x);
        max.y = Mathf.Clamp01(max.y);

        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
