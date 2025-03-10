using UnityEngine;

public class PhotonBlast : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private ParticleSystem impactParticles;
    [SerializeField] private Light blastLight;
    
    [Header("Blast Properties")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float maxDamage = 30f;
    [SerializeField] private float explosionRadius = 2f;
    
    private float speed;
    private float size;
    private float intensity;
    private float damage;
    
    public void Initialize(float speed, float size, float intensity)
    {
        this.speed = speed;
        this.size = size;
        this.intensity = intensity;
        this.damage = Mathf.Lerp(baseDamage, maxDamage, intensity);
        
        // Scale the blast
        transform.localScale = Vector3.one * size;
        
        // Adjust visual effects
        if (blastLight != null)
        {
            blastLight.intensity = intensity * 2f;
        }
        
        if (trailParticles != null)
        {
            var main = trailParticles.main;
            main.startSize = size;
            main.startColor = new Color(1f, 1f, 1f, intensity);
            trailParticles.Play();
        }
    }
    
    private void Update()
    {
        // Move the blast forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Create impact effect
        if (impactParticles != null)
        {
            impactParticles.Play();
        }
        
        // Apply damage in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            // Apply damage to damageable objects
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                damageable.TakeDamage(damage * damageMultiplier);
            }
        }
        
        // Destroy the blast
        Destroy(gameObject, 2f);
    }
} 