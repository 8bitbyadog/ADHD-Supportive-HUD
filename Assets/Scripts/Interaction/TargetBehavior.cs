using UnityEngine;

public class TargetBehavior : MonoBehaviour, IDamageable
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private Light targetLight;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.5f;
    
    [Header("Target Properties")]
    [SerializeField] private int pointValue = 100;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float glowIntensity = 1.5f;
    
    private float lifetime;
    private float spawnTime;
    private bool isHit = false;
    private TargetLauncher launcher;
    
    public void Initialize(float lifetime, ParticleSystem destroyParticles)
    {
        this.lifetime = lifetime;
        spawnTime = Time.time;
        
        // Set up visual effects
        if (targetLight != null)
        {
            targetLight.intensity = glowIntensity;
        }
        
        if (hitParticles == null && destroyParticles != null)
        {
            hitParticles = destroyParticles;
        }
    }
    
    private void Start()
    {
        // Find the launcher in the scene
        launcher = FindObjectOfType<TargetLauncher>();
    }
    
    private void Update()
    {
        if (!isHit)
        {
            // Rotate the target
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            
            // Pulse the light
            if (targetLight != null)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f) * pulseIntensity;
                targetLight.intensity = glowIntensity * (1f + pulse);
            }
            
            // Check lifetime
            if (Time.time - spawnTime >= lifetime)
            {
                DestroyTarget();
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (!isHit)
        {
            isHit = true;
            
            // Play hit effect
            if (hitParticles != null)
            {
                hitParticles.Play();
            }
            
            // Update score
            if (launcher != null)
            {
                launcher.TargetHit();
            }
            
            // Destroy the target
            DestroyTarget();
        }
    }
    
    private void DestroyTarget()
    {
        // Play destroy effect
        if (hitParticles != null)
        {
            hitParticles.transform.SetParent(null);
            hitParticles.Play();
            Destroy(hitParticles.gameObject, 2f);
        }
        
        // Destroy the target
        Destroy(gameObject);
    }
} 