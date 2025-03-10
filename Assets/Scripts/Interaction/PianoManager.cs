using UnityEngine;
using System.Collections.Generic;

public class PianoManager : MonoBehaviour
{
    [Header("Piano Settings")]
    [SerializeField] private GameObject whiteKeyPrefab;
    [SerializeField] private GameObject blackKeyPrefab;
    [SerializeField] private float keyWidth = 0.1f;
    [SerializeField] private float blackKeyOffset = 0.05f;
    [SerializeField] private float blackKeyHeight = 0.05f;
    [SerializeField] private int octaves = 2;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] whiteKeySounds;
    [SerializeField] private AudioClip[] blackKeySounds;
    [SerializeField] private float basePitch = 440f; // A4 note
    
    [Header("Visual Settings")]
    [SerializeField] private Material whiteKeyMaterial;
    [SerializeField] private Material blackKeyMaterial;
    [SerializeField] private Color keyGlowColor = new Color(0.2f, 0.8f, 1f, 1f);
    
    [Header("Gauntlet Settings")]
    [SerializeField] private float keySpacing = 0.15f;
    [SerializeField] private float keyHeight = 0.05f;
    [SerializeField] private float keyDepth = 0.1f;
    [SerializeField] private float armCurveRadius = 0.3f;
    [SerializeField] private float armCurveOffset = 0.1f;
    [SerializeField] private float keyPressDistance = 0.02f;
    [SerializeField] private float keyReturnSpeed = 5f;
    [SerializeField] private float keyPressSpeed = 10f;
    
    [Header("Monophonic Settings")]
    [SerializeField] private bool isMonophonic = true;
    [SerializeField] private Dictionary<string, PianoKey> activeHandKeys = new Dictionary<string, PianoKey>();
    
    private List<PianoKey> whiteKeys = new List<PianoKey>();
    private List<PianoKey> blackKeys = new List<PianoKey>();
    private Dictionary<string, float> noteFrequencies = new Dictionary<string, float>();
    
    private void Start()
    {
        InitializeNoteFrequencies();
        CreatePianoKeys();
        InitializeMonophonicSystem();
    }
    
    private void InitializeNoteFrequencies()
    {
        // Initialize frequencies for all notes (A0 to C8)
        string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        for (int octave = 0; octave < 8; octave++)
        {
            for (int i = 0; i < notes.Length; i++)
            {
                string note = notes[i] + octave;
                float frequency = basePitch * Mathf.Pow(2f, (octave - 4) + (i - 9) / 12f);
                noteFrequencies[note] = frequency;
            }
        }
    }
    
    private void CreatePianoKeys()
    {
        float currentX = 0f;
        float totalWidth = octaves * 7 * keyWidth;
        
        // Calculate the curve of the arm
        for (int octave = 0; octave < octaves; octave++)
        {
            // Create white keys in a curved layout
            for (int i = 0; i < 7; i++)
            {
                float x = currentX + i * keyWidth;
                float y = Mathf.Sin(x / totalWidth * Mathf.PI) * armCurveRadius;
                float z = Mathf.Cos(x / totalWidth * Mathf.PI) * armCurveRadius;
                
                GameObject keyObj = Instantiate(whiteKeyPrefab, transform);
                keyObj.transform.localPosition = new Vector3(x, y + armCurveOffset, z);
                keyObj.transform.localRotation = Quaternion.Euler(
                    Mathf.Atan2(y, z) * Mathf.Rad2Deg,
                    0f,
                    0f
                );
                
                PianoKey pianoKey = keyObj.GetComponent<PianoKey>();
                if (pianoKey != null)
                {
                    // Set up key visuals
                    var renderer = keyObj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = whiteKeyMaterial;
                    }
                    
                    // Add light component
                    Light keyLight = keyObj.AddComponent<Light>();
                    keyLight.color = keyGlowColor;
                    keyLight.intensity = 0f;
                    
                    // Add particle system
                    ParticleSystem particles = keyObj.AddComponent<ParticleSystem>();
                    var main = particles.main;
                    main.startColor = keyGlowColor;
                    main.startSize = 0.05f;
                    main.startLifetime = 0.5f;
                    
                    whiteKeys.Add(pianoKey);
                }
            }
            
            // Create black keys in a curved layout
            for (int i = 0; i < 5; i++)
            {
                float x = currentX + (i + 1) * keyWidth - blackKeyOffset;
                float y = Mathf.Sin(x / totalWidth * Mathf.PI) * armCurveRadius;
                float z = Mathf.Cos(x / totalWidth * Mathf.PI) * armCurveRadius;
                
                GameObject keyObj = Instantiate(blackKeyPrefab, transform);
                keyObj.transform.localPosition = new Vector3(x, y + armCurveOffset + blackKeyHeight, z);
                keyObj.transform.localRotation = Quaternion.Euler(
                    Mathf.Atan2(y, z) * Mathf.Rad2Deg,
                    0f,
                    0f
                );
                
                PianoKey pianoKey = keyObj.GetComponent<PianoKey>();
                if (pianoKey != null)
                {
                    // Set up key visuals
                    var renderer = keyObj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = blackKeyMaterial;
                    }
                    
                    // Add light component
                    Light keyLight = keyObj.AddComponent<Light>();
                    keyLight.color = keyGlowColor;
                    keyLight.intensity = 0f;
                    
                    // Add particle system
                    ParticleSystem particles = keyObj.AddComponent<ParticleSystem>();
                    var main = particles.main;
                    main.startColor = keyGlowColor;
                    main.startSize = 0.05f;
                    main.startLifetime = 0.5f;
                    
                    blackKeys.Add(pianoKey);
                }
            }
            
            currentX += 7 * keyWidth;
        }
        
        // Center the piano
        transform.localPosition = new Vector3(-currentX / 2f, 0f, 0f);
    }
    
    private void InitializeMonophonicSystem()
    {
        // Clear any existing active keys
        activeHandKeys.Clear();
    }
    
    private void Update()
    {
        if (isMonophonic)
        {
            UpdateMonophonicState();
        }
    }
    
    private void UpdateMonophonicState()
    {
        // Check for any hands that are no longer touching keys
        List<string> handsToRemove = new List<string>();
        foreach (var kvp in activeHandKeys)
        {
            if (kvp.Value == null)
            {
                handsToRemove.Add(kvp.Key);
            }
        }
        
        // Remove inactive hands
        foreach (var handId in handsToRemove)
        {
            activeHandKeys.Remove(handId);
        }
    }
    
    public void OnKeyPress(string handId, PianoKey key)
    {
        if (isMonophonic)
        {
            // If this hand was playing another key, release it
            if (activeHandKeys.ContainsKey(handId))
            {
                var previousKey = activeHandKeys[handId];
                if (previousKey != null && previousKey != key)
                {
                    previousKey.OnKeyRelease();
                }
            }
            
            // Update the active key for this hand
            activeHandKeys[handId] = key;
        }
    }
    
    public void OnKeyRelease(string handId)
    {
        if (isMonophonic && activeHandKeys.ContainsKey(handId))
        {
            activeHandKeys[handId] = null;
        }
    }
    
    public void PlayNote(string note, float velocity)
    {
        if (noteFrequencies.ContainsKey(note))
        {
            float frequency = noteFrequencies[note];
            // Find the appropriate key and play it
            // This would need to be implemented based on your specific key layout
        }
    }
    
    public void StopNote(string note)
    {
        // Find the appropriate key and release it
        // This would need to be implemented based on your specific key layout
    }
} 