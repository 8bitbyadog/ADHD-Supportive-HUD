using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BreathingGuide : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image breathCircle;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private ParticleSystem breathParticles;
    
    [Header("Breathing Pattern")]
    [SerializeField] private float inhaleTime = 4f;
    [SerializeField] private float holdTime = 4f;
    [SerializeField] private float exhaleTime = 4f;
    [SerializeField] private float restTime = 2f;
    [SerializeField] private int cyclesPerSession = 5;
    
    [Header("Visual Settings")]
    [SerializeField] private Color inhaleColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color holdColor = new Color(0.2f, 1f, 0.4f);
    [SerializeField] private Color exhaleColor = new Color(0.8f, 0.2f, 0.4f);
    [SerializeField] private AnimationCurve breathingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioClip inhaleSound;
    [SerializeField] private AudioClip exhaleSound;
    [SerializeField] private AudioClip ambientSound;
    
    private bool isActive;
    private int currentCycle;
    private AudioSource audioSource;
    private AudioSource ambientSource;
    
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        
        if (ambientSound != null)
        {
            ambientSource.clip = ambientSound;
            ambientSource.volume = 0.3f;
        }
    }
    
    public void StartBreathingExercise()
    {
        if (!isActive)
        {
            isActive = true;
            currentCycle = 0;
            
            if (ambientSound != null)
            {
                ambientSource.Play();
            }
            
            StartCoroutine(BreathingCycle());
        }
    }
    
    public void StopBreathingExercise()
    {
        if (isActive)
        {
            isActive = false;
            StopAllCoroutines();
            
            if (ambientSource.isPlaying)
            {
                ambientSource.Stop();
            }
            
            // Reset visuals
            if (breathCircle != null)
            {
                breathCircle.transform.localScale = Vector3.one;
            }
            
            if (instructionText != null)
            {
                instructionText.text = "Press Start to begin breathing exercise";
            }
            
            if (breathParticles != null)
            {
                breathParticles.Stop();
            }
        }
    }
    
    private IEnumerator BreathingCycle()
    {
        while (isActive && currentCycle < cyclesPerSession)
        {
            // Inhale
            yield return StartCoroutine(Inhale());
            
            // Hold
            yield return StartCoroutine(Hold());
            
            // Exhale
            yield return StartCoroutine(Exhale());
            
            // Rest
            yield return StartCoroutine(Rest());
            
            currentCycle++;
        }
        
        StopBreathingExercise();
    }
    
    private IEnumerator Inhale()
    {
        float elapsed = 0f;
        
        if (inhaleSound != null)
        {
            audioSource.PlayOneShot(inhaleSound);
        }
        
        if (breathParticles != null)
        {
            var main = breathParticles.main;
            main.startColor = inhaleColor;
            breathParticles.Play();
        }
        
        while (elapsed < inhaleTime)
        {
            float t = elapsed / inhaleTime;
            float scale = breathingCurve.Evaluate(t);
            
            if (breathCircle != null)
            {
                breathCircle.transform.localScale = Vector3.one * (1f + scale);
                breathCircle.color = inhaleColor;
            }
            
            if (instructionText != null)
            {
                instructionText.text = "Inhale";
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator Hold()
    {
        float elapsed = 0f;
        
        if (breathParticles != null)
        {
            var main = breathParticles.main;
            main.startColor = holdColor;
        }
        
        while (elapsed < holdTime)
        {
            if (breathCircle != null)
            {
                breathCircle.color = holdColor;
            }
            
            if (instructionText != null)
            {
                instructionText.text = "Hold";
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator Exhale()
    {
        float elapsed = 0f;
        
        if (exhaleSound != null)
        {
            audioSource.PlayOneShot(exhaleSound);
        }
        
        if (breathParticles != null)
        {
            var main = breathParticles.main;
            main.startColor = exhaleColor;
        }
        
        while (elapsed < exhaleTime)
        {
            float t = elapsed / exhaleTime;
            float scale = 1f - breathingCurve.Evaluate(t);
            
            if (breathCircle != null)
            {
                breathCircle.transform.localScale = Vector3.one * (1f + scale);
                breathCircle.color = exhaleColor;
            }
            
            if (instructionText != null)
            {
                instructionText.text = "Exhale";
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator Rest()
    {
        float elapsed = 0f;
        
        if (breathParticles != null)
        {
            breathParticles.Stop();
        }
        
        while (elapsed < restTime)
        {
            if (instructionText != null)
            {
                instructionText.text = "Rest";
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
} 