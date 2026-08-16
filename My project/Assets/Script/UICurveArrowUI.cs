using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UICurveArrowUI : MonoBehaviour
{
    public RectTransform canvasRect;
    public RectTransform segmentPrefab;

    public int resolution = 30;
    public float thickness = 20f;
    public float curveHeight = 200f;

    private List<RectTransform> segs = new List<RectTransform>();

    void Update()
    {
        Draw();
    }

    void Draw()
    {
        Vector2 start = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            null,
            out Vector2 end
        );

        Vector2 control = (start + end) / 2 + Vector2.up * curveHeight;

        // 생성
        while (segs.Count < resolution)
        {
            var s = Instantiate(segmentPrefab, transform);
            s.localScale = Vector3.one;
            segs.Add(s);
        }

        for (int i = 0; i < resolution - 1; i++)
        {
            float t1 = i / (float)(resolution - 1);
            float t2 = (i + 1) / (float)(resolution - 1);

            Vector2 p1 = Bezier(start, control, end, t1);
            Vector2 p2 = Bezier(start, control, end, t2);

            Vector2 dir = p2 - p1;

            var seg = segs[i];

            seg.anchoredPosition = (p1 + p2) / 2;
            seg.sizeDelta = new Vector2(dir.magnitude, thickness);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            seg.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        return (1 - t) * (1 - t) * a +
               2 * (1 - t) * t * b +
               t * t * c;
    }
}