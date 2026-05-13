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
        steering = Random.Range(-1f, 1f);
        engine = Random.Range(0.1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        steering = (float)DecisionSet.turnThreshold;
        engine = (float)DecisionSet.accelerateThreshold;

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
