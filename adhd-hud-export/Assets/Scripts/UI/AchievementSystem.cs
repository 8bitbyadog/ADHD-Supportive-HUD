using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class AchievementSystem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject achievementPopupPrefab;
    [SerializeField] private Transform achievementContainer;
    [SerializeField] private float popupDuration = 3f;
    [SerializeField] private float popupSpacing = 10f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem achievementParticles;
    [SerializeField] private AnimationCurve popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color epicColor = Color.purple;
    [SerializeField] private Color legendaryColor = Color.yellow;
    
    [Header("Audio")]
    [SerializeField] private AudioClip commonSound;
    [SerializeField] private AudioClip rareSound;
    [SerializeField] private AudioClip epicSound;
    [SerializeField] private AudioClip legendarySound;
    
    private Queue<Achievement> achievementQueue = new Queue<Achievement>();
    private List<GameObject> activePopups = new List<GameObject>();
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void UnlockAchievement(Achievement achievement)
    {
        achievementQueue.Enqueue(achievement);
        StartCoroutine(ProcessAchievementQueue());
    }
    
    private IEnumerator ProcessAchievementQueue()
    {
        while (achievementQueue.Count > 0)
        {
            var achievement = achievementQueue.Dequeue();
            yield return StartCoroutine(ShowAchievementPopup(achievement));
        }
    }
    
    private IEnumerator ShowAchievementPopup(Achievement achievement)
    {
        // Create popup
        GameObject popup = Instantiate(achievementPopupPrefab, achievementContainer);
        activePopups.Add(popup);
        
        // Set up popup content
        var iconImage = popup.GetComponentInChildren<Image>();
        var titleText = popup.GetComponentInChildren<TextMeshProUGUI>();
        var descriptionText = popup.GetComponentsInChildren<TextMeshProUGUI>()[1];
        
        if (iconImage != null) iconImage.sprite = achievement.icon;
        if (titleText != null) titleText.text = achievement.title;
        if (descriptionText != null) descriptionText.text = achievement.description;
        
        // Position popup
        RectTransform rect = popup.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -activePopups.Count * (rect.rect.height + popupSpacing));
        
        // Play effects
        PlayAchievementEffects(achievement);
        
        // Animate in
        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition + Vector2.right * 500f;
        Vector2 targetPos = rect.anchoredPosition;
        
        while (elapsed < 0.5f)
        {
            float t = popupCurve.Evaluate(elapsed / 0.5f);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Wait
        yield return new WaitForSeconds(popupDuration);
        
        // Animate out
        elapsed = 0f;
        startPos = rect.anchoredPosition;
        targetPos = rect.anchoredPosition + Vector2.right * 500f;
        
        while (elapsed < 0.5f)
        {
            float t = popupCurve.Evaluate(elapsed / 0.5f);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Clean up
        activePopups.Remove(popup);
        Destroy(popup);
        
        // Reposition remaining popups
        for (int i = 0; i < activePopups.Count; i++)
        {
            RectTransform popupRect = activePopups[i].GetComponent<RectTransform>();
            Vector2 newPos = new Vector2(0, -i * (popupRect.rect.height + popupSpacing));
            StartCoroutine(AnimatePopupPosition(popupRect, newPos));
        }
    }
    
    private IEnumerator AnimatePopupPosition(RectTransform rect, Vector2 targetPos)
    {
        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        
        while (elapsed < 0.3f)
        {
            float t = popupCurve.Evaluate(elapsed / 0.3f);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        rect.anchoredPosition = targetPos;
    }
    
    private void PlayAchievementEffects(Achievement achievement)
    {
        // Play particles
        if (achievementParticles != null)
        {
            var main = achievementParticles.main;
            main.startColor = GetAchievementColor(achievement);
            achievementParticles.Play();
        }
        
        // Play sound
        AudioClip soundToPlay = GetAchievementSound(achievement);
        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
    }
    
    private Color GetAchievementColor(Achievement achievement)
    {
        // You can customize this based on achievement type or rarity
        switch (achievement.rarity)
        {
            case AchievementRarity.Common:
                return commonColor;
            case AchievementRarity.Rare:
                return rareColor;
            case AchievementRarity.Epic:
                return epicColor;
            case AchievementRarity.Legendary:
                return legendaryColor;
            default:
                return commonColor;
        }
    }
    
    private AudioClip GetAchievementSound(Achievement achievement)
    {
        switch (achievement.rarity)
        {
            case AchievementRarity.Common:
                return commonSound;
            case AchievementRarity.Rare:
                return rareSound;
            case AchievementRarity.Epic:
                return epicSound;
            case AchievementRarity.Legendary:
                return legendarySound;
            default:
                return commonSound;
        }
    }
}

public enum AchievementRarity
{
    Common,
    Rare,
    Epic,
    Legendary
} 