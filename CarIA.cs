using UnityEngine;

public class CarIA : MonoBehaviour
{

    public float maxSteer = 15f;
    public float speedLerpSteer = 5f;
    public float distFromPath = 5f;
    public float maxTorque = 20f;
    public float topSpeed = 150f;
    public float decellerationSpeed = 10;
    public float currentSpeed;
    public int currentPathObj;

    [Header("\nBrake params")]
    public Renderer[] breakingMesh;
    public Material idleBreakingLight;
    public Material activeBreakingLight;
    public bool isBreaking;

    [Header("\nSensor Params")]
    public float sensorLength = 5f;
    public float sensorBreakingLength = 2.5f;
    public float decellerationSpeedSensor = 100f;
    public float sidewaySensorLength = 2f;
    public float frontSensorStartPoint = 0f;
    public float frontSensorSideDist = 0f;
    public float frontSensorsAngle = 30f;
    public float sensitivityFrontSensor = 1f;
    public float sensitivityFrontStraightSensor = 2f;
    public float sensitivityFrontAngleSensor = 1.5f;
    public float sensitivitySidewaySensor = 0.5f;
    public float avoidSpeed = 10f;
    [SerializeField] private int flag;

    [Header("\nReversing Params")]
    public float maxTorqueReverse;
    public bool reversing = false;
    public float reverCounter = 0.0f;
    public float waitToReverse = 2.0f;
    public float reverFor = 1.5f;

    [Header("\nRespawn Params")]
    public bool canRespawn = true;
    public float respawnWait = 5.0f;
    public float respawnCounter = 0.0f;


    [Header("\nCENTER OF MASS")]
    public Vector3 CenterOfMass;

    [Header("\nBooleans Params")]
    public bool isMoveToPaths;
    public bool inSelector;

    [Header("\n\n\nReferences")]
    public Transform pathGroup;
    public WheelCollider WFL;
    public WheelCollider WFR;
    public WheelCollider WL;
    public WheelCollider WR;
    Transform[] path;
    Rigidbody rb;

    void Start()
    {
        isMoveToPaths = true;
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = CenterOfMass;
        GetPath();    
    }

    void GetPath()
    {
        // Obtenir tous les enfants du GameObject (ce script est sur le parent)
        Transform[] path_objs = pathGroup.GetComponentsInChildren<Transform>();

        // Filtrer pour exclure le transform du parent lui-même
        path = new Transform[path_objs.Length - 1]; // On ne compte pas le parent lui-même
        int index = 0;

        for (int i = 0; i < path_objs.Length; i++)
        {
            if (path_objs[i] != pathGroup) // Ignorer le parent lui-même
            {
                path[index] = path_objs[i];
                index++;
            }
        }
    }

    void Update()
    {
        if(flag == 0) GetSteer();
        Move();
        BreakingEffect();
        Sensors();
    }

    void GetSteer()
    {
        Vector3 steerVector = transform.InverseTransformPoint(new Vector3(path[currentPathObj].position.x, transform.position.y, path[currentPathObj].position.z));
        float newSteer = maxSteer * (steerVector.x / steerVector.magnitude);

        WFL.steerAngle = Mathf.Lerp(WFL.steerAngle, newSteer, Time.deltaTime * speedLerpSteer);
        WFR.steerAngle = Mathf.Lerp(WFR.steerAngle, newSteer, Time.deltaTime * speedLerpSteer);
        
        if (steerVector.magnitude <= distFromPath) 
        {
            currentPathObj++;
            if (currentPathObj >= path.Length) currentPathObj = 0;
        }
    }

    void Move()
    {
        if(!isMoveToPaths) return;
        currentSpeed = 2 * Mathf.PI * WL.radius * WL.rpm * 60 / 1000;
        currentSpeed = Mathf.Round(currentSpeed);

        if(currentSpeed <= topSpeed && !inSelector)
        {
            if(!reversing)
            {
                WL.motorTorque = maxTorque;
                WR.motorTorque = maxTorque;
            }
            else
            {
                WL.motorTorque = -maxTorqueReverse;
                WR.motorTorque = -maxTorqueReverse;
            }

            WL.brakeTorque = 0;
            WR.brakeTorque = 0;
        }
        else if(!inSelector)
        {
            WL.motorTorque = 0.0f;
            WR.motorTorque = 0.0f;
            WL.brakeTorque = decellerationSpeed;
            WR.brakeTorque = decellerationSpeed;
        }
    }

    void BreakingEffect()
    {
        foreach (Renderer breakingRenderer in breakingMesh)
        {
            if (isBreaking)
            {
                breakingRenderer.material = activeBreakingLight;
            }
            else
            {
                breakingRenderer.material = idleBreakingLight;
            }
            
        }
    }

    public float intensityBrakeSensor = 1.5f;
    void Sensors()
    {
        flag = 0;
        float avoidSensitivity = 0.0f;

        Vector3 pos;
        RaycastHit hit;
        Vector3 angleRightSensor = Quaternion.AngleAxis(frontSensorsAngle, transform.up) * transform.forward;
        Vector3 angleLeftSensor = Quaternion.AngleAxis(-frontSensorsAngle, transform.up) * transform.forward;
        
        
        pos = transform.position;
        pos += transform.forward * frontSensorStartPoint;
        
        // Front Breaking Sensor
        if(Physics.Raycast(pos, transform.forward, out hit, sensorBreakingLength))
        {
            if(hit.transform.tag != "Terrain")
            {
                flag++;
                
                WR.brakeTorque = Mathf.Lerp(WR.brakeTorque, decellerationSpeedSensor, Time.deltaTime * intensityBrakeSensor);
                WL.brakeTorque = Mathf.Lerp(WL.brakeTorque, decellerationSpeedSensor, Time.deltaTime * intensityBrakeSensor);

                WFR.brakeTorque = Mathf.Lerp(WFR.brakeTorque, decellerationSpeedSensor, Time.deltaTime * intensityBrakeSensor);;
                WFL.brakeTorque = Mathf.Lerp(WFL.brakeTorque, decellerationSpeedSensor, Time.deltaTime * intensityBrakeSensor);;
                
                Debug.DrawLine(pos, hit.point, Color.red);
            }
        }
        else
        {
            WL.brakeTorque = 0.0f;
            WR.brakeTorque = 0.0f;
            WFL.brakeTorque = 0.0f;
            WFR.brakeTorque = 0.0f;

        }

        // Front Sensor Right
        pos += transform.right * frontSensorSideDist;

        if(Physics.Raycast(pos, transform.forward, out hit, sensorLength))
        {
            if(hit.transform.tag != "Terrain")
            {
                flag++;
                avoidSensitivity -= sensitivityFrontStraightSensor;
                Debug.DrawLine(pos, hit.point, Color.magenta);
            }
        }
        
        // Front Sensor Angled Right
        else if(Physics.Raycast(pos, angleRightSensor, out hit, sensorLength))
        {
            if(hit.transform.tag != "Terrain")
            {
                flag++;

                avoidSensitivity -= sensitivityFrontAngleSensor;
                Debug.DrawLine(pos, hit.point, Color.magenta);
            }
        }

        // Front Sensor Left
        pos = transform.position;
        pos += transform.forward * frontSensorStartPoint;
        pos += transform.right * -frontSensorSideDist;

        if(Physics.Raycast(pos, transform.forward, out hit, sensorLength))
        {
            if(hit.transform.tag != "Terrain")
            {
                flag++;

                avoidSensitivity += sensitivityFrontStraightSensor;
                Debug.DrawLine(pos, hit.point, Color.magenta);
            }
        }
        
        // Front Sensor Angled Left
        else if(Physics.Raycast(pos, angleLeftSensor, out hit, sensorLength))
        {
            if(hit.transform.tag != "Terrain")
            {
                flag++;

                avoidSensitivity += sensitivityFrontAngleSensor;
                Debug.DrawLine(pos, hit.point, Color.magenta);
            }
        }
        
        // Right SideWay (Coté) Sensor
        if(Physics.Raycast(transform.position, transform.right, out hit, sidewaySensorLength))
        {
            if(hit.transform.tag != "Terrain")
            {
                flag++;

                avoidSensitivity -= sensitivitySidewaySensor;
                Debug.DrawLine(transform.position, hit.point, Color.magenta);
            }
        }

        // Left SideWay (Coté) Sensor
        if(Physics.Raycast(transform.position, -transform.right, out hit, sidewaySensorLength))
        {
            if(hit.transform.tag != "Terrain")
            {
                flag++;

                avoidSensitivity += sensitivitySidewaySensor;
                Debug.DrawLine(transform.position, hit.point, Color.magenta);
            }
        }
        
        pos = transform.position;
        pos += transform.forward * frontSensorStartPoint;
        
        // Front Mid Sensor
        if(avoidSensitivity == 0)
        {
            if(Physics.Raycast(pos, transform.forward, out hit, sensorLength))
            {
                if(hit.transform.tag != "Terrain")
                {
                    Debug.Log(hit.normal.x);
                    if(hit.normal.x < 0) avoidSensitivity = -sensitivityFrontSensor;
                    else avoidSensitivity = sensitivityFrontSensor;

                    Debug.DrawLine(pos, hit.point, Color.magenta);
                }
            }
        }

        if(rb.velocity.magnitude < 2 && !reversing && flag > 0)
        {
            reverCounter += Time.deltaTime;
            if(reverCounter >= waitToReverse)
            {
                reverCounter = 0.0f;
                reversing = true;
            }
        }
        else if(!reversing) 
            reverCounter = 0.0f;

        if(reversing)
        {
            WL.brakeTorque = 0.0f;
            WR.brakeTorque = 0.0f;
            WFL.brakeTorque = 0.0f;
            WFR.brakeTorque = 0.0f;
            
            avoidSensitivity *= -sensitivityFrontSensor;

            reverCounter += Time.deltaTime;

            if(reverCounter >= reverFor)
            {
                reverCounter = 0.0f;
                reversing = false;
            }
        }

        if(flag != 0) AvoidSteer(avoidSensitivity);
    }

    void AvoidSteer(float sensitivity)
    {
        float targetAvoidAngle = avoidSpeed * sensitivity;
        WFR.steerAngle = Mathf.Lerp(WFR.steerAngle, targetAvoidAngle, Time.deltaTime * speedLerpSteer);
        WFL.steerAngle = Mathf.Lerp(WFL.steerAngle, targetAvoidAngle, Time.deltaTime * speedLerpSteer);
    }

    void Respawn()
    {
        if(!canRespawn) return;

        if(rb.velocity.magnitude < 2)
        {
            respawnCounter += Time.deltaTime;
            if(respawnCounter >= respawnWait)
            {
                if(currentPathObj == 0) transform.position = path[path.Length].position;
                else transform.position = path[currentPathObj - 1].position;
            }
        }
    }



    // DRAW GIZMOS
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distFromPath);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + CenterOfMass, .3f);

    }
}
