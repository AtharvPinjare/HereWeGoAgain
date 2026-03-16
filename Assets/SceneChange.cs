using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChange : MonoBehaviour
{
    [SerializeField] private string StartScene = "Scene_01";
    
    public void OnStartButtonPressed()
    {
        SceneManager.LoadScene(StartScene);
    }
    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainChalega");
    }
}
