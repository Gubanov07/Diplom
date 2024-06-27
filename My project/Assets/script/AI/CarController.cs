using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum GearState
{
    Neutral,
    Running,
    CheckingChange,
    Changing
}

public class CarController : MonoBehaviour
{
    public Rigidbody playerRB;

    [Header("Колеса")]
    public WheelColliders colliders;
    public WheelMeshs wheelMeshes;

    [Header("Управление")]
    public float gasInput;
    public float steeringInput;
    private float brakeInput;
    public bool handrbake = false;
    public float steerAngle;
    [Range(0, 1)]
    public float steeHelpValue = 0;

    [Header("Двигатель")]
    public float motorPower;
    public float speed;
    private float speedRB;
    private float speedClamped;
    public float maxSpeed;
    public float RPM; //обороты двишателя
    public float redLine;
    public float idleRPM;//Холостые обороты

    [Header("Тахометр")]
    public TMP_Text speedLabel;//скорость авто
    public TMP_Text gearText;//Текст вывода передача
    public Transform rpmNeedle;//Стрека тахометра
    public float minNeedleRotation;
    public float maxNeedleRotation;

    [Header("КПП")]
    public int currentGear;//Текущая передача
    public float [] gearRatios;//Передаточные числа
    public float differentialRation;//Диф. передаточное число
    private float currentTorque;//Текущий крутящий момент
    public float clutch;//сципление
    private float wheelRPM;//оборроты колес
    public AnimationCurve hpToRPMCurve;
    private GearState gearState;//Состояние передачи
    public float increaseGearRPM;//Повышение передачи
    public float decreaseGearRPM;//Понижение передачи
    public float changeGearTime = 0.5f;

    [Header("Тормоза")]
    public float brakePower;
    public GameObject tireTrail;

    [Header("Дым")]
    public WheelParticles wheelParticles;
    public GameObject smokePrefab;

    [Header("Центр массы авто")]
    public Transform CenterOfMass;

    public int isEngineRunning;
    Rigidbody rb;
    float lastYRotation;
    void Start()
    {

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = CenterOfMass.localPosition;
        InstantiateParticles();
    }

    void FixedUpdate()
    {
        if(rpmNeedle)
            rpmNeedle.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, RPM / (redLine * 1.1f)));
        if (gearText)
            gearText.text = (gearState == GearState.Neutral) ? "N" : (currentGear + 1).ToString();
        speed = colliders.FLWColider.rpm * colliders.FLWColider.radius * 2f * Mathf.PI / 10f;
        speedRB = playerRB.velocity.magnitude * 3.6f;
        speedClamped = Mathf.Lerp(speedClamped, speed, Time.deltaTime);

        //Скорость авто
        if (speedLabel != null)
            speedLabel.text = ((int)speedRB) + "";

        //Вызов функций
        ApplyMotor();
        ApplySteer();
        ApplyBrake();
        CheckParticles();
        SteerHelpAssist();
        ApplyWheelPosition();
    }

     public void CheckInput(float throttleIn, float steeringIn, float clutchIn, bool handrbakeIn)
    {
        //Управление 
        gasInput = throttleIn;
        if (Mathf.Abs(gasInput) > 0 && isEngineRunning==0)
        {
            StartCoroutine(GetComponent<EngineAudio>().StartEngine());
            gearState = GearState.Running;
        }

        steeringInput = steeringIn;

        //Передаточные числа
        if (gearState != GearState.Changing)
        {
            if (gearState == GearState.Neutral)
            {
                clutch = 0;
                if (Mathf.Abs(gasInput) > 0) gearState = GearState.Running;
            }
            else
            {
                clutch = clutchIn;
            }
        }
        else
        {
            clutch = 0;
        }

        handrbake = (handrbakeIn);
    }

    //Поворот колес 
    void ApplySteer()
    {
        colliders.FLWColider.steerAngle = steerAngle * steeringInput;
        colliders.FRWColider.steerAngle = steerAngle * steeringInput;
    }

    //Двиготель
    void ApplyMotor()
    {
        currentTorque = CalculateTorque();
        colliders.RRWColider.motorTorque = currentTorque * gasInput;
        colliders.RLWColider.motorTorque = currentTorque * gasInput;

    }

    float CalculateTorque()
    {
        float torque = 0;
        if (RPM < idleRPM + 200 && gasInput == 0 && currentGear == 0)
        {
            gearState = GearState.Neutral;
        }
        if (gearState == GearState.Running && clutch > 0)
        {
            if (RPM > increaseGearRPM)
            {
                StartCoroutine(ChangeGear(1));
            }
            else if (RPM < decreaseGearRPM)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }
        if (isEngineRunning > 0)
        {
            if (clutch < 0.1f)
            {
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, redLine * gasInput) + Random.Range(-50, 50), Time.deltaTime);
            }
            else
            {
                wheelRPM = Mathf.Abs((colliders.RRWColider.rpm + colliders.RLWColider.rpm) / 2f) * gearRatios[currentGear] * differentialRation;
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM - 100, wheelRPM), Time.deltaTime * 3f);
                torque = (hpToRPMCurve.Evaluate(RPM / redLine) * motorPower / RPM) * gearRatios[currentGear] * differentialRation * 5252f * clutch;
            }
        }
        return torque;
    }   

    //Тормоза
    void ApplyBrake()
    {
        colliders.FRWColider.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.FLWColider.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.RRWColider.brakeTorque = brakeInput * brakePower * 0.3f;
        colliders.RLWColider.brakeTorque = brakeInput * brakePower * 0.3f;

        if (handrbake)
        {
            clutch = 0;
            colliders.RRWColider.brakeTorque = brakePower * 1000f;
            colliders.RLWColider.brakeTorque = brakePower * 1000f;
        }
    }
    //Эфекты от колес
    void InstantiateParticles()
    {
        if (smokePrefab)
        {
            wheelParticles.FRWheel = Instantiate(smokePrefab, colliders.FRWColider.transform.position - Vector3.up * colliders.FRWColider.radius, Quaternion.identity, colliders.FRWColider.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.FLWheel = Instantiate(smokePrefab, colliders.FLWColider.transform.position - Vector3.up * colliders.FLWColider.radius, Quaternion.identity, colliders.FLWColider.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.RRWheel = Instantiate(smokePrefab, colliders.RRWColider.transform.position - Vector3.up * colliders.RRWColider.radius, Quaternion.identity, colliders.RRWColider.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.RLWheel = Instantiate(smokePrefab, colliders.RLWColider.transform.position - Vector3.up * colliders.RLWColider.radius, Quaternion.identity, colliders.RLWColider.transform)
                .GetComponent<ParticleSystem>();
        }
        if (tireTrail)
        {
            wheelParticles.FRWheelTrail = Instantiate(tireTrail, colliders.FRWColider.transform.position - Vector3.up * colliders.FRWColider.radius, Quaternion.identity, colliders.FRWColider.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.FLWheelTrail = Instantiate(tireTrail, colliders.FLWColider.transform.position - Vector3.up * colliders.FLWColider.radius, Quaternion.identity, colliders.FLWColider.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.RRWheelTrail = Instantiate(tireTrail, colliders.RRWColider.transform.position - Vector3.up * colliders.RRWColider.radius, Quaternion.identity, colliders.RRWColider.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.RLWheelTrail = Instantiate(tireTrail, colliders.RLWColider.transform.position - Vector3.up * colliders.RLWColider.radius, Quaternion.identity, colliders.RLWColider.transform)
                .GetComponent<TrailRenderer>();
        }
    }

    void CheckParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        colliders.FRWColider.GetGroundHit(out wheelHits[0]);
        colliders.FLWColider.GetGroundHit(out wheelHits[1]);
        colliders.RRWColider.GetGroundHit(out wheelHits[2]);
        colliders.RLWColider.GetGroundHit(out wheelHits[3]);

        float slipAllowance = 0.2f;

        //Переднее правое
        if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))
        {
            wheelParticles.FRWheel.Play();
            wheelParticles.FRWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.FRWheel.Stop();
            wheelParticles.FRWheelTrail.emitting = false;
        }
        //Переднее левое
        if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))
        {
            wheelParticles.FLWheel.Play();
            wheelParticles.FLWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.FLWheel.Stop();
            wheelParticles.FLWheelTrail.emitting = false;
        }
        //Заднне правое
        if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))
        {
            wheelParticles.RRWheel.Play();
            wheelParticles.RRWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.RRWheel.Stop();
            wheelParticles.RRWheelTrail.emitting = false;
        }
        //Заднне левое
        if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))
        {
            wheelParticles.RLWheel.Play();
            wheelParticles.RLWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.RLWheel.Stop();
            wheelParticles.RLWheelTrail.emitting = false;
        }

    }

    void SteerHelpAssist()
    {
        if (Mathf.Abs(transform.rotation.eulerAngles.y - lastYRotation) < 10f)
        {
            float turnAdjust = (transform.rotation.eulerAngles.y - lastYRotation) * steeHelpValue;
            Quaternion rotateHelp = Quaternion.AngleAxis(turnAdjust, Vector3.up);
            rb.velocity = rotateHelp * rb.velocity;
        }
        lastYRotation = transform.rotation.eulerAngles.y;
    }

    void ApplyWheelPosition()
    {
        UpdateWheel(colliders.FRWColider, wheelMeshes.FRWheel);
        UpdateWheel(colliders.FLWColider, wheelMeshes.FLWheel);
        UpdateWheel(colliders.RRWColider, wheelMeshes.RRWheel);
        UpdateWheel(colliders.RLWColider, wheelMeshes.RLWheel);
    }
    void UpdateWheel(WheelCollider coll, Transform wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }

    public float GetSpeedRatio()
    {
        var gas = Mathf.Clamp(Mathf.Abs(gasInput), 0.5f, 1f);
        return RPM * gas / redLine;
    }
    IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if(currentGear+gearChange >= 0)
        {
            if(gearChange > 0)
            {
                //Увеличить передачу
                yield return new WaitForSeconds(0.7f);
                if(RPM < increaseGearRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if(gearChange < 0) 
            {
                //Уменьшить передачу
                yield return new WaitForSeconds(0.1f);
                if (RPM > decreaseGearRPM || currentGear <= 0)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            yield return new WaitForSeconds(changeGearTime);
            currentGear += gearChange;
        }
        if(gearState!=GearState.Neutral)
        gearState = GearState.Running;
    }
}

[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWColider;
    public WheelCollider FLWColider;
    public WheelCollider RRWColider;
    public WheelCollider RLWColider;

}

[System.Serializable]
public class WheelMeshs
{
    public Transform FRWheel;
    public Transform FLWheel;
    public Transform RRWheel;
    public Transform RLWheel;
}

[System.Serializable]
public class WheelParticles 
{
    public ParticleSystem FRWheel;
    public ParticleSystem FLWheel;
    public ParticleSystem RRWheel;
    public ParticleSystem RLWheel;

    public TrailRenderer FRWheelTrail;
    public TrailRenderer FLWheelTrail;
    public TrailRenderer RRWheelTrail;
    public TrailRenderer RLWheelTrail;
}
