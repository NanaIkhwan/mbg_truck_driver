using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Forza Horizon-style smooth car camera — v5 (+ Orbit Free Look)
/// ─────────────────────────────────────────────────────────────
/// WAJIB: Rigidbody mobil → Interpolation = "Interpolate"
/// Attach ke Main Camera. Camera jangan jadi child mobil.
///
/// FREE LOOK / ORBIT:
///   - Geser mouse → kamera langsung merespons
///   - Tahan klik kanan (Mouse1) ATAU tahan FreeLookKey (default: Alt) → cursor lock
///   - Diam → kamera smooth kembali ke posisi belakang mobil
/// </summary>
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

    // ── ROTATION (follow normal) ──────────────────────────────────
    [Header("Rotation")]
    [Range(1f, 12f)]
    public float rotationSmoothSpeed = 4f;

    [Range(0f, 1f)]
    public float lookAheadStrength = 0.25f;

    // ── ORBIT FREE LOOK ───────────────────────────────────────────
    [Header("Orbit Free Look")]
    [Tooltip("Tahan tombol ini ATAU klik kanan untuk cursor lock.")]
    public KeyCode freeLookKey = KeyCode.LeftAlt;

    [Tooltip("Sensitivitas drag mouse saat orbit.")]
    public float mouseSensitivity = 3f;

    [Tooltip("Batas pitch orbit (derajat). Negatif = lihat ke bawah.")]
    public float pitchMin = -20f;
    public float pitchMax = 50f;

    [Tooltip("Kecepatan kamera kembali ke posisi belakang setelah orbit.")]
    [Range(1f, 10f)]
    public float returnSpeed = 4f;

    [Tooltip("Sembunyikan cursor saat orbit aktif.")]
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

    // ── PRIVATE: follow normal ────────────────────────────────────
    Camera _cam;
    float _currentFOV;
    float _currentLean;
    float _shakeTimer;
    Quaternion _currentYawRot;
    Vector3 _smoothVelocity;
    Vector3 _basePosition;

    // ── PRIVATE: orbit ────────────────────────────────────────────
    bool _isOrbiting;
    float _orbitYaw;
    float _orbitPitch;

    // ─────────────────────────────────────────────────────────────
    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = GetComponentInChildren<Camera>();

        if (carRigidbody != null &&
            carRigidbody.interpolation == RigidbodyInterpolation.None)
        {
            carRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Debug.Log("[ForzaCamera] Auto-set Rigidbody Interpolation = Interpolate");
        }
    }

    void Start()
    {
        SnapToTarget();
        _currentFOV = baseFOV;
    }

    void Update()
    {
        // ── BLOCK input kamera saat jari di UI atau steering wheel ──
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

        // Cek steering wheel sedang dipakai
        bool steeringActive = SteeringWheel.IsBeingUsed;

        // Kalau UI atau steering aktif, block semua input kamera
        if (touchingUI || steeringActive)
        {
            _isOrbiting = false;
            return;
        }

        bool holding = Input.GetMouseButton(1) || Input.GetKey(freeLookKey);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (holding)
        {
            if (!_isOrbiting)
            {
                Vector3 dir = transform.position - carTarget.position;
                _orbitYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                _orbitPitch = Mathf.Asin(Mathf.Clamp(dir.normalized.y, -1f, 1f))
                              * Mathf.Rad2Deg;
                _orbitPitch = Mathf.Clamp(_orbitPitch, pitchMin, pitchMax);
                _isOrbiting = true;

                if (hideCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
        else
        {
            if (_isOrbiting)
            {
                _isOrbiting = false;
                if (hideCursor)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        _orbitYaw   += mouseX * mouseSensitivity;
        _orbitPitch -= mouseY * mouseSensitivity;
        _orbitPitch  = Mathf.Clamp(_orbitPitch, pitchMin, pitchMax);
    }

    void LateUpdate()
    {
        if (carTarget == null || carRigidbody == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // ── 1. SPEED ──────────────────────────────────────────────
        Vector3 vel = carRigidbody.linearVelocity;
        float speed = vel.magnitude;
        float speedRatio = Mathf.Clamp01(speed / Mathf.Max(maxSpeedForFOV, 1f));

        // ── 2. HITUNG POSISI KAMERA DARI ORBIT ───────────────────
        float orbitRadius = distanceBehind;

        float yawRad   = _orbitYaw   * Mathf.Deg2Rad;
        float pitchRad = _orbitPitch * Mathf.Deg2Rad;

        Vector3 orbitOffset = new Vector3(
            orbitRadius * Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            orbitRadius * Mathf.Sin(pitchRad) + heightAbove,
            orbitRadius * Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
        );

        Vector3 desiredPos = carTarget.position + orbitOffset;

        // ── 3. SMOOTH POSITION ────────────────────────────────────
        float smoothTime = _isOrbiting ? positionSmoothTime * 0.5f : positionSmoothTime;
        float maxSpd = Vector3.Distance(_basePosition, desiredPos) / smoothTime * 1.5f;

        _basePosition = Vector3.SmoothDamp(
            _basePosition,
            desiredPos,
            ref _smoothVelocity,
            smoothTime,
            maxSpd,
            dt
        );

        // ── 4. LOOK AT MOBIL ──────────────────────────────────────
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

        // ── 5. DRIFT LEAN ─────────────────────────────────────────
        Quaternion leanRot = Quaternion.identity;
        if (!_isOrbiting)
        {
            float lateralVel = Vector3.Dot(vel, carTarget.right);
            float targetLean = 0f;
            if (speed > 2f)
                targetLean = Mathf.Clamp(
                    -(lateralVel / speed) * maxLeanAngle,
                    -maxLeanAngle, maxLeanAngle
                );
            _currentLean = Mathf.Lerp(_currentLean, targetLean, leanSmoothSpeed * dt);
            leanRot = Quaternion.Euler(0f, 0f, _currentLean);
        }
        else
        {
            _currentLean = Mathf.Lerp(_currentLean, 0f, leanSmoothSpeed * dt);
            leanRot = Quaternion.Euler(0f, 0f, _currentLean);
        }

        // ── 6. SHAKE (nonaktif saat orbit) ────────────────────────
        _shakeTimer += dt * shakeFrequency;
        float shakeMag = _isOrbiting ? 0f : speedRatio * shakeIntensity;
        Vector3 shakeOffset = new Vector3(
            Mathf.Sin(_shakeTimer * 1.1f + 0.7f) * shakeMag,
            Mathf.Sin(_shakeTimer * 0.9f) * shakeMag * 0.5f,
            0f
        );

        // ── 7. APPLY ──────────────────────────────────────────────
        transform.position = _basePosition + shakeOffset;
        transform.rotation = lookRot * leanRot;

        // ── 8. FOV ────────────────────────────────────────────────
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

        _orbitYaw   = carTarget.eulerAngles.y + 180f;
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
        _currentYawRot  = Quaternion.Euler(0f, carTarget.eulerAngles.y, 0f);
    }

    public bool IsOrbiting => _isOrbiting;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (carTarget == null) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        int   seg = 32;
        float r   = distanceBehind;
        for (int i = 0; i < seg; i++)
        {
            float a1 = (i       / (float)seg) * Mathf.PI * 2f;
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