using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineDecorator : MonoBehaviour
{
    public BezierSpline m_Spline;

    public int m_Frequency;

    public bool m_LookFoward;

    public Transform[] m_Items;

    private void Awake()
    {
        if(m_Frequency <= 0 || m_Items == null || m_Items.Length == 0)
        {
            return;
        }

        float stepSize = m_Frequency * m_Items.Length;
        if(m_Spline.Loop || stepSize == 1)
        {
            stepSize = 1f / stepSize;
        }
        else
        {
            stepSize = 1f / (stepSize - 1f);
        }

        for(int p = 0, f = 0; f < m_Frequency; f++)
        {
            for(int i = 0; i < m_Items.Length; i++, p++)
            {
                Transform item = Instantiate(m_Items[i]) as Transform;
                Vector3 position = m_Spline.GetPoint(p * stepSize);
                item.transform.localPosition = position;
                if(m_LookFoward)
                {
                    item.transform.LookAt(position + m_Spline.GetDirection(p * stepSize));
                }

                item.transform.parent = transform;
            }
        }
    }
}
