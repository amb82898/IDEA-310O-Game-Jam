using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScreen : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Button Clicked!");
        SceneManager.LoadScene("Dungeon1");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("IntroScene");
    }
}