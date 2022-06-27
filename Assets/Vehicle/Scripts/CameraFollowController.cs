using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    public void LookAtTarget()
    {
        Vector3 lookDirection = m_ObjectToFollow.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, m_LookSpeed * Time.deltaTime);
    }

    public void MoveToTarget()
    {
        Vector3 targetPos = m_ObjectToFollow.position + m_ObjectToFollow.forward * m_Offset.z +
            m_ObjectToFollow.right * m_Offset.x + m_ObjectToFollow.up * m_Offset.y;

        transform.position = Vector3.Lerp(transform.position, targetPos, m_FollowSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        LookAtTarget();
        MoveToTarget();
    }

    public Transform m_ObjectToFollow;
    public Vector3 m_Offset;
    public float m_FollowSpeed = 10f;
    public float m_LookSpeed = 10f;
}
