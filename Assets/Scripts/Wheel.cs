using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public float Radius = 0.5f;
    public GameObject SuspensionOrigin;
    public float SpringRate = 1000f;
    public float SpringLength = 1f;

    public float DamperBound = 0f;
    public float DamperRebound = 0f;

    public float GripCoefficient = 0.9f;
    public float LongStiffness = 1000f;
    public float LatStiffness = 1000f;

    public float Mass = 10f;

    public float EngineDerating = 1f;

    [HideInInspector]
    public float EngineMomentum { get; set; } = 0f;

    private Vector3 initialLocalPosition;
    private Rigidbody rb;
    private float lastLength;
    private float currentLength;
    private float springVelocity = 0f;

    private float rotation = 0f;
    private float angularVelocity = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody>();
        currentLength = lastLength = (SuspensionOrigin.transform.localPosition.y - transform.localPosition.y);
        //angularVelocity = 2f;
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        bool onGround = Physics.Raycast(SuspensionOrigin.transform.position, -SuspensionOrigin.transform.up, out hit, SpringLength + Radius);


        if (onGround)
        {
            currentLength = hit.distance - Radius;
            springVelocity = (currentLength - lastLength) / Time.fixedDeltaTime;
        }


        if (currentLength < 0.05f * SpringLength)
        {
            currentLength = 0.05f * SpringLength;
            springVelocity = 0f;
        }
        else if (currentLength > SpringLength)
        {
            currentLength = SpringLength;
            springVelocity = 0f;
        }

        float springForce = (SpringLength - currentLength) * SpringRate;

        float damper = springVelocity > 0f ? DamperRebound : DamperBound;
        float damperForce = -(springVelocity) * damper;

        float totalForce = springForce + damperForce;
        //Debug.Log($"Spring: {springForce}, Damper: {damperForce}");

        if (onGround)
        {
            rb.AddForceAtPosition(SuspensionOrigin.transform.up * totalForce, SuspensionOrigin.transform.position);
        }
        else
        {
            springVelocity += ((-totalForce) / Mass) * Time.fixedDeltaTime;
            currentLength -= springVelocity * Time.fixedDeltaTime;
        }

        lastLength = currentLength;
        transform.localPosition = SuspensionOrigin.transform.localPosition + new Vector3(0, -currentLength, 0);

        
        Vector3 contactPoint = SuspensionOrigin.transform.position - SuspensionOrigin.transform.up * (currentLength + Radius);
        Vector3 groundVelocity = rb.GetPointVelocity(contactPoint);

        Vector3 localVelocity = SuspensionOrigin.transform.InverseTransformDirection(groundVelocity);
        float tyreVelocity = angularVelocity * Radius;

        float slipZ = localVelocity.z - tyreVelocity;
        float slipX = localVelocity.x;

        float forceX = -slipX * LatStiffness;
        float forceZ = -slipZ * LongStiffness;

        float tyreForce = onGround ? springForce : 0f;
        float maxGrip = tyreForce * GripCoefficient;

        Vector3 totalTyreForce = new Vector3(forceX, 0f, forceZ);
        if (totalTyreForce.magnitude > maxGrip)
        {
            totalTyreForce = totalTyreForce.normalized * maxGrip;
        }

        rb.AddForceAtPosition(SuspensionOrigin.transform.TransformDirection(totalTyreForce), contactPoint);

        float engineMomentum = EngineMomentum / (1f + angularVelocity * EngineDerating);
        float momentum = totalTyreForce.z * Radius;
        float inertia = 0.5f * Mass * Radius * Radius;
        angularVelocity += ((EngineMomentum - momentum) / inertia) * Time.fixedDeltaTime;

        rotation += angularVelocity * Time.fixedDeltaTime;
        while (rotation > Mathf.PI)
            rotation -= 2f * Mathf.PI;
        while (rotation < -Mathf.PI)
            rotation += 2f * Mathf.PI;
        transform.localRotation = SuspensionOrigin.transform.localRotation * Quaternion.Euler(rotation * 180f / Mathf.PI, 0f, 90f);
    }
}
