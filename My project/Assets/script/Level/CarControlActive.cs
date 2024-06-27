using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControlActivator : MonoBehaviour
{
    public GameObject CarControl;
    public GameObject AI;

    void Start()
    {
        CarControl.GetComponent<CarUserControl>().enabled = true;
        AI.GetComponent<CarAIController>().enabled = true;
    }
}

