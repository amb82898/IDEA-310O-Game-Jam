using UnityEngine;

public class EndGameTrigger : MonoBehaviour
{
    public GameObject goodEndingScreen;
    public GameObject badEndingScreen;

    private DungeonMoodManager moodManager;

    private void Start()
    {
        moodManager = DungeonMoodManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.CompareTag("Player"))
        {
            string currentMoodState = moodManager.GetMoodState();
            Debug.Log($"Current Mood State: {currentMoodState}");
            
            if (currentMoodState == "Friendly")
            {
                goodEndingScreen.SetActive(true);
                Debug.Log("Good Ending Triggered!");
            }
            else
            {
                badEndingScreen.SetActive(true);
                Debug.Log("Bad Ending Triggered!");
            }
            Time.timeScale = 0;
            UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold = int.MaxValue;
        }
    }
}