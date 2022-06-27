using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class SimpleCarController : MonoBehaviour
{
    public List<AxelInfo> m_AxelInfos;//info about each axel
    public float m_MaxMotorTorque; // max torque the motor can apply to wheel
    public SteeringWheel m_SteeringWheel; // maximum steer angle the wheel can have

    public float m_BrakingForce = 300.0f;

    public float m_LiftCoefficient = 10; // use negative values for downforce

    public Vector3 m_CenterOfMassOffset;

    public Speedometer m_SpeedometerPrefab;

    [SerializeField]
    protected float m_MaxSpeed = 260f;

    [Header("Fuel")]
    public float m_CurrentFuel = 100f;
    public float m_MaxFuel = 100f;
    public float m_MaxFuelConsumptionRate;
    
    private float m_CurrentBrakingForce = 0f;

    private float m_Vertical;
    private float m_Horizontal;

    private Rigidbody m_Rigidbody;

    private float m_DistanceTraveled = 0f;
    private Vector3 m_LastPosition = new Vector3();
    private float m_FuelConsumptionRate ;


    protected void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        if(m_Rigidbody)
        {
            m_Rigidbody.centerOfMass += m_CenterOfMassOffset;
        }

        if(m_SpeedometerPrefab)
        {
            var obj = Instantiate<GameObject>(m_SpeedometerPrefab.gameObject);
            var speedometer = obj.GetComponent<Speedometer>();
            speedometer.Initalize(this);

            var fuelGauge = obj.GetComponent<FuelGuage>();
            fuelGauge.Initalize(this);
        }
    }

    void CalculateDistanceTraveled()
    {
        m_DistanceTraveled += Vector3.Distance(transform.position, m_LastPosition);
        m_LastPosition = transform.position;
    }

    protected void ApplyLocalPositionToVisuals(WheelCollider collider, Transform visualWheel)
    {
        if(visualWheel == null) return;

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position ;
        visualWheel.transform.rotation = rotation;
    }

    public float GetSpeed()
    {
        return m_Rigidbody.velocity.magnitude * 3.6f;
    }

    public bool IsStopped()
    {
        return m_Rigidbody.velocity == Vector3.zero;
    }

    public float GetMaxSpeed() { return m_MaxSpeed; }

    public float GetDistanceTraveled() {  return m_DistanceTraveled; }

    public float FuelLeft { get { return m_CurrentFuel / m_MaxFuel; } }

    protected bool HasFuel() { return m_CurrentFuel > 0f; }

    private void UseFuel()
    {
        m_FuelConsumptionRate = GetSpeed() / 20f;
        if (m_FuelConsumptionRate > m_MaxFuelConsumptionRate)
            m_FuelConsumptionRate = m_MaxFuelConsumptionRate;

        m_CurrentFuel -= m_FuelConsumptionRate;
        
        if(m_CurrentFuel < 0f)
        {
            m_CurrentFuel = 0f;
        }
    }

    public void FixedUpdate()
    {
        if (!m_SteeringWheel) return;

        float motor = m_MaxMotorTorque * m_Vertical;
        float steering = m_SteeringWheel.GetSteeringAngle();

        foreach (AxelInfo axel in m_AxelInfos)
        {
            if(axel.m_Steering)
            {
                axel.m_LeftWheel.steerAngle = steering;
                axel.m_RightWheel.steerAngle = steering;
            }

            if(axel.m_Motor )
            {
                if(GetSpeed() < GetMaxSpeed() && HasFuel())
                {
                    axel.m_LeftWheel.motorTorque = motor;
                    axel.m_RightWheel.motorTorque = motor;
                }
                else
                {
                    axel.m_LeftWheel.motorTorque = 0;
                    axel.m_RightWheel.motorTorque = 0;
                }
                
            }

            axel.m_LeftWheel.brakeTorque = m_CurrentBrakingForce;
            axel.m_RightWheel.brakeTorque = m_CurrentBrakingForce;


            ApplyLocalPositionToVisuals(axel.m_LeftWheel, axel.m_VisualLeftWheel);
            ApplyLocalPositionToVisuals(axel.m_RightWheel, axel.m_VisualRightWheel); 
        }

        ApplyDownPressure();

        
    }

    protected void Update()
    {
        var speed = GetSpeed();
        if (speed > m_MaxSpeed)
        {
            m_Rigidbody.velocity = Vector3.ClampMagnitude(m_Rigidbody.velocity, m_MaxSpeed);
            
        }

        if (speed > 0f)
            CalculateDistanceTraveled();

        //Consume fuel
        if ((m_Horizontal != 0f || m_Vertical != 0f) && HasFuel())
        {
            UseFuel();
        }

    }

    private void ApplyDownPressure()
    {
        if(m_Rigidbody)
        {
            float lift = -m_LiftCoefficient * m_Rigidbody.velocity.sqrMagnitude;
            m_Rigidbody.AddForceAtPosition(lift * transform.up, transform.position);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {

        var value = context.ReadValue<Vector2>();
        m_Horizontal = value.x;
        m_Vertical = value.y;   
    }

   public void ApplyBrakes(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            m_CurrentBrakingForce = m_BrakingForce;
        }
        else
        {
            m_CurrentBrakingForce = 0f;
        }

        Debug.Log(m_CurrentBrakingForce);
   }
}

[System.Serializable]
public class AxelInfo
{
    public WheelCollider m_LeftWheel;
    public WheelCollider m_RightWheel;

    public Transform m_VisualLeftWheel;
    public Transform m_VisualRightWheel;

    public bool m_Motor;//is this wheel attached to motor?
    public bool m_Steering;//does this wheel apply sterring angle?
}


