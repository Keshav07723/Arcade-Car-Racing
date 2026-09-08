using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float forwardAccel = 1000f;
    public float reverseAccel = 500f;
    public float maxSpeed = 25f; 
    public float turnStrength = 100f;
    public float gravityForce = 20f;

    [Header("Physics & Friction")]
    public float dragOnGround = 3f;
    public float maxWheelTurn = 30f;
    public float normalFriction = 15f;  // Keeps the car firmly gripping the road

    [Header("Visual Wheels")]
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;
    public float wheelRotationSpeed = 300f;

    private float speedInput;
    private float turnInput;
    private Rigidbody carRigidbody;

    private Quaternion flStartRot, frStartRot, rlStartRot, rrStartRot;
    private float flRoll = 0f, frRoll = 0f, rlRoll = 0f, rrRoll = 0f;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        
        if (carRigidbody != null)
        {
            carRigidbody.freezeRotation = true;
            carRigidbody.linearDamping = dragOnGround;
            carRigidbody.centerOfMass = new Vector3(0f, -0.5f, 0f);
            carRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        if (frontLeftWheelTransform != null) flStartRot = frontLeftWheelTransform.localRotation;
        if (frontRightWheelTransform != null) frStartRot = frontRightWheelTransform.localRotation;
        if (rearLeftWheelTransform != null) rlStartRot = rearLeftWheelTransform.localRotation;
        if (rearRightWheelTransform != null) rrStartRot = rearRightWheelTransform.localRotation;
    }

    void Update()
    {
        GetInput();
        UpdateWheels();
    }

    void FixedUpdate()
    {
        if (carRigidbody == null) return;

        ApplyMotor();
        ApplySteering();
        ApplyFriction();
        ApplyPhysics();
    }

    void GetInput()
    {
        speedInput = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) speedInput = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) speedInput = -1f;

        turnInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) turnInput = 1f;
    }

    void ApplyMotor()
    {
        float currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f;

        if (speedInput != 0 && currentSpeed < maxSpeed * 3.6f)
        {
            float accel = speedInput > 0 ? forwardAccel : reverseAccel;
            carRigidbody.AddRelativeForce(Vector3.forward * speedInput * accel);
        }
    }

    void ApplySteering()
    {
        float currentSpeed = carRigidbody.linearVelocity.magnitude;
        if (currentSpeed < 0.1f) return; 

        float moveDirection = Vector3.Dot(carRigidbody.linearVelocity, transform.forward);
        float effectiveTurnInput = turnInput;
    
        if (moveDirection < -0.1f) effectiveTurnInput = -turnInput;

        float turnAmount = effectiveTurnInput * turnStrength * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        carRigidbody.MoveRotation(carRigidbody.rotation * turnRotation);
    }

    void ApplyFriction()
    {
        // Keeps tires locked to driving direction cleanly without sliding or shaking
        Vector3 forwardVel = transform.forward * Vector3.Dot(carRigidbody.linearVelocity, transform.forward);
        Vector3 rightVel = transform.right * Vector3.Dot(carRigidbody.linearVelocity, transform.right);

        carRigidbody.linearVelocity = forwardVel + Vector3.Lerp(rightVel, Vector3.zero, normalFriction * Time.fixedDeltaTime);
    }

    void ApplyPhysics()
    {
        carRigidbody.linearDamping = dragOnGround;
        carRigidbody.AddForce(-transform.up * gravityForce * carRigidbody.mass);
    }

    void UpdateWheels()
    {
        float rollDelta = speedInput * wheelRotationSpeed * Time.deltaTime;
        float steerAngle = turnInput * maxWheelTurn;

        if (frontLeftWheelTransform != null)
        {
            flRoll += rollDelta;
            frontLeftWheelTransform.localRotation = flStartRot * Quaternion.Euler(0f, steerAngle, 0f) * Quaternion.Euler(flRoll, 0f, 0f);
        }

        if (frontRightWheelTransform != null)
        {
            frRoll += rollDelta;
            frontRightWheelTransform.localRotation = frStartRot * Quaternion.Euler(0f, steerAngle, 0f) * Quaternion.Euler(frRoll, 0f, 0f);
        }

        if (rearLeftWheelTransform != null)
        {
            rlRoll += rollDelta;
            rearLeftWheelTransform.localRotation = rlStartRot * Quaternion.Euler(rlRoll, 0f, 0f);
        }

        if (rearRightWheelTransform != null)
        { 
            rrRoll += rollDelta;
            rearRightWheelTransform.localRotation = rrStartRot * Quaternion.Euler(rrRoll, 0f, 0f);
        }
    }
}