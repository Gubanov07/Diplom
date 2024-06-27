using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    private AudioSource[] allAudioSources;
    public GameObject PauseMenuUI;
    public GameObject InterfaceUI;
    public GameObject MainCamera;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
                AudioListener.pause = false;
                Cursor.lockState = CursorLockMode.Locked;
                MainCamera.GetComponent<PostProcessLayer>().enabled = false;
            }
            else
            {
                Pause();
                AudioListener.pause = true;
                Cursor.lockState = CursorLockMode.None;
                MainCamera.GetComponent<PostProcessLayer>().enabled = true;
            }
        }
    }

    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        InterfaceUI.SetActive(true);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameIsPaused = false;
        MainCamera.GetComponent<PostProcessLayer>().enabled = false;
    }

    void Pause()
    {
        PauseMenuUI.SetActive(true);
        InterfaceUI.SetActive(false);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("Menu");
    }

    public void ResetScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        AudioListener.pause = false;
    }
}
