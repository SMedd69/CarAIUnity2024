using UnityEngine;

public class BreakingSelector : MonoBehaviour
{
    public float maxBreakTorque;
    public float minCarSpeed;


    void OnTriggerStay(Collider other)
    {
        if(other.tag == "AI")
        {
            float controlCurrentSpeed = other.transform.root.GetComponent<CarIA>().currentSpeed;
            if(controlCurrentSpeed >= minCarSpeed)
            {
                other.transform.root.GetComponent<CarIA>().inSelector = true;
                other.transform.root.GetComponent<CarIA>().WL.brakeTorque = maxBreakTorque;
                other.transform.root.GetComponent<CarIA>().WR.brakeTorque = maxBreakTorque;
            }
            else
            {
                other.transform.root.GetComponent<CarIA>().inSelector = false;
                other.transform.root.GetComponent<CarIA>().WL.brakeTorque = 0.0f;
                other.transform.root.GetComponent<CarIA>().WR.brakeTorque = 0.0f;

            }
            other.transform.root.GetComponent<CarIA>().isBreaking = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "AI")
        {
            other.transform.root.GetComponent<CarIA>().inSelector = false;
            other.transform.root.GetComponent<CarIA>().WL.brakeTorque = 0.0f;
            other.transform.root.GetComponent<CarIA>().WR.brakeTorque = 0.0f;

            other.transform.root.GetComponent<CarIA>().isBreaking = false;
        }
    }
}
