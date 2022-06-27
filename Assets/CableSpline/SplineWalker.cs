using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SplineWalkerMode
{
    Once,
    Loop,
    PingPong
}

public class SplineWalker : MonoBehaviour
{
    public BezierSpline m_Spline;

    public SplineWalkerMode m_SplineMode;

    public float m_Duration;

    public bool m_LookFoward;

    private float m_Progress;

    private bool m_GoingFoward = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_GoingFoward)
        {
            m_Progress += Time.deltaTime / m_Duration;
            if (m_Progress > 1f)
            {
                if(m_SplineMode == SplineWalkerMode.Once)
                {
                    m_Progress = 1f;
                }
                else if(m_SplineMode == SplineWalkerMode.Loop)
                {
                    m_Progress -= 1f;
                }
                else
                {
                    m_Progress = 2f - m_Progress;
                    m_GoingFoward = false;
                }
            }
        }
        else
        {
            m_Progress -= Time.deltaTime / m_Duration;
            if(m_Progress < 0f)
            {
                m_Progress = -m_Progress;
                m_GoingFoward = true;
            }
        }

        Vector3 position = m_Spline.GetPoint(m_Progress);
        transform.localPosition = position;
        if(m_LookFoward)
        {
            transform.LookAt(position + m_Spline.GetDirection(m_Progress));
        }
    }
}
