using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public GameObject SuspensionOriginFL;
    public GameObject SuspensionOriginFR;
    public GameObject WheelRL;
    public GameObject WheelRR;

    public float MaxEngineMomentum = 1f;
    public float MaxSteeringAngle = 30f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float steering = Input.GetAxis("Horizontal");
        SuspensionOriginFL.transform.localRotation = Quaternion.Euler(0f, steering * MaxSteeringAngle, 0f);
        SuspensionOriginFR.transform.localRotation = Quaternion.Euler(0f, steering * MaxSteeringAngle, 0f);

        float engine = Input.GetAxis("Vertical");
        WheelRL.GetComponent<Wheel>().EngineMomentum = engine * MaxEngineMomentum;
        WheelRR.GetComponent<Wheel>().EngineMomentum = engine * MaxEngineMomentum;
    }
}
