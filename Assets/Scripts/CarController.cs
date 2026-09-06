using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float forwardAccel = 1000f;
    public float reverseAccel = 500f;
    public float maxSpeed = 25f; // In m/s (approx 90 km/h)
    public float turnStrength = 100f;
    public float gravityForce = 20f;

    [Header("Physics")]
    public float dragOnGround = 3f;
    public float maxWheelTurn = 30f;

    private float speedInput;
    private float turnInput;
    private Rigidbody carRigidbody;

    public float driftDragMultiplier = 0.3f; // Lower drag = more sliding

    public float driftTurnMultiplier = 1.5f; // Sharper turn during drift

    private bool isHandbraking;
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;
    public float wheelRotationSpeed = 300f; // Speed at which visuals spin

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        
        // Optional: Freeze X and Z rotation so the car doesn't flip over easily
        if (carRigidbody != null)
        {
            carRigidbody.freezeRotation = true;
        }
    }

    void Update()
    {
        GetInput();
        UpdateWheels();
    }

    void UpdateWheels()
    {
        float rollAmount = speedInput * wheelRotationSpeed * Time.deltaTime;
        float steerAngle = turnInput * maxWheelTurn;

        // Rotate and steer Front Left Wheel
        if (frontLeftWheelTransform != null)
        {
            // Rotate around local X for rolling, and steer around local Y
            frontLeftWheelTransform.localRotation = Quaternion.Euler(
                frontLeftWheelTransform.localEulerAngles.x + rollAmount, 
                steerAngle, 
                0f
            );
        }

        // Rotate and steer Front Right Wheel
        if (frontRightWheelTransform != null)
        {
            frontRightWheelTransform.localRotation = Quaternion.Euler(
                frontRightWheelTransform.localEulerAngles.x + rollAmount, 
                steerAngle, 
                0f
            );
        }

        // Rotate Rear Left Wheel (Spin only, no steering)
        if (rearLeftWheelTransform != null)
        {
            rearLeftWheelTransform.Rotate(Vector3.right, rollAmount, Space.Self);
        }

        // Rotate Rear Right Wheel (Spin only, no steering)
        if (rearRightWheelTransform != null)
        { 
            rearRightWheelTransform.Rotate(Vector3.right, rollAmount, Space.Self);
        }
    }

    void FixedUpdate()
    {
        if (carRigidbody == null) return;

        ApplyMotor();
        ApplySteering();
        ApplyPhysics();
    }

    void GetInput()
    {
        speedInput = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            speedInput = 1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            speedInput = -1f;
        }

        turnInput = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            turnInput = -1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            turnInput = 1f;
        }

        isHandbraking = Input.GetKey(KeyCode.Space);
    }

    void ApplyMotor()
    {
        float currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f; // Convert to km/h for check

        // Apply force forward or backward relative to car orientation
        if (speedInput != 0)
        {
            float accel = speedInput > 0 ? forwardAccel : reverseAccel;
            carRigidbody.AddRelativeForce(Vector3.forward * speedInput * accel);
        }
    }

    void ApplySteering()
    {
        // Get current speed magnitude
        float currentSpeed = carRigidbody.linearVelocity.magnitude;

        // Only allow turning if the car is actually moving (e.g., speed > 0.1)
        if (currentSpeed < 0.1f)
        {
            return; // Exit the method early, preventing rotation when stationary
        }

        float moveDirection = Vector3.Dot(carRigidbody.linearVelocity, transform.forward);
        float effectiveTurnInput = turnInput;
    
        // Invert steering when moving backward so left stays left on screen
        if (moveDirection < -0.1f)
        {
            effectiveTurnInput = -turnInput;
        }

        float turnAmount = effectiveTurnInput * turnStrength * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        carRigidbody.MoveRotation(carRigidbody.rotation * turnRotation);
    }

    void ApplyPhysics()
    {
        // Apply lower drag when drifting so the car maintains momentum and slides
        float currentDrag = dragOnGround;
        if (isHandbraking)
        {
            currentDrag *= driftDragMultiplier;
        }

        carRigidbody.linearDamping = currentDrag;
        carRigidbody.AddForce(-transform.up * gravityForce * carRigidbody.mass);
    }
}