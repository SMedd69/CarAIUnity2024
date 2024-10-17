using UnityEngine;

public class WheelAI : MonoBehaviour
{
    public WheelCollider wheel;
    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(wheel.rpm / 60 * 360 * Time.deltaTime, 0.0f, 0.0f);
        Vector3 rotY = transform.localEulerAngles;
        rotY.y = wheel.steerAngle - transform.localEulerAngles.z;
        transform.localEulerAngles = rotY;

        RaycastHit hit;
        Vector3 wheelPos;
        if(Physics.Raycast(wheel.transform.position, -wheel.transform.up, out hit, wheel.radius + wheel.suspensionDistance))
            wheelPos = hit.point + wheel.transform.up * wheel.radius;
        else
            wheelPos = wheel.transform.position - wheel.transform.up * wheel.suspensionDistance;
        
        transform.position = wheelPos;
    }
}
