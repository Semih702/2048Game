using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMP_Text Record;


    private void Start()
    {
        Record.text = "Record: " + PlayerPrefs.GetInt("Score", 0).ToString();
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
