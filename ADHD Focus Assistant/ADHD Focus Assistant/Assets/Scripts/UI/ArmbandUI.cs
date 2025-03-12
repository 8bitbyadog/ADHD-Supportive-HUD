using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction.Input;
using UnityEngine.XR;
using System.Collections.Generic;

public class ArmbandUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform taskContainer;
    [SerializeField] private RectTransform pomodoroContainer;
    [SerializeField] private RectTransform healthContainer;
    
    [Header("Duel Disk Styling")]
    [SerializeField] private GameObject duelDiskBase;
    [SerializeField] private GameObject cardSlotsContainer;
    [SerializeField] private GameObject holographicDisplay;
    [SerializeField] private Material holographicMaterial;
    [SerializeField] private Color holographicColor = new Color(0.2f, 0.8f, 1f, 0.8f);
    [SerializeField] private float holographicPulseSpeed = 2f;
    [SerializeField] private float holographicPulseIntensity = 0.2f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private GameObject pomodoroDisplayPrefab;
    [SerializeField] private GameObject healthDisplayPrefab;

    [Header("Hand Tracking")]
    [SerializeField] private bool isLeftHanded = true; // Set to true for right-handed users
    [SerializeField] private float handTrackingSmoothing = 0.1f;
    [SerializeField] private Vector3 positionOffset = new Vector3(0.1f, 0.05f, 0.1f);
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 0f, 0f);

    [Header("Visual Settings")]
    [SerializeField] private Color workModeColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color breakModeColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private float elementSpacing = 10f;
    [SerializeField] private float containerPadding = 20f;
    [SerializeField] private float containerSpacing = 0.1f;
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private float backgroundBlur = 0.5f;

    [Header("Target Launcher")]
    [SerializeField] private GameObject targetLauncherPrefab;
    [SerializeField] private float launcherSpawnDistance = 5f;
    [SerializeField] private float launcherSpawnHeight = 1.5f;
    [SerializeField] private GameObject targetLauncherButton;
    [SerializeField] private ParticleSystem launcherSpawnEffect;

    [Header("Piano")]
    [SerializeField] private GameObject pianoPrefab;
    [SerializeField] private float pianoSpawnDistance = 3f;
    [SerializeField] private float pianoSpawnHeight = 1f;
    [SerializeField] private GameObject pianoButton;
    [SerializeField] private ParticleSystem pianoSpawnEffect;

    [Header("Piano Gauntlet")]
    [SerializeField] private float transformationDuration = 1.5f;
    [SerializeField] private AnimationCurve transformationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 pianoGauntletOffset = new Vector3(0.1f, 0.05f, 0.1f);
    [SerializeField] private Vector3 pianoGauntletRotation = new Vector3(0f, 0f, 0f);
    [SerializeField] private ParticleSystem transformationEffect;

    private List<GameObject> taskItems = new List<GameObject>();
    private GameObject pomodoroDisplay;
    private GameObject healthDisplay;
    private HUDCanvas hudCanvas;
    private MaterialPropertyBlock holographicProps;
    private float holographicPulseTime = 0f;
    private DuelDiskEffects duelDiskEffects;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 currentVelocity;
    private Quaternion currentRotation;
    private TargetLauncher activeLauncher;
    private bool isLauncherActive = false;
    private PianoManager activePiano;
    private bool isPianoActive = false;
    private bool isTransforming = false;
    private float transformationProgress = 0f;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Vector3 pianoGauntletScale = new Vector3(1.2f, 1.5f, 1.2f);
    private Quaternion pianoGauntletRotationQuat;

    private void Start()
    {
        hudCanvas = FindObjectOfType<HUDCanvas>();
        if (hudCanvas == null)
        {
            Debug.LogError("HUDCanvas not found in scene!");
            return;
        }

        InitializeDuelDisk();
        InitializeUI();
        InitializeEffects();
        
        // Store original transform values
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
        originalPosition = transform.position;
        pianoGauntletRotationQuat = Quaternion.Euler(pianoGauntletRotation);
    }

    private void InitializeDuelDisk()
    {
        // Create the model generator
        var modelGenerator = gameObject.AddComponent<DuelDiskModelGenerator>();

        // Create the base duel disk structure
        if (duelDiskBase == null)
        {
            duelDiskBase = CreateDuelDiskBase(modelGenerator);
        }

        // Create card slots container
        if (cardSlotsContainer == null)
        {
            cardSlotsContainer = CreateCardSlotsContainer(modelGenerator);
        }

        // Create holographic display
        if (holographicDisplay == null)
        {
            holographicDisplay = CreateHolographicDisplay(modelGenerator);
        }

        // Initialize holographic material properties
        holographicProps = new MaterialPropertyBlock();
        holographicProps.SetColor("_EmissionColor", holographicColor);
    }

    private GameObject CreateDuelDiskBase(DuelDiskModelGenerator generator)
    {
        GameObject baseObj = new GameObject("DuelDiskBase");
        baseObj.transform.SetParent(transform, false);

        // Add mesh filter and renderer
        MeshFilter meshFilter = baseObj.AddComponent<MeshFilter>();
        meshFilter.mesh = generator.GenerateBaseMesh();

        MeshRenderer meshRenderer = baseObj.AddComponent<MeshRenderer>();
        meshRenderer.material = generator.CreateBaseMaterial();

        return baseObj;
    }

    private GameObject CreateCardSlotsContainer(DuelDiskModelGenerator generator)
    {
        GameObject container = new GameObject("CardSlots");
        container.transform.SetParent(duelDiskBase.transform, false);

        // Create 5 card slots (like in Yu-Gi-Oh)
        for (int i = 0; i < 5; i++)
        {
            GameObject slot = CreateCardSlot(generator, i);
            slot.transform.SetParent(container.transform, false);
            slot.transform.localPosition = new Vector3(i * 0.1f, 0, 0);
        }

        return container;
    }

    private GameObject CreateCardSlot(DuelDiskModelGenerator generator, int index)
    {
        GameObject slot = new GameObject($"CardSlot_{index}");
        
        // Add mesh filter and renderer
        MeshFilter meshFilter = slot.AddComponent<MeshFilter>();
        meshFilter.mesh = generator.GenerateCardSlotMesh();

        MeshRenderer meshRenderer = slot.AddComponent<MeshRenderer>();
        meshRenderer.material = generator.CreateSlotMaterial();

        return slot;
    }

    private GameObject CreateHolographicDisplay(DuelDiskModelGenerator generator)
    {
        GameObject display = new GameObject("HolographicDisplay");
        display.transform.SetParent(duelDiskBase.transform, false);

        // Add mesh filter and renderer
        MeshFilter meshFilter = display.AddComponent<MeshFilter>();
        meshFilter.mesh = generator.GenerateHolographicDisplayMesh();

        MeshRenderer meshRenderer = display.AddComponent<MeshRenderer>();
        meshRenderer.material = generator.CreateHolographicMaterial();

        return display;
    }

    private void Update()
    {
        UpdateHandTracking();
        UpdateHolographicEffect();
        UpdateTransformation();
    }

    private void UpdateHolographicEffect()
    {
        if (holographicDisplay != null)
        {
            holographicPulseTime += Time.deltaTime * holographicPulseSpeed;
            float pulseValue = Mathf.PingPong(holographicPulseTime, 1f) * holographicPulseIntensity;
            
            holographicProps.SetColor("_EmissionColor", holographicColor * (1f + pulseValue));
            holographicDisplay.GetComponent<MeshRenderer>().SetPropertyBlock(holographicProps);
        }
    }

    private void UpdateHandTracking()
    {
        // Get the appropriate hand based on handedness
        XRNode handNode = isLeftHanded ? XRNode.LeftHand : XRNode.RightHand;
        
        // Get hand position and rotation
        if (InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) &&
            InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
        {
            // Calculate target position with offset
            targetPosition = position + rotation * positionOffset;
            
            // Calculate target rotation to face the user
            Vector3 forward = (Camera.main.transform.position - targetPosition).normalized;
            targetRotation = Quaternion.LookRotation(forward) * Quaternion.Euler(rotationOffset);
            
            // Smoothly interpolate position and rotation
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref currentVelocity,
                handTrackingSmoothing
            );
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime / handTrackingSmoothing
            );
        }
    }

    private void InitializeUI()
    {
        // Create containers if they don't exist
        if (taskContainer == null)
        {
            taskContainer = CreateContainer("TaskContainer");
        }
        if (pomodoroContainer == null)
        {
            pomodoroContainer = CreateContainer("PomodoroContainer");
        }
        if (healthContainer == null)
        {
            healthContainer = CreateContainer("HealthContainer");
        }

        // Set up visual properties
        SetupContainerVisuals(taskContainer);
        SetupContainerVisuals(pomodoroContainer);
        SetupContainerVisuals(healthContainer);
        
        // Arrange containers
        ArrangeContainers();

        // Create UI elements
        CreatePomodoroDisplay();
        CreateHealthDisplay();

        // Create target launcher button
        CreateTargetLauncherButton();

        // Create piano button
        CreatePianoButton();
    }

    private RectTransform CreateContainer(string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(transform, false);
        
        RectTransform rectTransform = container.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        VerticalLayoutGroup layoutGroup = container.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = elementSpacing;
        layoutGroup.padding = new RectOffset(
            (int)containerPadding,
            (int)containerPadding,
            (int)containerPadding,
            (int)containerPadding
        );

        return rectTransform;
    }

    private void SetupContainerVisuals(GameObject container)
    {
        // Add background panel
        GameObject background = new GameObject("Background");
        background.transform.SetParent(container.transform, false);
        
        // Add visual components (you'll need to implement these based on your UI system)
        // This could be a mesh with a material, or UI elements depending on your setup
    }
    
    private void ArrangeContainers()
    {
        float currentY = 0f;
        
        // Position containers vertically
        taskContainer.transform.localPosition = new Vector3(0f, currentY, 0f);
        currentY += containerSpacing;
        
        pomodoroContainer.transform.localPosition = new Vector3(0f, currentY, 0f);
        currentY += containerSpacing;
        
        healthContainer.transform.localPosition = new Vector3(0f, currentY, 0f);
    }

    private void CreatePomodoroDisplay()
    {
        if (pomodoroDisplayPrefab != null)
        {
            pomodoroDisplay = Instantiate(pomodoroDisplayPrefab, pomodoroContainer);
            var uiComponent = pomodoroDisplay.AddComponent<UIInteractionComponent>();
            uiComponent.interactionType = UIInteractionType.Pomodoro;
        }
    }

    private void CreateHealthDisplay()
    {
        if (healthDisplayPrefab != null)
        {
            healthDisplay = Instantiate(healthDisplayPrefab, healthContainer);
            var uiComponent = healthDisplay.AddComponent<UIInteractionComponent>();
            uiComponent.interactionType = UIInteractionType.Water;
        }
    }

    private void InitializeEffects()
    {
        duelDiskEffects = gameObject.AddComponent<DuelDiskEffects>();
        duelDiskEffects.duelDiskBase = duelDiskBase;
        duelDiskEffects.cardSlotsContainer = cardSlotsContainer;
        duelDiskEffects.holographicDisplay = holographicDisplay;
    }

    public void UpdateTaskDisplay(string[] tasks)
    {
        // Clear existing task items
        foreach (var item in taskItems)
        {
            Destroy(item);
        }
        taskItems.Clear();

        // Create new task items as cards
        if (taskItemPrefab != null)
        {
            for (int i = 0; i < Mathf.Min(tasks.Length, 5); i++)
            {
                GameObject taskItem = Instantiate(taskItemPrefab, cardSlotsContainer.transform.GetChild(i));
                var textComponent = taskItem.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = tasks[i];
                }

                var uiComponent = taskItem.AddComponent<UIInteractionComponent>();
                uiComponent.interactionType = UIInteractionType.Task;
                uiComponent.interactionIndex = i;

                taskItems.Add(taskItem);
                
                // Animate the card slot when adding a task
                duelDiskEffects.AnimateCardSlot(i);
            }
        }
    }

    public void UpdatePomodoroDisplay(float timeRemaining, bool isWorkMode)
    {
        if (pomodoroDisplay != null)
        {
            var textComponent = pomodoroDisplay.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                textComponent.text = $"{minutes:00}:{seconds:00}";
            }

            var imageComponent = pomodoroDisplay.GetComponentInChildren<Image>();
            if (imageComponent != null)
            {
                imageComponent.color = isWorkMode ? workModeColor : breakModeColor;
            }
        }
    }

    public void UpdateHealthDisplay(int waterIntake)
    {
        if (healthDisplay != null)
        {
            var textComponent = healthDisplay.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"Water: {waterIntake}/8 cups";
            }

            var imageComponent = healthDisplay.GetComponentInChildren<Image>();
            if (imageComponent != null)
            {
                imageComponent.fillAmount = waterIntake / 8f;
            }
        }
    }

    public void ShowDuelDisk()
    {
        if (duelDiskEffects != null)
        {
            duelDiskEffects.DeployDuelDisk();
        }
    }

    public void HideDuelDisk()
    {
        if (duelDiskEffects != null)
        {
            duelDiskEffects.RetractDuelDisk();
        }
    }

    private void CreateTargetLauncherButton()
    {
        if (targetLauncherButton == null)
        {
            targetLauncherButton = new GameObject("TargetLauncherButton");
            targetLauncherButton.transform.SetParent(transform, false);
            
            // Add button components
            Button button = targetLauncherButton.AddComponent<Button>();
            Image buttonImage = targetLauncherButton.AddComponent<Image>();
            TextMeshProUGUI buttonText = targetLauncherButton.AddComponent<TextMeshProUGUI>();
            
            // Set up button visuals
            buttonImage.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            buttonText.text = "Summon Target Launcher";
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontSize = 24;
            
            // Add click handler
            button.onClick.AddListener(ToggleTargetLauncher);
            
            // Position the button
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(200, 50);
            rectTransform.anchoredPosition = new Vector2(0, -100);
        }
    }

    private void ToggleTargetLauncher()
    {
        if (!isLauncherActive)
        {
            SpawnTargetLauncher();
        }
        else
        {
            DespawnTargetLauncher();
        }
    }

    private void SpawnTargetLauncher()
    {
        if (targetLauncherPrefab != null)
        {
            // Calculate spawn position in front of the player
            Vector3 playerForward = Camera.main.transform.forward;
            Vector3 spawnPosition = Camera.main.transform.position + 
                                  playerForward * launcherSpawnDistance + 
                                  Vector3.up * launcherSpawnHeight;
            
            // Spawn the launcher
            GameObject launcherObj = Instantiate(targetLauncherPrefab, spawnPosition, Quaternion.LookRotation(playerForward));
            activeLauncher = launcherObj.GetComponent<TargetLauncher>();
            
            // Play spawn effect
            if (launcherSpawnEffect != null)
            {
                launcherSpawnEffect.transform.position = spawnPosition;
                launcherSpawnEffect.Play();
            }
            
            // Update UI
            isLauncherActive = true;
            UpdateLauncherButtonState();
            
            // Start the launcher
            if (activeLauncher != null)
            {
                activeLauncher.StartNewRound();
            }
        }
    }

    private void DespawnTargetLauncher()
    {
        if (activeLauncher != null)
        {
            // Play despawn effect
            if (launcherSpawnEffect != null)
            {
                launcherSpawnEffect.transform.position = activeLauncher.transform.position;
                launcherSpawnEffect.Play();
            }
            
            // Destroy the launcher
            Destroy(activeLauncher.gameObject);
            activeLauncher = null;
            
            // Update UI
            isLauncherActive = false;
            UpdateLauncherButtonState();
        }
    }

    private void UpdateLauncherButtonState()
    {
        if (targetLauncherButton != null)
        {
            var buttonText = targetLauncherButton.GetComponent<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isLauncherActive ? "Dismiss Target Launcher" : "Summon Target Launcher";
            }
            
            var buttonImage = targetLauncherButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isLauncherActive ? 
                    new Color(0.8f, 0.2f, 0.2f, 0.8f) : 
                    new Color(0.2f, 0.8f, 1f, 0.8f);
            }
        }
    }

    private void CreatePianoButton()
    {
        if (pianoButton == null)
        {
            pianoButton = new GameObject("PianoButton");
            pianoButton.transform.SetParent(transform, false);
            
            // Add button components
            Button button = pianoButton.AddComponent<Button>();
            Image buttonImage = pianoButton.AddComponent<Image>();
            TextMeshProUGUI buttonText = pianoButton.AddComponent<TextMeshProUGUI>();
            
            // Set up button visuals
            buttonImage.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            buttonText.text = "Summon Piano";
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontSize = 24;
            
            // Add click handler
            button.onClick.AddListener(TogglePiano);
            
            // Position the button
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(200, 50);
            rectTransform.anchoredPosition = new Vector2(0, -150);
        }
    }

    private void TogglePiano()
    {
        if (!isPianoActive)
        {
            StartPianoTransformation();
        }
        else
        {
            EndPianoTransformation();
        }
    }

    private void StartPianoTransformation()
    {
        if (pianoPrefab != null)
        {
            // Start transformation animation
            isTransforming = true;
            transformationProgress = 0f;
            
            // Play transformation effect
            if (transformationEffect != null)
            {
                transformationEffect.Play();
            }
            
            // Spawn piano and parent it to the arm
            Vector3 spawnPosition = transform.position + transform.rotation * pianoGauntletOffset;
            GameObject pianoObj = Instantiate(pianoPrefab, spawnPosition, transform.rotation);
            activePiano = pianoObj.GetComponent<PianoManager>();
            
            // Update UI
            isPianoActive = true;
            UpdatePianoButtonState();
            
            // Hide duel disk elements
            if (duelDiskBase != null) duelDiskBase.SetActive(false);
            if (cardSlotsContainer != null) cardSlotsContainer.SetActive(false);
            if (holographicDisplay != null) holographicDisplay.SetActive(false);
        }
    }

    private void EndPianoTransformation()
    {
        if (activePiano != null)
        {
            // Start reverse transformation
            isTransforming = true;
            transformationProgress = 0f;
            
            // Play transformation effect
            if (transformationEffect != null)
            {
                transformationEffect.Play();
            }
            
            // Destroy piano
            Destroy(activePiano.gameObject);
            activePiano = null;
            
            // Show duel disk elements
            if (duelDiskBase != null) duelDiskBase.SetActive(true);
            if (cardSlotsContainer != null) cardSlotsContainer.SetActive(true);
            if (holographicDisplay != null) holographicDisplay.SetActive(true);
            
            // Update UI
            isPianoActive = false;
            UpdatePianoButtonState();
        }
    }

    private void UpdatePianoButtonState()
    {
        if (pianoButton != null)
        {
            var buttonText = pianoButton.GetComponent<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isPianoActive ? "Dismiss Piano" : "Summon Piano";
            }
            
            var buttonImage = pianoButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isPianoActive ? 
                    new Color(0.8f, 0.2f, 0.2f, 0.8f) : 
                    new Color(0.2f, 0.8f, 1f, 0.8f);
            }
        }
    }

    private void UpdateTransformation()
    {
        if (isTransforming)
        {
            transformationProgress += Time.deltaTime / transformationDuration;
            float t = transformationCurve.Evaluate(transformationProgress);
            
            // Interpolate between duel disk and piano gauntlet transforms
            transform.localScale = Vector3.Lerp(originalScale, pianoGauntletScale, t);
            transform.rotation = Quaternion.Slerp(originalRotation, pianoGauntletRotationQuat, t);
            
            // Update piano position if it exists
            if (activePiano != null)
            {
                Vector3 targetPosition = transform.position + transform.rotation * pianoGauntletOffset;
                activePiano.transform.position = targetPosition;
                activePiano.transform.rotation = transform.rotation;
            }
            
            if (transformationProgress >= 1f)
            {
                isTransforming = false;
                transformationProgress = 0f;
            }
        }
    }

    // Debug methods
    public void TestUIUpdate()
    {
        UpdateTaskDisplay(new string[] { "Test Task 1", "Test Task 2", "Test Task 3" });
        UpdatePomodoroDisplay(120f, true);
        UpdateHealthDisplay(4);
    }
} 