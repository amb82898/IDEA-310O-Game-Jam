using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[SelectionBase]
public class MoodBarTracker : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private Image bar;
    [SerializeField] private int moodCurrent = 50;
    [SerializeField] private int moodMax = 100;
    [SerializeField] private int moodAbsoluteMax = 100;
    [Space]
    [SerializeField] private bool overkillPossible;
    [Space]
    [SerializeField] private ShapeType shapeOfBar;

    public enum ShapeType
    {
        [InspectorName("Rectangle (Horizontal)")]
        RectangleHorizontal,
        [InspectorName("Rectangle (Vertical)")]
        RectangleVertical,
        [InspectorName("Circle")]
        Circle,
        Arc
    }

    [Header("Arc Settings")]
    [SerializeField, Range(0, 360)] private int endDegreeValue = 360;

    [Header("Animation Speed")]
    [SerializeField, Range(0, 0.5f)] private float _animationTime = 0.25f;
    private Coroutine _fillRoutine;

    [Header("Gradient Settings")]
    [SerializeField] private bool useGradient;
    [SerializeField] private Gradient barGradient;

    [Header("Events")]
    [SerializeField] private UnityEvent barIsFilledUp;
    private float _previousFillAmount;

    [Header("Test mode")]
    [SerializeField] private bool enableTesting;

    private void OnValidate()
    {
        ConfigureBarShapeAndProperties();
    }

    private void Start()
    {
        if (bar == null)
        {
            Debug.LogError("[MoodBarTracker] Bar Image is not assigned in the Inspector.");
            return;
        }

        if (DungeonMoodManager.Instance != null)
        {
            moodCurrent = Mathf.RoundToInt(DungeonMoodManager.Instance.currentMood);
            DungeonMoodManager.Instance.OnMoodChanged.AddListener(UpdateMoodBar);
            UpdateMoodBar(DungeonMoodManager.Instance.currentMood);
        }

        TriggerFillAnimation();
    }

    private void ConfigureBarShapeAndProperties()
    {
        if (bar == null)
        {
            Debug.LogError("[MoodBarTracker] Bar Image is not assigned. Please assign it.");
            return;
        }

        switch (shapeOfBar)
        {
            case ShapeType.RectangleHorizontal:
                bar.fillMethod = Image.FillMethod.Horizontal;
                break;
            case ShapeType.RectangleVertical:
                bar.fillMethod = Image.FillMethod.Vertical;
                break;
            case ShapeType.Circle:
            case ShapeType.Arc:
                bar.fillMethod = Image.FillMethod.Radial360;
                break;
        }

        if (!useGradient)
            bar.color = Color.white;

        UpdateBar();
    }

    private void UpdateMoodBar(float newMood)
    {
        Debug.Log($"[MoodBarTracker] UpdateMoodBar Triggered | New Mood: {newMood}");

        // If the object is disabled or destroyed, stop trying to update
        if (bar == null || !bar.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[MoodBarTracker] Bar is missing or inactive. Stopping updates.");
            return;
        }

        // Check if the enemy is dead, and if so, stop updating the bar
        DemoEnemyControls enemyControl = bar.GetComponentInParent<DemoEnemyControls>();
        if (enemyControl != null && enemyControl._isDead)
        {
            Debug.LogWarning("[MoodBarTracker] Enemy is dead. Stopping mood updates.");
            return;
        }

        moodCurrent = Mathf.RoundToInt(newMood);

        float percentage = DungeonMoodManager.Instance.GetMoodPercentage();

        bar.fillAmount = percentage;
        Debug.Log($"MoodBarTracker Update -> MoodCurrent: {moodCurrent} | FillAmount: {bar.fillAmount}");

        ApplyStateColor();
        TriggerFillAnimation();
    }

    private void UpdateBar()
    {
        // If the bar is missing or inactive, do nothing
        if (bar == null || !bar.gameObject.activeInHierarchy) return;

        if (moodMax <= 0)
        {
            bar.fillAmount = 0;
            return;
        }

        float fillAmount;

        if (shapeOfBar == ShapeType.Arc)
            fillAmount = CalculateCircularFillAmount();
        else
            fillAmount = (float)moodCurrent / moodMax;

        bar.fillAmount = fillAmount;
    }

    private float CalculateCircularFillAmount()
    {
        float fraction = (float)moodCurrent / moodMax;
        float fillRange = endDegreeValue / 360f;

        return fillRange * fraction;
    }

    private void TriggerFillAnimation()
    {
        if (bar == null) return;

        float targetFill = DungeonMoodManager.Instance != null
            ? DungeonMoodManager.Instance.GetMoodPercentage()
            : bar.fillAmount;

        if (Mathf.Approximately(bar.fillAmount, targetFill))
            return;

        if (_fillRoutine != null)
            StopCoroutine(_fillRoutine);

        _fillRoutine = StartCoroutine(SmoothlyTransitionToNewValue(targetFill));
    }

    private IEnumerator SmoothlyTransitionToNewValue(float targetFill)
    {
        // Exit if the object is destroyed
        if (bar == null || !bar.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[MoodBarTracker] Bar is missing or inactive. Stopping animation.");
            yield break;
        }

        float originalFill = bar.fillAmount;
        float elapsedTime = 0.0f;

        while (elapsedTime < _animationTime)
        {
            if (bar == null || !bar.gameObject.activeInHierarchy) yield break;

            elapsedTime += Time.deltaTime;
            float time = elapsedTime / _animationTime;
            bar.fillAmount = Mathf.Lerp(originalFill, targetFill, time);

            UseGradient();

            yield return null;
        }

        bar.fillAmount = targetFill;

        HandleEvent();
        _previousFillAmount = bar.fillAmount;
    }

    private void UseGradient()
    {
        if (!useGradient) return;

        Color targetColor;

        if (shapeOfBar == ShapeType.Arc)
        {
            float fillRange = bar.fillAmount / (endDegreeValue / 360f);
            targetColor = barGradient.Evaluate(fillRange);
        }
        else
        {
            targetColor = barGradient.Evaluate(bar.fillAmount);
        }

        bar.color = targetColor;
    }

    private void ApplyStateColor()
    {
        if (!useGradient) return;

        float fillPercentage = DungeonMoodManager.Instance.GetMoodPercentage();
        Color targetColor = barGradient.Evaluate(fillPercentage);

        StartCoroutine(AnimateColorChange(targetColor));
    }

    private IEnumerator AnimateColorChange(Color targetColor)
    {
        if (bar == null || !bar.gameObject.activeInHierarchy) yield break;

        Color originalColor = bar.color;
        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            bar.color = Color.Lerp(originalColor, targetColor, elapsedTime / duration);
            yield return null;
        }
        bar.color = targetColor;
    }

    private void HandleEvent()
    {
        if (_previousFillAmount >= 1)
            return;

        if (bar.fillAmount >= 1)
            barIsFilledUp?.Invoke();
    }
}
