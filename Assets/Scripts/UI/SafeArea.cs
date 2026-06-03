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

    private void Apply()
    {
        Rect safe = Screen.safeArea;
        if (safe == lastSafeArea) return;
        lastSafeArea = safe;

        Vector2 min = new Vector2(safe.xMin / Screen.width,  safe.yMin / Screen.height);
        Vector2 max = new Vector2(safe.xMax / Screen.width,  safe.yMax / Screen.height);

        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
