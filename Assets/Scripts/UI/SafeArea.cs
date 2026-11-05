using UnityEngine;

// Lectures 8–9: only interactive HUD goes inside this panel; backdrop stays full screen.
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform panel;
    Rect last;
    Vector2Int screenSize;
    void Awake() { panel = GetComponent<RectTransform>(); Apply(Screen.safeArea, Screen.width, Screen.height); }
    void Update()
    {
        if (last != Screen.safeArea || screenSize != new Vector2Int(Screen.width, Screen.height))
            Apply(Screen.safeArea, Screen.width, Screen.height);
    }
    public void Apply(Rect safe, int width, int height)
    {
        if (!panel) panel = GetComponent<RectTransform>();
        if (width <= 0 || height <= 0) return;
        last = safe; screenSize = new Vector2Int(width, height);
        panel.anchorMin = new Vector2(safe.xMin / width, safe.yMin / height);
        panel.anchorMax = new Vector2(safe.xMax / width, safe.yMax / height);
        panel.offsetMin = panel.offsetMax = Vector2.zero;
    }
}
