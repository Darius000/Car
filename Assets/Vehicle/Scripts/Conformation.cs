using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Conformation : MonoBehaviour
{
    public UnityEvent OnAcceptEvent;
    public UnityEvent OnRejectEvent;
    public UnityEvent OnDestroyEvent;

    private GameObject m_Interactee;

    private void Start()
    {
       
    }

    protected void OnDestroy()
    {
        OnDestroyEvent?.Invoke();
    }

    public void SetInteractee(GameObject interactee)
    {
        m_Interactee = interactee;
    }

    public void Confirm(bool accept)
    {
        if (accept)
        {
            OnAcceptEvent?.Invoke();
        }
        else
        {
            OnRejectEvent?.Invoke();
        }

        Destroy(gameObject);
    }
}