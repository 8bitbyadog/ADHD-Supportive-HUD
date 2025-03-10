using UnityEngine;
using System.Collections;

public class TargetLauncher : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private GameObject[] targetPrefabs;
    [SerializeField] private float launchInterval = 2f;
    [SerializeField] private float launchForce = 15f;
    [SerializeField] private float maxLaunchAngle = 45f;
    [SerializeField] private float minLaunchAngle = 15f;
    [SerializeField] private float targetLifetime = 5f;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float minSpawnDistance = 3f;
    [SerializeField] private float maxSpawnDistance = 8f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem launchParticles;
    [SerializeField] private ParticleSystem targetDestroyParticles;
    [SerializeField] private Light launchLight;
    
    [Header("Game Settings")]
    [SerializeField] private int targetsPerRound = 10;
    [SerializeField] private float roundDelay = 3f;
    [SerializeField] private bool autoStart = false;
    
    private int currentRound = 0;
    private int targetsHit = 0;
    private bool isActive = false;
    private float nextLaunchTime;
    
    private void Start()
    {
        if (autoStart)
        {
            StartNewRound();
        }
    }
    
    private void Update()
    {
        if (isActive && Time.time >= nextLaunchTime)
        {
            LaunchTarget();
            nextLaunchTime = Time.time + launchInterval;
        }
    }
    
    public void StartNewRound()
    {
        currentRound++;
        targetsHit = 0;
        isActive = true;
        nextLaunchTime = Time.time;
        
        // Visual feedback for round start
        if (launchLight != null)
        {
            StartCoroutine(FlashLight());
        }
    }
    
    private void LaunchTarget()
    {
        // Randomly select a target prefab
        GameObject targetPrefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];
        
        // Calculate random spawn position within radius
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPosition = transform.position + Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;
        
        // Spawn the target
        GameObject target = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
        
        // Add target behavior component
        TargetBehavior targetBehavior = target.AddComponent<TargetBehavior>();
        targetBehavior.Initialize(targetLifetime, targetDestroyParticles);
        
        // Calculate launch direction
        float launchAngle = Random.Range(minLaunchAngle, maxLaunchAngle);
        Vector3 launchDirection = Quaternion.Euler(launchAngle, angle, 0) * Vector3.forward;
        
        // Apply force to launch the target
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
        }
        
        // Play launch effects
        if (launchParticles != null)
        {
            launchParticles.Play();
        }
        
        // Check if we've launched all targets for this round
        if (targetsHit >= targetsPerRound)
        {
            EndRound();
        }
    }
    
    private void EndRound()
    {
        isActive = false;
        StartCoroutine(RoundDelay());
    }
    
    private IEnumerator RoundDelay()
    {
        yield return new WaitForSeconds(roundDelay);
        StartNewRound();
    }
    
    private IEnumerator FlashLight()
    {
        if (launchLight != null)
        {
            launchLight.enabled = true;
            launchLight.intensity = 2f;
            yield return new WaitForSeconds(0.2f);
            launchLight.intensity = 0f;
            launchLight.enabled = false;
        }
    }
    
    public void TargetHit()
    {
        targetsHit++;
    }
    
    public int GetCurrentScore()
    {
        return targetsHit;
    }
    
    public int GetRoundNumber()
    {
        return currentRound;
    }
} 