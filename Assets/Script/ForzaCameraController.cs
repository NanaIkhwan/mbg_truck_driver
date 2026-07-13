using UnityEngine;
using UnityEngine.EventSystems;

public class ForzaCameraController : MonoBehaviour
{
    // ── REFERENCES ────────────────────────────────────────────────
    [Header("Target")]
    public Rigidbody carRigidbody;
    public Transform carTarget;

    // ── POSITION ──────────────────────────────────────────────────
    [Header("Position")]
    public float distanceBehind = 7f;
    public float heightAbove = 2.8f;

    [Range(0.05f, 0.5f)]
    public float positionSmoothTime = 0.18f;

    // ── ROTATION ──────────────────────────────────────────────────
    [Header("Rotation")]
    [Range(1f, 12f)]
    public float rotationSmoothSpeed = 4f;

    [Range(0f, 1f)]
    public float lookAheadStrength = 0.25f;

    // ── ORBIT FREE LOOK ───────────────────────────────────────────
    [Header("Orbit Free Look")]
    public KeyCode freeLookKey = KeyCode.LeftAlt;
    public float mouseSensitivity = 3f;
    public float touchSensitivity = 0.15f; // sensitivitas drag jari di Android

    [Tooltip("Batas pitch orbit (derajat).")]
    public float pitchMin = -20f;
    public float pitchMax = 50f;

    [Tooltip("Kecepatan kamera kembali ke belakang truk setelah freelook.")]
    [Range(0.5f, 8f)]
    public float returnSpeed = 2f;

    [Tooltip("Kecepatan minimum truk (m/s) agar kamera mulai auto-return.")]
    public float returnSpeedThreshold = 1f;

    [Tooltip("Delay (detik) setelah jari dilepas sebelum kamera mulai balik.")]
    public float returnDelay = 0.8f;

    public bool hideCursor = true;

    // ── FOV ───────────────────────────────────────────────────────
    [Header("Speed FOV")]
    public float baseFOV = 60f;
    public float maxFOVAdd = 12f;
    public float maxSpeedForFOV = 55f;

    [Range(1f, 6f)]
    public float fovSmoothSpeed = 3f;

    // ── DRIFT LEAN ────────────────────────────────────────────────
    [Header("Drift Lean")]
    [Range(0f, 8f)]
    public float maxLeanAngle = 3.5f;

    [Range(1f, 8f)]
    public float leanSmoothSpeed = 3f;

    // ── SHAKE ─────────────────────────────────────────────────────
    [Header("Speed Shake")]
    [Range(0f, 0.04f)]
    public float shakeIntensity = 0.012f;
    public float shakeFrequency = 9f;

    // ── PRIVATE ───────────────────────────────────────────────────
    Camera _cam;
    float _currentFOV;
    float _currentLean;
    float _shakeTimer;
    Quaternion _currentYawRot;
    Vector3 _smoothVelocity;
    Vector3 _basePosition;

    bool _isOrbiting;
    float _orbitYaw;
    float _orbitPitch;

    float _returnTimer = 0f;       // countdown sebelum auto-return aktif
    bool _isReturning = false;     // sedang dalam proses return ke belakang truk

    // Touch tracking untuk Android
    #pragma warning disable 0414
    int _activeTouchId = -1;
    Vector2 _lastTouchPos;
    #pragma warning restore 0414

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = GetComponentInChildren<Camera>();

        if (carRigidbody != null &&
            carRigidbody.interpolation == RigidbodyInterpolation.None)
        {
            carRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Start()
    {
        SnapToTarget();
        _currentFOV = baseFOV;
    }

    void Update()
    {
        // ── Block input saat UI atau steering aktif ───────────────
        bool touchingUI = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (EventSystem.current != null)
            touchingUI = EventSystem.current.IsPointerOverGameObject();
#else
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
            {
                touchingUI = true;
                break;
            }
        }
#endif

        if (touchingUI || SteeringWheel.IsBeingUsed)
        {
            // Jangan reset _isOrbiting agar tidak snap saat jari geser UI
            return;
        }

        float deltaYaw = 0f;
        float deltaPitch = 0f;
        bool userDragging = false;

        // ── PC: mouse drag ────────────────────────────────────────
#if UNITY_EDITOR || UNITY_STANDALONE
        bool holding = Input.GetMouseButton(1) || Input.GetKey(freeLookKey);
        if (holding)
        {
            deltaYaw = Input.GetAxis("Mouse X") * mouseSensitivity;
            deltaPitch = -Input.GetAxis("Mouse Y") * mouseSensitivity;
            userDragging = true;

            if (!_isOrbiting && hideCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else if (_isOrbiting && hideCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
#endif

        // ── Android: satu jari drag di area non-UI ────────────────
#if !UNITY_EDITOR && !UNITY_STANDALONE
        if (_activeTouchId == -1)
        {
            // Cari jari baru yang tidak menyentuh UI
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began &&
                    !EventSystem.current.IsPointerOverGameObject(t.fingerId) &&
                    !SteeringWheel.IsBeingUsed)
                {
                    _activeTouchId  = t.fingerId;
                    _lastTouchPos   = t.position;
                    break;
                }
            }
        }
        else
        {
            // Tracking jari aktif
            bool found = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId == _activeTouchId)
                {
                    found = true;
                    if (t.phase == TouchPhase.Moved)
                    {
                        Vector2 delta = t.position - _lastTouchPos;
                        deltaYaw      =  delta.x * touchSensitivity;
                        deltaPitch    = -delta.y * touchSensitivity;
                        userDragging  = true;
                    }
                    _lastTouchPos = t.position;

                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        _activeTouchId = -1;
                        found = false;
                    }
                    break;
                }
            }
            if (!found) _activeTouchId = -1;
        }
#endif

        // ── Handle orbit state ────────────────────────────────────
        if (userDragging)
        {
            if (!_isOrbiting)
            {
                // Snap orbit yaw ke posisi kamera saat ini
                Vector3 dir = transform.position - carTarget.position;
                _orbitYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                _orbitPitch = Mathf.Asin(Mathf.Clamp(dir.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;
                _orbitPitch = Mathf.Clamp(_orbitPitch, pitchMin, pitchMax);
                _isOrbiting = true;
            }

            _orbitYaw += deltaYaw;
            _orbitPitch += deltaPitch;
            _orbitPitch = Mathf.Clamp(_orbitPitch, pitchMin, pitchMax);

            // Reset timer return setiap kali user masih drag
            _returnTimer = returnDelay;
            _isReturning = false;
        }
        else
        {
            _isOrbiting = false;

            // Countdown sebelum mulai return
            if (_returnTimer > 0f)
            {
                _returnTimer -= Time.deltaTime;
                if (_returnTimer <= 0f)
                    _isReturning = true;
            }
        }
    }

    void LateUpdate()
    {
        if (carTarget == null || carRigidbody == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 vel = carRigidbody.linearVelocity;
        float speed = vel.magnitude;
        float speedRatio = Mathf.Clamp01(speed / Mathf.Max(maxSpeedForFOV, 1f));

        // ── AUTO RETURN ke belakang truk ──────────────────────────
        float targetYaw = carTarget.eulerAngles.y + 180f;

        if (_isReturning && speed > returnSpeedThreshold)
        {
            // Smooth return yaw ke belakang truk
            _orbitYaw = Mathf.LerpAngle(_orbitYaw, targetYaw, returnSpeed * dt);

            // Return pitch ke 0
            _orbitPitch = Mathf.Lerp(_orbitPitch, 0f, returnSpeed * dt);

            // Snap jika sudah sangat dekat
            if (Mathf.Abs(Mathf.DeltaAngle(_orbitYaw, targetYaw)) < 0.5f &&
                Mathf.Abs(_orbitPitch) < 0.5f)
            {
                _orbitYaw = targetYaw;
                _orbitPitch = 0f;
                _isReturning = false;
            }
        }
        else if (!_isOrbiting && !_isReturning)
        {
            // Saat tidak freelook dan tidak returning:
            // Ikuti rotasi truk secara smooth supaya kamera tidak ketinggalan saat belok
            _orbitYaw = Mathf.LerpAngle(_orbitYaw, targetYaw, rotationSmoothSpeed * dt);
        }

        // ── Hitung posisi kamera ──────────────────────────────────
        float yawRad = _orbitYaw * Mathf.Deg2Rad;
        float pitchRad = _orbitPitch * Mathf.Deg2Rad;

        Vector3 orbitOffset = new Vector3(
            distanceBehind * Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            distanceBehind * Mathf.Sin(pitchRad) + heightAbove,
            distanceBehind * Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
        );

        Vector3 desiredPos = carTarget.position + orbitOffset;

        float smoothTime = _isOrbiting ? positionSmoothTime * 0.5f : positionSmoothTime;
        float maxSpd = Vector3.Distance(_basePosition, desiredPos) / smoothTime * 1.5f;

        _basePosition = Vector3.SmoothDamp(
            _basePosition, desiredPos,
            ref _smoothVelocity,
            smoothTime, maxSpd, dt
        );

        // ── Look at truk ──────────────────────────────────────────
        Vector3 lookPoint;
        if (_isOrbiting)
        {
            lookPoint = carTarget.position;
        }
        else
        {
            float lookAheadDist = Mathf.Lerp(0f, 4f, speedRatio);
            lookPoint = carTarget.position + carTarget.forward * lookAheadDist;
        }

        Vector3 lookDir = lookPoint - _basePosition;
        if (lookDir.sqrMagnitude < 0.001f) lookDir = Vector3.forward;
        Quaternion lookRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);

        // ── Drift lean ────────────────────────────────────────────
        Quaternion leanRot = Quaternion.identity;
        if (!_isOrbiting)
        {
            float lateralVel = Vector3.Dot(vel, carTarget.right);
            float targetLean = 0f;
            if (speed > 2f)
                targetLean = Mathf.Clamp(
                    -(lateralVel / speed) * maxLeanAngle,
                    -maxLeanAngle, maxLeanAngle);
            _currentLean = Mathf.Lerp(_currentLean, targetLean, leanSmoothSpeed * dt);
            leanRot = Quaternion.Euler(0f, 0f, _currentLean);
        }
        else
        {
            _currentLean = Mathf.Lerp(_currentLean, 0f, leanSmoothSpeed * dt);
            leanRot = Quaternion.Euler(0f, 0f, _currentLean);
        }

        // ── Shake ─────────────────────────────────────────────────
        _shakeTimer += dt * shakeFrequency;
        float shakeMag = _isOrbiting ? 0f : speedRatio * shakeIntensity;
        Vector3 shakeOffset = new Vector3(
            Mathf.Sin(_shakeTimer * 1.1f + 0.7f) * shakeMag,
            Mathf.Sin(_shakeTimer * 0.9f) * shakeMag * 0.5f,
            0f
        );

        // ── Apply ─────────────────────────────────────────────────
        transform.position = _basePosition + shakeOffset;
        transform.rotation = lookRot * leanRot;

        // ── FOV ───────────────────────────────────────────────────
        float targetFOV = _isOrbiting
            ? baseFOV
            : baseFOV + maxFOVAdd * speedRatio;
        _currentFOV = Mathf.Lerp(_currentFOV, targetFOV, fovSmoothSpeed * dt);
        if (_cam != null) _cam.fieldOfView = _currentFOV;
    }

    // ── PUBLIC ────────────────────────────────────────────────────
    public void TriggerImpact(float magnitude = 0.3f)
    {
        StopAllCoroutines();
        StartCoroutine(ImpactShake(magnitude));
    }

    System.Collections.IEnumerator ImpactShake(float magnitude)
    {
        float elapsed = 0f, duration = 0.2f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = _basePosition
                + Random.insideUnitSphere * (magnitude * (1f - t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = _basePosition;
    }

    public void SnapToTarget()
    {
        if (carTarget == null) return;

        _orbitYaw = carTarget.eulerAngles.y + 180f;
        _orbitPitch = 0f;

        float yawRad = _orbitYaw * Mathf.Deg2Rad;
        _basePosition = carTarget.position + new Vector3(
            distanceBehind * Mathf.Sin(yawRad),
            heightAbove,
            distanceBehind * Mathf.Cos(yawRad)
        );

        transform.position = _basePosition;
        transform.LookAt(carTarget);
        _smoothVelocity = Vector3.zero;
        _currentYawRot = Quaternion.Euler(0f, carTarget.eulerAngles.y, 0f);
        _returnTimer = 0f;
        _isReturning = false;
    }

    public bool IsOrbiting => _isOrbiting;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (carTarget == null) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        int seg = 32; float r = distanceBehind;
        for (int i = 0; i < seg; i++)
        {
            float a1 = (i / (float)seg) * Mathf.PI * 2f;
            float a2 = ((i + 1) / (float)seg) * Mathf.PI * 2f;
            Gizmos.DrawLine(
                carTarget.position + new Vector3(Mathf.Sin(a1) * r, heightAbove, Mathf.Cos(a1) * r),
                carTarget.position + new Vector3(Mathf.Sin(a2) * r, heightAbove, Mathf.Cos(a2) * r)
            );
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(carTarget.position, 0.15f);
    }
#endif
}