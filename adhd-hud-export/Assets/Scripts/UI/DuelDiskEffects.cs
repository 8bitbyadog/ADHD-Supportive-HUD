using UnityEngine;
using System.Collections;

public class DuelDiskEffects : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float deployAnimationDuration = 1.5f;
    [SerializeField] private float cardSlotAnimationDuration = 0.8f;
    [SerializeField] private float holographicPulseSpeed = 1.5f;
    [SerializeField] private float holographicPulseIntensity = 0.3f;
    [SerializeField] private float cardGlowIntensity = 2f;
    [SerializeField] private float cardGlowPulseSpeed = 0.8f;
    [SerializeField] private float goldenRatio = 1.618f;

    [Header("Effect Colors")]
    [SerializeField] private Color primaryColor = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color secondaryColor = new Color(0.8f, 0.2f, 1f, 1f);
    [SerializeField] private Color holographicColor = new Color(0.2f, 0.8f, 1f, 0.8f);
    [SerializeField] private Color energyColor = new Color(0.4f, 0.8f, 1f, 1f);

    [Header("References")]
    [SerializeField] private GameObject duelDiskBase;
    [SerializeField] private GameObject cardSlotsContainer;
    [SerializeField] private GameObject holographicDisplay;
    [SerializeField] private ParticleSystem energyParticles;
    [SerializeField] private ParticleSystem holographicParticles;

    private MaterialPropertyBlock holographicProps;
    private MaterialPropertyBlock cardGlowProps;
    private float holographicPulseTime = 0f;
    private float cardGlowPulseTime = 0f;
    private bool isDeployed = false;

    private void Start()
    {
        holographicProps = new MaterialPropertyBlock();
        cardGlowProps = new MaterialPropertyBlock();
        
        // Initialize particle systems if not set
        if (energyParticles == null)
        {
            CreateEnergyParticles();
        }
        if (holographicParticles == null)
        {
            CreateHolographicParticles();
        }

        // Start in retracted state
        transform.localScale = Vector3.zero;
    }

    private void CreateEnergyParticles()
    {
        GameObject particlesObj = new GameObject("EnergyParticles");
        particlesObj.transform.SetParent(transform, false);
        particlesObj.transform.localPosition = Vector3.zero;

        energyParticles = particlesObj.AddComponent<ParticleSystem>();
        var main = energyParticles.main;
        main.startLifetime = 3f;
        main.startSpeed = 1.5f;
        main.startSize = 0.05f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

        var emission = energyParticles.emission;
        emission.rateOverTime = 8;
        emission.burst = new ParticleSystem.Burst(0f, 5);

        var shape = energyParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        shape.arc = 360f;

        var colorOverLifetime = energyParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKey(0, energyColor);
        gradient.SetKey(1, new Color(energyColor.r, energyColor.g, energyColor.b, 0f));
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = energyParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0, 1);
        sizeCurve.AddKey(1, 0);
        sizeOverLifetime.size = sizeCurve;

        energyParticles.Play();
    }

    private void CreateHolographicParticles()
    {
        GameObject particlesObj = new GameObject("HolographicParticles");
        particlesObj.transform.SetParent(holographicDisplay.transform, false);
        particlesObj.transform.localPosition = Vector3.zero;

        holographicParticles = particlesObj.AddComponent<ParticleSystem>();
        var main = holographicParticles.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.5f;
        main.startSize = 0.02f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.loop = true;

        var emission = holographicParticles.emission;
        emission.rateOverTime = 15;

        var shape = holographicParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(0.8f, 0.5f, 0.1f);

        var colorOverLifetime = holographicParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKey(0, holographicColor);
        gradient.SetKey(1, new Color(holographicColor.r, holographicColor.g, holographicColor.b, 0f));
        colorOverLifetime.color = gradient;

        holographicParticles.Play();
    }

    private void Update()
    {
        if (isDeployed)
        {
            UpdateHolographicEffect();
            UpdateCardGlowEffect();
        }
    }

    private void UpdateHolographicEffect()
    {
        if (holographicDisplay != null)
        {
            holographicPulseTime += Time.deltaTime * holographicPulseSpeed;
            float pulseValue = Mathf.PingPong(holographicPulseTime, 1f) * holographicPulseIntensity;
            
            // Create a more complex pulsing effect using golden ratio
            float secondaryPulse = Mathf.PingPong(holographicPulseTime * goldenRatio, 1f) * holographicPulseIntensity * 0.5f;
            
            holographicProps.SetColor("_EmissionColor", holographicColor * (1f + pulseValue + secondaryPulse));
            holographicDisplay.GetComponent<MeshRenderer>().SetPropertyBlock(holographicProps);
        }
    }

    private void UpdateCardGlowEffect()
    {
        if (cardSlotsContainer != null)
        {
            cardGlowPulseTime += Time.deltaTime * cardGlowPulseSpeed;
            float pulseValue = Mathf.PingPong(cardGlowPulseTime, 1f) * cardGlowIntensity;
            
            // Create a more complex glow effect
            float secondaryPulse = Mathf.PingPong(cardGlowPulseTime * goldenRatio, 1f) * cardGlowIntensity * 0.5f;
            
            cardGlowProps.SetColor("_EmissionColor", primaryColor * (1f + pulseValue + secondaryPulse));
            
            foreach (Transform child in cardSlotsContainer.transform)
            {
                var renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.SetPropertyBlock(cardGlowProps);
                }
            }
        }
    }

    public void DeployDuelDisk()
    {
        if (!isDeployed)
        {
            StartCoroutine(DeployAnimation());
        }
    }

    public void RetractDuelDisk()
    {
        if (isDeployed)
        {
            StartCoroutine(RetractAnimation());
        }
    }

    private IEnumerator DeployAnimation()
    {
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        // Create a more complex deployment animation
        while (elapsedTime < deployAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / deployAnimationDuration;
            
            // Use golden ratio for animation timing
            float primaryT = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            float secondaryT = Mathf.Sin(t * Mathf.PI * goldenRatio * 0.5f);
            
            // Combine animations
            float finalT = (primaryT + secondaryT * 0.3f) / 1.3f;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, finalT);
            yield return null;
        }

        transform.localScale = targetScale;
        isDeployed = true;
        energyParticles.Play();
        holographicParticles.Play();
    }

    private IEnumerator RetractAnimation()
    {
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.zero;

        energyParticles.Stop();
        holographicParticles.Stop();

        while (elapsedTime < deployAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / deployAnimationDuration;
            
            // Use golden ratio for animation timing
            float primaryT = Mathf.Sin(t * Mathf.PI * 0.5f);
            float secondaryT = Mathf.Cos(t * Mathf.PI * goldenRatio * 0.5f);
            
            // Combine animations
            float finalT = (primaryT + secondaryT * 0.3f) / 1.3f;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, finalT);
            yield return null;
        }

        transform.localScale = targetScale;
        isDeployed = false;
    }

    public void AnimateCardSlot(int slotIndex)
    {
        if (cardSlotsContainer != null && slotIndex >= 0 && slotIndex < cardSlotsContainer.transform.childCount)
        {
            StartCoroutine(CardSlotAnimation(cardSlotsContainer.transform.GetChild(slotIndex)));
        }
    }

    private IEnumerator CardSlotAnimation(Transform slot)
    {
        Vector3 originalScale = slot.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float elapsedTime = 0f;

        // Create a more complex card slot animation
        while (elapsedTime < cardSlotAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / cardSlotAnimationDuration;
            
            // Use golden ratio for animation timing
            float primaryT = Mathf.PingPong(t * 2f, 1f);
            float secondaryT = Mathf.Sin(t * Mathf.PI * goldenRatio);
            
            // Combine animations
            float finalT = (primaryT + secondaryT * 0.3f) / 1.3f;
            
            slot.localScale = Vector3.Lerp(originalScale, targetScale, finalT);
            yield return null;
        }

        slot.localScale = originalScale;
    }

    public void PlayEnergyEffect()
    {
        if (energyParticles != null)
        {
            energyParticles.Play();
        }
        if (holographicParticles != null)
        {
            holographicParticles.Play();
        }
    }

    public void StopEnergyEffect()
    {
        if (energyParticles != null)
        {
            energyParticles.Stop();
        }
        if (holographicParticles != null)
        {
            holographicParticles.Stop();
        }
    }
} 