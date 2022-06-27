using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuelGuage : MonoBehaviour
{
    public Slider m_Slider;
    public TMPro.TMP_Text m_PercentageLabel;

    SimpleCarController m_CarController;

    public void Initalize(SimpleCarController controller)
    {
        m_CarController = controller;
    }

    // Update is called once per frame
    protected void Update()
    {
        if(m_CarController)
        {
            var fuelPercentage = m_CarController.FuelLeft;
            
            if(m_Slider)
            {
                m_Slider.value = fuelPercentage;
            }

            if(m_PercentageLabel)
            {
                m_PercentageLabel.text = string.Format("{0:0.00} %", fuelPercentage * 100f);
            }
        }
    }
}
