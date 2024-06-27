using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CarAIController : MonoBehaviour
{
    private CarController carController;

    [Header("���������")]
    public Transform path;
    private List<Transform> nodes;
    private int currectNode = 0;

    [Header("������ ���")]
    public bool reversing = false;
    public float reverCounter = 0;
    public float waitToReverse = 2;
    public float reverFor = 1.5f;

    [Header("������")]
    public bool isBraking;
    Rigidbody rb;

    [Header("����� � ���� ��������")]
    private float gasInput;
    private float gasDampen;
    public float currentAngle;
    public float maxAngle = 30f; // ���� �������� �����
    private float clanch;

    [Header("�������")]
    public float sensorLength = 3f; //��������� �������
    public Vector3 frontSensorPosition = new Vector3(0f, 0f, 0f); //������� ��������
    public float frontSideSensorPosition = 0.2f;
    public float frontSideAngle = 30f; //���� ������ ��������
    private bool avoiding = false;
    private float targetSteerAngle = 0; //�������� ��� �������� ����� �� �������


    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    void Start()
    {
        // ������ ������� ����� ��� ��
        Transform[] pathTransform = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransform.Length; i++)
        {
            if (pathTransform[i] != path.transform)
            {
                nodes.Add(pathTransform[i]);
            }
        }

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = carController.CenterOfMass.localPosition;
    }

    private void FixedUpdate()
    {
        CarBreak();
        Sensors();
        ApplySteer();
        Accelerate();
        CheckWaypointDistance();
        LerpToSteerAngle();
        Clanch();

        gasDampen = Mathf.Lerp(gasDampen, gasInput, Time.deltaTime * 3f);
        carController.CheckInput(gasDampen, currentAngle, clanch, isBraking);
    }


    //������� �������� �����
    private void ApplySteer()
    {
        if (avoiding) return;
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currectNode].position);
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxAngle;
        targetSteerAngle = newSteer;
    }
    private void LerpToSteerAngle()
    {
        carController.colliders.FLWColider.steerAngle = Mathf.Lerp(carController.colliders.FLWColider.steerAngle, targetSteerAngle, Time.deltaTime * maxAngle);
        carController.colliders.FRWColider.steerAngle = Mathf.Lerp(carController.colliders.FRWColider.steerAngle, targetSteerAngle, Time.deltaTime * maxAngle);
    }

    //��� � ������ ���
    private void Accelerate()
    {
        if (carController.speed < carController.maxSpeed && !isBraking)
        {
            if (!reversing)
            {
                gasInput = Mathf.Clamp01((maxAngle - Mathf.Abs(carController.speed * 0.01f * currentAngle) / maxAngle));
            }
            else
            {
                gasInput = -1;   
            }
        }
    }

    private void Clanch()
    {
        if (gasInput == 1)
        {
            clanch = Mathf.Lerp(carController.clutch, 1, Time.deltaTime);
        }
        else if (gasInput == -1)
        {
            clanch = Mathf.Lerp(carController.clutch, 1, Time.deltaTime);
        }
    }

    //�������� ��������� �� ��������� �����
    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, nodes[currectNode].position) < 10f)
        {
            if (currectNode == nodes.Count - 1)
            {
                currectNode = 0;
            }
            else
            {
                currectNode++;
            }
        }
    }

    //�������
    private void CarBreak()
    {
        if (!isBraking)
        {
            gasInput = -gasInput * ((Mathf.Clamp01((carController.speed) / carController.maxSpeed) * 2 - 1f));
            if (carController.speed == carController.maxSpeed)
            {
                isBraking = true;
            }
            else if (carController.speed == 170)
            {
                isBraking = false;
            }
        }
    }

    //�������
    private void Sensors()
    {
        RaycastHit hit;
        Vector3 sensorsStartPos = transform.position;
        sensorsStartPos += transform.forward * frontSensorPosition.z;
        sensorsStartPos += transform.up * frontSensorPosition.y;
        float avoidMultiplier = 0;
        avoiding = false;

        // �������� ����������� ������
        if (avoidMultiplier == 0)
        {
            if (Physics.Raycast(sensorsStartPos, transform.forward, out hit, sensorLength))
            {
                if (!hit.collider.CompareTag("Lap"))
                {
                    Debug.DrawLine(sensorsStartPos, hit.point);
                    avoiding = true;
                    if (hit.normal.x < 0)
                    {
                        avoidMultiplier = -1;
                    }
                    else
                    {
                        avoidMultiplier = 1;
                    }
                }
            }

            if (rb.velocity.magnitude < 2 && !reversing && !isBraking)
            {
                reverCounter += Time.deltaTime;
                if (reverCounter >= waitToReverse)
                {
                    reverCounter = 0;
                    reversing = true;
                }
            }
            else if (!reversing)
            {
                reverCounter = 0;
            }

        }

        if (reversing)
        {
            avoidMultiplier *= -1;
            reverCounter += Time.deltaTime;
            if (reverCounter >= reverFor)
            {
                reverCounter = 0;
                reversing = false;
            }
        }

        //�������� ������ ������
        sensorsStartPos += transform.right * frontSideSensorPosition;
        if (Physics.Raycast(sensorsStartPos, transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Lap"))
            {
                Debug.DrawLine(sensorsStartPos, hit.point);
                avoiding = true;
                avoidMultiplier -= 1f;
            }
        }

        //�������� ������ ������� ������
        else if (Physics.Raycast(sensorsStartPos, Quaternion.AngleAxis(frontSideAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            Debug.DrawLine(sensorsStartPos, hit.point);
            if (!hit.collider.CompareTag("Lap"))
            {
                avoiding = true;
                avoidMultiplier -= 0.5f;
            }
        }

        //�������� ����� ������
        sensorsStartPos -= transform.right * frontSideSensorPosition * 2;
        if (Physics.Raycast(sensorsStartPos, transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Lap"))
            {
                Debug.DrawLine(sensorsStartPos, hit.point);
                avoiding = true;
                avoidMultiplier += 1f;
            }
        }

        //�������� ����� ������� ������
        else if (Physics.Raycast(sensorsStartPos, Quaternion.AngleAxis(-frontSideAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            Debug.DrawLine(sensorsStartPos, hit.point);
            if (hit.collider.CompareTag("Lap"))
            {
                avoiding = true;
                avoidMultiplier += 0.5f;
            }
        }

        if (avoiding)
        {
            targetSteerAngle = maxAngle * avoidMultiplier;
        }
    }
}
