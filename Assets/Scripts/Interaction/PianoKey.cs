using UnityEngine;
using System.Collections;

public class PianoKey : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private float keyTravelDistance = 0.1f;
    [SerializeField] private float keyReturnSpeed = 5f;
    [SerializeField] private float keyPressSpeed = 10f;
    [SerializeField] private float keyWeight = 0.5f;
    [SerializeField] private float keyDamping = 0.8f;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip keySound;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;
    [SerializeField] private float velocityToVolume = 0.5f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem keyPressParticles;
    [SerializeField] private Light keyLight;
    [SerializeField] private float lightIntensity = 1f;
    [SerializeField] private float lightFadeSpeed = 2f;
    
    [Header("Key Movement")]
    [SerializeField] private float keyPressDistance = 0.02f;
    
    [Header("Monophonic Settings")]
    [SerializeField] private bool isMonophonic = true;
    [SerializeField] private string handId = ""; // Unique identifier for the hand playing this key
    
    private Vector3 initialPosition;
    private Vector3 initialRotation;
    private Vector3 pressDirection;
    private float currentVelocity;
    private bool isPressed;
    private float pressStartTime;
    private float currentPressDistance;
    private AudioSource audioSource;
    private float keyPressTime;
    private float currentLightIntensity;
    private PianoManager pianoManager;
    
    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localEulerAngles;
        
        // Calculate press direction based on key rotation
        pressDirection = Quaternion.Euler(initialRotation) * Vector3.down;
        
        // Set up audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = keySound;
        audioSource.playOnAwake = false;
        
        // Set up light if present
        if (keyLight != null)
        {
            keyLight.intensity = 0f;
        }
        
        // Get reference to piano manager
        pianoManager = GetComponentInParent<PianoManager>();
    }
    
    private void Update()
    {
        if (isPressed)
        {
            // Calculate target press distance with easing
            float pressProgress = (Time.time - pressStartTime) * keyPressSpeed;
            float targetDistance = Mathf.Lerp(0f, keyPressDistance, Mathf.Clamp01(pressProgress));
            
            // Apply spring physics to press movement
            float displacement = currentPressDistance - targetDistance;
            float springForce = -displacement * keyWeight;
            springForce *= keyDamping;
            
            currentVelocity += springForce * Time.deltaTime;
            currentVelocity *= keyDamping;
            
            currentPressDistance += currentVelocity * Time.deltaTime;
        }
        else
        {
            // Return to initial position with spring physics
            float displacement = currentPressDistance;
            float springForce = -displacement * keyWeight;
            springForce *= keyDamping;
            
            currentVelocity += springForce * Time.deltaTime;
            currentVelocity *= keyDamping;
            
            currentPressDistance += currentVelocity * Time.deltaTime;
        }
        
        // Apply the press movement
        transform.localPosition = initialPosition + pressDirection * currentPressDistance;
        
        // Update light intensity
        if (keyLight != null)
        {
            float targetIntensity = isPressed ? lightIntensity : 0f;
            currentLightIntensity = Mathf.Lerp(currentLightIntensity, targetIntensity, Time.deltaTime * lightFadeSpeed);
            keyLight.intensity = currentLightIntensity;
        }
    }
    
    public void OnKeyPress(Vector3 pressPosition, float pressVelocity)
    {
        if (!isPressed)
        {
            isPressed = true;
            pressStartTime = Time.time;
            
            // Play sound with velocity-based volume and random pitch
            if (audioSource != null && keySound != null)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.volume = Mathf.Clamp01(pressVelocity * velocityToVolume);
                audioSource.Play();
            }
            
            // Play particles
            if (keyPressParticles != null)
            {
                keyPressParticles.Play();
            }
        }
    }
    
    public void OnKeyRelease()
    {
        if (isPressed)
        {
            isPressed = false;
            currentPressDistance = 0f;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is a finger
        if (other.CompareTag("FingerTip"))
        {
            // Get the hand ID from the finger's parent
            string newHandId = GetHandIdFromFinger(other.gameObject);
            
            // If monophonic, only allow one hand to play at a time
            if (isMonophonic)
            {
                if (string.IsNullOrEmpty(handId))
                {
                    // This key is free, assign it to the new hand
                    handId = newHandId;
                    float pressVelocity = other.attachedRigidbody.velocity.magnitude;
                    OnKeyPress(other.transform.position, pressVelocity);
                    
                    // Notify piano manager
                    if (pianoManager != null)
                    {
                        pianoManager.OnKeyPress(handId, this);
                    }
                }
                else if (handId == newHandId)
                {
                    // Same hand, allow playing
                    float pressVelocity = other.attachedRigidbody.velocity.magnitude;
                    OnKeyPress(other.transform.position, pressVelocity);
                }
                else
                {
                    // Different hand, ignore
                    return;
                }
            }
            else
            {
                // Polyphonic behavior
                float pressVelocity = other.attachedRigidbody.velocity.magnitude;
                OnKeyPress(other.transform.position, pressVelocity);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FingerTip"))
        {
            string exitingHandId = GetHandIdFromFinger(other.gameObject);
            
            // Only release if it's the same hand that pressed the key
            if (handId == exitingHandId)
            {
                OnKeyRelease();
                
                // Notify piano manager
                if (pianoManager != null)
                {
                    pianoManager.OnKeyRelease(handId);
                }
                
                handId = ""; // Clear the hand ID
            }
        }
    }
    
    private string GetHandIdFromFinger(GameObject finger)
    {
        // Get the hand object (assuming fingers are children of the hand)
        Transform hand = finger.transform.parent;
        if (hand != null)
        {
            // Use the hand's name as the ID
            return hand.name;
        }
        return "";
    }
} 