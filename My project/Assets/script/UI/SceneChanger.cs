using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    public new GameObject camera;

    public GameObject MainMenuUI;
    public GameObject SettingsMenuUI;

    public void Menu()
    {
        SettingsMenuUI.SetActive(false);
        MainMenuUI.SetActive(true);
        camera.GetComponent<PostProcessLayer>().enabled = false;
    }

    public void Settings()
    {
        SettingsMenuUI.SetActive(true);
        MainMenuUI.SetActive(false);
        camera.GetComponent<PostProcessLayer>().enabled = true;
    }

    public void PlayGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene("Lavel1");
    }
    public void ExitGame()
    {
        Application.Quit();
    }

}
