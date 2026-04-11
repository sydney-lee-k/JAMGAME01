using System.Collections.Generic;
using UnityEngine;

public class BezierPath : MonoBehaviour
{
    public List<BezierPoint> Points = new List<BezierPoint>();
    public bool loop;

    public int SegmentCount => loop ? Points.Count : Points.Count - 1;

    public Vector3 GetPoint(float t)
    {
        int segment = GetSegment(t, out float localT);

        BezierPoint p0 = Points[segment];
        BezierPoint p1 = Points[(segment + 1) % Points.Count];
        
        Vector3 point = GetBezierPoint(p0.transform.position, p0.GetControl1(), p1.GetControl2(), p1.transform.position, localT);
        return point;
    }

    public float GetSpeedMultiplier(float t)
    {
        int segment = GetSegment(t, out float localT);

        float a = Points[segment].speedMultiplier;
        float b = Points[(segment + 1) % Points.Count].speedMultiplier;

        return Mathf.Lerp(a, b, localT);
    }

    private int GetSegment(float t, out float localT)
    {
        int segments = SegmentCount;

        float scaled = t * segments;
        int segment = Mathf.FloorToInt(scaled);

        if (segment >= segments)
            segment = segments - 1;

        localT = scaled - segment;
        return segment;
    }

    private Vector3 GetBezierPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 X = Vector3.Lerp(a, b, t);
        Vector3 Y = Vector3.Lerp(b, c, t);
        Vector3 Z = Vector3.Lerp(c, d, t);

        Vector3 P = Vector3.Lerp(X, Y, t);
        Vector3 Q = Vector3.Lerp(Y, Z, t);

        return Vector3.Lerp(P, Q, t);
    }
    
    private void OnDrawGizmos()
    {
        if (Points == null || Points.Count < 2)
            return;

        Gizmos.color = Color.green;

        int stepsPerSegment = 10;

        int segments = SegmentCount;

        for (int s = 0; s < segments; s++)
        {
            BezierPoint p0 = Points[s];
            BezierPoint p1 = Points[(s + 1) % Points.Count];

            if (p0 == null || p1 == null)
                continue;

            Vector3 prev = p0.transform.position;

            for (int i = 1; i <= stepsPerSegment; i++)
            {
                float t = i / (float)stepsPerSegment;

                Vector3 point = GetBezierPoint(
                    p0.transform.position,
                    p0.GetControl1(),
                    p1.GetControl2(),
                    p1.transform.position,
                    t
                );

                Gizmos.DrawLine(prev, point);
                prev = point;
            }
        }
    }
}