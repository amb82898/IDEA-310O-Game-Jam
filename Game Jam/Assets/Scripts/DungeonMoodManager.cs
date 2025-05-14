using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DungeonMoodManager : MonoBehaviour
{
    public static DungeonMoodManager Instance { get; private set; }
    public UnityEvent<float> OnMoodChanged = new UnityEvent<float>();

    [Range(0f, 100f)]
    public float currentMood = 50f; 
    // 0 = Hostile, 50 = Neutral, 100 = Friendly

    public float moodChangeAmount = 5f;

    [Header("Lighting Settings")]
    public Gradient lightGradient;

    private List<Light> pointLights = new List<Light>();
    private List<Renderer> emissiveObjects = new List<Renderer>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log("DungeonMoodManager Start Method Called!");

        // Subscribe the UpdateLightColor function to the event
        OnMoodChanged.AddListener(UpdateLightColor);

        if (OnMoodChanged == null || OnMoodChanged.GetPersistentEventCount() == 0)
        {
            Debug.LogError("[DungeonMoodManager] OnMoodChanged has NO subscribers!");
        }
        else
        {
            Debug.Log("[DungeonMoodManager] OnMoodChanged is properly subscribed!");
        }

        // Find all GameObjects tagged "PointLight" and store their Light and Renderer components
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("PointLight");
        Debug.Log($"[DungeonMoodManager] Found {lightObjects.Length} Point Light objects");

        foreach (GameObject obj in lightObjects)
        {
            Light lightComponent = obj.GetComponent<Light>();
            Renderer rendererComponent = obj.GetComponent<Renderer>();

            if (lightComponent != null)
            {
                pointLights.Add(lightComponent);
                Debug.Log($"[DungeonMoodManager] Light added: {lightComponent.name}");
            }

            if (rendererComponent != null && rendererComponent.material.IsKeywordEnabled("_EMISSION"))
            {
                emissiveObjects.Add(rendererComponent);
                Debug.Log($"[DungeonMoodManager] Emissive object added: {rendererComponent.name}");
            }
        }

        // Initialize their colors based on the current mood
        UpdateLightColor(currentMood);
    }

    public float GetMoodPercentage()
    {
        return currentMood / 100f;
    }

    public void ShiftMood(bool towardsFriendly)
    {
        Debug.Log($"ShiftMood Called | Current Mood Before: {currentMood} | Change Amount: {moodChangeAmount} | Towards Friendly: {towardsFriendly}");

        if (towardsFriendly)
            currentMood = Mathf.Clamp(currentMood + moodChangeAmount, 0f, 100f);
        else
            currentMood = Mathf.Clamp(currentMood - moodChangeAmount, 0f, 100f);

        Debug.Log($"Mood after Shift: {currentMood}");


        if (OnMoodChanged == null)
        {
            Debug.LogError("[DungeonMoodManager] OnMoodChanged event is NULL!");
        }
        else
        {
            Debug.Log("[DungeonMoodManager] OnMoodChanged event is NOT NULL");
        }

        Debug.Log("[DungeonMoodManager] Invoking Mood Change Event...");
        OnMoodChanged.Invoke(currentMood);
        Debug.Log("[DungeonMoodManager] Mood Change Event Invoked!");
    }
    private void UpdateLightColor(float newMood)
    {
        if (lightGradient == null)
        {
            Debug.LogError("[DungeonMoodManager] No gradient found.");
            return;
        }

        float percentage = newMood / 100f;
        Color targetColor = lightGradient.Evaluate(percentage);

        Debug.Log($"[DungeonMoodManager] New Mood Percentage: {percentage}");
        Debug.Log($"[DungeonMoodManager] Target Color from Gradient: {targetColor}");

        StartCoroutine(SmoothLightTransition(targetColor));

        foreach (var renderer in emissiveObjects)
        {
            if (renderer != null)
            {
                renderer.material.SetColor("_EmissionColor", targetColor);
                renderer.material.EnableKeyword("_EMISSION");
                Debug.Log($"[DungeonMoodManager] Force-Set Emission Color to: {targetColor}");
            }
        }
    }
    private IEnumerator SmoothLightTransition(Color targetColor)
    {
        float duration = 0.5f; 
        float time = 0;

        List<Color> originalColors = new List<Color>();
        List<Color> originalEmissionColors = new List<Color>();

        foreach (var light in pointLights)
        {
            if (light != null)
            {
                originalColors.Add(light.color);
            }
        }

        foreach (var renderer in emissiveObjects)
        {
            if (renderer != null)
            {
                originalEmissionColors.Add(renderer.material.GetColor("_EmissionColor"));
            }
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            for (int i = 0; i < pointLights.Count; i++)
            {
                if (pointLights[i] != null)
                {
                    pointLights[i].color = Color.Lerp(originalColors[i], targetColor, time / duration);
                }
            }

            for (int i = 0; i < emissiveObjects.Count; i++)
            {
                if (emissiveObjects[i] != null)
                {
                    Color emissionColor = Color.Lerp(originalEmissionColors[i], targetColor, time / duration);
                    emissiveObjects[i].material.SetColor("_EmissionColor", emissionColor);
                    emissiveObjects[i].material.EnableKeyword("_EMISSION");
                }
            }
            yield return null;
        }

        foreach (var light in pointLights)
        {
            if (light != null)
                light.color = targetColor;
        }
        foreach (var renderer in emissiveObjects)
        {
            if (renderer != null)
            {
                renderer.material.SetColor("_EmissionColor", targetColor);
                renderer.material.EnableKeyword("_EMISSION");
            }
        }
    }

    public string GetMoodState()
    {
        if (currentMood >= 75f) 
            return "Friendly";
        else if (currentMood <= 25f) 
            return "Hostile";
        else 
            return "Neutral";
    }
}
