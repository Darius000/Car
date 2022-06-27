using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    [SerializeField]
    protected Vector3[] m_Points;

    public int GetControlPointCount
    {
        get { return m_Points.Length; }
    }

    public Vector3 GetControlPoint(int index)
    {
        return m_Points[index];
    }

    public virtual void SetControlPoint(int index, Vector3 point)
    {
        m_Points[index] = point;
    }

    public virtual int CurveCount
    {
        get { return 1; }
    }

    public virtual void Reset()
    {
        m_Points = new Vector3[]
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 1f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
    }

    public virtual Vector3 GetPoint(float t)
    {
        return transform.TransformPoint(Bezier.GetPoint(m_Points[0], m_Points[1], m_Points[2], m_Points[3], t));
    }

    public virtual Vector3 GetVelocity(float t)
    {
        return transform.TransformPoint(Bezier.GetFirstDerivative(m_Points[0], m_Points[1], m_Points[2], m_Points[3], t)) -
            transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }
}
