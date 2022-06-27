using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BezierSpline : BezierCurve
{
    [SerializeField]
    private BezierControlPointMode[] m_Modes;

    [SerializeField]
    private bool m_Loop;

    public bool Loop
    {
        get { return m_Loop; }
        set { 
            m_Loop = value; 
            if(value == true)
            {
                m_Modes[m_Modes.Length - 1] = m_Modes[0];
                SetControlPoint(0, m_Points[0]);
            }
        
        }
    }

    public void AddCurve()
    {
        Vector3 point = m_Points[m_Points.Length - 1];
        Array.Resize(ref m_Points, m_Points.Length + 3);
        point.x += 1f;
        m_Points[m_Points.Length - 3] = point;
        point.x += 1f;
        m_Points[m_Points.Length - 2] = point;
        point.x += 1f;
        m_Points[m_Points.Length - 1] = point;

        Array.Resize(ref m_Modes, m_Modes.Length + 1);
        m_Modes[m_Modes.Length - 1] = m_Modes[m_Modes.Length - 2];

        EnforceMode(m_Points.Length - 4);

        if(Loop)
        {
            m_Points[m_Points.Length - 1] = m_Points[0];
            m_Modes[m_Modes.Length - 1] = m_Modes[0];
            EnforceMode(0);
        }
    }

    public override void Reset()
    {
        base.Reset();

        m_Modes = new BezierControlPointMode[]
        {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
    }

    public override int CurveCount
    {
        get { return (m_Points.Length - 1) / 3; }
    }

    public override void SetControlPoint(int index, Vector3 point)
    {
        if(index % 3 == 0)
        {
            Vector3 delta = point - m_Points[index];
            if(Loop)
            {
                if(index == 0)
                {
                    m_Points[1] += delta;
                    m_Points[m_Points.Length - 2] += delta;
                    m_Points[m_Points.Length - 1] = point;
                }
                else if(index == m_Points.Length - 1)
                {
                    m_Points[0] = point;
                    m_Points[1] += delta;
                    m_Points[index - 1] += delta;
                }
                else
                {
                    m_Points[index - 1] += delta;
                    m_Points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    m_Points[index - 1] += delta;
                }

                if (index + 1 < m_Points.Length)
                {
                    m_Points[index + 1] += delta;
                }
            }
        }

        base.SetControlPoint(index, point);
        EnforceMode(index);
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return m_Modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        m_Modes[modeIndex] = mode;

        if(Loop)
        {
            if(modeIndex == 0)
            {
                m_Modes[m_Modes.Length - 1] = mode;
            }
            else if(modeIndex == m_Modes.Length - 1)
            {
                m_Modes[0] = mode;
            }
        }

        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = m_Modes[modeIndex];
        if(mode == BezierControlPointMode.Free || !Loop && (modeIndex == 0 || modeIndex == m_Modes.Length - 1))
        {
            return;
        }

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if(index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if(fixedIndex < 0)
            {
                fixedIndex = m_Points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if(enforcedIndex >= m_Points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if(fixedIndex >= m_Points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if(enforcedIndex < 0)
            {
                enforcedIndex = m_Points.Length - 2;
            }
        }

        Vector3 middle = m_Points[middleIndex];
        Vector3 enforcedTangent = middle - m_Points[fixedIndex];
        if(mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, m_Points[enforcedIndex]);
        }
        m_Points[enforcedIndex] = middle + enforcedTangent;
    }

    private int GetCurveIndex(ref float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = m_Points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        return i;
    }

    public override Vector3 GetPoint(float t)
    {      
        int i = GetCurveIndex(ref t);
        return transform.TransformPoint(Bezier.GetPoint(m_Points[i], m_Points[i + 1], m_Points[i + 2], m_Points[i + 3], t));
    }

    public override Vector3 GetVelocity(float t)
    {
        int i = GetCurveIndex(ref t);
        return transform.TransformPoint(Bezier.GetFirstDerivative(m_Points[i], m_Points[i + 1], m_Points[i + 2], m_Points[i + 3], t)) - 
            transform.position;
    }
}
