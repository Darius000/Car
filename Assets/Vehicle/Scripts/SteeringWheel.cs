using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheel : MonoBehaviour
{
    [SerializeField]
    protected float m_MaxSteeringAngle = 30f;

    private float m_Horizontal = 0f;

    private float m_StartRotation;

    protected void Start()
    {
        m_StartRotation = transform.localRotation.eulerAngles.z;

        //Debug.Log(m_StartRotation);
        //Debug.Log(transform.rotation.eulerAngles.z);
    }

    protected void Update()
    {
        var turnAngle = GetSteeringAngle();

        if(m_Horizontal != 0f)
        {
            LerpAngle(m_StartRotation, turnAngle, 1f);
        }
        else 
        {
            if(m_StartRotation != transform.localRotation.eulerAngles.z)
            {
                LerpAngle(transform.localRotation.eulerAngles.z, m_StartRotation, 1f);
            }
            
        }

    }


    public void TurnWheel(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        m_Horizontal = value.x;
    }

    public float GetSteeringAngle()
    {
        return m_Horizontal * m_MaxSteeringAngle;
    }

    void LerpAngle(float start,float end, float duration)
    {
        float time = 0;
        float startValue = start;
        while(time < duration)
        {
            float angle = Mathf.LerpAngle(startValue, end, time / duration);
            transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -angle));
            time += Time.deltaTime;
        }

        transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -end));
    }
}