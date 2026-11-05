using UnityEngine;
using UnityEngine.UI;

// A native uGUI heart; no font-dependent missing glyphs.
[RequireComponent(typeof(CanvasRenderer))]
public class HeartGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        vh.AddVert(new Vector3(r.center.x, r.center.y), color, Vector2.zero);
        const int count = 64;
        for (int i = 0; i < count; i++)
        {
            float t = i * Mathf.PI * 2 / count;
            float x = 16 * Mathf.Pow(Mathf.Sin(t), 3);
            float y = 13 * Mathf.Cos(t) - 5 * Mathf.Cos(2*t) - 2 * Mathf.Cos(3*t) - Mathf.Cos(4*t);
            vh.AddVert(new Vector3(r.center.x + x / 34 * r.width, r.center.y + (y + 2) / 34 * r.height), color, Vector2.zero);
        }
        for (int i = 0; i < count; i++) vh.AddTriangle(0, 1+i, 1+(i+1)%count);
    }
}

