using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cable
{
    internal class CableParticle
    {
        private Vector3 m_Position, m_OldPosition;
        private Transform m_BoundTo = null;
        private Rigidbody m_BoundRigidBody = null;

        public Vector3 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public Vector3 OldPosition
        {
            get { return m_OldPosition; }
            set { m_OldPosition = value; }
        }

        public Vector3 Velocity
        {
            get { return ( m_Position - m_OldPosition ); }
        }

        public CableParticle(Vector3 newPosition)
        {
            m_OldPosition = m_Position = newPosition;
        }

        public void UpdateVerlet(Vector3 gravityDisplacement, float gravityScale, float subTimeStep, Vector3 force)
        {
            if(this.IsBound())
            {
                if(m_BoundRigidBody == null)
                {
                    this.UpdatePosition(m_BoundTo.position);
                }
                else
                {
                    switch(m_BoundRigidBody.interpolation)
                    {
                        case RigidbodyInterpolation.Interpolate:
                            this.UpdatePosition(m_BoundRigidBody.position + (m_BoundRigidBody.velocity * Time.fixedDeltaTime) / 2);
                            break;
                        case RigidbodyInterpolation.None:
                        default:
                            this.UpdatePosition(m_BoundRigidBody.position + m_BoundRigidBody.velocity * Time.fixedDeltaTime);
                            break;
                    }
                }
            }
            else
            {
                var particleForce = gravityDisplacement + force;
                var subTimeStepSqr = subTimeStep * subTimeStep;
                Vector3 newPosition = this.Position + this.Velocity + (subTimeStepSqr * particleForce);
                this.UpdatePosition(newPosition);
            }
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            m_OldPosition = m_Position;
            m_Position = newPosition;
        }

        public void Bind(Transform to)
        {
            m_BoundTo = to;
            m_BoundRigidBody = to.GetComponent<Rigidbody>();
            m_OldPosition = m_Position = m_BoundTo.position;
        }

        public void UnBind()
        {
            m_BoundTo = null;
            m_BoundRigidBody = null;
        }

        public bool IsFree()
        {
            return m_BoundTo == null;
        }

        public bool IsBound()
        {
            return m_BoundTo != null;
        }
    }
}
