using UnityEngine;

public class mobil : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentBrakeForce;
    private bool isBraking;

    [Header("Truck Settings")]
    [SerializeField] private float motorForce = 3500f;
    [SerializeField] private float brakeForce = 6000f;
    [SerializeField] private float maxSteerAngle = 28f;
    [SerializeField] private float maxSpeed = 120f;

    [Header("Truck Physics")]
    [SerializeField] private float downforce = 150f;
    [SerializeField] private Transform centerOfMass;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("Wheel Meshes")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    private Rigidbody rb;

    [HideInInspector] public bool engineOn = false;

    // ==========================
    // INPUT BUTTON MOBILE
    // ==========================
    private bool gasButtonPressed = false;
    private bool reverseButtonPressed = false;
    private bool brakeButtonPressed = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.mass = 4000f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (centerOfMass != null)
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        else
            rb.centerOfMass = new Vector3(0f, -1.2f, 0f);

        SetupWheelFriction(frontLeftWheelCollider);
        SetupWheelFriction(frontRightWheelCollider);
        SetupWheelFriction(rearLeftWheelCollider);
        SetupWheelFriction(rearRightWheelCollider);
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        ApplyDownforce();
        UpdateWheels();
    }

    private void GetInput()
    {
        // Horizontal: gabung keyboard + steering wheel
        float keyboardHorizontal = Input.GetAxis("Horizontal");

        if (Mathf.Abs(steerInput) > 0.01f)
            horizontalInput = steerInput;
        else
            horizontalInput = keyboardHorizontal;

        // Vertikal dan rem tidak berubah
        float keyboardVertical = Input.GetAxis("Vertical");

        if (gasButtonPressed)
            verticalInput = 1f;
        else if (reverseButtonPressed)
            verticalInput = -1f;
        else
            verticalInput = keyboardVertical;

        isBraking = Input.GetKey(KeyCode.Space) || brakeButtonPressed;
    }

    private void HandleMotor()
    {
        float speed = rb.linearVelocity.magnitude * 3.6f;

        if (engineOn && speed < maxSpeed)
        {
            rearLeftWheelCollider.motorTorque = verticalInput * motorForce;
            rearRightWheelCollider.motorTorque = verticalInput * motorForce;
        }
        else
        {
            rearLeftWheelCollider.motorTorque = 0f;
            rearRightWheelCollider.motorTorque = 0f;
        }

        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downforce * rb.linearVelocity.magnitude);
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;

        wheelCollider.GetWorldPose(out pos, out rot);

        wheelTransform.position = pos;

        if (wheelTransform.name.Contains("Right"))
            wheelTransform.rotation = rot * Quaternion.Euler(0f, 180f, 0f);
        else
            wheelTransform.rotation = rot;
    }

    private void SetupWheelFriction(WheelCollider wheel)
    {
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        forwardFriction.stiffness = 1.7f;
        wheel.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.stiffness = 2.2f;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    // ==================================================
    // DIGUNAKAN OLEH TruckEngine
    // ==================================================
    public float GetVerticalInput()
    {
        return verticalInput;
    }

    public bool IsBraking()
    {
        return isBraking;
    }

    public bool IsGasPressed()
    {
        return gasButtonPressed || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
    }

    public bool IsReversePressed()
    {
        return reverseButtonPressed || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
    }

    public bool IsBrakePressed()
    {
        return brakeButtonPressed || Input.GetKey(KeyCode.Space);
    }

    private float steerInput = 0f;

    public void SetSteerInput(float value)
    {
        steerInput = value;
    }

    // ==================================================
    // BUTTON GAS
    // ==================================================
    public void GasDown()
    {
        gasButtonPressed = true;
    }

    public void GasUp()
    {
        gasButtonPressed = false;
    }

    // ==================================================
    // BUTTON MUNDUR
    // ==================================================
    public void ReverseDown()
    {
        reverseButtonPressed = true;
    }

    public void ReverseUp()
    {
        reverseButtonPressed = false;
    }

    // ==================================================
    // BUTTON REM
    // ==================================================
    public void BrakeDown()
    {
        brakeButtonPressed = true;
    }

    public void BrakeUp()
    {
        brakeButtonPressed = false;
    }
}