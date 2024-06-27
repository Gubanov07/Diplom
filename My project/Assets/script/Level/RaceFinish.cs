using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal.Internal;

public class RaceFinish : MonoBehaviour
{
    public RacePositions racePositionsScript;

    [Header("Car")]
    [SerializeField] private GameObject MyCar;
    [SerializeField] private GameObject AICar;

    [Header("Camera")]
    [SerializeField] private GameObject FinishCam;
    [SerializeField] private GameObject MainCamera;
    [SerializeField] private GameObject ViewModels;

    [Header("Audio")]
    [SerializeField] private GameObject CarAudio;
    [SerializeField] private GameObject CarAiAudio;
    [SerializeField] private AudioSource LevelMusic;
    [SerializeField] private AudioSource FinishMusic;

    [Header("UI")]
    [SerializeField] private Animation anim;
    [SerializeField] private GameObject InterfaceUI;
    [SerializeField] private GameObject FinishUI;
    [SerializeField] private GameObject TextFinish;

    [Header("Позиция игрока")]
    [SerializeField] private GameObject PlayerPosDisplay;
    [SerializeField] private TextMeshProUGUI PlayerPosText;

    [Header("Время")]
    [SerializeField] private GameObject PlayerTimeDisplay;
    [SerializeField] private TextMeshProUGUI PlayerTimeText;

    [SerializeField] private GameObject RestartBattuon;
    [SerializeField] private GameObject ExitBattuon;

    [Header("Время и позиция")]
    [SerializeField] private float currentLapTime;
    [SerializeField] private int playerPosition;

    private void Start()
    {
        var LapTime = MyCar.GetComponent<LapSystem>();
        currentLapTime = LapTime.currentLapTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            this.GetComponent<BoxCollider>().enabled = false;

            MyCar.SetActive(false);
            MyCar.GetComponent<CarController>().enabled = false;
            CarAudio.SetActive(false); 
            MyCar.SetActive(true);

            AICar.SetActive(false);
            AICar.GetComponent<CarAIController>().enabled = false;
            AICar.GetComponent<CarController>().enabled = false;
            CarAiAudio.SetActive(false);
            AICar.SetActive(true);

            InterfaceUI.SetActive(false);
            MainCamera.SetActive(false);
            FinishCam.SetActive(true);
            LevelMusic.Stop();
            FinishMusic.Play();
            StartCoroutine(CountFinsh());
            anim.Play("CameraBlur");
        }
        else if (other.tag == "AI")
        {
            AICar.SetActive(false);
            AICar.GetComponent<CarAIController>().enabled = false;
            AICar.GetComponent<CarController>().enabled = false;
            AICar.SetActive(true);
        }
    }

    IEnumerator CountFinsh()
    {
        yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(1);
        FinishUI.SetActive(true);

        yield return new WaitForSeconds(1);
        TextFinish.SetActive(true);

        yield return new WaitForSeconds(1);
        PlayerPosDisplay.SetActive(true);
        PlayerPosText.text = $"Место: {RacePositions.publicPosition}";

        yield return new WaitForSeconds(1);
        PlayerTimeDisplay.SetActive(true);
        PlayerTimeText.text = $"{Mathf.FloorToInt(currentLapTime / 60)}:{currentLapTime % 60:00.00}";

        yield return new WaitForSeconds(1);
        RestartBattuon.SetActive(true);
        ExitBattuon.SetActive(true); 
        Cursor.lockState = CursorLockMode.None;
    }
}
