using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FuelTank : MonoBehaviour
{
    [field : SerializeField]
    public float refuelRate { get; set; } = .1f;

    public GameObject m_ConfirmationUI;

    private GameObject m_Interactee;

    private static bool m_IsConfirmationShown;

    private bool m_IsRefueling = false;

    private void Refuel()
    {
        m_IsRefueling = true;
    }

    protected void Update()
    {
        if(m_Interactee)
        {
            var rb = m_Interactee.GetComponentInChildren<SimpleCarController>();

            if(!rb)
            {
                rb = m_Interactee.GetComponentInParent<SimpleCarController>();
            }

            if (rb)
            {
                if(rb && rb.IsStopped()  && !m_IsConfirmationShown && !m_IsRefueling)
                {
                    ShowConformationUI(m_Interactee);
                }
            }

            if(m_IsRefueling)
            {
                if(rb.IsStopped())
                {
                    rb.m_CurrentFuel += refuelRate * Time.deltaTime;
                }
                else
                {
                    m_IsRefueling = false;
                }
            }
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if(other.gameObject)
        {
            m_Interactee = other.gameObject;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if(m_Interactee == other.gameObject)
        {
            m_Interactee = null;
        }
    }

    private void ShowConformationUI(GameObject interactee)
    {
        var obj = Instantiate(m_ConfirmationUI);
        var confirmation = obj.GetComponent<Conformation>();
        confirmation.SetInteractee(interactee);
        confirmation.OnAcceptEvent.AddListener(OnConfirm);
        m_IsConfirmationShown = true;
        confirmation.OnDestroyEvent.AddListener(() => { 
            m_IsConfirmationShown = false;
        });
    }

    protected void OnConfirm()
    {
        Refuel();
    }
}
