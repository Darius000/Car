using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cable
{
    public class CableComponent : MonoBehaviour
    {
        [SerializeField] private Transform m_StartPoint;
        [SerializeField] private Transform m_EndPoint;
        [SerializeField] private Material m_Material;

        [Header("Configuration")]
        [SerializeField] private float m_Length = .5f;
        [SerializeField] private int m_TotalSegements = 5;
        [SerializeField] private float m_SegmentsPerUnit = 2f;
        [SerializeField] private float m_TiledMaterial = 1;

        private int m_Segments = 0;

        [SerializeField] private float m_Width = .1f;
        [SerializeField] private int m_Sides = 6;

        [Header("Solver")]
        [SerializeField] private int m_VerletIterations = 1;
        [SerializeField] private int m_SolverIterations = 1;

        [SerializeField] private bool m_EnableStiffness = false;
        [SerializeField] private bool m_EnableCollision = true;
        [SerializeField] private bool m_UseSubStepping = true;

        [SerializeField] private float m_SubStepTime = 0f;
        [SerializeField] private float m_Friction = 0f;

        [Header("Forces")]
        public Vector3 m_Force;
        public float m_GravityScale = 1f;


        private float m_TimeRemaining = 0f;
        private MeshRenderer m_MeshRenderer;
        
        private CableParticle[] m_Points;


        private CableMesh m_CableMesh;

        private void Start()
        {
            InitCableParticles();
            InitRenderer();
        }

        private void OnDestroy()
        {
            m_Points = null;
        }
        /*
         * Creates cable particles along cable length
         * binds the start and end points
        */

        private void InitCableParticles()
        {
            //Calculate Segements to use
            if(m_TotalSegements > 0)
            {
                m_Segments = m_TotalSegements;
            }
            else
            {
                m_Segments = Mathf.CeilToInt(m_Length * m_SegmentsPerUnit);
            }

            Vector3 direction = (m_EndPoint.position - transform.position).normalized;
            float initialSegmentLength = m_Length / m_Segments;
            m_Points = new CableParticle[m_Segments + 1];

            //Foreach point
            for(int pointIdx = 0; pointIdx <= m_Segments; pointIdx++)
            {
                //Initial position
                Vector3 initialPosition = transform.position + (direction * (initialSegmentLength * pointIdx));
                m_Points[pointIdx] = new CableParticle(initialPosition);
            }

            //Bind start and end particle 
            CableParticle start = m_Points[0];
            CableParticle end = m_Points[m_Segments];
            start.Bind(m_StartPoint);
            end.Bind(m_EndPoint);
        }

        private Vector3[] GetPointPoisitons()
        {
            var points = new Vector3[m_Points.Length];
            for(int i = 0; i < points.Length; i++)
            {
                points[i] = m_Points[i].Position;
            }

            return points;
        }

        private void InitRenderer()
        {
            var points = GetPointPoisitons();


            m_CableMesh = new CableMesh(m_Segments, m_Sides, m_Width, m_TiledMaterial, gameObject);
            m_CableMesh.BuildMesh(points);

            m_MeshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            m_MeshRenderer.material = m_Material;
            m_MeshRenderer.GetComponent<Renderer>().enabled = true;
        }

        #region Render Pass
        private void Update()
        {

            RenderCable();
        }

        void RenderCable()
        {
            if (m_CableMesh == null) return;

            m_CableMesh.ReBuild(GetPointPoisitons());

        }

        #endregion

        #region Verlet intergration & solver

        private void FixedUpdate()
        {
            float UseSubStep = Mathf.Max(m_SubStepTime, .005f);

            m_TimeRemaining += Time.fixedDeltaTime;
            while (m_TimeRemaining > UseSubStep)
            {
                for (int verletIdx = 0; verletIdx < m_VerletIterations; verletIdx++)
                {
                    VerletIntegrate();
                    SolveContraints();

                }

                PerformCableCollision();

                if(m_UseSubStepping)
                {
                    m_TimeRemaining -= UseSubStep;
                }
                else
                {
                    m_TimeRemaining = 0f;
                }
            }


        }

       
        private void VerletIntegrate()
        {
            Vector3 gravityDisplacement = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
            foreach(CableParticle particle in m_Points)
            {
                particle.UpdateVerlet(gravityDisplacement, m_GravityScale, m_SubStepTime , m_Force);
            }
        }

        private void SolveContraints()
        {
            for(int iterationIdx = 0; iterationIdx < m_SolverIterations; iterationIdx++)
            {
                SolveDistanceContraint();
                SolveStiffnessContraint();
            }
        }


        #endregion

        #region Solver Contraints

        void SolveDistanceContraint()
        {
            float segmentLength = m_Length / m_Segments;
            for(int segIdx = 0; segIdx < m_Segments; segIdx++)
            {
                CableParticle a = m_Points[segIdx];
                CableParticle b = m_Points[segIdx + 1];

                //solve for this pair of particles
                SolveDistanceContraint(a , b, segmentLength);
            }
        }

        void SolveDistanceContraint(CableParticle a, CableParticle b, float segmentLength)
        {
            Vector3 delta = b.Position - a.Position;

            float currentDistance = delta.magnitude;

            float errorFactor = (currentDistance - segmentLength) / currentDistance;

            //only move free particles to satisfy contraints
            if(a.IsFree() && b.IsFree())
            {
                a.Position += errorFactor * .5f * delta;
                b.Position -= errorFactor * .5f * delta;
            }
            else if(a.IsFree())
            {
                a.Position += errorFactor * delta;
            }
            else if(b.IsFree())
            {
                b.Position -= errorFactor * delta;
            }
        }

        void SolveStiffnessContraint()
        {
            if(!m_EnableStiffness) return;
            float segmentLength = m_Length / m_Segments;
            for (int segIdx = 0; segIdx < m_Segments - 1; segIdx++)
            {
                CableParticle a = m_Points[segIdx];
                CableParticle b = m_Points[segIdx + 2];
                SolveDistanceContraint(a, b, 2f * segmentLength);
            }
        }

        void PerformCableCollision()
        {
            if (!m_EnableCollision) return;

            foreach(CableParticle particle in m_Points)
            {
                if(particle.IsFree())
                {
                    
                    RaycastHit hit;
 
                    var origin = particle.OldPosition;
                    var direction = Vector3.down;
                    var radius = .5f * m_Width;

                    if (Physics.SphereCast(origin, radius, direction, out hit, radius))
                    {
                        particle.Position = hit.point;

                        //find velocity
                        Vector3 delta = particle.Position - particle.OldPosition;

                        float normalDelta = Vector3.Dot(hit.normal, delta);

                        Vector3 planeDelta = delta - (normalDelta * hit.normal);

                        particle.OldPosition += (normalDelta * hit.normal);

                        if(m_Friction > 1e-4f)
                        {
                            Vector3 scaledPlaneDelta = planeDelta * m_Friction;
                            particle.OldPosition += scaledPlaneDelta;
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_Points != null)
            {
                foreach (CableParticle particle in m_Points)
                {
                    Gizmos.DrawWireSphere(particle.Position, m_Width);
                }
            }
            
        }

        #endregion
    }
}
