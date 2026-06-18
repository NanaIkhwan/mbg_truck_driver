using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TruckEngine : MonoBehaviour
{
    [Header("Engine Audio Clips")]
    [SerializeField] private AudioClip startClip;       // start.wav
    [SerializeField] private AudioClip idleClip;        // idle.wav (loop)
    [SerializeField] private AudioClip up01Clip;        // up 0-1.wav
    [SerializeField] private AudioClip up12Clip;        // up 1-2.wav
    [SerializeField] private AudioClip idle2Clip;       // 2 idle.wav (loop, saat jalan)
    [SerializeField] private AudioClip downClip;        // down.wav
    [SerializeField] private AudioClip brakeClip;       // brake.wav (loop selama nahan)
    [SerializeField] private AudioClip mundurClip;      // mundur.wav (loop selama nahan)

    [Header("Engine Settings")]
    [SerializeField] private float speedThreshold = 2f;
    [SerializeField] private float reverseThreshold = -0.1f;

    private AudioSource audioSource;
    private mobil truckScript;
    private Rigidbody rb;

    public enum EngineState
    {
        Off,
        Starting,
        Idle,
        Up01,
        Up12,
        Running,
        Down,
        Braking,
        Reverse
    }

    private EngineState currentState = EngineState.Off;
    private bool isTransitioning = false;
    private float verticalInput = 0f;
    private float speed = 0f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        truckScript = GetComponent<mobil>();
        rb = GetComponent<Rigidbody>();

        SetMotorEnabled(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentState == EngineState.Off)
                StartEngine();
            else if (currentState != EngineState.Starting)
                StopEngine();
        }

        if (currentState == EngineState.Off || currentState == EngineState.Starting)
            return;

        verticalInput = Input.GetAxisRaw("Vertical");
        speed = rb.linearVelocity.magnitude * 3.6f;

        HandleEngineState();
    }

    private void HandleEngineState()
    {
        bool isBraking = Input.GetKey(KeyCode.Space);
        bool isReversing = verticalInput < reverseThreshold;
        bool isAccelerating = verticalInput > 0.1f;

        // =====================
        // REM — loop selama Space ditahan
        // =====================
        if (isBraking && speed > speedThreshold)
        {
            if (currentState != EngineState.Braking)
                TransitionTo(EngineState.Braking);
            return;
        }

        // Rem dilepas → kembali idle
        if (currentState == EngineState.Braking)
        {
            if (!isBraking || speed <= speedThreshold)
                TransitionTo(EngineState.Idle);
            return;
        }

        // =====================
        // MUNDUR — loop selama S ditahan
        // =====================
        if (isReversing)
        {
            if (currentState != EngineState.Reverse)
                TransitionTo(EngineState.Reverse);
            return;
        }

        // S dilepas → kembali idle
        if (currentState == EngineState.Reverse)
        {
            TransitionTo(EngineState.Idle);
            return;
        }

        // =====================
        // NORMAL STATE
        // =====================
        switch (currentState)
        {
            case EngineState.Idle:
                if (isAccelerating)
                    TransitionTo(EngineState.Up01);
                break;

            case EngineState.Running:
                if (!isAccelerating && speed < speedThreshold)
                    TransitionTo(EngineState.Down);
                break;

            case EngineState.Down:
                // User tekan gas lagi saat down → langsung up lagi
                if (isAccelerating && !isTransitioning)
                    TransitionTo(EngineState.Up01);
                break;
        }
    }

    private void TransitionTo(EngineState newState)
    {
        // Boleh interrupt saat pindah ke Braking, Reverse, atau Up01
        bool canInterrupt = newState == EngineState.Braking
                         || newState == EngineState.Reverse
                         || newState == EngineState.Up01
                         || newState == EngineState.Idle;

        if (isTransitioning && !canInterrupt) return;

        StopAllCoroutines();
        isTransitioning = false;
        currentState = newState;

        switch (newState)
        {
            case EngineState.Idle:
                PlayLoop(idleClip);
                break;

            case EngineState.Up01:
                PlayOneShot(up01Clip, EngineState.Up12);
                break;

            case EngineState.Up12:
                PlayOneShot(up12Clip, EngineState.Running);
                break;

            case EngineState.Running:
                PlayLoop(idle2Clip);
                break;

            case EngineState.Down:
                PlayOneShot(downClip, EngineState.Idle);
                break;

            case EngineState.Braking:
                // Loop selama ditahan
                PlayLoop(brakeClip);
                break;

            case EngineState.Reverse:
                // Loop selama ditahan
                PlayLoop(mundurClip);
                break;
        }
    }

    private void StartEngine()
    {
        currentState = EngineState.Starting;
        SetMotorEnabled(false);
        PlayOneShot(startClip, EngineState.Idle);
        Debug.Log("Engine Starting...");
    }

    private void StopEngine()
    {
        StopAllCoroutines();
        currentState = EngineState.Off;
        audioSource.Stop();
        SetMotorEnabled(false);
        isTransitioning = false;
        Debug.Log("Engine Off");
    }

    private void PlayOneShot(AudioClip clip, EngineState nextState)
    {
        if (clip == null)
        {
            TransitionTo(nextState);
            return;
        }

        isTransitioning = true;
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();

        SetMotorEnabled(currentState != EngineState.Starting);
        StartCoroutine(WaitAndTransition(clip.length, nextState));
    }

    private void PlayLoop(AudioClip clip)
    {
        if (clip == null) return;

        isTransitioning = false;
        audioSource.loop = true;
        audioSource.clip = clip;
        audioSource.Play();

        SetMotorEnabled(true);
    }

    private IEnumerator WaitAndTransition(float duration, EngineState nextState)
    {
        yield return new WaitForSeconds(duration);
        isTransitioning = false;
        TransitionTo(nextState);
    }

    private void SetMotorEnabled(bool enabled)
    {
        if (truckScript != null)
            truckScript.engineOn = enabled;
    }

    public bool IsEngineOn() => currentState != EngineState.Off && currentState != EngineState.Starting;
    public EngineState GetEngineState() => currentState;
}