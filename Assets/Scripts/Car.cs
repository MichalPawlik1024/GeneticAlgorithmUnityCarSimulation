using GeneticAlgorithm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public GameObject SuspensionOriginFL;
    public GameObject SuspensionOriginFR;
    public GameObject WheelRL;
    public GameObject WheelRR;

    public Sensor SensorLeft;
    public Sensor SensorRight;
    public Sensor SensorForward;

    public LayerMask WallLayerMask;

    public float MaxEngineMomentum = 1f;
    public float MaxSteeringAngle = 30f;

    public bool IsDeactivated = false;

    public DecisionSet DecisionSet;

    private float steering = 0f;
    private float engine = 0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float leftSensor = SensorLeft.GetValue();
        float rightSensor = SensorRight.GetValue();
        float forwardSensor = SensorForward.GetValue();

        steering = 0f;
        engine = 0f;

        if (leftSensor < rightSensor)
        {
            if (leftSensor < DecisionSet.turnThreshold)
                steering = (float)DecisionSet.steerValue;
        }
        else
        {
            if (rightSensor < DecisionSet.turnThreshold)
                steering = (float)-DecisionSet.steerValue;
        }

        float minDistance = Mathf.Min(leftSensor, rightSensor);

        if (forwardSensor < DecisionSet.decelerateThreshold)
        {
            engine = (float)-DecisionSet.decelerateValue;
        }
        else if (minDistance > DecisionSet.accelerateThreshold)
        {
            engine = (float)DecisionSet.accelerateValue;
        }

            //steering = (float)DecisionSet.turnThreshold;
            //engine = (float)DecisionSet.accelerateThreshold;

            //float steering = Input.GetAxis("Horizontal");
            SuspensionOriginFL.transform.localRotation = Quaternion.Euler(0f, steering * MaxSteeringAngle, 0f);
        SuspensionOriginFR.transform.localRotation = Quaternion.Euler(0f, steering * MaxSteeringAngle, 0f);

        //float engine = Input.GetAxis("Vertical");
        WheelRL.GetComponent<Wheel>().EngineMomentum = engine * MaxEngineMomentum;
        WheelRR.GetComponent<Wheel>().EngineMomentum = engine * MaxEngineMomentum;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & WallLayerMask.value) != 0)
        {
            IsDeactivated = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
