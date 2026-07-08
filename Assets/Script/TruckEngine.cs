using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TruckEngine : MonoBehaviour
{
    [Header("Engine Audio Clips")]
    [SerializeField] private AudioClip startClip;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip up01Clip;
    [SerializeField] private AudioClip up12Clip;
    [SerializeField] private AudioClip idle2Clip;
    [SerializeField] private AudioClip downClip;
    [SerializeField] private AudioClip brakeClip;
    [SerializeField] private AudioClip mundurClip;

    [Header("Engine Settings")]
    [SerializeField] private float speedThreshold = 2f;
    [SerializeField] private float reverseThreshold = -0.1f;

    private AudioSource audioSource;
    private mobil truckScript;
    private Rigidbody rb;

    public enum EngineState
    {
        Off, Starting, Idle, Up01, Up12, Running, Down, Braking, Reverse
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

        // Sync volume dengan SettingManager
        audioSource.volume = SettingManager.SFXVolume;

        truckScript = GetComponent<mobil>();
        rb = GetComponent<Rigidbody>();

        SetMotorEnabled(false);
    }

    private void Update()
    {
        // Sync volume tiap frame kalau berubah dari setting
        audioSource.volume = SettingManager.SFXVolume;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentState == EngineState.Off)
                StartEngine();
            else if (currentState != EngineState.Starting)
                StopEngine();
        }

        if (currentState == EngineState.Off || currentState == EngineState.Starting)
            return;

        verticalInput = truckScript.GetVerticalInput();
        speed = rb.linearVelocity.magnitude * 3.6f;

        HandleEngineState();
    }

    private void HandleEngineState()
    {
        bool isBraking = truckScript.IsBraking();
        bool isReversing = verticalInput < reverseThreshold;
        bool isAccelerating = verticalInput > 0.1f;

        if (isBraking)
        {
            if (currentState != EngineState.Braking)
                TransitionTo(EngineState.Braking);
            return;
        }

        if (currentState == EngineState.Braking)
        {
            if (!isBraking)
                TransitionTo(EngineState.Idle);
            return;
        }

        if (isReversing)
        {
            if (currentState != EngineState.Reverse)
                TransitionTo(EngineState.Reverse);
            return;
        }

        if (currentState == EngineState.Reverse)
        {
            TransitionTo(EngineState.Idle);
            return;
        }

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
                if (isAccelerating && !isTransitioning)
                    TransitionTo(EngineState.Up01);
                break;
        }
    }

    private void TransitionTo(EngineState newState)
    {
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
            case EngineState.Idle:     PlayLoop(idleClip);                    break;
            case EngineState.Up01:     PlayOneShot(up01Clip, EngineState.Up12);   break;
            case EngineState.Up12:     PlayOneShot(up12Clip, EngineState.Running); break;
            case EngineState.Running:  PlayLoop(idle2Clip);                   break;
            case EngineState.Down:     PlayOneShot(downClip, EngineState.Idle);   break;
            case EngineState.Braking:  PlayLoop(brakeClip);                   break;
            case EngineState.Reverse:  PlayLoop(mundurClip);                  break;
        }
    }

    public void ToggleEngine()
    {
        if (currentState == EngineState.Off)
            StartEngine();
        else if (currentState != EngineState.Starting)
            StopEngine();
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
        if (clip == null) { TransitionTo(nextState); return; }

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