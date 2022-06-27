using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    SimpleCarController m_CarController;

    public void Initalize(SimpleCarController controller)
    {
        m_CarController = controller;
    }

    public float m_MinSpeedArrowAngle;
    public float m_MaxSpeedArrowAngle;

    [Header("UI")]
    public TMPro.TMP_Text m_SpeedLabel; //the label that displays speed
    public TMPro.TMP_Text m_DistanceLabel; //label that displays miles
    public RectTransform m_Arrow; // the arrow in speedometer

    protected void Update()
    {
        if (m_CarController == null) return;

        var speed = m_CarController.GetSpeed();
        var maxSpeed = m_CarController.GetMaxSpeed();

        if (m_SpeedLabel)
        {
            m_SpeedLabel.text = ((int)speed) + " km/h";

            
        }

        if (m_Arrow)
        {
            m_Arrow.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(m_MinSpeedArrowAngle, m_MaxSpeedArrowAngle, speed / maxSpeed));
        }

        if(m_DistanceLabel)
        {
            m_DistanceLabel.text = ((int)m_CarController.GetDistanceTraveled() / 1000f) + "\n km";
        }
    }

}
