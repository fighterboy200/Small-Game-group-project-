using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        
    }


    public void PlayGame()
    {
        SceneManager.LoadScene("MainLevel");
    }

    public void SettingsMenu()
    {
    
    }

    public void BackButton()
    {
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
