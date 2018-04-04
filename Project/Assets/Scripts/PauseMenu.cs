using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static bool GameIsPaused = false; //public to accessable, static for not to reference just to check from other classes if the game is paused

    public GameObject PauseMenuUI;  //to control the gameobject
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
	}

    public void Resume()
    {
        PauseMenuUI.SetActive(false); //disable the menu
        Time.timeScale = 1f;           //set time back to normal rate
        GameIsPaused = false;
    }

    void Pause()
    {
        PauseMenuUI.SetActive(true); //enable the gameobject
        Time.timeScale = 0f; //freeze the gametime
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
