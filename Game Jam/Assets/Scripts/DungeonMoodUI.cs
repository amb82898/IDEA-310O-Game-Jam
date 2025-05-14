using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMoodUI : MonoBehaviour
{
    public Slider moodSlider;                 // Reference to the slider in UI
    public Image fillImage;                   // The color-changing fill
    public Color friendlyColor = Color.green;
    public Color neutralColor = Color.yellow;
    public Color hostileColor = Color.red;

    void Start()
    {
        moodSlider.value = 0;
    }


    void Update()
    {
        if (DungeonMoodManager.Instance != null)
        {
            float moodValue = DungeonMoodManager.Instance.currentMood; // 0 to 100
            moodSlider.value = moodValue;

        }
    }
}
