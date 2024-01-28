using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LostMenu : MonoBehaviour
{
    public TMP_Text recordScoreText;
    public TMP_Text latestScoreText;

    public void Start()
    {
        recordScoreText.text = "Record: "+PlayerPrefs.GetInt("Score", 0).ToString();
        latestScoreText.text = "Latest Score: " + PlayerPrefs.GetInt("latestScore", 0).ToString();
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }


    public void StartGame()
    {
        SceneManager.LoadScene("InGame");
    }

}
