using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class RacePositions : MonoBehaviour
{
    [SerializeField] private GameObject RaceFinish;
    public GameObject player;
    public GameObject[] bots;
    public GameObject[] checkpoints;
    public TMP_Text positionText;
    public int totalLaps = 3;

    public static int publicPosition;

    //Структуры данных для хранения информации о гонщиках
    public struct RacerData
    {
        public GameObject racer;
        public int currentLap;
        public int checkpointIndex;
        public float distanceToCheckpoint;
    }

    public RacerData[] racerData;

    void Start()
    {
        // Инициализировать массив данных гонщиков
        racerData = new RacerData[bots.Length + 1];

        //Все данные игрока
        racerData[0].racer = player;
        racerData[0].currentLap = 1;
        racerData[0].checkpointIndex = 0;
        racerData[0].distanceToCheckpoint = Vector3.Distance(player.transform.position, checkpoints[0].transform.position);

        //Все данные ИИ
        for (int i = 0; i < bots.Length; i++)
        {
            racerData[i + 1].racer = bots[i];
            racerData[i + 1].currentLap = 1;
            racerData[i + 1].checkpointIndex = 0;
            racerData[i + 1].distanceToCheckpoint = Vector3.Distance(bots[i].transform.position, checkpoints[0].transform.position);
        }
    }

    void Update()
    {
        for (int i = 0; i < racerData.Length; i++)
        {
            //Проверка на прохождения чекпоинта
            if (Vector3.Distance(racerData[i].racer.transform.position, checkpoints[racerData[i].checkpointIndex].transform.position) < 20f)
            {
                racerData[i].checkpointIndex++;

                //Проверка на завершения круга гоншиком
                if (racerData[i].checkpointIndex >= checkpoints.Length)
                {
                    racerData[i].checkpointIndex = 0;
                    racerData[i].currentLap++;

                    if (racerData[i].currentLap > totalLaps)
                    {
                        RaceFinish.SetActive(true);
                    }
                }

                //Растояние до следущего чекпоинта
                racerData[i].distanceToCheckpoint = Vector3.Distance(racerData[i].racer.transform.position, checkpoints[racerData[i].checkpointIndex].transform.position);
            }
            else
            {
                //Растояние до текущего чекпоинта
                racerData[i].distanceToCheckpoint = Vector3.Distance(racerData[i].racer.transform.position, checkpoints[racerData[i].checkpointIndex].transform.position);
            }
        }

        SortRacers();

        //Позиция игрока
        int playerPosition = 1;
        for (int i = 0; i < racerData.Length; i++)
        {
            if (racerData[i].racer == player)
            {
                break;
            }
            else
            {
                playerPosition++;
            }
        }
        publicPosition = playerPosition;

        positionText.text = "Позиция: " + playerPosition + " / " + racerData.Length;
    }

    //Сортирует гонщиков по кругу, текущему чекпоинту и расстоянию до следующего
    private void SortRacers()
    {
        for (int i = 0; i < racerData.Length - 1; i++)
        {
            for (int j = i + 1; j < racerData.Length; j++)
            {
                //Сравнение текущего круга гонщиков
                if (racerData[i].currentLap < racerData[j].currentLap)
                {
                    RacerData temp = racerData[i];
                    racerData[i] = racerData[j];
                    racerData[j] = temp;
                }
                else if (racerData[i].currentLap == racerData[j].currentLap)
                {
                    //Сравните собранных чекпоинтов
                    if (racerData[i].checkpointIndex < racerData[j].checkpointIndex)
                    {
                        RacerData temp = racerData[i];
                        racerData[i] = racerData[j];
                        racerData[j] = temp;
                    }
                    else if (racerData[i].checkpointIndex == racerData[j].checkpointIndex)
                    {
                        //Сравните расстояния до чекпоинта
                        if (racerData[i].distanceToCheckpoint > racerData[j].distanceToCheckpoint)
                        {
                            RacerData temp = racerData[i];
                            racerData[i] = racerData[j];
                            racerData[j] = temp;
                        }
                    }
                }
            }
        }
    }
}
