using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AutomailArmorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRController rightController;
    [SerializeField] private GameObject automailModel;
    [SerializeField] private GameObject ironManArmorModel;
    [SerializeField] private Transform photonBlastSpawnPoint;
    
    [Header("Transformation Settings")]
    [SerializeField] private float transformDuration = 1.5f;
    [SerializeField] private AnimationCurve transformCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Photon Blast Settings")]
    [SerializeField] private GameObject photonBlastPrefab;
    [SerializeField] private float normalBlastSpeed = 20f;
    [SerializeField] private float chargedBlastSpeed = 15f;
    [SerializeField] private float normalBlastSize = 1f;
    [SerializeField] private float chargedBlastSize = 2.5f;
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private float maxChargeIntensity = 3f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem transformParticles;
    [SerializeField] private ParticleSystem chargeParticles;
    [SerializeField] private ParticleSystem blastParticles;
    
    private bool isTransformed = false;
    private bool isCharging = false;
    private float chargeStartTime;
    private float currentChargeIntensity;
    
    private void Start()
    {
        // Start with automail visible and armor hidden
        automailModel.SetActive(true);
        ironManArmorModel.SetActive(false);
        
        // Initialize particle systems
        if (transformParticles != null) transformParticles.Stop();
        if (chargeParticles != null) chargeParticles.Stop();
        if (blastParticles != null) blastParticles.Stop();
    }
    
    private void Update()
    {
        // Check for transformation trigger (grip + trigger)
        bool gripPressed = rightController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue) && gripValue;
        bool triggerPressed = rightController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue) && triggerValue;
        
        if (gripPressed && triggerPressed && !isTransformed)
        {
            StartTransformation();
        }
        
        // Handle photon blast charging and firing
        if (isTransformed)
        {
            if (triggerPressed && !isCharging)
            {
                StartCharging();
            }
            else if (!triggerPressed && isCharging)
            {
                FireChargedBlast();
            }
            else if (triggerPressed && isCharging)
            {
                UpdateCharging();
            }
        }
    }
    
    private void StartTransformation()
    {
        StartCoroutine(TransformSequence());
    }
    
    private System.Collections.IEnumerator TransformSequence()
    {
        float elapsedTime = 0f;
        
        if (transformParticles != null)
        {
            transformParticles.Play();
        }
        
        while (elapsedTime < transformDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = transformCurve.Evaluate(elapsedTime / transformDuration);
            
            // Scale and rotate automail down while scaling and rotating armor up
            automailModel.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            ironManArmorModel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            
            yield return null;
        }
        
        automailModel.SetActive(false);
        ironManArmorModel.SetActive(true);
        isTransformed = true;
    }
    
    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        currentChargeIntensity = 0f;
        
        if (chargeParticles != null)
        {
            chargeParticles.Play();
        }
    }
    
    private void UpdateCharging()
    {
        float chargeProgress = (Time.time - chargeStartTime) / chargeTime;
        currentChargeIntensity = Mathf.Min(chargeProgress * maxChargeIntensity, maxChargeIntensity);
        
        // Update charge particle effects
        if (chargeParticles != null)
        {
            var emission = chargeParticles.emission;
            emission.rateOverTime = 50 * currentChargeIntensity;
        }
    }
    
    private void FireChargedBlast()
    {
        if (isCharging)
        {
            GameObject blast = Instantiate(photonBlastPrefab, photonBlastSpawnPoint.position, photonBlastSpawnPoint.rotation);
            PhotonBlast photonBlast = blast.GetComponent<PhotonBlast>();
            
            if (photonBlast != null)
            {
                photonBlast.Initialize(
                    currentChargeIntensity > 0.5f ? chargedBlastSpeed : normalBlastSpeed,
                    currentChargeIntensity > 0.5f ? chargedBlastSize : normalBlastSize,
                    currentChargeIntensity
                );
            }
            
            if (blastParticles != null)
            {
                blastParticles.Play();
            }
        }
        
        isCharging = false;
        if (chargeParticles != null)
        {
            chargeParticles.Stop();
        }
    }
} 