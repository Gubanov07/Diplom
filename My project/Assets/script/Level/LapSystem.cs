using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LapSystem : MonoBehaviour
{
    public RacePositions racePositionsScript;

    [Header("Чекпоинты")]
    [SerializeField] private GameObject start;
    [SerializeField] private GameObject finish;
    [SerializeField] private GameObject[] checkpoints;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI LapDisplayText;
    [SerializeField] private TextMeshProUGUI LapDisplayTime;

    [Header("Настройки")]
    [SerializeField] private int TotalLaps;

    [Header("Информация")]
    [SerializeField] private float currentCheckpoint;
    public float currentLapTime;
    [SerializeField] private int currentLap;
    public bool started;
    [SerializeField] private bool finished;

    private void Start()
    {
        currentCheckpoint = 0;
        currentLap = 1;
        started = false;
        finished = false;
    }

    void Update()
    {
        if (started && !finished)
        {
            currentLapTime += Time.deltaTime;
        }

        LapDisplayTime.text = $"Время: {Mathf.FloorToInt(currentLapTime / 60)}:{currentLapTime % 60:00.00}";
        LapDisplayText.text = "Круг: " + currentLap + "/" + TotalLaps;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == ("Lap"))
        {
            GameObject thisCheckpoint = other.gameObject;

            if (thisCheckpoint == start && !started)
            {
                started = true;
            }
            else if (thisCheckpoint == finish && started)
            {
                // Если все круги пройдены, завершите гонку
                if (currentLap == TotalLaps)
                {
                    if (currentCheckpoint == checkpoints.Length)
                    {
                        finished = true;
                    }
                }

                // Если не все круги пройдены, начается новый круг
                else if (currentLap < TotalLaps)
                {
                    if (currentCheckpoint == checkpoints.Length)
                    {
                        currentLap++;
                        currentCheckpoint = 0;
                    }
                }
            }

            // Проверка по контрольным точкам, чтобы сравнить и проверить, к кому из них прокосался игрок
            for (int i = 0; i < checkpoints.Length; i++)
            {
                if (finished)
                    return;

                // Если контрольная точка указана правильно
                if (thisCheckpoint == checkpoints[i] && i + 1 == currentCheckpoint + 1)
                {
                    currentCheckpoint++;
                }
            }
        }
    }
}
