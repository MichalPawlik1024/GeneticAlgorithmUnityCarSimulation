using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float MaxDistance = 10f;
    public LayerMask SensorLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetValue()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, MaxDistance, SensorLayerMask.value))
        {
            return hitInfo.distance / MaxDistance;
        }
        else
        {
            return 1f;
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, MaxDistance, SensorLayerMask.value))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * hitInfo.distance);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * MaxDistance);
            }
        }
        else
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * MaxDistance);
        }
    }
}
