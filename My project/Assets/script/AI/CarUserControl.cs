using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarUserControl : MonoBehaviour
{
    private CarController carController;

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    void Update()
    {
       float throttleInput = Input.GetAxis("Vertical");

       float steeringInput = Input.GetAxis("Horizontal");

       float clutchInput = Input.GetKey(KeyCode.LeftShift) ? 0 : Mathf.Lerp(carController.clutch, 1, Time.deltaTime);

       bool handrbakeInput = Input.GetKey(KeyCode.Space);

       carController.CheckInput(throttleInput, steeringInput, clutchInput, handrbakeInput);
    }
}
